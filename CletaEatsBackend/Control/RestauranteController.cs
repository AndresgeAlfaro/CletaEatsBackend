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

        public string AgregarCombo(int idRestaurante, string descripcion, double precio, int? numeroComboOpcional)
        {
            if (_dao.BuscarPorId(idRestaurante) == null)
                return "Error: restaurante no encontrado.";
            if (string.IsNullOrWhiteSpace(descripcion))
                return "Error: descripcion obligatoria.";
            if (precio <= 0)
                return "Error: precio invalido.";
            int num;
            if (numeroComboOpcional.HasValue)
            {
                num = numeroComboOpcional.Value;
                if (num is < 1 or > 9)
                    return "Error: numeroCombo debe estar entre 1 y 9.";
                if (_comboDAO.BuscarPorRestauranteYNumero(idRestaurante, num) != null)
                    return "Error: ya existe un combo con ese numero en este restaurante.";
            }
            else
            {
                num = Enumerable.Range(1, 9).FirstOrDefault(n =>
                    _comboDAO.BuscarPorRestauranteYNumero(idRestaurante, n) == null);
                if (num == 0)
                    return "Error: el restaurante ya tiene el maximo de combos (9).";
            }
            var c = new Combo
            {
                IdRestaurante = idRestaurante,
                NumeroCombo = num,
                Descripcion = descripcion.Trim(),
                Precio = precio,
            };
            return _comboDAO.Insertar(c) ? $"Combo #{num} agregado." : "Error al guardar combo.";
        }

        public string EliminarCombo(int idRestaurante, int numeroCombo)
        {
            if (_dao.BuscarPorId(idRestaurante) == null)
                return "Error: restaurante no encontrado.";
            if (numeroCombo is < 1 or > 9)
                return "Error: numeroCombo invalido.";
            if (_comboDAO.BuscarPorRestauranteYNumero(idRestaurante, numeroCombo) == null)
                return "Error: combo no encontrado.";
            return _comboDAO.Eliminar(idRestaurante, numeroCombo)
                ? "Combo eliminado."
                : "Error al eliminar combo.";
        }

        public string ActualizarCombo(int idRestaurante, int numeroCombo, string descripcion, double precio)
        {
            if (_dao.BuscarPorId(idRestaurante) == null)
                return "Error: restaurante no encontrado.";
            if (numeroCombo is < 1 or > 9)
                return "Error: numeroCombo invalido.";
            if (_comboDAO.BuscarPorRestauranteYNumero(idRestaurante, numeroCombo) == null)
                return "Error: combo no encontrado.";
            if (string.IsNullOrWhiteSpace(descripcion))
                return "Error: descripcion obligatoria.";
            if (precio <= 0)
                return "Error: precio invalido.";
            return _comboDAO.Actualizar(idRestaurante, numeroCombo, descripcion.Trim(), precio)
                ? "Combo actualizado."
                : "Error al actualizar combo.";
        }

        public string Actualizar(int id, string nombre, string cedulaJuridica, string direccion, string tipoComidaStr)
        {
            if (_dao.BuscarPorId(id) == null)
                return "Error: restaurante no encontrado.";
            if (!Enum.TryParse<TipoComida>(tipoComidaStr, true, out var tipo))
                return "Tipo de comida no valido. Use: RAPIDA, CHINA, SALUDABLE, ITALIANA, MEXICANA, MARISCOS, OTRA";
            var otro = _dao.BuscarPorCedulaJuridica(cedulaJuridica);
            if (otro != null && otro.Id != id)
                return $"Error: cedula juridica {cedulaJuridica} ya registrada.";
            var r = new Restaurante(id, nombre.Trim(), cedulaJuridica.Trim(), direccion.Trim(), tipo);
            return _dao.Actualizar(r) ? "Restaurante actualizado." : "Error al actualizar.";
        }

        public string Eliminar(int id)
        {
            var pedidoDao = new PedidoDAO();
            if (_dao.BuscarPorId(id) == null)
                return "Error: restaurante no encontrado.";
            if (pedidoDao.ContarPorRestaurante(id) > 0)
                return "Error: hay pedidos asociados a este restaurante.";
            _dao.EliminarCombosDeRestaurante(id);
            return _dao.Eliminar(id) ? "Restaurante eliminado." : "Error al eliminar.";
        }
    }
}
