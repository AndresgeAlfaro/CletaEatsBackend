using CletaEatsBackend.Control;
using CletaEatsBackend.AccesoDatos;
using CletaEatsBackend.Modelo;

namespace CletaEatsBackend
{
    class Program
    {
        static void Main()
        {
            var clienteCtrl = new ClienteController();
            var restCtrl = new RestauranteController();
            var repartCtrl = new RepartidorController();
            var pedidoCtrl = new PedidoController();
            var reporteCtrl = new ReporteController();

            bool salir = false;
            while (!salir)
            {
                Console.WriteLine("\n========= CletaEats Backend =========");
                Console.WriteLine(" 1. Registrar cliente");
                Console.WriteLine(" 2. Registrar restaurante");
                Console.WriteLine(" 3. Registrar repartidor");
                Console.WriteLine(" 4. Realizar pedido");
                Console.WriteLine(" 5. Marcar pedido como entregado");
                Console.WriteLine(" 6. Reportes");
                Console.WriteLine(" 0. Salir");
                Console.Write(" Opcion: ");
                switch (Console.ReadLine()?.Trim())
                {
                    case "1": RegistrarCliente(clienteCtrl); break;
                    case "2": RegistrarRestaurante(restCtrl); break;
                    case "3": RegistrarRepartidor(repartCtrl); break;
                    case "4": RealizarPedido(pedidoCtrl, restCtrl); break;
                    case "5": MarcarEntregado(pedidoCtrl); break;
                    case "6": MenuReportes(reporteCtrl, clienteCtrl, restCtrl, repartCtrl); break;
                    case "0": salir = true; break;
                    default: Console.WriteLine(" Opcion no valida."); break;
                }
            }
        }

        static void RegistrarCliente(ClienteController ctrl)
        {
            Console.Write(" Cedula: "); string ced = Console.ReadLine() ?? "";
            Console.Write(" Nombre: "); string nom = Console.ReadLine() ?? "";
            Console.Write(" Direccion: "); string dir = Console.ReadLine() ?? "";
            Console.Write(" Tarjeta: "); string tar = Console.ReadLine() ?? "";
            Console.Write(" Celular: "); string cel = Console.ReadLine() ?? "";
            Console.Write(" Correo: "); string cor = Console.ReadLine() ?? "";
            Console.WriteLine(ctrl.Registrar(ced, nom, dir, tar, cel, cor));
        }

        static void RegistrarRestaurante(RestauranteController ctrl)
        {
            Console.Write(" Nombre: "); string nom = Console.ReadLine() ?? "";
            Console.Write(" Cedula juridica: "); string ced = Console.ReadLine() ?? "";
            Console.Write(" Direccion: "); string dir = Console.ReadLine() ?? "";
            Console.Write(" Tipo comida (RAPIDA/CHINA/SALUDABLE/ITALIANA/MEXICANA/MARISCOS/OTRA): "); string tipo = Console.ReadLine() ?? "";
            Console.WriteLine(ctrl.Registrar(nom, ced, dir, tipo));
        }

        static void RegistrarRepartidor(RepartidorController ctrl)
        {
            Console.Write(" Cedula: "); string ced = Console.ReadLine() ?? "";
            Console.Write(" Nombre: "); string nom = Console.ReadLine() ?? "";
            Console.Write(" Correo: "); string cor = Console.ReadLine() ?? "";
            Console.Write(" Direccion: "); string dir = Console.ReadLine() ?? "";
            Console.Write(" Celular: "); string cel = Console.ReadLine() ?? "";
            Console.Write(" Tarjeta: "); string tar = Console.ReadLine() ?? "";
            Console.WriteLine(ctrl.Registrar(ced, nom, cor, dir, cel, tar));
        }

        static void RealizarPedido(PedidoController pedidoCtrl, RestauranteController restCtrl)
        {
            restCtrl.MostrarRestaurantes();
            Console.Write(" Id restaurante: ");
            if (!int.TryParse(Console.ReadLine(), out int idRest) || idRest <= 0)
            {
                Console.WriteLine(" Id no valido.");
                return;
            }
            Console.Write(" Cedula cliente: "); string ced = Console.ReadLine() ?? "";
            Console.Write(" Distancia (km): ");
            if (!double.TryParse(Console.ReadLine(), out double dist) || dist < 0) dist = 1;
            Console.Write(" Es feriado? (s/n): "); bool esFeriado = (Console.ReadLine()?.Trim().ToLower() == "s");

            var comboDAO = new ComboDAO();
            var combos = comboDAO.ObtenerPorRestaurante(idRest);
            if (combos.Count == 0)
            {
                Console.WriteLine(" No hay combos. Precios fijos: Combo 1=4000, 2=5000 ... 9=12000. Ingrese numeroCombo,cantidad (ej: 1,2)");
            }
            else
            {
                Console.WriteLine(" Combos disponibles:");
                foreach (var c in combos) Console.WriteLine($"  #{c.NumeroCombo} {c.Descripcion} {c.Precio:C}");
            }
            Console.WriteLine(" Ingrese items como: numeroCombo,cantidad (uno por linea, linea vacia para terminar). Ej: 1,2");
            var items = new List<ItemPedido>();
            double[] preciosFijos = { 0, 4000, 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000 };
            while (true)
            {
                string? linea = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(linea)) break;
                var partes = linea.Split(',');
                if (partes.Length >= 2 && int.TryParse(partes[0], out int numCombo) && int.TryParse(partes[1], out int cant) && numCombo >= 1 && numCombo <= 9 && cant > 0)
                {
                    var combo = combos.FirstOrDefault(c => c.NumeroCombo == numCombo);
                    string desc = combo?.Descripcion ?? $"Combo {numCombo}";
                    double precio = combo?.Precio ?? preciosFijos[numCombo];
                    items.Add(new ItemPedido { NumeroCombo = numCombo, Descripcion = desc, PrecioUnitario = precio, Cantidad = cant });
                }
            }
            if (items.Count == 0)
            {
                Console.WriteLine(" No se ingresaron items.");
                return;
            }
            var (ok, msg) = pedidoCtrl.RealizarPedido(ced, idRest, items, dist, esFeriado);
            Console.WriteLine(ok ? msg : $" Error: {msg}");
        }

        static void MarcarEntregado(PedidoController ctrl)
        {
            Console.Write(" Id pedido: ");
            if (!int.TryParse(Console.ReadLine(), out int idPed)) { Console.WriteLine(" Id no valido."); return; }
            Console.Write(" Id repartidor: ");
            if (!int.TryParse(Console.ReadLine(), out int idRep)) { Console.WriteLine(" Id no valido."); return; }
            Console.WriteLine(ctrl.MarcarEntregado(idPed, idRep));
        }

        static void MenuReportes(ReporteController reporteCtrl, ClienteController clienteCtrl, RestauranteController restCtrl, RepartidorController repartCtrl)
        {
            Console.WriteLine("\n--- REPORTES ---");
            Console.WriteLine(" e) Listado clientes ACTIVOS (id, cedula, nombre)");
            Console.WriteLine(" f) Listado clientes SUSPENDIDOS");
            Console.WriteLine(" g) Repartidores con 0 amonestaciones");
            Console.WriteLine(" h) Listado restaurantes (nombre, ced.jur, dir, tipo)");
            Console.WriteLine(" i) Restaurante con mas pedidos");
            Console.WriteLine(" j) Monto por restaurante");
            Console.WriteLine(" k) Total general");
            Console.WriteLine(" l) Restaurante con menos pedidos");
            Console.WriteLine(" m) Quejas por repartidor");
            Console.WriteLine(" n) Pedidos por cliente");
            Console.WriteLine(" o) Cliente con mas pedidos");
            Console.WriteLine(" p) Hora pico");
            Console.Write(" Opcion: ");
            switch (Console.ReadLine()?.Trim().ToLower())
            {
                case "e": clienteCtrl.MostrarActivos(); break;
                case "f": clienteCtrl.MostrarSuspendidos(); break;
                case "g": repartCtrl.MostrarRepartidoresConCeroAmonestaciones(); break;
                case "h": restCtrl.MostrarRestaurantes(); break;
                case "i": reporteCtrl.RestauranteConMasPedidos(); break;
                case "j": reporteCtrl.MontoPorRestaurante(); break;
                case "k": reporteCtrl.MontoTotalGeneral(); break;
                case "l": reporteCtrl.RestauranteConMenosPedidos(); break;
                case "m": reporteCtrl.QuejasPorRepartidor(); break;
                case "n": reporteCtrl.PedidosPorCliente(); break;
                case "o": reporteCtrl.ClienteConMasPedidos(); break;
                case "p": reporteCtrl.HoraPico(); break;
                default: Console.WriteLine(" Opcion no valida."); break;
            }
        }
    }
}
