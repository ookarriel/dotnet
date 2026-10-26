# Evidencia de verificación — fifo API (.NET 8 + SQL Server)

**Fecha de ejecución:** 26-10-2026  
**Configuración:** Release  
**Comando ejecutado:**

```bash
dotnet test fifo.sln --configuration Release --logger "console;verbosity=normal"
```

## Resultado

```text
Compilación correcta: 0 advertencias, 0 errores
Pruebas totales: 9
Correctas: 9
Fallidas: 0
Omitidas: 0
```

La salida completa se conserva en `docs/test-results.txt`.

## Cobertura funcional de la suite

| Prueba | Evidencia comprobada |
|---|---|
| `Productos_SinToken_Retorna401` | Protección de endpoints |
| `Login_Administrador_RetornaJwtConRolAdmin` | Login, generación JWT y rol Admin |
| `Productos_CrudCompleto_FuncionaConAdministrador` | GET, GET/{id}, POST, PUT y DELETE del recurso principal |
| `Producto_DatosInvalidos_Retorna400` | Validación de entrada y códigos de estado |
| `UsuarioBasico_PuedeConsultarPeroNoCrearCategoria` | Autorización diferenciada por roles y respuesta 403 |
| `Salida_ConsumeLotesEnOrdenFifo` | Consumo FIFO, movimientos por lote y actualización de stock |
| `Salida_SinStockSuficiente_Retorna409` | Regla de negocio y error estructurado |
| `Entrada_ActualizaStockDelLote` | Entrada de inventario y persistencia del nuevo stock |
| `Swagger_ExponeDocumentoOpenApiVersionado` | OpenAPI disponible y rutas versionadas |

## Modos de ejecución de la base de datos de prueba

- **Predeterminado (sin servidor):** cada prueba crea una base **SQLite temporal relacional**, genera el esquema desde el modelo (`EnsureCreated`) y elimina los archivos al finalizar. No necesita credenciales ni altera la base SQL Server.
- **Contra SQL Server real:** defina `FIFO_TESTS_SQLSERVER=1` y `FIFO_TESTS_CONNECTION`. Cada prueba crea una base `fifo_tests_{guid}`, **aplica la migración SQL Server** (`MigrateAsync`) y la elimina al finalizar.

> En esta máquina no había una instancia de SQL Server disponible, por lo que la ejecución registrada corresponde al modo SQLite temporal. La validez del esquema SQL Server se comprobó además generando su DDL con EF Core.

## Verificación del esquema SQL Server (offline)

```bash
dotnet ef migrations script --project src/fifo.Api/fifo.Api.csproj --output docs/sqlserver-schema.sql
```

Resultado: script T-SQL generado correctamente con `IDENTITY`, `nvarchar`, `decimal(18,2)`, `datetime2`, claves foráneas e índices para **Categorias, Usuarios, Productos, Lotes y Movimientos**, más `__EFMigrationsHistory`. No requiere conexión al servidor.

## Verificación adicional de OpenAPI

```text
GET /swagger/v1/swagger.json → 200 OK
OpenAPI 3.0.1
14 rutas exportadas en docs/openapi-v1.json
```

## Reproducir

```bash
# Sin servidor (predeterminado)
dotnet test fifo.sln --configuration Release

# Contra su SQL Server
export FIFO_TESTS_SQLSERVER=1
export FIFO_TESTS_CONNECTION="Server=localhost,1433;User Id=sa;Password=SU_PASSWORD;TrustServerCertificate=True"
dotnet test fifo.sln --configuration Release
```
