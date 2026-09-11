# SPEC_TarjetasCredito-003-ReglasNegocio

## Estado: Vigente (2026-09-08)

## Motor de recomendación de tarjeta óptima ("agente economista")

**Decisión**: v1 usa un motor de reglas determinístico y explicable (`TarjetasCredito.Domain.Reglas.MotorRecomendacionTarjeta`), no un LLM. Cada recomendación debe poder explicarse con una frase clara ("Se recomienda X porque su corte fue hace 2 días, lo que te da el mayor plazo antes de pagar"). Esto se puede evolucionar a un asistente basado en Claude API en un sprint futuro (ver sprints.md), usando el motor de reglas como fuente de verdad y el LLM solo para redactar la explicación en lenguaje natural.

### Algoritmo: ¿qué tarjeta usar hoy para una compra?

Para cada tarjeta activa del usuario con crédito disponible suficiente (`LimiteCredito - saldoUsadoEstimado >= monto`), se calcula:

1. **DiasDesdeCorte**: días transcurridos desde el último día de corte hasta hoy. Cuanto más reciente fue el corte, más días de plazo obtendrá la compra antes de que aparezca en un estado de cuenta y empiece a correr el plazo de pago (maximiza el "float" financiero, es decir el tiempo antes de tener que pagar sin intereses).
2. **DiasHastaProximoPagoSiUsaEstaTarjeta**: si compra hoy, en qué fecha exacta debe pagar esa compra sin generar intereses = próxima fecha de corte + `DiasParaPago`.
3. **UtilizaciónResultante**: (saldoUsadoEstimado + monto) / LimiteCredito. Se penalizan tarjetas que quedarían con utilización > 30% porque afecta negativamente el score de buró (ver sección siguiente).
4. **CostoSiNoSePagaDeContado**: si el usuario indica que no pagará de contado, se estima costo con `TasaInteresAnual`.

**Regla de selección**: se recomienda la tarjeta que maximiza `DiasHastaProximoPagoSiUsaEstaTarjeta` (más días para pagar sin intereses) entre las tarjetas cuya `UtilizaciónResultante` quede por debajo del 30%. Si ninguna queda debajo del 30%, se recomienda la de menor utilización resultante y se muestra una advertencia explícita. Si dos tarjetas empatan en días de plazo, se prefiere la de menor utilización resultante.

La salida siempre incluye: tarjeta recomendada, fecha límite de pago resultante, utilización resultante, y una explicación en una frase.

## Fechas de pago óptimas para mejorar el score de buró

Reglas (basadas en los factores estándar de scoring de buró: historial de pago ~35%, utilización de crédito ~30%, antigüedad, mezcla de crédito, consultas nuevas):

1. **Pagar antes de la fecha de corte, no solo antes de la fecha límite**: si el usuario paga el saldo antes de que cierre el ciclo (`DiaCorte`), el saldo reportado al buró en ese corte es menor o cero, lo que reduce la utilización reportada aunque haya usado la tarjeta durante el mes. Recomendación por defecto: pagar el saldo total 2-3 días antes del corte cuando sea posible.
2. **Nunca pagar solo el mínimo**: el sistema marca en rojo cualquier ciclo donde el pago registrado sea menor al `MontoEstadoCuenta` completo, con una nota de que pagar solo el mínimo genera intereses y no mejora el score tanto como el pago de contado.
3. **Mantener utilización total (todas las tarjetas) por debajo de 30%, idealmente por debajo de 10%** para el mejor efecto en el score.
4. **No dejar pasar la fecha límite de pago nunca**: un solo pago tardío reportado (>30 días de atraso) tiene el mayor impacto negativo de todos los factores. El sistema debe notificar (ver Web Push) con al menos 3 días de anticipación a cada `FechaLimitePago`.
5. El dashboard muestra, por cada tarjeta, la "fecha óptima de pago" = `DiaCorte - 3 días` (configurable), distinta de la "fecha límite" = `DiaCorte + DiasParaPago`.

## Generación y gestión de recordatorios de pago (`PaymentReminder`)

**Decisión — generación perezosa (lazy) como mecanismo base**: `GET /api/paymentreminders` **genera bajo demanda** los recordatorios que falten para el ciclo vigente y el siguiente de cada tarjeta activa del usuario, cada vez que se consulta el endpoint (idempotente: si ya existe un `PaymentReminder` para ese `CreditCardId` + `CicloFin`, no se duplica). La lógica vive en `PaymentReminderGenerationService.AsegurarRecordatoriosAsync(userId, ct)` (Infrastructure), que el controller llama con el usuario autenticado del request.

**Generación proactiva (Sprint 8, 2026-09-09)**: `PaymentReminderPushHostedService` (`BackgroundService`, ver SPEC-001) llama a ese mismo `AsegurarRecordatoriosAsync` pero **para todos los usuarios con al menos una tarjeta activa**, no solo el que hace el GET — necesario para poder avisarle a un usuario aunque no haya abierto la app. El mecanismo perezoso original se conserva sin cambios como respaldo (si el hosted service estuviera detenido, el usuario sigue viendo sus recordatorios correctos al entrar a Pagos). El hosted service corre cada hora (`PeriodicTimer`) y, tras asegurar los recordatorios, evalúa para cada uno no pagado si hoy coincide con alguna de las 3 fechas de aviso (ver "Notificaciones push" en SPEC-004) — cada aviso se manda como máximo una vez por recordatorio (columnas `NotificacionOptimaEnviadaUtc`/`NotificacionT3EnviadaUtc`/`NotificacionT1EnviadaUtc`).

**Ciclo que se genera por tarjeta activa — un solo ciclo a la vez (decisión revertida, 2026-09-11)**: `CicloInicio` = corte anterior al de hoy, `CicloFin` = próximo corte (`CicloFacturacionHelper.CicloVigente`), `FechaLimitePago` = `CicloFin + DiasParaPago`. **Antes** (Sprint 23) también se generaba el ciclo siguiente por adelantado, para que el usuario viera con antelación su próxima fecha límite — el usuario, probando con una tarjeta real, encontró que ver 2 pendientes por tarjeta (el del mes en curso y uno del mes siguiente sin monto de estado de cuenta todavía) era confuso, no útil. Se revirtió a generar solo el ciclo vigente; el próximo ciclo se genera solo hasta que efectivamente llegue (la próxima vez que `AsegurarRecordatoriosAsync` corra después de que el ciclo actual haya cerrado).

**Urgencia visual (para colorear/ordenar en la UI)**, calculada como días restantes hasta `FechaLimitePago` desde hoy:
- Vencido (`< 0` días) o vence hoy/mañana (`0-1` días): rojo/`--danger`, máxima prioridad de orden.
- Vence en 2-3 días (ventana de la "fecha óptima de pago" del punto 4 arriba): ámbar/alerta discreta.
- Más de 3 días: neutral/`--text-muted`.
- Recordatorios ya `Pagado = true` no participan en el orden de urgencia — se muestran aparte o al final, atenuados.

**Marcar como pagado**: `PUT /api/paymentreminders/{id}/pagado` — el usuario captura `MontoEstadoCuenta` (opcional, si lo sabe) y la fecha de pago se registra como "ahora" (UTC) salvo que se indique otra. Si `MontoEstadoCuenta` queda vacío/null, no se puede aplicar la regla 2 de arriba ("nunca pagar solo el mínimo") para ese ciclo — la UI lo deja como dato faltante, no bloquea marcar como pagado.

## Guía de fase del ciclo (Dashboard — Sprint 36, 2026-09-11)

**Decisión**: además de mostrar fechas ("Próximos vencimientos"), el Dashboard tiene una sección "Mejora tu score" que le dice explícitamente al usuario, por cada tarjeta activa, **qué acción tomar hoy** según en qué momento del ciclo está — aplicando las reglas de la sección "Fechas de pago óptimas" de arriba de forma accionable, no solo informativa. No introduce datos nuevos: se calcula en el cliente a partir de los mismos campos que ya expone `PaymentReminderDto` (`CicloFin`, `FechaOptimaPago`, `FechaLimitePago`, `Pagado`).

Cinco fases posibles para el recordatorio vigente de una tarjeta, evaluadas con `hoy` = fecha local del dispositivo:

1. **Pagado** (`Pagado == true`): "Ciclo al día" — refuerzo positivo, sin acción pendiente.
2. **Antes de la ventana óptima** (`hoy < FechaOptimaPago`): informa el corte próximo, sugiere pagar antes de `FechaOptimaPago` si se quiere bajar la utilización reportada — sin urgencia.
3. **Ventana óptima de pago** (`FechaOptimaPago ≤ hoy < CicloFin`): la regla 1 de "Fechas de pago óptimas" en acción — recomienda pagar ahora, antes de que cierre el ciclo, para que el saldo reportado en el próximo corte sea menor.
4. **Corte hecho, dentro del plazo** (`CicloFin ≤ hoy < FechaLimitePago`): el "período de gracia" estándar — recomienda pagar el total antes de `FechaLimitePago` para no generar intereses ni atraso.
5. **Vencido** (`hoy ≥ FechaLimitePago`, no pagado): máxima urgencia — un pago tardío es el factor de mayor impacto negativo en el score (regla 4 de arriba).

**Qué recordatorio se evalúa por tarjeta — bug real encontrado probando en navegador**: no es simplemente "el de `CicloFin` más reciente". Una tarjeta puede tener a la vez un recordatorio viejo sin pagar (su corte ya pasó, pero sigue dentro del plazo) *y* uno nuevo recién generado para el ciclo que acaba de empezar (`AsegurarRecordatoriosAsync` crea el del ciclo vigente en cuanto el anterior deja de serlo, sin esperar a que se pague). Elegir por `CicloFin` más reciente mostraba el nuevo y escondía que había un pago pendiente real. Regla correcta: el recordatorio **no pagado** con `FechaLimitePago` más próxima; si no hay ninguno pendiente, el más reciente ya pagado (para el estado "Ciclo al día").

Implementación: `RecordatorioRelevante`/`FaseDeCiclo` en `Pages/Home.razor` (privados, único consumidor por ahora — si se reusan en otro lugar, ej. el copy de las notificaciones push del Sprint 8, se deben extraer a un helper compartido en vez de duplicar la lógica).

### Monto sugerido de abono en "Ventana óptima de pago" (Sprint 38, 2026-09-11)

**Pedido explícito del usuario**: la fase 3 ("Ventana óptima de pago") solo decía *que* convenía pagar, sin decir *cuánto*. Ahora agrega un rango sugerido, calculado con los mismos umbrales de utilización de la regla 3 de "Fechas de pago óptimas" (arriba: ideal ≤10%, aceptable ≤30%) contra el `saldoActual` y `LimiteCredito` que ya trae `CreditCardDto` (`saldoActual = LimiteCredito - Disponible`, ver "Abonos y utilización neta"):

```
montoPara10 = max(0, saldoActual - 0.10 × LimiteCredito)   // para llegar al nivel ideal
montoPara30 = max(0, saldoActual - 0.30 × LimiteCredito)   // para llegar al techo aceptable
```

- Si la utilización actual ya es ≤10%, no se muestra ningún monto — la tarjeta ya está en un nivel óptimo, sugerir un abono sería ruido.
- Si `montoPara30` es 0 (utilización entre 10% y 30%), se sugiere un solo monto: *"abona hasta $X para bajar tu utilización al 10% ideal"*.
- Si la utilización supera 30%, se sugiere un **rango**: *"abona entre $Y (mínimo, llega a 30%) y $X (ideal, llega a 10%)"*.

Es una **opinión, no un monto exacto de estado de cuenta** — se basa en la utilización ya calculada por el Server (compras netas de abonos del ciclo vigente), no en una consulta nueva ni en el `MontoEstadoCuenta` capturado manualmente (que puede no estar disponible todavía). Implementación: `MontoSugeridoAbono` en `Pages/Home.razor`, junto a `FaseDeCiclo` (mismo criterio de "privado hasta que se reuse en otro lado").

## Abonos y utilización neta (Sprint 38, 2026-09-10)

**Problema real reportado por el usuario**: registró un pago a una tarjeta ("Marcar como pagado" en un recordatorio) y la utilización mostrada en Tarjetas/Dashboard no se movió. Causa raíz: `CreditCardsController.CalcularUtilizacionAsync` solo sumaba compras del ciclo vigente (`IPurchaseRepository.ObtenerSaldoCicloVigenteAsync`) — no existía ningún concepto de "pago/abono" que restara de ese saldo. "Marcar como pagado" solo cambiaba el booleano `Pagado` de un `PaymentReminder`, sin efecto en el cálculo.

**Decisión**: nueva entidad `CardPayment` ("abono") — ver [SPEC-002](SPEC_TarjetasCredito-002-BaseDeDatos.md) — que representa dinero puesto hacia una tarjeta, independiente de `Purchase` (que suma) y de "marcar un recordatorio como pagado" (que hoy solo es una bandera informativa por ciclo).

**Fórmula de utilización y disponible** (`CreditCardsController.CalcularSaldoNetoAsync` + `ToDto`):
```
saldoCompras = suma de Purchase.Monto con Fecha >= último corte
totalAbonado = suma de CardPayment.Monto con Fecha >= último corte
saldoNeto = max(0, saldoCompras - totalAbonado)
utilización = saldoNeto / LimiteCredito
disponible = LimiteCredito - saldoNeto
```
Un abono resta del saldo del ciclo vigente en cuanto se registra, sin importar si es antes o después del corte, y se pueden registrar tantos como se quiera dentro del mismo ciclo — cada uno baja la utilización (y sube el disponible) un poco más (responde directo al caso "pago parcial, y luego quiero abonar otra vez para ir disminuyendo"). `saldoNeto` nunca es negativo (un abono de más dentro de un ciclo con poca compra deja la utilización en 0%, no negativa) — pero `disponible` **sí** puede quedar negativo si el saldo excede el límite (sobregiro), a propósito, para no ocultarle al usuario que se pasó del límite (ver [SPEC-005](SPEC_TarjetasCredito-005-UI.md) "Tarjetas apiladas").

**`Disponible` en `CreditCardDto` (Sprint 38, pedido explícito del usuario)**: se expone junto a `UtilizacionActual` para que Tarjetas/Dashboard muestren "cuánto me queda", no solo el porcentaje usado — mismo `saldoNeto` de la fórmula de arriba, sin cálculo adicional en el cliente.

**"Marcar como pagado" ahora también crea un abono**: `PaymentRemindersController.MarcarPagado`, si recibe (o ya tenía guardado) un `MontoEstadoCuenta`, además de marcar `Pagado=true` crea automáticamente un `CardPayment` por ese monto (`Fecha` = `FechaPago`, `PaymentReminderId` = el recordatorio, para trazabilidad). Así el flujo que el usuario ya usaba (Historial → "Marcar como pagado") queda corregido sin que tenga que aprender una pantalla nueva. Si no se captura monto, no se crea abono — igual que hoy, sin dato no hay nada que restar.

**Registro manual de abonos**: sección "Abonos" en `/pagos` (`Recordatorios.razor`), independiente de los recordatorios por ciclo — para pagos parciales sueltos que el usuario quiera llevar aparte sin necesidad de "cerrar" un recordatorio. Mismo patrón CRUD que `Compras.razor` (crear/editar/eliminar).

**Simplificación conocida, aceptada a propósito (no es un bug)**: esta app no modela un "saldo pendiente que rueda mes a mes" (revolving balance) — la utilización ya ignoraba, desde antes de este sprint, cualquier deuda vieja no pagada de un ciclo cerrado (solo mira "lo comprado desde el último corte"). Un abono fechado hoy para saldar una deuda de un ciclo cerrado hace 2+ meses se resta igual del ciclo **vigente** (no existe ningún "saldo viejo" separado del cual restarlo), lo cual puede mostrar una utilización más baja de lo estrictamente correcto durante ese caso borde. Es la misma limitación ya documentada en "un solo ciclo a la vez" (Sprint 35) llevada a su consecuencia lógica en abonos — modelar el saldo pendiente completo que rueda entre ciclos queda como posible sprint futuro si el usuario lo pide.

## Consulta y mejora de buró de crédito

**Decisión**: v1 usa `IBuroCreditoService` con implementación mock (`BuroCreditoMockService`) que genera un score simulado realista (300-850, distribución centrada en 650-720) y una lista de factores (ej. "Utilización de crédito alta en 2 tarjetas", "Antigüedad de crédito corta") derivados de los datos reales que el usuario ya capturó en la app (utilización actual, historial de pagos registrados en `PaymentReminder`), para que las recomendaciones sean coherentes aunque el score en sí sea simulado. La UI deja explícito en todo momento que el score es una **estimación simulada**, no una consulta real a un buró, hasta que se conecte un proveedor real.

**Integración real futura**: cuando el usuario aporte credenciales de un proveedor (Buró de Crédito México, Círculo de Crédito, Equifax, TransUnion u otro), se implementa una nueva clase que satisface `IBuroCreditoService` sin cambiar el resto de la app. Requiere además: consentimiento explícito del usuario (aviso de privacidad), y cumplimiento de la normativa aplicable de protección de datos crediticios del país del proveedor.

Recomendaciones de mejora que la app puede dar (derivadas de datos propios, sin necesidad de buró real):
- Utilización alta por tarjeta o total → sugerir pagar antes de corte o solicitar aumento de línea.
- Pagos tardíos detectados en `PaymentReminder` → alertar y sugerir activar recordatorios push.
- Pocas tarjetas / antigüedad corta → explicar que cancelar tarjetas antiguas perjudica el score (no lo recomienda la app).
- Concentración de uso en una sola tarjeta → sugerir distribuir consumo si mejora utilización relativa.

## Multi-tarjeta, multi-usuario

Un usuario solo ve y opera sobre las tarjetas que él mismo dio de alta en su cuenta. No existe ninguna función de "compartir" tarjeta entre usuarios en v1.
