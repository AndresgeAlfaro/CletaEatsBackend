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
  observacion TEXT NOT NULL DEFAULT '',
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

-- Tabla de procedimientos almacenados (SQLite no tiene procedures nativos; el SQL se guarda aqui y los DAOs solo los invocan por nombre)
CREATE TABLE IF NOT EXISTS ProcedimientoAlmacenado (
  Nombre TEXT PRIMARY KEY,
  SqlTexto TEXT NOT NULL
);

-- Cliente
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_Insertar', 'INSERT INTO Cliente (cedula, nombre, direccion, tarjeta, celular, correo, estado) VALUES (@ced, @nom, @dir, @tar, @cel, @cor, @est)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_ObtenerTodos', 'SELECT * FROM Cliente ORDER BY nombre');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_BuscarPorCedula', 'SELECT * FROM Cliente WHERE cedula = @ced');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_ActualizarEstado', 'UPDATE Cliente SET estado = @est WHERE id = @id');

-- Restaurante
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_Insertar', 'INSERT INTO Restaurante (nombre, cedulaJuridica, direccion, tipoComida) VALUES (@nom, @ced, @dir, @tipo)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_ObtenerTodos', 'SELECT * FROM Restaurante ORDER BY nombre');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_BuscarPorId', 'SELECT * FROM Restaurante WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_BuscarPorCedulaJuridica', 'SELECT * FROM Restaurante WHERE cedulaJuridica = @ced');

-- Combo
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_Insertar', 'INSERT INTO Combo (idRestaurante, numeroCombo, descripcion, precio) VALUES (@idRest, @num, @desc, @precio)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_ObtenerPorRestaurante', 'SELECT * FROM Combo WHERE idRestaurante = @id ORDER BY numeroCombo');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_BuscarPorRestauranteYNumero', 'SELECT * FROM Combo WHERE idRestaurante = @id AND numeroCombo = @num');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_Eliminar', 'DELETE FROM Combo WHERE idRestaurante = @id AND numeroCombo = @num');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_Actualizar', 'UPDATE Combo SET descripcion = @desc, precio = @precio WHERE idRestaurante = @id AND numeroCombo = @num');

-- Repartidor
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_Insertar', 'INSERT INTO Repartidor (nombre, cedula, correo, direccion, celular, tarjeta, estado, distanciaPedido, kmDiarios, amonestaciones) VALUES (@nom, @ced, @cor, @dir, @cel, @tar, @est, @dist, @km, @amon)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_ObtenerTodos', 'SELECT * FROM Repartidor');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_ObtenerPrimerDisponible', 'SELECT * FROM Repartidor WHERE estado = ''DISPONIBLE'' AND amonestaciones < 4 ORDER BY id LIMIT 1');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_ActualizarEstado', 'UPDATE Repartidor SET estado = @est WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_IncrementarAmonestacion', 'UPDATE Repartidor SET amonestaciones = amonestaciones + 1, estado = CASE WHEN amonestaciones + 1 >= 4 THEN ''EXPULSADO'' ELSE estado END WHERE id = @id');

-- Pedido
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_Insertar', 'INSERT INTO Pedido (idCliente, idRestaurante, idRepartidor, horaRealizacion, estado) VALUES (@cli, @rest, @rep, @hora, @est)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ActualizarEstado', 'UPDATE Pedido SET estado = @est WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ActualizarEstadoConHora', 'UPDATE Pedido SET estado = @est, horaEntrega = @hora WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ObtenerTodos', 'SELECT * FROM Pedido');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ObtenerPorCliente', 'SELECT * FROM Pedido WHERE idCliente = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_BuscarPorId', 'SELECT * FROM Pedido WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('LastInsertRowId', 'SELECT last_insert_rowid()');

-- ItemPedido
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('ItemPedido_Insertar', 'INSERT INTO ItemPedido (idPedido, numeroCombo, descripcion, precioUnitario, cantidad) VALUES (@idPed, @num, @desc, @precio, @cant)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('ItemPedido_ObtenerPorPedido', 'SELECT * FROM ItemPedido WHERE idPedido = @id');

-- Factura
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Factura_Insertar', 'INSERT INTO Factura (idPedido, subtotal, costoTransporte, iva, total, fechaEmision) VALUES (@idPed, @sub, @trans, @iva, @total, @fecha)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Factura_BuscarPorPedido', 'SELECT * FROM Factura WHERE idPedido = @id');

-- Queja
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Queja_Insertar', 'INSERT INTO Queja (idRepartidor, idPedido, idCliente, descripcion, fecha, categoria) VALUES (@idRep, @idPed, @idCli, @desc, @fecha, @cat)');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Queja_ObtenerTodas', 'SELECT * FROM Queja ORDER BY fecha');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Queja_ObtenerPorRepartidor', 'SELECT * FROM Queja WHERE idRepartidor = @id ORDER BY fecha');

-- Reportes
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_RestauranteConMasPedidos', 'SELECT nombreRestaurante, totalPedidos FROM vw_PedidosPorRestaurante ORDER BY totalPedidos DESC LIMIT 1');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_MontoPorRestaurante', 'SELECT nombreRestaurante, montoTotal FROM vw_PedidosPorRestaurante');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_MontoTotalGeneral', 'SELECT COALESCE(SUM(total), 0) FROM Factura');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_RestauranteConMenosPedidos', 'SELECT nombreRestaurante, totalPedidos FROM vw_PedidosPorRestaurante ORDER BY totalPedidos ASC LIMIT 1');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_QuejasPorRepartidor', 'SELECT rp.nombre, rp.cedula, q.id, q.fecha, q.categoria, q.descripcion FROM Repartidor rp LEFT JOIN Queja q ON q.idRepartidor = rp.id ORDER BY rp.nombre, q.fecha');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_PedidosPorCliente', 'SELECT c.nombre, c.cedula, p.id, p.horaRealizacion, p.estado FROM Cliente c LEFT JOIN Pedido p ON p.idCliente = c.id ORDER BY c.nombre, p.horaRealizacion');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_ClienteConMasPedidos', 'SELECT nombreCliente, cedula, totalPedidos FROM vw_PedidosPorCliente ORDER BY totalPedidos DESC LIMIT 1');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Reporte_HoraPico', 'SELECT SUBSTR(horaRealizacion, 12, 2) AS hora, COUNT(*) AS cnt FROM Pedido GROUP BY hora ORDER BY cnt DESC LIMIT 1');

INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Factura_EliminarPorPedido', 'DELETE FROM Factura WHERE idPedido = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('ItemPedido_EliminarPorPedido', 'DELETE FROM ItemPedido WHERE idPedido = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_Eliminar', 'DELETE FROM Pedido WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ActualizarObservacion', 'UPDATE Pedido SET observacion = @obs WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ContarPorRestaurante', 'SELECT COUNT(*) FROM Pedido WHERE idRestaurante = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ContarPorCliente', 'SELECT COUNT(*) FROM Pedido WHERE idCliente = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Pedido_ContarPorRepartidor', 'SELECT COUNT(*) FROM Pedido WHERE idRepartidor = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Combo_EliminarTodosRestaurante', 'DELETE FROM Combo WHERE idRestaurante = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_Actualizar', 'UPDATE Restaurante SET nombre = @nom, cedulaJuridica = @ced, direccion = @dir, tipoComida = @tipo WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Restaurante_Eliminar', 'DELETE FROM Restaurante WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_ActualizarDatos', 'UPDATE Cliente SET nombre = @nom, direccion = @dir, tarjeta = @tar, celular = @cel, correo = @cor, estado = @est WHERE cedula = @ced');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Cliente_EliminarPorCedula', 'DELETE FROM Cliente WHERE cedula = @ced');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_ActualizarDatos', 'UPDATE Repartidor SET nombre = @nom, cedula = @ced, correo = @cor, direccion = @dir, celular = @cel, tarjeta = @tar, amonestaciones = @amon WHERE id = @id');
INSERT OR REPLACE INTO ProcedimientoAlmacenado VALUES ('Repartidor_Eliminar', 'DELETE FROM Repartidor WHERE id = @id');
