namespace CletaEatsBackend.Modelo
{
    public enum CategoriaQueja { AMABILIDAD, TIEMPO_RESPUESTA, PRESENTACION, OTRA }

    public class Queja
    {
        public int Id { get; set; }
        public int IdRepartidor { get; set; }
        public int IdPedido { get; set; }
        public int IdCliente { get; set; }
        public string Descripcion { get; set; } = "";
        public string Fecha { get; set; } = "";
        public CategoriaQueja Categoria { get; set; }

        public Queja() { }

        public Queja(int id, int idRepartidor, int idPedido, int idCliente, string descripcion, string fecha, CategoriaQueja categoria)
        {
            Id = id; IdRepartidor = idRepartidor; IdPedido = idPedido; IdCliente = idCliente;
            Descripcion = descripcion; Fecha = fecha; Categoria = categoria;
        }

        public override string ToString() =>
            $"[Queja] Id={Id} | Repartidor={IdRepartidor} | Categoria={Categoria} | {Fecha}";
    }
}
