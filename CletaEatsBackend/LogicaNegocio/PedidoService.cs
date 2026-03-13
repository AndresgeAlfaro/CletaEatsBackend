using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.LogicaNegocio
{
    public class PedidoService
    {
        private readonly PedidoDAO _pedidoDAO = new();
        private readonly RepartidorDAO _repartidorDAO = new();
        private readonly ClienteDAO _clienteDAO = new();
        private readonly FacturaDAO _facturaDAO = new();
        private readonly ItemPedidoDAO _itemDAO = new();

        public (Pedido? pedido, Factura? factura, string? error) CrearPedido(
            string cedulaCliente, int idRestaurante,
            List<ItemPedido> items, double distanciaKm, bool esFeriado)
        {
            var cliente = _clienteDAO.BuscarPorCedula(cedulaCliente);
            if (cliente == null)
                return (null, null, "Cliente no registrado.");
            if (cliente.Estado == EstadoCliente.SUSPENDIDO)
                return (null, null, "Cuenta suspendida. No puede realizar pedidos.");

            var repartidor = _repartidorDAO.ObtenerPrimerDisponible();
            if (repartidor == null)
                return (null, null, "Sin repartidores disponibles.");

            repartidor.DistanciaDelPedido = distanciaKm;
            double subtotal = items.Sum(i => i.PrecioUnitario * i.Cantidad);
            double costoTransporte = repartidor.CalcularCostoTransporte(esFeriado);
            string ahora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            var pedido = new Pedido
            {
                IdCliente = cliente.Id,
                IdRestaurante = idRestaurante,
                IdRepartidor = repartidor.Id,
                HoraRealizacion = ahora,
                Estado = EstadoPedido.EN_PREPARACION
            };
            int idPedido = _pedidoDAO.Insertar(pedido);
            pedido.Id = idPedido;

            foreach (var item in items)
            {
                item.IdPedido = idPedido;
                _itemDAO.Insertar(item);
            }

            var factura = new Factura(idPedido, subtotal, costoTransporte, ahora);
            _facturaDAO.Insertar(factura);

            _repartidorDAO.ActualizarEstado(repartidor.Id, EstadoRepartidor.OCUPADO);

            return (pedido, factura, null);
        }

        public string MarcarEntregado(int idPedido, int idRepartidor)
        {
            string hora = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            _pedidoDAO.ActualizarEstado(idPedido, EstadoPedido.ENTREGADO, hora);
            _repartidorDAO.ActualizarEstado(idRepartidor, EstadoRepartidor.DISPONIBLE);
            return $"Pedido #{idPedido} entregado exitosamente a las {hora}.";
        }

        public List<Pedido> ObtenerPorCliente(int idCliente) => _pedidoDAO.ObtenerPorCliente(idCliente);
        public Pedido? BuscarPorId(int id) => _pedidoDAO.BuscarPorId(id);
    }
}
