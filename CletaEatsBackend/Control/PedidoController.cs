using CletaEatsBackend.LogicaNegocio;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.Control
{
    public class PedidoController
    {
        private readonly PedidoService _service = new();

        public (bool ok, string msg) RealizarPedido(string cedulaCliente, int idRestaurante, List<ItemPedido> items, double distanciaKm, bool esFeriado)
        {
            var (pedido, factura, error) = _service.CrearPedido(cedulaCliente, idRestaurante, items, distanciaKm, esFeriado);
            if (error != null)
                return (false, error);
            return (true, $"Pedido #{pedido!.Id} creado. Total: {factura!.Total:C}");
        }

        public string MarcarEntregado(int idPedido, int idRepartidor)
        {
            return _service.MarcarEntregado(idPedido, idRepartidor);
        }

        public string ActualizarObservacion(int idPedido, string observacion) =>
            _service.ActualizarObservacion(idPedido, observacion);

        public string EliminarPedido(int idPedido) => _service.EliminarPedido(idPedido);
    }
}
