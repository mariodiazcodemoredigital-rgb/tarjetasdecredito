# Sprints — TarjetasCredito

Última actualización: 2026-09-09

Convención: `[ ]` pendiente, `[~]` en progreso, `[x]` completado.

## Sprint 0 — Documentación base
- [x] Crear AGENTS.md con flujo de trabajo y reglas de seguridad.
- [x] Crear SPEC_TarjetasCredito-001-Arquitectura.
- [x] Crear SPEC_TarjetasCredito-002-BaseDeDatos.
- [x] Crear SPEC_TarjetasCredito-003-ReglasNegocio (motor de recomendación, fechas óptimas, buró).
- [x] Crear SPEC_TarjetasCredito-004-Seguridad.
- [x] Crear SPEC_TarjetasCredito-005-UI.
- [x] Crear sprints.md (este archivo).

## Sprint 1 — Fundación de la solución ✅ (2026-09-08)
- [x] Scaffold de solución: Client (Blazor WASM PWA), Server (Web API), Shared, Domain, Infrastructure.
- [x] Entidades de dominio (ApplicationUser extendido, CreditCard, Purchase, PaymentReminder, BuroScoreSnapshot, PushSubscription).
- [x] DbContext (EF Core + SQLite), migración inicial aplicada.
- [x] Auth: Identity + `MapIdentityApi`, registro/login por correo, bearer token opaco desde el cliente (ver corrección en SPEC-004: no es JWT, el `ClaimsPrincipal` de UI se arma vía `GET /api/profile`).
- [x] Layout base responsivo (nav inferior móvil / lateral desktop), tema claro/oscuro con variables CSS y módulo de Configuración.
- [x] Páginas de Login/Registro (intuitivas mobile + desktop).
- [x] Build end-to-end verificado (`dotnet build` limpio) y prueba manual en navegador (registro, login, sesión persistente, CRUD tarjeta, compra, recomendación, buró mock, tema oscuro, vista móvil — todo probado en Sept 2026).

## Sprint 2 — PWA instalable (parcial)
- [x] manifest.webmanifest actualizado con nombre/paleta de la app (branding real de iconos pendiente — usa los placeholders del template).
- [x] Service worker generado por el template (cachea app shell); estrategia fina para llamadas API queda pendiente de revisión.
- [ ] Prueba de instalación real en Android/Chrome desktop y iOS/Safari (agregar a pantalla de inicio) — no probado en este sprint.

## Sprint 3 — Módulo de tarjetas ✅ (2026-09-08)
- [x] CRUD de tarjetas (alta, baja lógica) con aislamiento por usuario (probado). Edición aún no expuesta en UI.
- [x] Captura de día de corte, días para pago, límite, tasa, banco, marca, últimos 4 dígitos.
- [x] Visual de tarjeta tipo "card" premium en dashboard y listado.

## Sprint 4 — Módulo de compras ✅ (2026-09-08)
- [x] Registrar compra asociada a una tarjeta (monto, descripción, categoría, fecha, MSI opcional).
- [x] Historial de compras (probado end-to-end).
- [ ] Filtros de historial por categoría/rango de fechas (solo por tarjeta vía API; falta UI de filtro).
- [x] Cálculo de utilización actual por tarjeta a partir de compras registradas.

## Sprint 5 — Motor de recomendación ("agente economista") ✅ (2026-09-08)
- [x] `MotorRecomendacionTarjeta` en Domain según SPEC-003 (reglas determinísticas, ver [MotorRecomendacionTarjeta.cs](../../src/TarjetasCredito.Domain/Reglas/MotorRecomendacionTarjeta.cs)).
- [x] Endpoint + UI: "¿Qué tarjeta uso hoy?" con monto, muestra tarjeta recomendada + explicación (probado, funciona correctamente).
- [x] Cálculo y visualización de "fecha óptima de pago" vs "fecha límite" por tarjeta en la UI — ver Sprint 23.
- [x] Recordatorios de pago (PaymentReminder) generados automáticamente por ciclo — ver Sprint 23.

## Sprint 6 — Buró de crédito (mock) ✅ (2026-09-08)
- [x] `IBuroCreditoService` + `BuroCreditoMockService` (score simulado + factores derivados de datos reales del usuario, probado).
- [x] Pantalla de score con aviso claro de "estimación simulada, no consulta real".
- [x] Recomendaciones de mejora de score basadas en datos propios (utilización, mezcla de tarjetas).
- [x] Punto de extensión documentado en SPEC-003 para un proveedor real cuando el usuario aporte credenciales.

## Sprint 7 — Acceso por FaceID (WebAuthn) ✅ (2026-09-09)
- [x] Migración EF Core `AgregarPasskeys` — tabla `UserPasskeys`. Bug real encontrado y corregido: `IdentityDbContext<ApplicationUser>` hereda hasta la sobrecarga con `IdentityUserPasskey<string>`, pero esa entidad no se incluye en el modelo por convención; hubo que declararla a mano en `AppDbContext.OnModelCreating` (`HasKey(CredentialId)` + `OwnsOne(Data)`). También se detectó y corrigió un desfase de versión entre `dotnet-ef` (10.0.10) y el paquete del proyecto (10.0.11) que producía migraciones vacías sin avisar — ver detalle en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md).
- [x] `IdentityPasskeyOptions.ServerDomain` configurado en `Program.cs` (`localhost` en dev, `WebAuthn:ServerDomain` en producción).
- [x] `PasskeyEndpoints.cs` (minimal API, ver SPEC-001 "Endpoints de passkeys: excepción puntual a Controllers"): opciones de registro/acceso, registro, acceso (emite el mismo `AccessTokenResponse` que `/api/auth/login` vía `PasskeySignInAsync` + esquema Bearer), listar y eliminar credenciales — todos sobre los métodos nativos de `SignInManager`/`UserManager` de .NET 10, sin Fido2NetLib.
- [x] `wwwroot/js/faceIdAuth.js` + `Services/PasskeyApiService.cs` + `AuthService.IniciarSesionConFaceIdAsync`.
- [x] Botón "Entrar con Face ID" en Login (solo si el navegador lo soporta, passkey discoverable sin pedir correo) — corregido de paso un bug de contraste real: `.btn-secondary` normal no calzaba con el fondo oscuro fijo del login (se veía una píldora blanca sólida), se agregó el mismo tratamiento "glass" que ya tenían los inputs.
- [x] Tarjeta "Face ID" en Configuración: lista de credenciales, agregar, eliminar.
- [x] Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md), [SPEC-001](docs/specs/SPEC_TarjetasCredito-001-Arquitectura.md) y [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).
- [x] Probado en navegador: `POST /api/auth/passkey/registro/opciones` devuelve JSON de creación válido (`rp.id:"localhost"`, `residentKey:"preferred"`, challenge, etc.) — confirma que todo el pipeline nativo de .NET 10 funciona de extremo a extremo del lado servidor. En Login, al no haber una passkey real en el dispositivo de pruebas, `navigator.credentials.get()` fue rechazado por el navegador y la app lo manejó con gracia (toast "No se pudo verificar con Face ID. Intenta de nuevo.", botón nunca se quedó pegado). En Configuración, `navigator.credentials.create()` se quedó esperando indefinidamente (sin autenticador de plataforma real que lo resuelva) — comportamiento esperado del entorno de pruebas, no un bug: la ceremonia biométrica real completa (Face ID/Windows Hello, registrar y luego iniciar sesión con ella) queda pendiente de que el usuario la confirme en su propio dispositivo.
- [x] **Bug real reportado por el usuario probando en su propio equipo (2026-09-09)**: al intentar registrar el Face ID de verdad (con autenticador real, desde Visual Studio), tronaba con `InvalidOperationException: No passkey attestation is underway`. Causa raíz confirmada decompilando `SignInManager` (`ilspycmd`): el reto de la ceremonia viaja en una cookie efímera que nunca cruzaba entre Client (`:7160`) y Server (`:7287`, orígenes distintos). Corregido en dos partes — cookie con `SameSite=None`/`Secure` en `Program.cs`, y `PasskeyApiService` mandando `BrowserRequestCredentials.Include` en las 4 llamadas de la ceremonia — ver detalle completo en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md). Verificado con una llamada `fetch` directa (`credentials:'include'`) al flujo anónimo de acceso: antes del fix tronaba con 500/"No passkey assertion is underway"; después del fix, con una credencial inválida a propósito, responde `401` (el manejo normal de "credencial rechazada"), confirmando que el estado de la ceremonia ya viaja correctamente entre los dos orígenes. Pendiente de que el usuario confirme el flujo completo (registrar y luego entrar) con su Face ID/Windows Hello real.

## Sprint 8 — Notificaciones Web Push ✅ (2026-09-10)
- [x] Investigado antes de diseñar: .NET 10/ASP.NET Core **no trae soporte nativo de Web Push/VAPID** (a diferencia de passkeys) — confirmado por búsqueda exhaustiva de strings en los ensamblados del SDK 10.0.11. Se usa el paquete NuGet `WebPush` 1.0.13 (`web-push-libs/web-push-csharp`, target nativo `net10.0`), con su API verificada por reflexión antes de usarla.
- [x] Claves VAPID generadas una vez (`VapidHelper.GenerateVapidKeys()` desde un proyecto de consola desechable) y guardadas con `dotnet user-secrets` (`WebPush:VapidPublicKey/VapidPrivateKey/Subject`) — `TarjetasCredito.Server` no tenía `UserSecretsId`, se inicializó en este sprint.
- [x] Migración EF Core `NotificacionesPush`: índice único en `PushSubscriptionRecord.Endpoint` (upsert por navegador) + 3 columnas nuevas en `PaymentReminder` (`NotificacionOptimaEnviadaUtc`/`NotificacionT3EnviadaUtc`/`NotificacionT1EnviadaUtc`) para idempotencia.
- [x] `IPushSubscriptionRepository`/`PushSubscriptionRepository`, `PushSubscriptionsController` (`GET vapid-public-key`, `POST`/`DELETE`), `Shared/Dtos/PushSubscriptionDtos.cs`.
- [x] `PaymentReminderGenerationService` extraído de `PaymentRemindersController` (mismo comportamiento, ahora reusable) — reusado por el nuevo `PaymentReminderPushHostedService`.
- [x] `PaymentReminderPushHostedService`: primer `BackgroundService` de la app (`PeriodicTimer` cada hora + `IServiceScopeFactory`). Por cada usuario con tarjetas activas asegura sus recordatorios y evalúa los 3 disparos posibles por recordatorio (fecha óptima, T-3 y T-1 de la fecha límite — semántica confirmada con el usuario), enviando el push que corresponda y podando suscripciones caducadas (`WebPushException` 404/410).
- [x] Cliente: `wwwroot/js/webPush.js` (feature-detect, suscribir/desuscribir/estado actual vía `PushManager`), `PushNotificationApiService`, tarjeta "Notificaciones" en Configuración (opt-in explícito, igual que Face ID).
- [x] `push`/`notificationclick` agregados a **ambos** `service-worker.js` y `service-worker.published.js` (duplicado a propósito para poder probar en `dotnet run`/dev, no solo en `dotnet publish`) — abren `/pagos` al hacer clic.
- [x] Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md), [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md) y [SPEC-001](docs/specs/SPEC_TarjetasCredito-001-Arquitectura.md).
- [x] **Probado end-to-end con envío real de prueba**: se registró un usuario y tarjeta de prueba, se generó un par de llaves EC P-256 real (no simulada) para simular una suscripción válida, se forzó la `FechaLimitePago` de un recordatorio a "mañana" (dispara T-1) y se insertó la suscripción de prueba directo en la base. Al reiniciar el Server (el `BackgroundService` corre un tick inmediato al arrancar), el log confirmó el flujo completo real: intento de envío contra el endpoint real de FCM → `WebPushException` (suscripción inexistente) → poda automática (`DELETE FROM PushSubscriptions` confirmado) → flag `NotificacionT1EnviadaUtc` marcado (idempotencia). Se encontró y corrigió un detalle real durante esta prueba (no del código de producción, sino del script de prueba): el primer intento insertó el `Id` de prueba en minúsculas por SQL crudo, y como EF Core/Microsoft.Data.Sqlite comparan parámetros `Guid` en mayúsculas, la búsqueda de poda no encontraba la fila — quedó documentado como una trampa a tener en cuenta si se vuelve a insertar datos de prueba a mano en tablas con clave `Guid`.
- [x] También probado en navegador: `Configuración` detecta el soporte de Push API y muestra el botón "Activar notificaciones"; `GET /api/pushsubscriptions/vapid-public-key` responde 200 con la clave real. El navegador de automatización de esta sesión corre en un contexto que Chrome trata como incógnito, donde el propio Chrome bloquea la Push API a propósito (`Chrome currently does not support the Push API in incognito mode`) — la app lo manejó con gracia (toast en español, sin crash), pero la suscripción real desde un navegador normal queda pendiente de que el usuario la confirme en su propio dispositivo (misma limitación ya documentada para la ceremonia real de Face ID en el Sprint 7).
- [x] Verificado sin regresiones: `PaymentRemindersController.ObtenerTodos` (ahora vía `PaymentReminderGenerationService`) probado en navegador — genera los mismos 2 recordatorios (vigente + siguiente) que antes de la extracción.
- [x] **Confirmado por el usuario en su propio dispositivo (2026-09-10)**: notificación push real recibida de Windows/Chrome ("Último día para pagar sin atraso... Mañana vence tu fecha límite de pago"), cerrando la limitación pendiente de la nota anterior. Dos ajustes reales que salieron de esta prueba con el usuario:
  1. Los secretos VAPID que yo guardé con `dotnet user-secrets` desde esta sesión no eran visibles para el proceso que el usuario corre desde Visual Studio (la petición a `vapid-public-key` le daba 404) — aunque ambos corren como el mismo usuario de Windows, son sesiones/perfiles distintos. Se resolvió pegando las mismas claves directamente en el `secrets.json` del usuario vía "Administrar secretos de usuario" de Visual Studio. **Lección para sesiones futuras**: no asumir que un `user-secrets set` hecho desde esta sesión será visible en el proceso que el usuario corre por su cuenta — mejor darle el valor para que lo pegue él mismo.
  2. Se agregó `POST /api/pushsubscriptions/prueba` + botón "Enviar notificación de prueba" en Configuración (visible solo si ya está suscrito) para poder confirmar la entrega real sin depender de esperar a que el `BackgroundService` dispare un aviso (solo revisa al arrancar el Server o cada hora) — turnó útil como herramienta de diagnóstico y queda como feature permanente.
  3. **Bug real encontrado con la notificación ya recibida**: el ícono mostrado se veía como una mancha azul irreconocible en vez del logo de la app. Causa: `wwwroot/icon-192.png` estaba corrupto/mal generado desde el Sprint 11 (una forma azul con esquina redondeada ocupando parte del lienzo, no el ícono real) — el resto de los íconos (`icon-512.png`, `favicon-64.png`, `apple-touch-icon.png`) sí estaban bien. Se regeneró `icon-192.png` redimensionando `icon-512.png` (que sí tiene el logo correcto de tarjeta de crédito). Esto también corrige el ícono usado por el manifest PWA (`icon-192.png` es uno de los dos tamaños declarados en `manifest.webmanifest`), no solo el de las notificaciones.

## Sprint 9 — Pulido UI premium y tema oscuro ✅ (2026-09-10)
- [x] Responsive base probado en móvil y desktop (nav inferior/lateral, dashboard, tarjetas, buró).
- [x] Módulo de Configuración: cambio de tema (probado, persiste en perfil vía `/api/profile/theme`).
- [x] Gestión de método FaceID y de notificaciones en Configuración (Sprints 7 y 8, tarjetas "Face ID" y "Notificaciones").
- [x] Micro-interacciones y estados vacíos/carga más cuidados:
  - `Components/EstadoCarga.razor` (skeleton con barrido de brillo, respeta `prefers-reduced-motion`) reemplaza el `<p>Cargando...</p>` plano en Dashboard, Tarjetas, Compras, Pagos, Buró y Configuración (Face ID/Notificaciones).
  - `Components/EstadoVacio.razor` (icono + título + descripción + CTA opcional) reemplaza las líneas sueltas de "no hay datos" en las mismas páginas.
  - **Gap real encontrado**: `Tarjetas.razor` no tenía ningún estado vacío — con 0 tarjetas activas no mostraba ningún mensaje. Corregido.
  - `.btn:active:not(:disabled) { transform: scale(0.97) }` — feedback táctil al presionar cualquier botón.
  - `.entrada-suave` agregado a los ítems individuales de listas (compras, recordatorios, factores de buró) que antes solo animaban el contenedor.
  - Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).
  - Probado en navegador (desktop y móvil, tema claro y oscuro): estado vacío de Dashboard/Tarjetas/Compras/Pagos con una cuenta de prueba sin datos y luego con una tarjeta registrada, sin regresiones en el flujo normal.

## Sprint 10 — Endurecimiento y pruebas
- [ ] Pruebas de aislamiento multi-usuario (un usuario no puede ver tarjetas/compras de otro).
- [ ] Pruebas del motor de recomendación con casos límite (todas las tarjetas sobre 30% de utilización, empates, etc.).
- [ ] Revisión de seguridad (secretos, HTTPS, tokens) antes de cualquier despliegue.

## Sprint 11 — Perfil de usuario, identidad en UI y favicon ✅ (2026-09-08)
- [x] Favicon/íconos reales de la app (tarjeta de crédito, paleta de marca) en `favicon-32.png`, `favicon-64.png`, `apple-touch-icon.png`, `icon-192.png`, `icon-512.png`, referenciados en `index.html`.
- [x] `ApplicationUser.Nombres` / `Apellidos` (reemplaza `DisplayName`), migración `PerfilYColorTarjeta` aplicada.
- [x] `ProfileController`: endpoint `PUT /api/profile` para actualizar Nombres/Apellidos.
- [x] Cliente: `UserProfileService` (carga perfil tras login/restauración de sesión, expone Nombres/Apellidos/Email/Iniciales).
- [x] Sidebar de escritorio: tarjeta de usuario (avatar iniciales + nombre + cerrar sesión) al fondo del nav lateral — probado en navegador.
- [x] Dashboard: saludo "Hola, {Nombres}" — probado en navegador.
- [x] Configuración: formulario para editar Nombres/Apellidos — probado en navegador (guarda y refleja en sidebar/dashboard al instante).

## Sprint 12 — Tarjetas apiladas, personalización y SweetAlert2 ✅ (2026-09-08)
- [x] `CreditCard.ColorHex` + autoasignación de color por paleta rotativa (`PaletaColoresTarjeta`) si el usuario no elige uno.
- [x] Selector de color (paleta + personalizado con `<input type="color">`) en el formulario de alta de tarjeta — probado en navegador.
- [x] Glifos SVG/CSS propios por marca (Visa/Mastercard/Amex/Otra) en `.credit-card-visual` vía componente `MarcaBadge`, sin logos oficiales.
- [x] Componente `CreditCardVisual` + CSS `.tarjetas-stack` reusado en Dashboard y en Tarjetas — probado en navegador (efecto de pila con hover funcionando).
- [x] Botón "Dar de baja" individual dentro de cada tarjeta de la pila (ícono de papelera en la esquina de cada card).
- [x] Espaciado corregido entre "+ Agregar tarjeta" y el listado.
- [x] Integrar SweetAlert2 (CDN + wrapper `DialogService`/`dialogs.js`), con estilos de esquinas redondeadas acordes al tema — probado en navegador.
- [x] Confirmación SweetAlert2 antes de dar de baja una tarjeta — probado en navegador (diálogo con esquinas redondeadas, botones custom).
- [x] Reemplazar mensajes de error/validación inline por toasts SweetAlert2 con colores discretos en Login, Register, Tarjetas y Compras.

## Sprint 13 — Edición de tarjetas y pulido de UX ✅ (2026-09-08)
- [x] Endpoint `PUT /api/creditcards/{id}` (edición, con validación y recálculo de utilización).
- [x] `CreditCardApiService.ActualizarAsync`.
- [x] Componente `TarjetaFormCard`: mismo formulario para alta y edición, con vista previa de la tarjeta en vivo (cambia mientras escribes: alias, banco, dígitos, marca, color) — probado en navegador end-to-end.
- [x] Íconos individuales de editar (lápiz) y dar de baja (papelera) por tarjeta en `CreditCardVisual`.
- [x] Corregido bug real de Blazor: `InputText`/`InputNumber` con `@bind-Value:event="oninput"` crashea (`ArgumentException`) — documentado en SPEC-005 y corregido usando `<input>` nativo con `@bind:event="oninput"`.
- [x] Espacio entre botones "Cancelar"/"Dar de baja" del diálogo SweetAlert2 (`.swal2-actions { gap }`).
- [x] Botón "Cerrar sesión" del sidebar restyleado como botón (`.btn.btn-danger`), ya no como link subrayado.
- [x] Quitada la sección de tarjetas apiladas del Dashboard (quedó solo el widget de recomendación).

## Sprint 14 — Pulido de tarjeta visual y protección de formularios sin guardar ✅ (2026-09-08)
- [x] Inputs monetarios (Límite de crédito, Monto de compra en Compras y Dashboard) con prefijo "$" (`.input-currency`).
- [x] Límite de crédito visible directamente en el card visual (formateado con separador de miles), siempre visible aunque la tarjeta esté apilada/colapsada.
- [x] Día de corte movido al header del card visual (junto al límite) para que también sea visible siempre, no solo al expandir — se quitó de la fila inferior para no duplicar.
- [x] Diálogo de confirmación (SweetAlert2, ícono de pregunta) antes de reemplazar un formulario de tarjeta abierto sin guardar (alta o edición) por otro — nuevo método genérico `DialogService.ConfirmarAsync` además del `ConfirmarEliminarAsync` existente.
- [x] Todo probado end-to-end en navegador.

## Sprint 15 — Login dinámico (original) ✅ (2026-09-08)
- [x] Layout dividido en desktop: panel visual (gradiente + 3 tarjetas ilustrativas propias con parallax al cursor) + formulario. Panel visual oculto en móvil.
- [x] Efecto parallax implementado desde cero (`wwwroot/js/loginVisual.js`, sin librerías ni assets de terceros) — probado en navegador, funciona en todo el rango del panel.
- [x] Tipografía Sora (Google Fonts, licencia libre) para el headline del panel visual.
- [x] Se rechazó explícitamente clonar el sitio de referencia que trajo el usuario (sitio real de otra empresa con instrucción de ocultar la atribución); se adoptaron solo técnicas genéricas de UI con contenido/copy/assets 100% propios de TarjetasCredito. Detalle en SPEC-005.
- [x] Bug real encontrado y corregido de paso: `IntentarRestaurarSesionAsync` no tenía try/catch — si el Server no respondía, la app se quedaba trabada en la pantalla de carga en vez de mostrar el login.
- [ ] Pendiente: aplicar el mismo layout dividido a `Register.razor` (hoy sigue con el diseño simple centrado anterior).

## Sprint 16 — Login en una sola sección + patrón "parallax stage" documentado ✅ (2026-09-08)
- [x] Login rediseñado de panel dividido a **sección única** (mismo orden en desktop y móvil): encabezado → animación de tarjetas → formulario. Ya no se oculta la animación en móvil.
- [x] `wwwroot/js/loginVisual.js` renombrado/generalizado a `wwwroot/js/parallaxStage.js`: motor reusable con `init(stageSelector, itemSelector)`, ya no atado al login.
- [x] Bug real encontrado y corregido: el JS pisaba el `transform` completo de cada tarjeta al mover el cursor, perdiendo su posición base del abanico (las 3 tarjetas colapsaban hacia el centro). Corregido con variables CSS `--base-x/--base-y/--base-r` (posición de reposo, en CSS) + `--px/--py/--pr` (empujón del cursor, en JS), combinadas vía `calc()` — el JS ya nunca vuelve a escribir `transform` directamente.
- [x] Patrón documentado de forma prominente y reutilizable (no solo en este sprint): sección dedicada "Patrón reutilizable: parallax stage" en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) con guía de uso paso a paso, más puntero desde [AGENTS.md](AGENTS.md) para que cualquier sesión futura lo encuentre antes de reinventar un efecto similar.
- [x] Todo probado en navegador (desktop y móvil).

## Sprint 17 — Login: formulario "glass" + eliminar scroll + fin del traslape ✅ (2026-09-08)
- [x] Formulario del login rediseñado a estilo "glass" (translúcido, `backdrop-filter: blur`, texto blanco/Sora) para que combine con el panel oscuro — ya no es una tarjeta blanca genérica que desentonaba.
- [x] Eliminado el scroll del login: `.login-shell` pasó a `position: fixed; inset: 0;` para escapar del padding de `.app-content` (pensado para páginas autenticadas con nav inferior) que estaba sumando altura extra. Tamaños/gaps comprimidos con `clamp()`. Verificado sin scroll en desktop y móvil.
- [x] Corregido el traslape de las tarjetas flotantes sobre el formulario: `.login-visual-stage` ahora tiene `overflow:hidden` como límite duro, y el `depthStep` del parallax bajó de 4 a 3 para este espacio compacto. Probado en posiciones extremas del cursor sin traslape.
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md): sección "Login" ampliada con el porqué de `position:fixed`, el patrón "glass", y el límite `overflow:hidden`; sección del patrón "parallax stage" ampliada con el parámetro `depthStep` y el tip de `overflow:hidden` para espacios compactos.

## Sprint 18 — Login: espacio real para las tarjetas, anclado al top ✅ (2026-09-09)
- [x] `overflow:hidden` por sí solo no bastaba: el stage era muy angosto y las tarjetas se veían recortadas Y aun así pegadas al encabezado/formulario. Se agrandó el "carril" real (`.login-visual-stage` de ~110px a `clamp(140px, 19vh, 175px)`, `gap` de `.login-section` subido) para que el recorrido completo del parallax quepa con margen antes de acercarse a los vecinos. `overflow:hidden` se conserva solo como red de seguridad para viewports extremos.
- [x] Sección anclada cerca del top (`align-items:flex-start` + padding-top chico) en vez de centrada verticalmente — antes dejaba demasiado aire arriba en viewports altos.
- [x] Encabezado y formulario compactados un poco más (fuentes/paddings) para que el stage más grande siga cabiendo sin scroll — probado sin scroll hasta 400×680px.
- [x] Probado en navegador: hover en las 4 esquinas + centro del stage, desktop, móvil (375×812) y viewport corto (400×680) — ninguna posición del cursor logra tocar el encabezado ni el formulario.
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).

## Sprint 19 — Causa raíz del recorte: motor "parallax stage" sin tope al empujón ✅ (2026-09-09)
- [x] Causa raíz real encontrada (Sprint 18 solo mitigaba el síntoma): `parallaxStage.js` no acotaba `dx`/`dy`. Como el listener de `pointermove` es global, el cursor podía estar muy lejos del stage (ej. hasta el fondo de la página) y el desplazamiento crecía sin límite — mientras más se alejaba el cursor, más se salía la tarjeta del `overflow:hidden` y más se veía "recortada"/"perdida", exactamente como describió el usuario.
- [x] Corregido con un `clamp(-1, 1)` sobre `dx`/`dy` antes de multiplicarlos por `depth` — el elemento ahora llega a un desplazamiento máximo y se queda ahí, siempre completo, sin importar qué tan lejos se mueva el cursor más allá del stage. Es una mejora al motor genérico, no un parche solo para el login — beneficia cualquier uso futuro del patrón.
- [x] Probado en navegador con el cursor en las 4 esquinas del viewport (muy fuera del stage) y en el fondo de la página: la tarjeta siempre se ve completa, nunca recortada. Confirmado sin scroll en desktop y móvil.
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md): sección "Cómo funciona" del patrón ampliada con la explicación del clamp; nota del login corregida para explicar la causa raíz real (no solo espacio insuficiente).

## Sprint 20 — Aclaración de interacción táctil + tag oculto sin mover el layout ✅ (2026-09-09)
- [x] Aclarado (pregunta del usuario): en móvil el efecto de las tarjetas responde a arrastrar el dedo (`pointermove` también cubre touch-drag), no a solo tener el teléfono abierto ni a inclinarlo. Decisión explícita: se deja así, sin agregar giroscopio ni desactivar la interacción táctil.
- [x] Quitado el texto del tag "tarjetas de crédito" del login, pero sin recorrer el resto del contenido hacia arriba: el `<span>` se dejó en el layout con `visibility:hidden` (conserva su alto) en vez de eliminarlo del DOM. Probado en desktop y móvil — el encabezado "Tu dinero, bajo control." queda exactamente en la misma posición que antes.
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).

## Sprint 21 — Touch no se sentía fluido: dos causas reales corregidas en el motor ✅ (2026-09-09)
- [x] Usuario reportó que arrastrar con el dedo en móvil no respondía tan fluido como el mouse en desktop. Encontradas dos causas reales, ambas corregidas en `parallaxStage.js`/`app.css` (no son parches solo del login):
  1. Faltaba `touch-action: none` en `.login-visual-stage`/`.login-visual-copy` — el navegador se tomaba un instante decidiendo si el arrastre era scroll/zoom antes de entregarle el evento a JS, y podía activar selección de texto nativa. Agregado `touch-action:none` + `user-select:none`.
  2. La transición CSS de 0.15s (para suavizar el mouse) hacía que en touch —eventos más espaciados— la tarjeta se sintiera "persiguiendo" al dedo en vez de pegada a él. `parallaxStage.js` ahora detecta `e.pointerType` y en touch pone `transitionDuration:'0s'` (seguimiento 1:1 inmediato); en mouse conserva la transición.
- [x] Verificado con eventos `PointerEvent` sintéticos (`pointerType:'touch'` vs `'mouse'`) inyectados por consola: `transitionDuration` cambia correctamente entre `0s` y `0.15s` según el tipo de puntero; `touch-action`/`user-select` confirmados en `computedStyle`. (El drag automatizado del navegador de pruebas se colgó en modo mobile — se verificó la lógica directamente, no fue posible grabar un drag real de punta a punta con la herramienta.)
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md), incluida la guía de reutilización del patrón.

## Sprint 22 — h1 "marcado" al recargar + Register unificado con el login ✅ (2026-09-09)
- [x] Corregido bug real: `App.razor` usa `<FocusOnNavigate Selector="h1" />` (accesibilidad de Blazor) que enfoca el `<h1>` de cada página en cada navegación/recarga, dejando el contorno de foco del navegador visualmente pegado a "Bienvenido" — se veía como si quedara "seleccionado". Corregido con `h1:focus { outline: none; }` en `app.css` (no se quitó `FocusOnNavigate`, sigue funcionando para lectores de pantalla).
- [x] `Register.razor` unificado con el mismo diseño del login (panel oscuro + tarjetas con parallax + formulario "glass"): se extrajo el layout compartido a `Components/AuthShell.razor` (parametrizado con headline/subtítulo + `ChildContent` para el formulario), usado ahora por `Login.razor` y `Register.razor` — sin duplicar CSS/markup/la llamada a `parallaxStage.init`.
- [x] Probado en navegador: login y register se ven idénticos en estructura, el parallax funciona en ambos, sin scroll en desktop/móvil, `h1` ya no se ve marcado en ninguna de las dos tras recargar varias veces.
- [x] Documentación actualizada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).

## Sprint 23 — Recordatorios de pago ✅ (2026-09-09)
- [x] `CicloFacturacionHelper.CiclosARecordar(diaCorte, fecha)` en Domain: calcula el ciclo vigente + el siguiente a partir del día de corte, reusando `ProximoCorte`/`UltimoCorte` ya existentes.
- [x] `IPaymentReminderRepository` + `PaymentReminderRepository` (EF, con `.Include(Tarjeta)`), registrado en DI del Server.
- [x] `PaymentRemindersController`: `GET /api/paymentreminders` genera de forma perezosa (sin `IHostedService`/cron) los recordatorios faltantes del ciclo vigente + siguiente de cada tarjeta activa del usuario antes de devolver la lista — idempotente vía lookup por `(CreditCardId, CicloFin)`. `PUT /api/paymentreminders/{id}/pagado` marca como pagado con monto opcional del estado de cuenta (fecha de pago = UTC-now si no se especifica).
- [x] DTOs compartidos (`PaymentReminderDto`, `UrgenciaPagoDto`, `MarcarPagadoRequest`) + `PaymentReminderApiService` en el cliente.
- [x] Página `Recordatorios.razor` (`/pagos`): sección "Pendientes" (ordenada por urgencia, badge de días restantes, input de monto opcional, botón "Marcar como pagado" con confirmación SweetAlert2 + toast) y "Historial de pagos".
- [x] Umbrales de urgencia para la UI documentados en [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md): Vencido ≤1 día, Inminente ≤3 días, Próximo ≤7 días, Normal en adelante; los ya pagados quedan fuera del ordenamiento por urgencia.
- [x] Enlace "📅 Pagos" agregado al nav lateral y al nav inferior (`MainLayout.razor`), entre Compras y Buró.
- [x] Widget "Próximos vencimientos" en el Dashboard (`Home.razor`): top 3 recordatorios pendientes ordenados por días restantes, con color de tarjeta, fecha con badge de urgencia y enlace "Ver todos" a `/pagos`.
- [x] Probado en navegador end-to-end: generación automática de recordatorios para tarjetas existentes, marcar uno como pagado (mueve correctamente de "Pendientes" a "Historial de pagos" con monto y fecha persistidos), y widget del Dashboard verificado visualmente.
- [x] Documentación actualizada en [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md) con la sección "Generación y gestión de recordatorios de pago".

## Sprint 24 — Mensajes de Identity en español ✅ (2026-09-09)
- [x] Encontrado: los mensajes de error de registro/login (ej. requisitos de contraseña, correo duplicado) venían en inglés porque son el `IdentityErrorDescriber` default de ASP.NET Core Identity, no texto propio de la app.
- [x] `DescriptorErroresIdentityEspanol` (Infrastructure/Identity) sobreescribe todos los mensajes de `IdentityErrorDescriber` al español, registrado con `.AddErrorDescriber<...>()` en el Server.
- [x] Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md), sección "Idioma: todo mensaje visible al usuario debe estar en español".
- [x] Probado en navegador: registro con contraseña débil y con correo duplicado muestran el toast en español.

## Sprint 25 — Navegación "wave" (pill elástica) ✅ (2026-09-09)
- [x] `wwwroot/js/navWave.js`: motor JS reusable (`init(navSelector, itemSelector, orientation)` / `detener(navSelector)`), mismo esquema "JS solo pone variables CSS, la transición la anima CSS" que `parallaxStage.js`.
- [x] `.nav-wave-pill` en `app.css`: pill con degradado que se desliza con rebote (`cubic-bezier` overshoot) detrás del ítem con hover/foco/ruta activa, en `.side-nav` (vertical) y `.bottom-nav` (horizontal).
- [x] Fallback táctil/navegación: `MutationObserver` sobre la clase `active` de los `<a>` del nav, para que la pill se reposicione al navegar aunque no haya hover real (pantallas táctiles) — verificado navegando por clic entre Dashboard/Tarjetas/Compras/Buró/Pagos/Configuración, la pill sigue la ruta activa cada vez.
- [x] `MainLayout.razor`: inyecta `IJSRuntime`, agrega `<span class="nav-wave-pill">` a ambos navs, llama `navWave.init` en **cada** `OnAfterRenderAsync` (no solo `firstRender` — necesario, ver bugs abajo) y `navWave.detener` en `Dispose()`.
- [x] Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md), sección "Patrón reutilizable: navegación 'wave' (pill elástica)", incluidos los 4 bugs reales encontrados y corregidos durante la implementación (nav no montado aún en el primer render; Blazor reemplaza el `<nav>` cuando el perfil termina de cargar, dejando listeners enlazados a nodos desconectados; medición en `0` justo al patchear el DOM; medición incorrecta al cruzar el breakpoint móvil/escritorio en un resize).
- [x] Probado en navegador: posicionamiento inicial correcto sobre la ruta activa (desktop y móvil vía `resize_window`), navegación por clic entre las 6 secciones, cruce del breakpoint tablet↔desktop sin quedar mal posicionada, tema claro y oscuro. La interacción de **hover** en sí se verificó inyectando `PointerEvent`/`mouseenter` sintéticos y comprobando el cambio de clase + posición de la pill antes/después (la herramienta de automatización del navegador usada en esta sesión no dispara eventos de puntero reales con su acción de "hover", mismo tipo de limitación ya documentada en el Sprint 21 para el drag táctil del login) — la lógica de la app usa la API estándar de Pointer/Mouse Events, por lo que un mouse real la dispara sin problema.

## Sprint 26 — Elevación premium de pantallas internas ✅ (2026-09-09)
- [x] Nuevas clases reusables en `app.css`: `.page-icon-chip`, `.card-interactive`, `.entrada-suave` (+ `@keyframes entrar`), `h1`/`h2`/`h3`/`h4` con Sora, `.btn` con degradado.
- [x] Compras: historial migrado de `style` inline a `.compra-item` + `.card-interactive`.
- [x] Buró: tarjeta de score y factores migrados a `.buro-score-card`/`.buro-factor` + `.card-interactive` (con halo decorativo detrás del número).
- [x] Configuración: selector de tema migrado a `.tema-selector` (control segmentado).
- [x] Dashboard, Tarjetas, Compras, Pagos, Buró, Configuración: `.page-icon-chip` junto al `<h1>` + `.entrada-suave` en la tarjeta principal.
- [x] Decisión documentada en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md): se mantiene el tema claro/oscuro configurable (no se adopta el fondo oscuro fijo del login en pantallas internas) — decisión explícita del usuario al validar el plan antes de implementar.
- [x] Probado en navegador en tema claro y oscuro (Dashboard, Tarjetas, Compras, Buró, Pagos, Configuración): alta de tarjeta con vista previa en vivo, registro de compra (toast + aparece en historial con el nuevo estilo), recordatorios generados automáticamente, cambio de tema — todo funcional, sin regresiones.

## Sprint 27 — Manejo de errores de red y estados sin conexión ✅ (2026-09-09)
- [x] `Services/ResultadoApi.cs` + `Services/ApiCallHelper.cs`: clasifican falla de conexión (`HttpRequestException.StatusCode is null` / `TaskCanceledException`) vs. error de servidor (`StatusCode` presente).
- [x] `CreditCardApiService.ObtenerTodasAsync`, `PurchaseApiService.ObtenerTodasAsync`, `PaymentReminderApiService.ObtenerTodosAsync`, `BuroApiService.ObtenerScoreAsync` migrados a `Task<ResultadoApi<T>>`.
- [x] `Components/EstadoSinConexion.razor`: tarjeta reusable con mensaje + botón "Reintentar", usada en Buró, Tarjetas, Compras, Pagos y Dashboard cuando falla la carga.
- [x] `AuthService.RegistrarAsync`/`IniciarSesionAsync` y `UserProfileService.ActualizarAsync` protegidos contra falla de conexión (fix a nivel de servicio, sin tocar las páginas).
- [x] Acciones de botón (`TarjetaFormCard.GuardarAsync`, `Tarjetas.DesactivarAsync`, `Compras.GuardarAsync`, `Recordatorios.MarcarPagadoAsync`, `Home.RecomendarAsync`) protegidas con `try/catch` + toast de error, sin dejar el botón pegado en "Guardando...".
- [x] `MainLayout.razor` envuelve `@Body` en `<ErrorBoundary>` (usa la clase `.blazor-error-boundary` que `app.css` ya traía preparada y sin usar).
- [x] Banner default `#blazor-error-ui` traducido al español ("Ocurrió un error inesperado." / "Recargar").
- [x] Documentado en [SPEC-001](docs/specs/SPEC_TarjetasCredito-001-Arquitectura.md), sección "Manejo de errores de red y estados sin conexión".
- [x] Probado en navegador apagando el Server (proceso detenido, no simulación): Buró, Dashboard, Tarjetas, Compras y Pagos muestran "No se pudo conectar. Revisa tu conexión a internet." con botón "Reintentar" en vez de spinner trabado + banner en inglés — confirmado con `read_network_requests` que las llamadas fallan con `ERR_CONNECTION_REFUSED` y con la consola que no queda ninguna excepción .NET sin manejar. Guardar perfil con el Server apagado: el botón vuelve de "Guardando..." a "Guardar" (no se queda pegado). "Reintentar" en Buró recupera los datos correctamente al volver a levantar el Server, sin recargar la página. Flujo normal (con Server arriba) verificado sin regresiones: alta de tarjeta, listado, y navegación entre módulos.

## Sprint 28 — Privacidad del login en dispositivos compartidos ✅ (2026-09-09)
- [x] `Login.razor`/`Register.razor`: `autocomplete="off"` en el `<EditForm>` y en los campos de correo/contraseña, para que el navegador no prellene ni sugiera correos usados antes en ese dispositivo.
- [x] No se usa `autocomplete="new-password"` (bloquearía el guardado de contraseñas del navegador) — el usuario confirmó que el navegador sí puede seguir ofreciendo guardar la contraseña, solo no quiere que la pantalla de login la revele/sugiera visualmente.
- [x] Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md), sección "Privacidad del login en dispositivos compartidos" (incluye la limitación conocida: Chrome/Edge ignoran `autocomplete="off"` en contraseñas por política propia del navegador, no es controlable al 100% desde el sitio).
- [x] Probado en navegador: confirmado con JS (`getAttribute('autocomplete')`) que el `<form>` y ambos `<input>` (correo y contraseña) renderizan `autocomplete="off"` en Login y Register; sin regresión visual.
- [x] **Bug real confirmado por el usuario (2026-09-09): `autocomplete="off"` no fue suficiente** — Chrome igual rellenaba el correo/contraseña del usuario anterior al volver al login tras cerrar sesión. Corregido con `readonly` + `onfocus="this.removeAttribute('readonly')"` (HTML plano, no Blazor) en ambos campos de Login y Register — el navegador no autocompleta un campo de solo lectura, y se vuelve editable de forma síncrona en cuanto el usuario lo toca. Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md).

## Sprint 29 — Auditoría completa de notificaciones en español ✅ (2026-09-09)
- [x] `AuthService.IniciarSesionAsync`: mensaje fijo en español ("Correo o contraseña incorrectos.") en vez de reenviar `ProblemDetails.Title` (bug real reportado por el usuario: mostraba "Unauthorized" crudo).
- [x] `AuthService.ExtraerErroresAsync` (usado por `RegistrarAsync`): quitado el mismo *fallback* a `problema.Title`, reemplazado por mensaje fijo en español.
- [x] `Services/ErrorMessageHelper.cs` (nuevo): solo confía en el cuerpo de una respuesta fallida cuando su `Content-Type` es `text/plain` (la única vía por la que los controllers de esta app escriben un `BadRequest(string)` en español) — cualquier otro caso usa un mensaje genérico en español. Aplicado en `Compras.razor` y `TarjetaFormCard.razor`, que antes mostraban `response.Content.ReadAsStringAsync()` crudo.
- [x] Auditoría completa de los ~20 sitios que muestran toasts/diálogos en el Client — el resto ya eran seguros (literales en español o ya pasaban por servicios corregidos).
- [x] Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md), sección "Idioma: todo mensaje visible al usuario debe estar en español".
- [x] Probado en navegador: login con contraseña incorrecta ahora muestra "Correo o contraseña incorrectos." en vez de "Unauthorized"; registro con contraseña débil sigue mostrando los mensajes de `DescriptorErroresIdentityEspanol` en español sin cambios (no se rompió el camino que sí funcionaba).

## Sprint 30 — Despliegue con Docker (contenedor único) ✅ (2026-09-10)
- [x] Investigado el patrón ya usado y probado por el usuario en `NoteReminder` (Dockerfile/docker-compose en la raíz, desplegado en Easypanel) antes de replicarlo — decisión explícita del usuario de usar ESE Dockerfile tal cual, adaptado a la estructura de carpetas de TarjetasCredito (`src/`), en vez de mantener dos contenedores separados (que hubiera sido más fiel a SPEC-001 original, pero el usuario prefirió la simplicidad de un solo contenedor como ya tiene en producción para NoteReminder).
- [x] `TarjetasCredito.Server` ahora también hospeda el Client compilado (paquete `Microsoft.AspNetCore.Components.WebAssembly.Server` + `ProjectReference` al Client + `MapStaticAssets()`/`MapFallbackToFile("index.html")` en `Program.cs`) — el flujo de desarrollo de dos procesos separados no cambia, solo el build publicado.
- [x] Migraciones de EF Core automáticas al arrancar (`Database.Migrate()`) — el contenedor crea/actualiza la base SQLite solo.
- [x] Persistencia de Data Protection (`DataProtection__KeysPath`, opcional) para que un reinicio del contenedor no invalide las sesiones activas.
- [x] `Dockerfile`, `docker-compose.yml` (volumen `/app/data`, variables de entorno para connection string, `WebAuthn:ServerDomain` y claves VAPID), `.dockerignore` en la raíz del repo — mismo patrón que `NoteReminder`.
- [x] `.gitignore` actualizado (`publish/`, `.claude/`).
- [x] **Bug real encontrado y corregido, verificado con el build publicado real (no por analogía)**: `index.html` usaba el placeholder `_framework/blazor.webassembly#[.{fingerprint}].js` para que `MapStaticAssets()` lo resolviera en tiempo de ejecución — el navegador corta todo desde el `#` (lo trata como fragmento de URL) antes de pedir el archivo, así que nunca cargaba (404 en `_framework/blazor.webassembly`, la app se quedaba en "Cargando" para siempre). Corregido usando la ruta simple sin fingerprint (`_framework/blazor.webassembly.js`), que el manifiesto de assets confirma que sí existe y resuelve bien.
- [x] `Client/Program.cs`: `ServerBaseAddress` ahora usa `builder.HostEnvironment.BaseAddress` como fallback (mismo origen, correcto para el contenedor único) en vez de un valor fijo; el valor de desarrollo (`https://localhost:7287/`) se movió a `wwwroot/appsettings.Development.json`, que solo se carga en ese entorno.
- [x] Documentado en [SPEC-001](docs/specs/SPEC_TarjetasCredito-001-Arquitectura.md), sección "Despliegue con Docker".
- [x] **Probado de extremo a extremo sin Docker instalado en este entorno** (limitación honesta: no se pudo correr `docker build`/`docker compose up` aquí): se corrió `dotnet publish TarjetasCredito.Server -c Release` real y se ejecutó el artefacto resultante directamente (`dotnet TarjetasCredito.Server.dll`) — confirmado en navegador que la PWA carga completa desde el mismo origen que la API (sin CORS), que las migraciones se aplican solas contra una base nueva, que el registro de cuenta crea una fila real en SQLite, y que la navegación a una ruta profunda (`/register`) funciona vía `MapFallbackToFile`. **Pendiente que el usuario confirme el `docker build`/`docker compose up` real en su máquina o VPS/Easypanel.**
- [ ] **Gap real encontrado durante la prueba, bloquea el login en la versión publicada**: `RequireConfirmedEmail = true` en ambiente Production (`Program.cs`) + no hay `IEmailSender` configurado → nadie puede confirmar su correo → nadie puede iniciar sesión en el contenedor desplegado, aunque el registro sí funciona. Mismo gap ya anotado abajo para "Recuperación de contraseña". No se cambió unilateralmente — pendiente de que el usuario decida cómo resolverlo (desactivar la confirmación de correo mientras no haya proveedor de email, o configurar un `IEmailSender` real) antes de poder probar login/PWA en el despliegue real.

## Sprint 31 — Ajustes de UX reportados probando en celular real ✅ (2026-09-10)
- [x] **Face ID ya no deja usable el resto de la pantalla mientras "Verificando..."**: nuevo `Components/OverlayBloqueante.razor` (ver [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) "Patrón reutilizable: overlay de carga bloqueante") — cubre toda la pantalla con spinner + mensaje y captura todos los clicks/touches mientras dura la ceremonia. Usado en `Login.razor`; los campos de correo/contraseña y el botón "Entrar" también quedan `disabled` como defensa adicional.
- [x] **Spinner de arranque de la app: porcentaje/texto ahora sí centrado dentro del círculo** — el template default de .NET posicionaba el texto con un cálculo de número mágico que no coincidía con el centro real; corregido para que comparta la misma caja que el círculo y se centre con flexbox.
- [x] **Timeout propio de 15s en la ceremonia de Face ID** (`wwwroot/js/faceIdAuth.js`) — antes, sin ninguna credencial registrada en el dispositivo, el mensaje de error podía tardar mucho más de lo razonable en aparecer porque dependía del tiempo de espera interno del navegador/SO, fuera de nuestro control.
- [x] **Scroll vertical que aparecía en el login tras escribir y enviar el formulario en móvil** — corregido con una clase `body.auth-route` que `AuthShell.razor` agrega/quita del `<body>` mientras el login/register está montado, bloqueando el scroll del documento completo (no solo de `.login-shell`) sin afectar el resto de la app.
- [x] Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md) y [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).
- [~] Probado en navegador de escritorio (spinner de arranque, overlay bloqueante disparándose y bloqueando inputs, sin regresión en login/registro normal) y en viewport móvil emulado (sin scroll visible). **Pendiente que el usuario confirme en su celular real** — el bug de scroll específicamente depende del comportamiento del teclado táctil real de iOS/Android, que no se puede reproducir con el navegador de automatización de esta sesión (misma limitación ya documentada varias veces en sprints anteriores).

## Sprint 32 — Envío real de correo (SMTP) ✅ (2026-09-10)
- [x] `Infrastructure/Email/SmtpEmailSender.cs`: implementa `IEmailSender<ApplicationUser>` (interfaz nativa de Identity desde .NET 8, verificada por reflexión antes de implementar) con **MailKit** 4.17.0 contra el buzón real del usuario (`admin@codemore.com.mx`, Hostinger) — registrado en `Program.cs` antes de `AddIdentityApiEndpoints` para que Identity lo use en vez de su envío no-op.
- [x] `TarjetasCredito.Infrastructure.csproj`: agregado `<FrameworkReference Include="Microsoft.AspNetCore.App" />` — `IEmailSender<TUser>` vive en `Microsoft.AspNetCore.Identity.dll` (framework compartido de ASP.NET Core, no un paquete NuGet instalable), esta librería (`Sdk="Microsoft.NET.Sdk"` simple) no lo veía sin esta referencia explícita. De paso se detectó y quitó un `PackageReference` a `Microsoft.Extensions.Hosting.Abstractions` que quedó redundante (ya lo trae el framework compartido).
- [x] Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md), sección "Envío de correo (SMTP)".
- [x] **Probado de extremo a extremo con un envío real**: el usuario dio la contraseña real del buzón, guardada en `user-secrets` (`Smtp:Host/Port/Username/Password/FromAddress/FromName`, puerto 465 SSL vía `SecureSocketOptions.Auto` de MailKit). Se disparó `POST /api/auth/forgotPassword` contra una cuenta real (`admin@codemore.com.mx`, ya existente en la base de desarrollo) — `200 OK`, sin ninguna excepción en el log del servidor. Pendiente que el usuario confirme haber recibido el correo en su bandeja real (no se puede verificar la bandeja desde esta sesión).
- [ ] **Fuera de alcance de este sprint, sigue pendiente**: el Client todavía no tiene pantalla de "¿Olvidaste tu contraseña?" (ni para pedir el correo ni para capturar el código/nueva contraseña) — con esto solo se conectó el envío, los endpoints de Identity ya funcionan mejor pero la UI para usarlos no existe.

## Sprint 33 — Ajustes reportados probando registro/login reales en producción ✅ (2026-09-10)
- [x] **Bloqueo de zoom táctil**: `maximum-scale=1.0, user-scalable=no` en el meta viewport + `touch-action: pan-x pan-y` en `html,body` — bug real reportado por el usuario: al pellizcar para hacer zoom, o incluso solo al enfocar/cerrar el teclado táctil, la app perdía su encuadre fijo y quedaba con scroll libre en las cuatro direcciones. Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) "Bloqueo de zoom táctil" (incluye el trade-off de accesibilidad asumido a propósito).
- [x] **Registro sin confirmar: el formulario ya no se queda "en nada"** — `Register.razor` intentaba iniciar sesión automáticamente tras registrar; en producción eso siempre falla (correo sin confirmar todavía) y antes no avisaba nada. Ahora muestra un toast claro y navega a `/login`. Documentado en [SPEC-004](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md).
- [x] Placeholders de "Nombre(s)"/"Apellidos" en Configuración ya no muestran un nombre de ejemplo real (`Mario`/`Díaz`) — genéricos ahora.
- [x] Aclarado con el usuario: si nunca confirma su correo, en producción no puede iniciar sesión (mensaje genérico, misma decisión de seguridad del Sprint 29 de no revelar la causa exacta); en desarrollo no aplica.
- [x] **Decidido con el usuario**: por ahora, borrar cuentas de prueba se sigue pidiendo directamente en el chat (sin código nuevo) — mismo patrón usado toda la sesión. Un panel de administrador real queda registrado como Sprint futuro (ver abajo), no se implementa todavía.
- [ ] Pendiente que el usuario confirme en su celular real (bloqueo de zoom táctil no se puede verificar con el navegador de automatización de esta sesión — misma limitación de siempre).

## Sprint 34 — Panel de administrador (superadmin) — futuro, sin empezar
**Decidido con el usuario (2026-09-10)**: un solo rol de administrador (`SuperAdmin` o similar), pensado para que exista **un único usuario** con ese rol en todo el sistema — no un sistema general de "administradores" ni autoservicio de asignar el rol. Sin diseñar todavía; antes de implementar, definir con el usuario:
- [ ] Cómo se asigna el rol la primera vez (¿seed manual en base de datos? ¿variable de entorno con el correo del superadmin? — nunca un endpoint público que permita auto-asignarse el rol).
- [ ] Qué puede hacer el panel: como mínimo, listar y eliminar usuarios (el caso de uso que originó el pedido — poder reusar un correo en pruebas). Confirmar si necesita algo más (ver actividad, resetear contraseñas de otros, etc.) o si se mantiene acotado a esto.
- [ ] Cómo se protege la ruta/endpoints (`[Authorize(Roles = "SuperAdmin")]` de ASP.NET Core Identity ya soporta esto de fábrica una vez que exista el rol).
- [ ] Si el panel vive en el mismo Client (Blazor) con una ruta oculta, o si conviene mantenerlo completamente aparte.

## Sprint 35 — Un solo ciclo de recordatorios a la vez ✅ (2026-09-11)
- [x] `CicloFacturacionHelper.CiclosARecordar` (devolvía vigente + siguiente) reemplazado por `CicloVigente` (un solo ciclo) — `PaymentReminderGenerationService.AsegurarRecordatoriosAsync` ya no genera el ciclo siguiente por adelantado. Decisión del usuario probando con una tarjeta real: ver 2 pendientes por tarjeta (uno del mes en curso, otro del mes siguiente sin monto de estado de cuenta) era confuso.
- [x] Documentado en [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md).
- [x] Compila limpio; único call site de la función anterior ya actualizado, sin referencias sueltas.
- [ ] **Pendiente**: los recordatorios del "ciclo siguiente" que ya se hayan generado antes de este cambio (ej. en la cuenta real de producción del usuario, tarjeta "Stori") van a seguir existiendo en la base hasta que el usuario los borre a mano o hasta que ese ciclo efectivamente llegue y se convierta en el vigente — el fix solo evita generar más hacia adelante, no limpia retroactivamente. Se le ofreció un script de limpieza si lo pide.

## Sprint 36 — "Mejora tu score" en el Dashboard ✅ (2026-09-11)
- [x] Nueva sección en `Home.razor`, debajo de "Próximos vencimientos": una tarjeta de consejo por cada tarjeta activa, con 5 fases posibles (pagado / antes de la ventana óptima / ventana óptima de pago / corte hecho dentro del plazo / vencido) — reusa campos que `PaymentReminderDto` ya traía, sin endpoints ni datos nuevos.
- [x] Documentado en [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md), sección "Guía de fase del ciclo".
- [x] **Bug real encontrado probando en navegador (no hipotético)**: la primera versión elegía, por tarjeta, el recordatorio con `CicloFin` más reciente — pero una tarjeta puede tener a la vez un recordatorio viejo sin pagar (corte ya pasado, aún dentro del plazo) y uno nuevo recién generado para el ciclo que apenas empezó. Elegir por `CicloFin` mostraba el nuevo y ocultaba que había un pago pendiente real. Corregido: se elige el **no pagado con `FechaLimitePago` más próxima**; si no hay ninguno pendiente, el más reciente ya pagado.
- [x] Probado en navegador las 5 fases una por una, forzando fechas reales en la base de datos local (mismo patrón de prueba de sprints anteriores) — las 5 mostraron el título/color/mensaje correcto, incluida la que expuso el bug de selección de arriba.
- [x] Verificado sin regresión: "Próximos vencimientos" y "¿Qué tarjeta uso hoy?" siguen funcionando igual; la nueva sección no aparece si no hay tarjetas (ya cubierto por `EstadoVacio`).
- [x] Documentado en [AGENTS.md](AGENTS.md) el bloqueo de zoom táctil como comportamiento a replicar en futuros proyectos PWA del usuario (pedido explícito), además de quedar guardado en memoria de la sesión.

## Sprint 37 — Pulido reportado probando con datos reales (toasts, fechas, compras, historial) ✅ (2026-09-10)
- [x] **Toast con acabado premium**: `.app-swal-toast` reescrito en `app.css` (border-radius/borde/sombra con tokens de `.card`, ancho `auto` acotado, título alineado a la izquierda, ícono más chico) — el toast sin estilizar se veía desalineado y genérico. Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md).
- [x] **Formato de fecha en español consistente**: `Services/FechaFormato.Corta` (Client, tabla fija de meses — evita depender de `CultureInfo`/ICU en Blazor WASM) aplicado en `Compras.razor`, `Recordatorios.razor` y `Home.razor` en vez de `dd/MM/yyyy`. Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) "Formato de fecha".
- [x] **"Próximos vencimientos" ya no muestra vencidos sin pagar**: el Dashboard y la sección "Pendientes" de `/pagos` ahora filtran `!Pagado && DiasHastaLimite >= 0` — antes de este cambio un recordatorio vencido seguía apareciendo ahí, compitiendo visualmente con los que sí hay tiempo de atender.
- [x] **Nueva sección Historial en `/pagos`**: lista todo lo que ya no es "próximo" (pagado, o vencido sin pagar), filtrable por mes/año con dos `<select>`; un vencido sin pagar conserva ahí el botón "Marcar como pagado". Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) "Próximos vencimientos vs. Historial".
- [x] **Monto de compra sin "0" fantasma**: el input de Monto en `Compras.razor` ahora nace vacío (placeholder `0.00`, `decimal?` en vez de `decimal`) — antes mostraba un `0` real que había que borrar a mano cada vez.
- [x] **Categoría de compra: texto libre con sugerencias, ya no un enum cerrado**: `Purchase.Categoria` pasó de `CategoriaCompra` (enum) a `string`; la UI usa un `<input list>`+`<datalist>` nativo que sugiere categorías por defecto más las que el propio usuario ya haya escrito antes (`GET /api/purchases/categorias`) — ya no obliga a elegir "Otro" cuando la categoría deseada no está en una lista fija. Migración de datos escrita a mano (no la auto-generada por EF Core) para traducir cada valor entero viejo a su nombre real en vez de perderlo; verificada insertando los 7 valores posibles en la base de desarrollo antes de aplicar. Documentado en [SPEC-002](docs/specs/SPEC_TarjetasCredito-002-BaseDeDatos.md) "Categoría de compra: texto libre con sugerencias".
- [x] **Editar y eliminar una compra ya registrada**: antes no existía ninguna forma de corregir un monto/tarjeta/categoría equivocados ni de borrar una compra — solo se podían crear. Nuevo `PUT /api/purchases/{id}` y `DELETE /api/purchases/{id}`, con confirmación (`Dialogs.ConfirmarEliminarAsync`) antes de borrar, mismo patrón ya usado para tarjetas.
- [x] **Gotcha real encontrado al construir el formulario de edición de Compras**: renderizar un `decimal?` directo en el `value` de un `<input type="number">` puede producir coma decimal (`"250,50"`) según la cultura del runtime WASM, que el input descarta en silencio (spec HTML5 exige punto). Corregido forzando `CultureInfo.InvariantCulture` en `Compras.razor` y `Recordatorios.razor`. Documentado en [SPEC-005](docs/specs/SPEC_TarjetasCredito-005-UI.md) "Formato de fecha" (mismo gotcha, para números).
- [x] Probado de punta a punta en navegador con una cuenta de prueba desechable (creada y eliminada al terminar): alta de tarjeta, alta/edición/eliminación de compra, toast premium, marcar como pagado un recordatorio vencido desde Historial, y la generación automática de un nuevo recordatorio para el ciclo vigente al forzar fechas viejas en un recordatorio existente (confirma que el diseño de "recordatorio relevante" del Sprint 36 sigue funcionando con datos reales).
- [ ] **Pendiente — items 5 y 7 de la misma solicitud, no implementados todavía**: la utilización de una tarjeta (`CreditCardsController.CalcularUtilizacionAsync`) hoy es puramente `suma de compras del ciclo vigente / límite` — **no resta ningún pago registrado**, así que registrar un abono (parcial o total, antes o después del corte) no mueve el indicador de utilización, y no existe ningún lugar para ver/registrar más de un pago por ciclo. Esto requiere un concepto de dominio nuevo (un "abono"/pago distinto de `Purchase` y de "marcar el ciclo como pagado" en `PaymentReminder`) que además cambia un cálculo financiero central de la app — se identificó la causa raíz pero se dejó pendiente de una sesión de diseño dedicada antes de implementar, en vez de improvisar el modelo de datos.

## Sprint 38 — Abonos a tarjeta y utilización que sí refleja los pagos ✅ (2026-09-11)
- [x] **Nueva entidad `CardPayment` ("abono")**: dinero puesto hacia una tarjeta, independiente de `Purchase` (que suma) y de "marcar un recordatorio como pagado" (que antes solo era una bandera informativa sin ningún efecto en la utilización). CRUD completo (`CardPaymentsController`, `CardPaymentApiService`) y nueva sección "Registrar abono"/"Abonos registrados" en `/pagos`, con editar/eliminar (mismo patrón ya probado en Compras).
- [x] **Utilización neta de abonos**: `CreditCardsController.CalcularUtilizacionAsync` ahora resta del saldo del ciclo vigente la suma de abonos con esa misma fecha de corte como referencia (`saldoNeto = max(0, saldoCompras - totalAbonado)`) — un abono baja la utilización de inmediato, y se pueden registrar tantos como se quiera dentro del mismo ciclo (responde al item 7 de la solicitud original: pagos parciales sucesivos).
- [x] **"Marcar como pagado" ahora también mueve la utilización**: `PaymentRemindersController.MarcarPagado` crea automáticamente un `CardPayment` cuando hay `MontoEstadoCuenta` capturado — corrige exactamente el bug que el usuario reportó (item 5: "registré un pago... pero no veo que se mueva"), sin que tenga que aprender la pantalla nueva de Abonos si no quiere.
- [x] Documentado en [SPEC-002](docs/specs/SPEC_TarjetasCredito-002-BaseDeDatos.md) (entidad `CardPayment`) y [SPEC-003](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md) "Abonos y utilización neta" (fórmula, regla del abono automático, y la simplificación conocida y aceptada de no modelar un saldo pendiente que rueda entre ciclos).
- [x] Migración EF (`CrearCardPayments`) — tabla nueva, sin conversión de datos existentes, aplicada limpio a la base de desarrollo.
- [x] Probado de punta a punta en navegador con una cuenta de prueba desechable (creada y eliminada al terminar, mismo patrón del Sprint 37): tarjeta con $4,000 en compras sobre límite de $10,000 (40% utilización) → abono manual de $1,000 (baja a 30%) → segundo abono manual de $500 (baja a 25%, confirma múltiples abonos) → "Marcar como pagado" con $1,000 capturados crea el abono automático "Pago de ciclo" (baja a 15%, confirma el fix del bug original) → editar un abono de $1,000 a $2,000 (baja a 5%, confirma que editar recalcula) → abonar de más (baja a 0%, nunca negativo).
- [ ] **Fuera de alcance, documentado como simplificación conocida**: un abono fechado hoy para saldar una deuda de un ciclo ya cerrado hace 2+ meses se resta del ciclo vigente actual (no existe ningún "saldo pendiente que rueda" del cual restarlo) — modelar eso completo queda como posible sprint futuro si el usuario lo pide tras usar esta versión.

## Backlog futuro (sin sprint asignado)
- [ ] Integración real con proveedor de buró de crédito (requiere credenciales del usuario).
- [ ] Explicaciones de recomendación generadas por Claude API sobre el motor de reglas (híbrido).
- [ ] Sincronización offline de compras registradas sin conexión.
- [ ] **Recuperación de contraseña** ("¿Olvidaste tu contraseña?"): el backend de Identity (`MapIdentityApi`) ya trae de fábrica los endpoints `forgotPassword`/`resetPassword`, pero hoy no hacen nada útil (no hay `IEmailSender` configurado, ningún correo llegaría a nadie) y no existe ninguna pantalla en el Client para esto — falta por completo. Pendiente definir con el usuario cómo se envía el correo (proveedor real con credenciales vía `dotnet user-secrets`, vs. modo desarrollo mostrando el enlace en pantalla/log mientras no haya proveedor, mismo criterio que el Buró mock) antes de implementar. Explícitamente pospuesto por el usuario el 2026-09-09. **Relacionado con el gap del Sprint 30**: mientras no haya `IEmailSender`, tampoco se puede confirmar correo para poder iniciar sesión en producción — no es solo "olvidé mi contraseña", bloquea el login nuevo por completo.

## Propuestas de módulos futuros (2026-09-08, a priorizar con el usuario)

Candidatos a convertirse en Sprint 14+, agrupados por tema. Ninguno implementado aún.

**Recordatorios y calendario de pagos**
- [x] `PaymentReminder` completado en Sprint 23: generación automática por ciclo, pantalla de "próximos vencimientos" (fecha óptima vs. fecha límite por tarjeta), marcar como pagado.
- [x] Notificaciones Web Push sobre esos vencimientos — Sprint 8.

**Analítica y control de gasto**
- [ ] Presupuesto mensual por categoría de compra (Supermercado, Restaurantes, etc.) con barra de progreso y alerta al acercarse al límite.
- [ ] Reportes de gasto: gráficas por categoría/mes/tarjeta, tendencia de utilización, exportar a CSV.
- [ ] "Score financiero interno" propio de la app (distinto del buró mock): combina utilización, puntualidad de pagos registrados y diversidad de tarjetas en un solo indicador visible en el Dashboard.

**Compras y MSI**
- [ ] Simulador de meses sin intereses: dado un monto y número de MSI, calendario de cuotas pendientes y su impacto mensual en el flujo de caja.
- [ ] Historial de compras con filtros (por tarjeta, categoría, rango de fechas) — el listado ya existe pero sin filtros en la UI.

**Cuenta y datos**
- [ ] Exportar/respaldar tarjetas y compras a CSV o Excel.
- [ ] Onboarding guiado la primera vez que un usuario entra (sin tarjetas registradas).

**IA / asistente**
- [ ] Chat conversacional con el "agente economista" (Claude API) sobre tus propios datos: preguntas en lenguaje natural como "¿cuánto debo pagar esta quincena?" — usa el motor de reglas como fuente de verdad, el LLM solo redacta.

Ya en el roadmap: FaceID/WebAuthn (Sprint 7 ✅) y notificaciones Web Push (Sprint 8 ✅) completados; pulido de PWA instalable (Sprint 9, parcial) y endurecimiento/pruebas antes de despliegue (Sprint 10) siguen pendientes.
