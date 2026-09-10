# SPEC_TarjetasCredito-001-Arquitectura

## Estado: Vigente (2026-09-08)

## Decisión

- **.NET 10** en toda la solución.
- **Cliente**: Blazor WebAssembly standalone, configurado como **PWA** instalable (manifest.webmanifest + service worker), consumido desde navegador de escritorio y móvil (instalable en pantalla de inicio, iOS/Android/desktop).
- **Servidor**: ASP.NET Core Web API (.NET 10), sin IdentityServer/Duende. Expone endpoints REST/minimal API para auth, tarjetas, compras, recomendaciones, buró y push.
- **Comunicación**: HttpClient desde el cliente WASM hacia el API, con bearer token emitido por ASP.NET Core Identity (`MapIdentityApi`). El token es opaco (cifrado con Data Protection), no un JWT — no se puede ni se debe decodificar en el cliente (ver SPEC-004).
- **Persistencia**: EF Core + SQLite en desarrollo (archivo local). Se documentará migración a SQL Server/PostgreSQL si el usuario despliega a un hosting compartido con múltiples instancias.

## Estructura de solución

```
src/
  TarjetasCredito.sln
  TarjetasCredito.Client/         Blazor WASM PWA (UI)
  TarjetasCredito.Server/         ASP.NET Core Web API (host, Identity, endpoints)
  TarjetasCredito.Shared/         DTOs y contratos compartidos cliente/servidor
  TarjetasCredito.Domain/         Entidades, enums, motor de reglas de negocio (sin dependencias externas)
  TarjetasCredito.Infrastructure/ EF Core DbContext, repositorios, servicios (buró mock, push, etc.)
```

Referencias: `Server` → `Domain`, `Infrastructure`, `Shared`. `Infrastructure` → `Domain`. `Client` → `Shared`. `Domain` no referencia nada (capa pura, testeable).

## Por qué no Blazor Server ni Duende IdentityServer

- Blazor Server requiere conexión SignalR persistente — mal fit para "instalar en el celular y usar como app", que se espera funcione con conectividad intermitente y se sienta nativa.
- El template `--hosted -au Individual` de .NET usa IdentityServer (ahora Duende), que exige licencia comercial fuera de escenarios de desarrollo/evaluación. Se evita por completo usando `MapIdentityApi<ApplicationUser>()`, disponible desde .NET 8, pensado exactamente para SPA/WASM con bearer tokens.

## PWA

- `manifest.webmanifest` con iconos 192/512, `display: standalone`, `theme_color` y `background_color` alineados a la paleta definida en [SPEC_TarjetasCredito-005-UI.md](SPEC_TarjetasCredito-005-UI.md).
- Service worker cachea los assets estáticos de la app (app shell) para arranque offline; las llamadas a la API no se cachean agresivamente porque son datos financieros que deben reflejar el estado real.
- Requiere HTTPS en todos los entornos (incluido desarrollo local) porque WebAuthn/FaceID y Web Push lo exigen.

## Herramientas de desarrollo

- **Swagger UI** disponible solo en `Development`, en `/swagger` del Server (`https://localhost:7287/swagger`). Usa el documento OpenAPI generado por `Microsoft.AspNetCore.OpenApi` (`/openapi/v1.json`) — no genera un documento propio, solo aporta la interfaz visual (`Swashbuckle.AspNetCore.SwaggerUI`). Incluye botón "Authorize" para pegar el `accessToken` (bearer opaco, ver SPEC-004) y probar endpoints protegidos. Accede siempre por HTTPS (`https://localhost:7287/swagger`, no por `http://localhost:5254`) — por HTTP redirige a HTTPS y el fetch del documento falla entre orígenes.

## Manejo de errores de red y estados sin conexión

**Decisión (2026-09-09)**: no se cachean ni se muestran datos financieros desactualizados sin conexión (ver sección PWA arriba) — el objetivo de este patrón es que la app **falle con gracia** (mensaje claro en español + reintentar) en vez de quedarse con un spinner trabado para siempre o mostrar el banner default de Blazor en inglés. Se encontró que solo el arranque de sesión (`TokenAuthenticationStateProvider.IntentarRestaurarSesionAsync`, `UserProfileService.CargarAsync`) tenía manejo de conexión (Sprint 15); el resto de la app (Buró, Tarjetas, Compras, Pagos, Dashboard, y todas las acciones de botón) no.

**Patrón para carga de datos al entrar a una página** (`CreditCardApiService.ObtenerTodasAsync`, `PurchaseApiService.ObtenerTodasAsync`, `PaymentReminderApiService.ObtenerTodosAsync`, `BuroApiService.ObtenerScoreAsync`):
- `Services/ResultadoApi.cs` — `record ResultadoApi<T>(T? Datos, bool SinConexion, bool ErrorServidor)` con `Exitoso => !SinConexion && !ErrorServidor`.
- `Services/ApiCallHelper.cs` — `EjecutarAsync<T>(Func<Task<T>> llamada)` envuelve la llamada HTTP y clasifica la falla: `HttpRequestException` con `StatusCode is null` (no llegó respuesta — sin red/DNS/conexión rechazada) es `SinConexion`; con `StatusCode` presente (sí hubo respuesta, pero de error, ej. 500) es `ErrorServidor`; `TaskCanceledException` (timeout) cuenta como `SinConexion`.
- Los 4 métodos de lectura de arriba devuelven `Task<ResultadoApi<T>>` en vez del dato crudo.
- `Components/EstadoSinConexion.razor` — tarjeta reusable (ícono + mensaje según `SinConexion`/no + botón "Reintentar" vía `EventCallback OnReintentar`), usada por cada página cuando `!resultado.Exitoso` en vez de spinner infinito.
- Cuando una página carga un dato secundario además del principal (ej. `Home.razor` carga tarjetas + recordatorios; `Compras.razor` carga tarjetas + historial), solo el dato **principal** bloquea la página con `EstadoSinConexion` — el secundario simplemente no se muestra si falla, sin tumbar el resto.

**Patrón para acciones de botón** (crear/editar/eliminar, no necesitan estado persistente — un toast basta):
- Si el método del servicio ya devuelve algo que el caller interpreta como éxito/fracaso (`bool`, `ResultadoAuth`), el `try/catch` se agrega **dentro del servicio** devolviendo el mismo tipo de "fracaso" que ya maneja el caller — cero cambios en la página (ej. `AuthService.RegistrarAsync`/`IniciarSesionAsync`, `UserProfileService.ActualizarAsync`).
- Si el servicio devuelve `HttpResponseMessage` crudo, el `try/catch` se agrega **en el sitio de la llamada** (la página/componente), mostrando `DialogService.ToastErrorAsync("No se pudo conectar. Revisa tu conexión a internet.")` y moviendo la bandera de "guardando" a un `finally` para que el botón nunca quede pegado (ej. `TarjetaFormCard.GuardarAsync`, `Tarjetas.DesactivarAsync`, `Compras.GuardarAsync`, `Recordatorios.MarcarPagadoAsync`, `Home.RecomendarAsync`).

**Red de seguridad adicional**:
- `MainLayout.razor` envuelve `@Body` en `<ErrorBoundary>` — contiene cualquier excepción no prevista en el área de contenido (sin tumbar el nav/sidebar), usando el `ErrorContent` default de Blazor, que ya coincide con la clase `.blazor-error-boundary` que `app.css` traía preparada (`::after { content: "Ocurrió un error."; }`) pero que no se usaba en ningún lado hasta este sprint.
- El banner default `#blazor-error-ui` (`index.html`) está traducido al español ("Ocurrió un error inesperado." / "Recargar") como última red de seguridad para cualquier error no cubierto por lo anterior.

Cualquier módulo nuevo que cargue datos al entrar a una página debe usar `ResultadoApi<T>`/`ApiCallHelper`/`EstadoSinConexion`; cualquier acción de botón nueva debe envolver su llamada en `try/catch` con el mismo mensaje de toast, siguiendo este patrón en vez de dejar la excepción sin manejar.

## Endpoints de passkeys (WebAuthn): excepción puntual a Controllers

**Decisión (Sprint 7, 2026-09-09)**: todos los endpoints propios del Server (tarjetas, compras, recordatorios, buró, perfil) son Controllers (`[ApiController]`) — la única excepción son los endpoints de passkey (`POST /api/auth/passkey/registro/opciones`, `/registro`, `/acceso/opciones`, `/acceso`, `GET`/`DELETE /api/auth/passkey`), escritos como **minimal API** en `src/TarjetasCredito.Server/PasskeyEndpoints.cs` (método de extensión `MapPasskeyEndpoints`, llamado justo después de `MapIdentityApi` en `Program.cs`).

**Por qué**: el endpoint de login por passkey (`/acceso`) tiene que emitir el mismo `AccessTokenResponse` (bearer token) que ya emite `/api/auth/login` de `MapIdentityApi`, y el mecanismo real para eso es `signInManager.AuthenticationScheme = IdentityConstants.BearerScheme; await signInManager.SignInAsync(usuario, isPersistent: false);` seguido de `Results.Empty` — el handler de autenticación Bearer escribe el cuerpo de la respuesta (el JSON del token) directamente como efecto secundario de `SignInAsync`, antes de que el resultado del endpoint se ejecute. Es exactamente el mismo patrón interno que usa el propio `MapIdentityApi`. Envolver esto en una acción de Controller (`ControllerBase.Ok()`/etc.) arriesga un intento de escribir la respuesta dos veces; replicar el patrón minimal-API tal como lo usa el framework es lo seguro y probado. Cualquier endpoint de auth futuro que necesite emitir un bearer token de la misma forma (login social, magic link, etc.) debe seguir este mismo patrón minimal-API, no un Controller.

## Notificaciones Web Push: primer `BackgroundService` de la app (Sprint 8)

**Decisión (2026-09-09)**: `PaymentReminderPushHostedService` (`Infrastructure`, registrado con `AddHostedService<...>()` en `Program.cs`) es el primer trabajo proactivo/en segundo plano de toda la solución — hasta este sprint, todo (incluidos los recordatorios de pago, Sprint 23) usaba generación perezosa bajo demanda (ver SPEC-003). Patrón usado, para cualquier `BackgroundService` futuro que necesite acceso a datos:
- `PeriodicTimer` (no `Task.Delay` en loop) para el intervalo entre verificaciones (1 hora).
- Como los repositorios de EF Core son `Scoped` y un `BackgroundService` es `Singleton`, cada tick crea su propio scope con `IServiceScopeFactory.CreateScope()` y resuelve los repositorios/servicios ahí — nunca se inyectan repositorios `Scoped` directamente en el constructor del hosted service.
- Reusa `PaymentReminderGenerationService.AsegurarRecordatoriosAsync` (la misma lógica que ya usaba `PaymentRemindersController`) en vez de duplicar el cálculo de ciclos — ver SPEC-003.
- El envío en sí se delega a `IWebPushSenderService` (envuelve el paquete `WebPush`, ver SPEC-004); una suscripción caducada (`WebPushException.StatusCode` 404/410) se poda de la base ahí mismo, no se reintenta indefinidamente.

## Despliegue con Docker (2026-09-10)

**Decisión**: un solo contenedor, mismo patrón que ya usa el proyecto hermano `NoteReminder` (Dockerfile/docker-compose en su raíz, desplegado con Easypanel). El usuario pidió explícitamente replicar ese Dockerfile tal cual, adaptado a la estructura de carpetas de este proyecto (`src/`).

- **`TarjetasCredito.Server` pasa a hospedar también el Client compilado** — se le agregó el paquete `Microsoft.AspNetCore.Components.WebAssembly.Server` y una `ProjectReference` al Client. Al hacer `dotnet publish TarjetasCredito.Server`, el SDK de WebAssembly compila el Client y copia su `wwwroot` dentro del `wwwroot` del Server automáticamente — un solo artefacto publicado, un solo contenedor. `Program.cs` agrega `app.MapStaticAssets()` + `app.MapFallbackToFile("index.html")` al final del pipeline (después de `MapControllers()`, a propósito, para no capturar rutas de la API).
- **El flujo de desarrollo NO cambia**: `Client` y `Server` se siguen corriendo por separado con `dotnet run` (dos procesos, dos puertos, CORS entre ellos) exactamente como hasta ahora — la referencia nueva solo tiene efecto real al publicar. La cookie cross-origin de Face ID (SPEC-004) sigue haciendo falta en dev; en el contenedor Docker (mismo origen) simplemente no se activa, sin que haya que quitarla.
- **`ServerBaseAddress` del Client** (`Client/Program.cs`) ahora usa `builder.HostEnvironment.BaseAddress` como fallback en vez de un valor fijo — en dev sigue apuntando a `https://localhost:7287/` (movido a `wwwroot/appsettings.Development.json`, que solo se carga en ese entorno), y en el build publicado/hosted usa su propio origen automáticamente (correcto para el contenedor único, donde Client y Server son el mismo origen).
- **Bug real encontrado y corregido al verificar el publish** (no asumido por analogía con NoteReminder, se probó de verdad): `index.html` traía `<script src="_framework/blazor.webassembly#[.{fingerprint}].js">` — el placeholder `#[.{fingerprint}]` que en teoría resuelve `MapStaticAssets()` en tiempo de ejecución. El navegador interpreta el `#` como inicio de fragmento de URL y lo recorta antes de pedir el archivo, mandando la petición a `_framework/blazor.webassembly` (sin extensión) → 404, `blazor.webassembly.js` nunca carga, la app se queda en "Cargando" para siempre. Confirmado con el manifiesto de assets (`TarjetasCredito.Server.staticwebassets.endpoints.json`) que sí existe una ruta sin fingerprint (`_framework/blazor.webassembly.js`) que resuelve bien — se cambió `index.html` a esa referencia simple. Verificado corriendo el build publicado real (`dotnet TarjetasCredito.Server.dll` fuera de Docker) y confirmando en el navegador que la app carga, permite registrar cuenta (fila real creada en SQLite) y navega por rutas profundas (`/register` vía `MapFallbackToFile`) sin errores de CORS (mismo origen).
- **Migraciones automáticas al arrancar**: `Program.cs` corre `Database.Migrate()` justo después de `builder.Build()` — necesario para que el contenedor cree/actualice la base SQLite solo, sin tener que entrar a ejecutar `dotnet ef` a mano dentro del contenedor. Verificado: el build publicado, apuntado a una base vacía, crea las 4 migraciones existentes correctamente al primer arranque.
- **Persistencia entre recreaciones del contenedor** (volumen `/app/data`, ver `docker-compose.yml`):
  - `ConnectionStrings__Default` apunta la base SQLite dentro del volumen.
  - `DataProtection__KeysPath` (nuevo, opcional — solo tiene efecto si está configurado): persiste las llaves de Data Protection que cifran el bearer token opaco de Identity. Sin esto, cada recreación del contenedor invalidaría todas las sesiones activas (obligaría a reloguearse). En desarrollo no se configura, cero cambio de comportamiento.
- **Variables de entorno en producción** (ya anticipado en SPEC-004, ahora con nombres concretos): `WebAuthn__ServerDomain` (dominio real, sin protocolo/puerto), `WebPush__VapidPublicKey`/`WebPush__VapidPrivateKey`/`WebPush__Subject`. Todas se leen automáticamente vía el proveedor de configuración de variables de entorno de ASP.NET Core (doble guion bajo = separador de sección), sin código adicional.
- **Gap real detectado durante la prueba, pendiente de decisión del usuario**: `AddIdentityApiEndpoints` tiene `options.SignIn.RequireConfirmedEmail = builder.Environment.IsProduction()` — en el contenedor (ambiente Production por default) esto bloquea el login de cualquier cuenta nueva porque no hay `IEmailSender` configurado (mismo gap ya anotado en el backlog de `sprints.md` para "recuperación de contraseña"). Sin resolverlo, nadie puede iniciar sesión en la versión publicada. No se cambió unilateralmente — ver sprints.md.
- **No verificado en esta sesión**: el `docker build`/`docker compose up` real (no hay Docker instalado en este entorno) — sí se verificó exhaustivamente la parte que Docker solo empaqueta (`dotnet publish` produciendo un artefacto que arranca, migra la base solo, y sirve la PWA + API desde el mismo origen sin errores). Queda pendiente que el usuario confirme el build de imagen y el despliegue real en su VPS/Easypanel.

## Pendiente de definir en sprints futuros

- Estrategia de sincronización offline (si se permite registrar compras sin conexión y sincronizar después).
