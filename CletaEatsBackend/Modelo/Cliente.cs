namespace CletaEatsBackend.Modelo
{
    public enum EstadoCliente { ACTIVO, SUSPENDIDO }

    public class Cliente
    {
        public int Id { get; set; }
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string DireccionExacta { get; set; } = "";
        public string NumeroTarjeta { get; set; } = "";
        public string NumeroCelular { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public EstadoCliente Estado { get; set; }

        public Cliente() { Estado = EstadoCliente.ACTIVO; }

        public Cliente(int id, string cedula, string nombre, string direccionExacta,
            string numeroTarjeta, string numeroCelular,
            string correoElectronico, EstadoCliente estado)
        {
            Id = id; Cedula = cedula; Nombre = nombre;
            DireccionExacta = direccionExacta; NumeroTarjeta = numeroTarjeta;
            NumeroCelular = numeroCelular; CorreoElectronico = correoElectronico;
            Estado = estado;
        }

        public bool PuedeRealizarPedidos() => Estado == EstadoCliente.ACTIVO;

        public string TarjetaEnmascarada()
        {
            if (string.IsNullOrEmpty(NumeroTarjeta) || NumeroTarjeta.Length < 4) return "****";
            return "**** **** **** " + NumeroTarjeta[^4..];
        }

        public override string ToString() =>
            $"[Cliente] Id={Id} | Cedula={Cedula} | Nombre={Nombre} | Estado={Estado}";
    }
}
