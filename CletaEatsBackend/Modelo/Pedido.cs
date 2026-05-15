namespace CletaEatsBackend.Modelo
{
    public enum EstadoPedido { EN_PREPARACION, EN_CAMINO, ENTREGADO, SUSPENDIDO }

    public class Pedido
    {
        public int Id { get; set; }
        public int IdCliente { get; set; }
        public int IdRestaurante { get; set; }
        public int IdRepartidor { get; set; }
        public string HoraRealizacion { get; set; } = "";
        public string? HoraEntrega { get; set; }
        public EstadoPedido Estado { get; set; }
        public string Observacion { get; set; } = "";

        public Pedido() { Estado = EstadoPedido.EN_PREPARACION; }

        public Pedido(int id, int idCliente, int idRestaurante, int idRepartidor,
            string horaRealizacion, string? horaEntrega, EstadoPedido estado)
        {
            Id = id; IdCliente = idCliente; IdRestaurante = idRestaurante;
            IdRepartidor = idRepartidor; HoraRealizacion = horaRealizacion;
            HoraEntrega = horaEntrega; Estado = estado;
        }

        public bool EstaFinalizado() =>
            Estado == EstadoPedido.ENTREGADO || Estado == EstadoPedido.SUSPENDIDO;

        public override string ToString() =>
            $"[Pedido] Id={Id} | Cliente={IdCliente} | Restaurante={IdRestaurante} | Estado={Estado} | Hora={HoraRealizacion}";
    }
}
