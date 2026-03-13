## CletaEatsBackend — Módulo 0 (Backend C# con SQLite)

Este proyecto es el **backend en C# puro** del sistema CletaEats (Módulo 0 de la especificación).  
Implementa las 4 capas clásicas:

- **Datos**: conexión SQLite y script `schema.sql`.
- **AccesoDatos**: DAOs para todas las entidades.
- **Modelo**: clases de dominio (Cliente, Restaurante, Pedido, etc.).
- **Control / Lógica de negocio**: Services + Controllers + menú de consola.

El backend expone un **menú de consola** para registrar información y ejecutar los reportes.

---

## Requisitos

- **.NET SDK 8.0 o superior** (el proyecto está en `net8.0`).
- Sistema operativo: Windows, macOS o Linux.

Puedes comprobar tu versión con:

```bash
dotnet --version
```

---

## Estructura principal

En la carpeta `CletaEatsBackend` (proyecto):

- `CletaEatsBackend.csproj` – Proyecto de consola .NET 8 con referencia a `Microsoft.Data.Sqlite`.
- `Program.cs` – Punto de entrada y menú principal en consola.
- `Datos/`
  - `DatabaseManager.cs` – Singleton que abre la conexión SQLite y ejecuta `schema.sql`.
  - `schema.sql` – Define todas las tablas y vistas necesarias.
- `Modelo/` – Clases de dominio (`Cliente`, `Restaurante`, `Repartidor`, `Pedido`, `ItemPedido`, `Factura`, `Queja`, enums).
- `AccesoDatos/` – DAOs (`ClienteDAO`, `RestauranteDAO`, `RepartidorDAO`, `PedidoDAO`, `ItemPedidoDAO`, `FacturaDAO`, `QuejaDAO`, `ComboDAO`).
- `LogicaNegocio/` – Services (`ClienteService`, `PedidoService`, `RepartidorService`, `ReporteService`).
- `Control/` – Controllers usados por `Program.cs`.

Al correr por primera vez, se crea automáticamente el archivo **`cletaeats.db`** en la carpeta de salida (`bin/Debug/net8.0`).

---

## Cómo compilar y ejecutar desde la terminal

1. **Ir a la carpeta del proyecto de consola**

   ```bash
   cd "c:\Users\andre\Documents\Mobiles\Proyecto\Proyecto\Backend\CletaEatsBackend\CletaEatsBackend"
   ```

2. **Restaurar/compilar (opcional, para verificar)**

   ```bash
   dotnet build
   ```

3. **Ejecutar el backend**

   ```bash
   dotnet run
   ```

   La primera vez:

   - `DatabaseManager` ejecuta `Datos/schema.sql`.
   - Se crea el archivo `cletaeats.db` con todas las tablas y vistas.
   - Aparece el menú de consola.

---

## Menú de consola

Cuando ejecutas `dotnet run` verás algo como:

```text
========= CletaEats Backend =========
 1. Registrar cliente
 2. Registrar restaurante
 3. Registrar repartidor
 4. Realizar pedido
 5. Marcar pedido como entregado
 6. Reportes
 0. Salir
 Opcion:
```

### 1. Registrar cliente

Pide: cédula, nombre, dirección, tarjeta, celular, correo.  
Valida que la cédula no esté repetida (usa `ClienteDAO.BuscarPorCedula`).

### 2. Registrar restaurante

Pide: nombre, cédula jurídica, dirección y tipo de comida  
(`RAPIDA`, `CHINA`, `SALUDABLE`, `ITALIANA`, `MEXICANA`, `MARISCOS`, `OTRA`).

### 3. Registrar repartidor

Pide datos básicos (cédula, nombre, correo, dirección, celular, tarjeta)  
y crea un repartidor en estado `DISPONIBLE` con 0 amonestaciones.

### 4. Realizar pedido

Flujo resumido:

1. Muestra los restaurantes registrados.
2. Pide **id restaurante**, **cédula cliente**, **distancia en km** y si es feriado.
3. Pide los combos en formato `numeroCombo,cantidad` por línea (ej: `1,2`).  
   - Si hay combos registrados en BD se usan sus precios/descriciones.  
   - Si no, se usan precios fijos: combo 1=4000, 2=5000, …, 9=12000.
4. `PedidoService`:
   - Valida cliente (ACTIVO y existente).
   - Asigna el primer repartidor `DISPONIBLE` con amonestaciones < 4.
   - Calcula subtotal, costo de transporte, IVA 13 %, total.
   - Inserta `Pedido`, `ItemPedido`s y `Factura`.
   - Cambia al repartidor a estado `OCUPADO`.

Al final muestra el número de pedido y el total.

### 5. Marcar pedido como entregado

Pide:

- `idPedido`
- `idRepartidor`

`PedidoService.MarcarEntregado`:

- Cambia el estado del pedido a `ENTREGADO` y registra la hora de entrega.
- Cambia el repartidor nuevamente a `DISPONIBLE`.

### 6. Reportes

Al elegir la opción 6 se abre un submenú:

```text
--- REPORTES ---
 e) Listado clientes ACTIVOS (id, cedula, nombre)
 f) Listado clientes SUSPENDIDOS
 g) Repartidores con 0 amonestaciones
 h) Listado restaurantes (nombre, ced.jur, dir, tipo)
 i) Restaurante con mas pedidos
 j) Monto por restaurante
 k) Total general
 l) Restaurante con menos pedidos
 m) Quejas por repartidor
 n) Pedidos por cliente
 o) Cliente con mas pedidos
 p) Hora pico
 Opcion:
```

- **e, f**: usan `ClienteController` para mostrar clientes **ACTIVOS** y **SUSPENDIDOS**.
- **g**: usa `RepartidorController` para mostrar repartidores con 0 amonestaciones.
- **h**: muestra todos los restaurantes.
- **i–p**: usan `ReporteService` y las vistas `vw_PedidosPorRestaurante` y `vw_PedidosPorCliente`.

---

## Notas útiles

- La base de datos SQLite está en el archivo `cletaeats.db`.  
  Puedes inspeccionarlo con cualquier GUI de SQLite (por ejemplo, DB Browser for SQLite).
- Si cambias el `schema.sql`, borra el `cletaeats.db` para que se recree desde cero.
- Todo corre en una sola aplicación de consola, **no** usa ASP.NET ni HTTP.

