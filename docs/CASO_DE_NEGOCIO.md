# Caso de negocio — Control de inventario FIFO

## Propuesta

Una pyme que comercializa alimentos, insumos o medicamentos necesita controlar existencias por lote para reducir pérdidas por vencimiento y mantener trazabilidad. El sistema **fifo** permite registrar productos y lotes, controlar stock mínimo y asegurar que cada salida consuma primero las unidades ingresadas más antiguamente.

El caso es realista porque pequeños comercios suelen registrar entradas y salidas en planillas, sin trazabilidad suficiente para saber qué lote debe venderse primero o cuál está próximo a agotarse.

> **Estado de validación docente:** pendiente de confirmación formal. Este documento es la propuesta técnica; no se declara una validación que todavía no existe. Adjuntar aquí la evidencia (correo, mensaje o captura) cuando el docente la entregue.

## Actores y roles

- **Admin:** administra categorías, productos y lotes; registra entradas y consulta movimientos.
- **User:** consulta catálogo/stock y registra salidas; no puede modificar el catálogo ni registrar entradas.

## Reglas de negocio

1. Cada producto tiene un SKU único y pertenece a una categoría activa.
2. Cada lote tiene un código único, producto, cantidad inicial/disponible y fecha de ingreso.
3. El stock de un producto es la suma disponible de sus lotes activos.
4. Una salida ordena los lotes por fecha de ingreso e Id, y consume primero el más antiguo.
5. Una salida mayor al stock disponible se rechaza con `409 Conflict` y no genera movimientos.
6. Las entradas y salidas conservan fecha, cantidad, precio, lote, producto, observaciones y usuario responsable.
7. Las bajas del catálogo son lógicas para no perder trazabilidad histórica.
8. El registro público solo crea rol `User`; `Admin` se configura mediante seed protegido por variables de entorno.

## Diseño de base de datos

```text
Categoria 1 ─── N Producto
Producto   1 ─── N Lote
Producto   1 ─── N Movimiento
Lote       1 ─── N Movimiento
```

La tabla `Usuarios` almacena correo único, hash BCrypt, rol y estado. Las claves foráneas usan `Restrict` para impedir borrados físicos que destruyan el historial. Hay índices únicos para nombre de categoría, SKU, código de lote y correo, además de índices para consultar lotes y movimientos.

## Arquitectura expuesta

- **Presentación:** controladores ASP.NET Core versionados en `/api/v1`.
- **Aplicación:** servicios Scoped con validación y reglas FIFO/autenticación.
- **Persistencia:** patrón Repository, EF Core, `FifoDbContext` y migraciones SQL Server.
- **Transversal:** middleware propio de logging y errores `ProblemDetails`, Swagger, CORS y JWT.
- **Seguridad:** hash BCrypt, tokens firmados HMAC-SHA256 y políticas `AdminOnly`/`UserOrAdmin`.

## Endpoints para la demostración

1. Iniciar sesión como Admin con `POST /api/v1/auth/login`.
2. Crear una categoría y un producto.
3. Crear dos lotes con fechas de ingreso distintas.
4. Registrar una salida cuya cantidad obligue a consumir ambos lotes.
5. Mostrar que el primer movimiento corresponde al lote más antiguo y que el stock quedó actualizado.
6. Repetir una salida superior al stock y mostrar el `409 ProblemDetails`.
7. Iniciar sesión como User y comprobar que puede consultar, pero recibe `403` al modificar el catálogo.
8. Mostrar Swagger y ejecutar `dotnet test fifo.sln -c Release`.

## Guion breve de exposición

- **Negocio:** explicar pérdida por vencimiento y necesidad de trazabilidad FIFO.
- **BD:** mostrar las cinco tablas, relaciones e índices.
- **API:** explicar DTOs, controladores, códigos de estado y versionamiento.
- **Arquitectura:** justificar Repository, servicios Scoped y TokenService Singleton.
- **Seguridad:** demostrar 401 sin token, 403 por rol y login con BCrypt/JWT.
- **Calidad:** mostrar migración aplicada y las nueve pruebas de integración.
