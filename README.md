# CletaEats Backend — Módulo 0 (API C# con SQLite)

Backend del sistema CletaEats en **C# con ASP.NET Core**. Expone una **API REST** en HTTP y usa SQLite como base de datos.

Implementa las capas:

- **Datos**: conexión SQLite y script `schema.sql`.
- **AccesoDatos**: DAOs para todas las entidades.
- **Modelo**: clases de dominio (Cliente, Restaurante, Pedido, etc.).
- **Control / Lógica de negocio**: Services + Controllers.
- **Api**: controladores HTTP que exponen los endpoints REST (JSON).

---

## Requisitos

- **.NET SDK 8.0** o superior.

Comprobar versión:

```bash
dotnet --version
```

---

## Estructura principal

Dentro de `CletaEatsBackend` (proyecto):

| Carpeta / archivo | Descripción |
|-------------------|-------------|
| `CletaEatsBackend.csproj` | Proyecto .NET 8 (SDK Web) con `Microsoft.Data.Sqlite`. |
| `Program.cs` | Configuración de la API: CORS, controladores, inicialización de BD. |
| `Datos/` | `DatabaseManager.cs` (Singleton, SQLite), `schema.sql` (tablas, vistas, procedimientos almacenados). |
| `Modelo/` | Clases de dominio: Cliente, Restaurante, Repartidor, Pedido, ItemPedido, Factura, Queja, Combo y enums. |
| `Dtos/` | DAOs (AccesoDatos): ClienteDAO, RestauranteDAO, RepartidorDAO, PedidoDAO, ItemPedidoDAO, FacturaDAO, QuejaDAO, ComboDAO. |
| `Servicios/` | Lógica de negocio: ClienteService, PedidoService, RepartidorService, ReporteService. |
| `Control/` | Controllers de lógica: ClienteController, RestauranteController, RepartidorController, PedidoController, ReporteController. |
| `Api/` | Controladores HTTP: ClientesApiController, RestaurantesApiController, RepartidoresApiController, PedidosApiController, ReportesApiController. |

La SQL de las operaciones está en la tabla **`ProcedimientoAlmacenado`**; los DAOs ejecutan ese SQL mediante `DatabaseManager.GetSqlProcedimiento(nombre)`.

Al ejecutar por primera vez se crea **`cletaeats.db`** en el directorio de salida (o en el directorio de trabajo según la configuración de `DatabaseManager`).

---

## Cómo ejecutar la API

1. Ir a la carpeta del proyecto:

   ```bash
   cd Backend/CletaEatsBackend/CletaEatsBackend
   ```

2. Compilar (opcional):

   ```bash
   dotnet build
   ```

3. Ejecutar:

   ```bash
   dotnet run
   ```

La API queda disponible en **http://localhost:5000**. CORS está configurado para permitir peticiones desde `http://localhost:5173` y `http://127.0.0.1:5173` (frontend en desarrollo).

---

## Endpoints de la API

Base URL: `http://localhost:5000/api`

### Clientes (`/api/ClientesApi`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/registrar` | Registrar cliente (body: cedula, nombre, direccion, tarjeta, celular, correo). |
| GET | `/verificar/{cedula}` | Verificar estado del cliente (ACTIVO / SUSPENDIDO / NO_REGISTRADO). |
| GET | `/activos` | Lista de clientes activos. |
| GET | `/suspendidos` | Lista de clientes suspendidos. |

### Restaurantes (`/api/RestaurantesApi`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/registrar` | Registrar restaurante (body: nombre, cedulaJuridica, direccion, tipoComida). |
| GET | `/` | Lista de todos los restaurantes. |
| GET | `/{idRestaurante}/combos` | Combos del restaurante. |

Tipos de comida: `RAPIDA`, `CHINA`, `SALUDABLE`, `ITALIANA`, `MEXICANA`, `MARISCOS`, `OTRA`.

### Repartidores (`/api/RepartidoresApi`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/registrar` | Registrar repartidor (body: cedula, nombre, correo, direccion, celular, tarjeta). |
| GET | `/` | Lista de todos los repartidores. |
| GET | `/cero-amonestaciones` | Repartidores con 0 amonestaciones. |

### Pedidos (`/api/PedidosApi`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/realizar` | Crear pedido (body: cedulaCliente, idRestaurante, distanciaKm, esFeriado, items[]). |
| POST | `/marcar-entregado` | Marcar pedido entregado (body: idPedido, idRepartidor). |

Cada item en `items` debe tener: `numeroCombo`, `descripcion`, `precioUnitario`, `cantidad`.

### Reportes (`/api/ReportesApi`)

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/restaurante-mas-pedidos` | Restaurante con más pedidos. |
| GET | `/restaurante-menos-pedidos` | Restaurante con menos pedidos. |
| GET | `/monto-por-restaurante` | Monto por restaurante. |
| GET | `/monto-total` | Monto total general. |
| GET | `/quejas-por-repartidor` | Quejas por repartidor. |
| GET | `/pedidos-por-cliente` | Pedidos por cliente. |
| GET | `/cliente-mas-pedidos` | Cliente con más pedidos. |
| GET | `/hora-pico` | Hora pico. |

Las respuestas son JSON. En caso de error de validación, la API devuelve 400 con un objeto `{ mensaje: "..." }`.

---

## Notas

- Base de datos: archivo **`cletaeats.db`** (SQLite). Se puede inspeccionar con DB Browser for SQLite u otra herramienta.
- Si modificas `schema.sql`, borra `cletaeats.db` para que se regenere.
- La URL y el puerto se pueden cambiar en `Properties/launchSettings.json` (perfil `CletaEatsBackend`).
