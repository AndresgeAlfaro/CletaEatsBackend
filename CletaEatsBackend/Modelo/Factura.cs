namespace CletaEatsBackend.Modelo
{
    public class Factura
    {
        public int Id { get; set; }
        public int IdPedido { get; set; }
        public double Subtotal { get; set; }
        public double CostoTransporte { get; set; }
        public double Iva { get; set; }
        public double Total { get; set; }
        public string FechaEmision { get; set; } = "";

        public Factura() { }

        public Factura(int idPedido, double subtotal, double costoTransporte, string fechaEmision)
        {
            IdPedido = idPedido;
            Subtotal = subtotal;
            CostoTransporte = costoTransporte;
            FechaEmision = fechaEmision;
            CalcularTotales();
        }

        public void CalcularTotales()
        {
            Iva = (Subtotal + CostoTransporte) * 0.13;
            Total = Subtotal + CostoTransporte + Iva;
        }

        public string GenerarTextoImprimible() =>
            "===========================================\n" +
            $" FACTURA CletaEats #{IdPedido}\n" +
            "===========================================\n" +
            $" Fecha: {FechaEmision}\n" +
            "-------------------------------------------\n" +
            $" Subtotal:     {Subtotal,12:C}\n" +
            $" Transporte:   {CostoTransporte,12:C}\n" +
            $" IVA (13%):    {Iva,12:C}\n" +
            "-------------------------------------------\n" +
            $" TOTAL:        {Total,12:C}\n" +
            "===========================================\n";

        public override string ToString() =>
            $"[Factura] Pedido={IdPedido} | Sub={Subtotal:C} | Trans={CostoTransporte:C} | IVA={Iva:C} | Total={Total:C}";
    }
}
