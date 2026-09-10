# SPEC_TarjetasCredito-002-BaseDeDatos

## Estado: Vigente (2026-09-08)

## Motor

EF Core 10 + SQLite (`tarjetascredito.db`) en desarrollo. Migraciones versionadas en `TarjetasCredito.Infrastructure/Migrations`.

## Regla de aislamiento multi-usuario (crítica)

Toda tabla de datos de negocio (`CreditCards`, `Purchases`, `PaymentReminders`, `BuroScoreSnapshots`) tiene una columna `UserId` (FK a `AspNetUsers.Id`) **NOT NULL**. Ningún repositorio ni endpoint puede devolver o modificar una fila cuyo `UserId` no coincida con el usuario autenticado de la request actual. Esto se implementa como filtro obligatorio en la capa de repositorio (no confiar solo en la UI). Ver [SPEC_TarjetasCredito-004-Seguridad.md](SPEC_TarjetasCredito-004-Seguridad.md).

## Entidades principales

### ApplicationUser (extiende IdentityUser)
- `Id` (string, GUID)
- `Email` (login por correo)
- `Nombres` (string, nullable) — nombre(s) de pila, editable desde Configuración.
- `Apellidos` (string, nullable) — apellido(s), editable desde Configuración.
- `ThemePreference` (enum: Light, Dark, System) — preferencia de tema guardada por usuario.

El nombre para mostrar en la UI (saludo del dashboard, sidebar) se calcula como `Nombres` si existe, si no `Email` como respaldo — nunca se deja vacío. `Nombres`/`Apellidos` reemplazaron al campo `DisplayName` de la versión inicial (sin uso real, se descartó a favor de campos separados).

### CreditCard
- `Id` (Guid)
- `UserId` (FK)
- `Nombre` (alias que el usuario le da, ej. "Platino BBVA")
- `Banco`
- `UltimosCuatroDigitos` (string, 4 chars) — nunca el número completo
- `Marca` (enum: Visa, Mastercard, AmericanExpress, Otra)
- `LimiteCredito` (decimal)
- `DiaCorte` (int, 1-31) — día del mes de corte de estado de cuenta
- `DiasParaPago` (int) — días entre corte y fecha límite de pago (ej. 20)
- `TasaInteresAnual` (decimal, nullable) — para cálculos de costo si no se paga de contado
- `Activa` (bool)
- `FechaAlta` (DateTime)
- `ColorHex` (string, ej. `#3B5BFE`) — color del "card visual" en la UI. Si el usuario no elige uno al crear la tarjeta, se autoasigna tomando el siguiente color de una paleta fija rotando por cantidad de tarjetas ya registradas (ver SPEC-005). El logo de marca (Visa/Mastercard/Amex/Otra) se renderiza en el cliente a partir de `Marca` — no se almacena como imagen, son glifos SVG propios (ver SPEC-005), para evitar reproducir logos oficiales de las marcas.

Fecha límite de pago de un ciclo = fecha de corte del ciclo + `DiasParaPago`.

### Purchase
- `Id` (Guid)
- `UserId` (FK, redundante con CreditCard.UserId pero explícito para queries e integridad)
- `CreditCardId` (FK a CreditCard)
- `Monto` (decimal)
- `Descripcion`
- `Categoria` (enum libre/abierta: Supermercado, Restaurantes, Transporte, Servicios, Entretenimiento, Salud, Otro)
- `Fecha` (DateTime)
- `Msi` (int, nullable) — meses sin intereses si aplica

### PaymentReminder (recordatorios/estado de pago por ciclo)
- `Id` (Guid)
- `UserId` (FK)
- `CreditCardId` (FK)
- `CicloInicio` / `CicloFin` (DateTime) — rango del ciclo de facturación
- `FechaLimitePago` (DateTime)
- `MontoEstadoCuenta` (decimal, nullable — se captura cuando se conoce)
- `Pagado` (bool)
- `FechaPago` (DateTime, nullable)
- `NotificacionOptimaEnviadaUtc` / `NotificacionT3EnviadaUtc` / `NotificacionT1EnviadaUtc` (DateTime, nullable — Sprint 8) — marca de qué avisos push ya se mandaron para este recordatorio, ver [SPEC-004](SPEC_TarjetasCredito-004-Seguridad.md) "Notificaciones push".

### BuroScoreSnapshot
- `Id` (Guid)
- `UserId` (FK)
- `Fecha` (DateTime)
- `Score` (int)
- `Proveedor` (string — "Mock" hasta que se conecte un proveedor real)
- `FactoresJson` (string — detalle serializado de factores que afectan el score, ver SPEC-003)

### PushSubscription (clase `PushSubscriptionRecord` en código — "Record" para no chocar con `System.Net` `PushSubscription`)
- `Id` (Guid)
- `UserId` (FK)
- `Endpoint` (string, **único** — Sprint 8: re-suscribirse desde el mismo navegador actualiza la fila existente en vez de duplicarla), `P256dh`, `Auth` (campos estándar Web Push)
- `FechaAlta` (DateTime)

## Convenciones

- Nombres de tabla en inglés técnico (`CreditCards`, `Purchases`) para alinear con convenciones EF Core estándar; nombres de propiedades de dominio en español porque así los definió el usuario en sus reglas de negocio.
- Todas las fechas se guardan en UTC; la conversión a hora local del usuario ocurre en el cliente.
- Borrado de tarjetas es lógico (`Activa = false`), no físico, para no perder el historial de compras asociado.
- **Trampa real encontrada (Sprint 8) al insertar datos de prueba a mano con SQL crudo en una columna `Guid`**: EF Core + `Microsoft.Data.Sqlite` comparan/enlazan parámetros `Guid` usando el formato **mayúsculas** (`Guid.ToString("D").ToUpperInvariant()`), aunque la columna sea `TEXT` sin afinidad especial. Un `INSERT` manual que escriba el `Id` en minúsculas (ej. `Guid.NewGuid().ToString()` tal cual) queda "invisible" para cualquier lookup posterior hecho por EF Core (`WHERE Id = @id` no encuentra la fila) — sin error, simplemente cero resultados. Si se necesita insertar/editar una fila con clave `Guid` desde un script fuera de EF (como los scripts desechables de limpieza de este proyecto), el valor debe escribirse en mayúsculas (`.ToUpperInvariant()`) para que EF pueda encontrarla después.
