using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend.Control
{
    public class RestauranteController
    {
        private readonly RestauranteDAO _dao = new();
        private readonly ComboDAO _comboDAO = new();

        public string Registrar(string nombre, string cedulaJuridica, string direccion, string tipoComidaStr)
        {
            if (!Enum.TryParse<TipoComida>(tipoComidaStr, true, out var tipo))
                return "Tipo de comida no valido. Use: RAPIDA, CHINA, SALUDABLE, ITALIANA, MEXICANA, MARISCOS, OTRA";
            if (_dao.BuscarPorCedulaJuridica(cedulaJuridica) != null)
                return $"Error: cedula juridica {cedulaJuridica} ya registrada.";
            var r = new Restaurante
            {
                Nombre = nombre,
                CedulaJuridica = cedulaJuridica,
                Direccion = direccion,
                TipoComida = tipo
            };
            return _dao.Insertar(r) ? $"Restaurante '{nombre}' registrado." : "Error al guardar.";
        }

        public void MostrarRestaurantes()
        {
            Console.WriteLine("\n=== Restaurantes ===");
            foreach (var r in _dao.ObtenerTodos())
                Console.WriteLine($"{r.Nombre} | Ced.Jur: {r.CedulaJuridica} | Dir: {r.Direccion} | Tipo: {r.TipoComida}");
        }

        public List<Restaurante> ObtenerTodos() => _dao.ObtenerTodos();
        public List<Combo> ObtenerCombos(int idRestaurante) => _comboDAO.ObtenerPorRestaurante(idRestaurante);
    }
}
