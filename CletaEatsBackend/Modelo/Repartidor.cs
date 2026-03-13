namespace CletaEatsBackend.Modelo
{
    public enum EstadoRepartidor { DISPONIBLE, OCUPADO, EXPULSADO }

    public class Repartidor
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string CorreoElectronico { get; set; } = "";
        public string DireccionExacta { get; set; } = "";
        public string NumeroCelular { get; set; } = "";
        public string NumeroTarjeta { get; set; } = "";
        public EstadoRepartidor Estado { get; set; }
        public double DistanciaDelPedido { get; set; }
        public double KilometrosDiarios { get; set; }
        public int NumeroAmonestaciones { get; set; }

        public const double CostoKmHabil = 1000.0;
        public const double CostoKmFeriado = 1500.0;

        public Repartidor() { Estado = EstadoRepartidor.DISPONIBLE; }

        public Repartidor(int id, string nombre, string cedula,
            string correo, string direccion, string celular,
            string tarjeta, EstadoRepartidor estado,
            double distancia, double kmDiarios, int amonestaciones)
        {
            Id = id; Nombre = nombre; Cedula = cedula;
            CorreoElectronico = correo; DireccionExacta = direccion;
            NumeroCelular = celular; NumeroTarjeta = tarjeta;
            Estado = estado; DistanciaDelPedido = distancia;
            KilometrosDiarios = kmDiarios; NumeroAmonestaciones = amonestaciones;
        }

        public double CalcularCostoTransporte(bool esFeriado)
        {
            double tarifa = esFeriado ? CostoKmFeriado : CostoKmHabil;
            return DistanciaDelPedido * tarifa;
        }

        public bool EstaDisponible() =>
            Estado == EstadoRepartidor.DISPONIBLE && NumeroAmonestaciones < 4;

        public bool DebeSerExpulsado() => NumeroAmonestaciones >= 4;

        public override string ToString() =>
            $"[Repartidor] Id={Id} | Nombre={Nombre} | Estado={Estado} | Amonestaciones={NumeroAmonestaciones}/4";
    }
}
