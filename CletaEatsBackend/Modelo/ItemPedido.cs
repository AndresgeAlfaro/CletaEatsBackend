namespace CletaEatsBackend.Modelo
{
    public class ItemPedido
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public int NumeroCombo { get; set; }
        public string Descripcion { get; set; } = "";
        public double PrecioUnitario { get; set; }
        public int Cantidad { get; set; } = 1;

        public ItemPedido() { }

        public ItemPedido(int id, int idPedido, int numeroCombo, string descripcion, double precioUnitario, int cantidad)
        {
            Id = id; IdPedido = idPedido; NumeroCombo = numeroCombo;
            Descripcion = descripcion; PrecioUnitario = precioUnitario; Cantidad = cantidad;
        }

        public override string ToString() =>
            $"[ItemPedido] Combo #{NumeroCombo} x{Cantidad} | {PrecioUnitario * Cantidad:C}";
    }
}
