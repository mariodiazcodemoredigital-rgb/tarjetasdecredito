# AGENTS.md — Correo de seguridad del proyecto TarjetasCredito

Este archivo es el punto de entrada obligatorio para cualquier agente (humano o IA) que trabaje en este repositorio. Léelo completo antes de tocar código.

## Qué es este proyecto

Aplicación PWA (Blazor WebAssembly + .NET 10) para que un usuario registre sus tarjetas de crédito personales, reciba recomendaciones sobre qué tarjeta usar en cada compra, optimice sus fechas de pago para mejorar su score de buró de crédito, y reciba notificaciones push. Es una app financiera personal — trátala con el mismo cuidado que un gestor de contraseñas.

## Flujo de trabajo estándar (repetible en cada sesión)

1. **Contexto**: lee este archivo, luego [sprints.md](sprints.md) para saber qué está hecho y qué sigue, luego todo `docs/specs/` para las reglas vigentes.
2. **Documentar antes de codificar**: si la solicitud del usuario introduce un patrón nuevo (UI, regla de negocio, arquitectura, base de datos, seguridad), actualiza o crea el SPEC correspondiente en `docs/specs/` ANTES de implementar. Usa la convención `SPEC_TarjetasCredito-NNN-Categoria.md`.
3. **Ejecutar**: implementa respetando estrictamente las specs. Si una solicitud choca con una spec existente, señala el conflicto al usuario antes de proceder.
4. **Actualizar sprints.md**: registra tareas nuevas y marca como completadas las que se terminaron en la sesión.

## Reglas de seguridad no negociables

- **Nunca** commitear secretos: connection strings con credenciales reales, API keys (Claude, VAPID, buró de crédito), certificados. Usa `dotnet user-secrets` en desarrollo y variables de entorno / Key Vault en producción.
- **Aislamiento por usuario**: todo query de tarjetas, compras o datos de buró DEBE filtrar por el `UserId` del usuario autenticado. Ningún usuario puede leer ni escribir datos de tarjetas que no haya dado de alta él mismo. Esto se valida en la capa de servicio del Server, nunca solo en el cliente.
- **Datos financieros sensibles**: números de tarjeta completos NUNCA se almacenan ni se muestran — solo los últimos 4 dígitos. No se piden CVV ni PIN en ningún formulario.
- **HTTPS obligatorio** en todos los entornos, incluido desarrollo (requisito de PWA instalable, WebAuthn/FaceID y Web Push).
- **Buró de crédito real**: cualquier integración con un proveedor real (Buró de Crédito México, Círculo de Crédito, Equifax, TransUnion, etc.) requiere que el usuario provea credenciales/contrato propio. Nunca inventar ni simular que una consulta es real si es mock.
- Ver detalle completo en [SPEC_TarjetasCredito-004-Seguridad.md](docs/specs/SPEC_TarjetasCredito-004-Seguridad.md).

## Decisiones de arquitectura ya tomadas (no revisitar sin confirmar con el usuario)

- **Hosting**: Blazor WebAssembly (standalone, PWA instalable) + ASP.NET Core Web API separado. Ver [SPEC_TarjetasCredito-001-Arquitectura.md](docs/specs/SPEC_TarjetasCredito-001-Arquitectura.md).
- **Auth**: ASP.NET Core Identity + `MapIdentityApi` (bearer tokens), registro por correo. Sin Duende IdentityServer (licencia comercial).
- **Buró de crédito**: interfaz `IBuroCreditoService` con implementación mock hasta que el usuario aporte credenciales reales de un proveedor.
- **Motor de recomendación de tarjeta**: reglas determinísticas (no LLM) en v1. Ver [SPEC_TarjetasCredito-003-ReglasNegocio.md](docs/specs/SPEC_TarjetasCredito-003-ReglasNegocio.md).

## Patrones de UI reutilizables (consultar antes de crear un efecto nuevo)

- **"Parallax stage"** (elementos decorativos que reaccionan al cursor, ej. las tarjetas flotantes del login): motor propio en `src/TarjetasCredito.Client/wwwroot/js/parallaxStage.js` + regla `.parallax-item` en `app.css`. Al usuario le gustó este efecto y quiere reutilizarlo en pantallas futuras — la guía completa de uso (y el bug ya corregido que hay que evitar repetir) está documentada en [SPEC_TarjetasCredito-005-UI.md](docs/specs/SPEC_TarjetasCredito-005-UI.md), sección "Patrón reutilizable: parallax stage". Léela antes de construir un efecto similar en vez de reinventarlo.

## Convenciones de nombres de specs

`docs/specs/SPEC_TarjetasCredito-NNN-Categoria.md` — numeración secuencial, categoría en una palabra (Arquitectura, BaseDeDatos, ReglasNegocio, Seguridad, UI, etc.).
