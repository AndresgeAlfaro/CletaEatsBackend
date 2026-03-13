namespace CletaEatsBackend.Modelo
{
    public enum TipoComida { RAPIDA, CHINA, SALUDABLE, ITALIANA, MEXICANA, MARISCOS, OTRA }

    public class Restaurante
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string CedulaJuridica { get; set; } = "";
        public string Direccion { get; set; } = "";
        public TipoComida TipoComida { get; set; }

        public Restaurante() { }

        public Restaurante(int id, string nombre, string cedulaJuridica, string direccion, TipoComida tipoComida)
        {
            Id = id; Nombre = nombre; CedulaJuridica = cedulaJuridica;
            Direccion = direccion; TipoComida = tipoComida;
        }

        public override string ToString() =>
            $"[Restaurante] Id={Id} | Nombre={Nombre} | Tipo={TipoComida}";
    }
}
