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

**Ciclos que se generan por tarjeta activa**:
- El ciclo vigente: `CicloInicio` = corte anterior al de hoy, `CicloFin` = próximo corte (usa `CicloFacturacionHelper`), `FechaLimitePago` = `CicloFin + DiasParaPago`.
- El ciclo siguiente al vigente (para que el usuario vea con antelación su próxima fecha límite, no solo la inminente).

**Urgencia visual (para colorear/ordenar en la UI)**, calculada como días restantes hasta `FechaLimitePago` desde hoy:
- Vencido (`< 0` días) o vence hoy/mañana (`0-1` días): rojo/`--danger`, máxima prioridad de orden.
- Vence en 2-3 días (ventana de la "fecha óptima de pago" del punto 4 arriba): ámbar/alerta discreta.
- Más de 3 días: neutral/`--text-muted`.
- Recordatorios ya `Pagado = true` no participan en el orden de urgencia — se muestran aparte o al final, atenuados.

**Marcar como pagado**: `PUT /api/paymentreminders/{id}/pagado` — el usuario captura `MontoEstadoCuenta` (opcional, si lo sabe) y la fecha de pago se registra como "ahora" (UTC) salvo que se indique otra. Si `MontoEstadoCuenta` queda vacío/null, no se puede aplicar la regla 2 de arriba ("nunca pagar solo el mínimo") para ese ciclo — la UI lo deja como dato faltante, no bloquea marcar como pagado.

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
