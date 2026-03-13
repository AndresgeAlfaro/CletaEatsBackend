namespace CletaEatsBackend.Modelo
{
    public class Combo
    {
        public int Id { get; set; }
        public int IdRestaurante { get; set; }
        public int NumeroCombo { get; set; }
        public string Descripcion { get; set; } = "";
        public double Precio { get; set; }

        public Combo() { }

        public Combo(int id, int idRestaurante, int numeroCombo, string descripcion, double precio)
        {
            Id = id; IdRestaurante = idRestaurante; NumeroCombo = numeroCombo;
            Descripcion = descripcion; Precio = precio;
        }

        public override string ToString() =>
            $"[Combo] #{NumeroCombo} {Descripcion} | {Precio:C}";
    }
}
