-- Clientes del sistema
CREATE TABLE IF NOT EXISTS Cliente (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  cedula TEXT NOT NULL UNIQUE,
  nombre TEXT NOT NULL,
  direccion TEXT NOT NULL,
  tarjeta TEXT NOT NULL,
  celular TEXT NOT NULL,
  correo TEXT NOT NULL,
  estado TEXT NOT NULL DEFAULT 'ACTIVO'
  CHECK ( estado IN ('ACTIVO', 'SUSPENDIDO') )
);

-- Restaurantes registrados (maximo 7 en el negocio)
CREATE TABLE IF NOT EXISTS Restaurante (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  nombre TEXT NOT NULL,
  cedulaJuridica TEXT NOT NULL UNIQUE,
  direccion TEXT NOT NULL,
  tipoComida TEXT NOT NULL
  CHECK ( tipoComida IN ('RAPIDA', 'CHINA', 'SALUDABLE', 'ITALIANA', 'MEXICANA', 'MARISCOS', 'OTRA') )
);

-- Combos de cada restaurante (cada restaurante tiene del 1 al 9)
-- Precios fijos: 1=4000, 2=5000 ... 9=12000 colones
CREATE TABLE IF NOT EXISTS Combo (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  idRestaurante INTEGER NOT NULL,
  numeroCombo INTEGER NOT NULL CHECK ( numeroCombo BETWEEN 1 AND 9),
  descripcion TEXT NOT NULL,
  precio REAL NOT NULL,
  FOREIGN KEY ( idRestaurante ) REFERENCES Restaurante ( id ),
  UNIQUE ( idRestaurante, numeroCombo )
);

-- Repartidores registrados
CREATE TABLE IF NOT EXISTS Repartidor (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  nombre TEXT NOT NULL,
  cedula TEXT NOT NULL UNIQUE,
  correo TEXT NOT NULL,
  direccion TEXT NOT NULL,
  celular TEXT NOT NULL,
  tarjeta TEXT NOT NULL,
  estado TEXT NOT NULL DEFAULT 'DISPONIBLE'
  CHECK ( estado IN ('DISPONIBLE', 'OCUPADO', 'EXPULSADO') ),
  distanciaPedido REAL NOT NULL DEFAULT 0,
  kmDiarios REAL NOT NULL DEFAULT 0,
  amonestaciones INTEGER NOT NULL DEFAULT 0
);

-- Quejas de clientes sobre repartidores
CREATE TABLE IF NOT EXISTS Queja (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  idRepartidor INTEGER NOT NULL,
  idPedido INTEGER NOT NULL,
  idCliente INTEGER NOT NULL,
  descripcion TEXT NOT NULL,
  fecha TEXT NOT NULL,
  categoria TEXT NOT NULL
  CHECK ( categoria IN ('AMABILIDAD', 'TIEMPO_RESPUESTA', 'PRESENTACION', 'OTRA') ),
  FOREIGN KEY ( idRepartidor ) REFERENCES Repartidor ( id ),
  FOREIGN KEY ( idCliente ) REFERENCES Cliente ( id )
);

-- Pedidos realizados
CREATE TABLE IF NOT EXISTS Pedido (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  idCliente INTEGER NOT NULL,
  idRestaurante INTEGER NOT NULL,
  idRepartidor INTEGER NOT NULL,
  horaRealizacion TEXT NOT NULL,
  horaEntrega TEXT,
  estado TEXT NOT NULL DEFAULT 'EN_PREPARACION'
  CHECK ( estado IN ('EN_PREPARACION', 'EN_CAMINO', 'ENTREGADO', 'SUSPENDIDO') ),
  FOREIGN KEY ( idCliente ) REFERENCES Cliente ( id ),
  FOREIGN KEY ( idRestaurante ) REFERENCES Restaurante ( id ),
  FOREIGN KEY ( idRepartidor ) REFERENCES Repartidor ( id )
);

-- Detalle de combos dentro de un pedido
CREATE TABLE IF NOT EXISTS ItemPedido (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  idPedido INTEGER NOT NULL,
  numeroCombo INTEGER NOT NULL,
  descripcion TEXT NOT NULL,
  precioUnitario REAL NOT NULL,
  cantidad INTEGER NOT NULL DEFAULT 1,
  FOREIGN KEY ( idPedido ) REFERENCES Pedido ( id )
);

-- Facturas generadas por pedido
CREATE TABLE IF NOT EXISTS Factura (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  idPedido INTEGER NOT NULL UNIQUE,
  subtotal REAL NOT NULL,
  costoTransporte REAL NOT NULL,
  iva REAL NOT NULL,
  total REAL NOT NULL,
  fechaEmision TEXT NOT NULL,
  FOREIGN KEY ( idPedido ) REFERENCES Pedido ( id )
);

-- Vista para reportes i, j, k, l
CREATE VIEW IF NOT EXISTS vw_PedidosPorRestaurante AS
SELECT
  r.id AS idRestaurante,
  r.nombre AS nombreRestaurante,
  COUNT(p.id) AS totalPedidos,
  COALESCE(SUM(f.total), 0.0) AS montoTotal
FROM Restaurante r
LEFT JOIN Pedido p ON p.idRestaurante = r.id
LEFT JOIN Factura f ON f.idPedido = p.id
GROUP BY r.id, r.nombre;

-- Vista para reportes n, o
CREATE VIEW IF NOT EXISTS vw_PedidosPorCliente AS
SELECT
  c.id AS idCliente,
  c.nombre AS nombreCliente,
  c.cedula,
  COUNT(p.id) AS totalPedidos
FROM Cliente c
LEFT JOIN Pedido p ON p.idCliente = c.id
GROUP BY c.id, c.nombre, c.cedula;
