using System;
using System.Collections.Generic;

namespace ProyectoFinal
{
    public enum TipoCliente { Normal, Silver, Gold, Black }

    public struct ProductoConfiteria
    {
        public string Nombre;
        public decimal Precio;
        public int Cantidad;

        public ProductoConfiteria(string nombre, decimal precio, int cantidad = 0)
        {
            Nombre = nombre;
            Precio = precio;
            Cantidad = cantidad;
        }

        public decimal Total => Precio * Cantidad;
    }

    public struct Reserva
    {
        public string NombreCliente;
        public TipoCliente TipoCliente;
        public bool EsVip;
        public string Pelicula;
        public string Horario;
        public int Fila;
        public int Columna;
        public List<ProductoConfiteria> PedidoConfiteria;
        public decimal TotalAPagar;
        public bool Activa;
    }

    public class Sala
    {
        public string Nombre { get; }
        public string Pelicula { get; }
        public List<string> Horarios { get; }
        public Reserva[,] Asientos;
        public const int FILAS = 8;
        public const int COLUMNAS = 8;

        public Sala(string nombre, string pelicula, List<string> horarios)
        {
            Nombre = nombre;
            Pelicula = pelicula;
            Horarios = horarios;
            Asientos = new Reserva[FILAS, COLUMNAS];
        }

        public void DibujarMapa()
        {
            Console.WriteLine("\n          PANTALLA");
            Console.WriteLine(new string('-', COLUMNAS * 4));
            Console.Write("   ");
            for (int c = 0; c < COLUMNAS; c++)
                Console.Write($"{c + 1,2} ");
            Console.WriteLine();

            for (int f = 0; f < FILAS; f++)
            {
                Console.Write($"{f + 1,2} |");
                for (int c = 0; c < COLUMNAS; c++)
                {
                    char marcador = 'O'; // libre
                    if (Asientos[f, c].Activa) marcador = 'X'; // ocupado
                    else if (f >= FILAS - 2) marcador = 'V';  // VIP asiento en últimas dos filas
                    Console.Write($"{marcador}  ");
                }
                Console.WriteLine();
            }
            Console.WriteLine("\nO=Libre, X=Ocupado, V=VIP");
        }
    }

    class Program
    {
        static List<Sala> salas = new List<Sala>();
        static List<Reserva> reservas = new List<Reserva>();

        static readonly Dictionary<TipoCliente, decimal> DescuentosPeliculas = new Dictionary<TipoCliente, decimal>
        {
            { TipoCliente.Normal, 0m },
            { TipoCliente.Silver, 0.05m },
            { TipoCliente.Gold, 0.10m },
            { TipoCliente.Black, 0.15m }
        };

        // Confitería común para todos
        static readonly List<ProductoConfiteria> ConfiteriaComun = new List<ProductoConfiteria>
        {
            new ProductoConfiteria("Canchita",5m),
            new ProductoConfiteria("Refresco",3.5m),
            new ProductoConfiteria("Chocolates",4m),
            new ProductoConfiteria("Gaseosa",3m),
            new ProductoConfiteria("Dulces",2.5m),
            new ProductoConfiteria("Helado",4.5m)
        };

        // Combos exclusivos por tipo de cliente
        static readonly Dictionary<TipoCliente, List<ProductoConfiteria>> CombosEspeciales = new Dictionary<TipoCliente, List<ProductoConfiteria>>
        {
            { TipoCliente.Normal, new List<ProductoConfiteria> { new ProductoConfiteria("Combo Normal: Canchita Pequeña + Refresco",8m) } },
            { TipoCliente.Silver, new List<ProductoConfiteria> { new ProductoConfiteria("Combo Silver: Canchita Mediana + Refresco + Dulces",12m) } },
            { TipoCliente.Gold, new List<ProductoConfiteria> { new ProductoConfiteria("Combo Gold: Canchita Grande + Refresco + Chocolates",16m) } },
            { TipoCliente.Black, new List<ProductoConfiteria> { new ProductoConfiteria("Combo Black: Canchita XL + Refresco XL + Chocolates + Helado",22m) } }
        };

        const decimal PrecioNormal = 12.5m;
        const decimal PrecioVip = 18m;

        static void Main()
        {
            InicializarSalas();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== SISTEMA DE RESERVAS DE CINE ===");
                Console.WriteLine("1. Reservar asiento");
                Console.WriteLine("2. Ver mapa de sala");
                Console.WriteLine("3. Ver reservas");
                Console.WriteLine("4. Editar reserva (agregar confitería)");
                Console.WriteLine("5. Cancelar reserva");
                Console.WriteLine("6. Salir");
                Console.Write("Elija una opción: ");

                string opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        RealizarReserva();
                        break;
                    case "2":
                        MostrarMapaSala();
                        break;
                    case "3":
                        MostrarReservas();
                        break;
                    case "4":
                        EditarReserva();
                        break;
                    case "5":
                        CancelarReserva();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Opción inválida.");
                        break;
                }
                Console.WriteLine("Presione cualquier tecla para continuar...");
                Console.ReadKey();
            }
        }

        static void InicializarSalas()
        {
            salas.Add(new Sala("Sala 1", "Lilo y Stich", new List<string> { "2:00PM", "4:20PM", "7:00PM" }));
            salas.Add(new Sala("Sala 2", "Misión Imposible", new List<string> { "3:00PM", "7:10PM", "10:10PM" }));
            salas.Add(new Sala("Sala 3", "Karate Kid: Leyendas", new List<string> { "2:00PM", "4:50PM", "8:00PM" }));
        }

        static int PedirNumero(string mensaje, int min, int max)
        {
            while (true)
            {
                Console.Write(mensaje);
                if (int.TryParse(Console.ReadLine(), out int num) && num >= min && num <= max)
                    return num;
                Console.WriteLine($"Por favor ingrese un número entre {min} y {max}.");
            }
        }

        static int PedirSala()
        {
            Console.WriteLine("\nSalas disponibles:");
            for (int i = 0; i < salas.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {salas[i].Nombre} - Película: {salas[i].Pelicula}");
            }
            return PedirNumero("Seleccione sala (número): ", 1, salas.Count) - 1;
        }

        static string PedirHorario(int salaIndex)
        {
            var horarios = salas[salaIndex].Horarios;
            Console.WriteLine($"\nHorarios para {salas[salaIndex].Nombre} - Película: {salas[salaIndex].Pelicula}:");
            for (int i = 0; i < horarios.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {horarios[i]}");
            }
            int sel = PedirNumero("Seleccione horario (número): ", 1, horarios.Count);
            return horarios[sel - 1];
        }

        static TipoCliente PedirTipoCliente()
        {
            Console.WriteLine("\nTipos de cliente:");
            foreach (var tipo in Enum.GetValues(typeof(TipoCliente)))
                Console.WriteLine($"{(int)tipo} - {tipo}");

            Console.Write("Seleccione tipo: ");
            if (int.TryParse(Console.ReadLine(), out int t) && Enum.IsDefined(typeof(TipoCliente), t))
                return (TipoCliente)t;

            Console.WriteLine("Tipo no válido, se asigna Normal.");
            return TipoCliente.Normal;
        }

        static void MostrarConfiteriaComun()
        {
            Console.WriteLine("\nProductos de confitería comunes:");
            for (int i = 0; i < ConfiteriaComun.Count; i++)
            {
                var p = ConfiteriaComun[i];
                Console.WriteLine($"{i + 1}. {p.Nombre} - Precio: {p.Precio:C2}");
            }
        }

        static void MostrarCombosExclusivos(TipoCliente tipoCliente)
        {
            Console.WriteLine($"\nCombos exclusivos para cliente {tipoCliente}:");
            var combos = CombosEspeciales[tipoCliente];
            for (int i = 0; i < combos.Count; i++)
            {
                var c = combos[i];
                Console.WriteLine($"{i + 1 + ConfiteriaComun.Count}. {c.Nombre} - Precio: {c.Precio:C2}");
            }
        }

        static List<ProductoConfiteria> PedirConfiteria(TipoCliente tipoCliente)
        {
            List<ProductoConfiteria> pedidos = new List<ProductoConfiteria>();

            MostrarConfiteriaComun();
            MostrarCombosExclusivos(tipoCliente);

            Console.WriteLine("\nIngrese los números separados por coma para seleccionar productos y combos (ejemplo: 1,3,7), o presione Enter para omitir:");
            string entrada = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(entrada))
            {
                string[] seleccionados = entrada.Split(',');

                foreach (string sel in seleccionados)
                {
                    if (int.TryParse(sel.Trim(), out int num))
                    {
                        if (num >= 1 && num <= ConfiteriaComun.Count)
                        {
                            var producto = ConfiteriaComun[num - 1];
                            Console.Write($"Ingrese cantidad para {producto.Nombre}: ");
                            if (int.TryParse(Console.ReadLine(), out int cant) && cant > 0)
                            {
                                pedidos.Add(new ProductoConfiteria(producto.Nombre, producto.Precio, cant));
                            }
                        }
                        else
                        {
                            int comboIndex = num - 1 - ConfiteriaComun.Count;
                            var combos = CombosEspeciales[tipoCliente];
                            if (comboIndex >= 0 && comboIndex < combos.Count)
                            {
                                var combo = combos[comboIndex];
                                Console.Write($"Ingrese cantidad para combo {combo.Nombre}: ");
                                if (int.TryParse(Console.ReadLine(), out int cant) && cant > 0)
                                {
                                    pedidos.Add(new ProductoConfiteria(combo.Nombre, combo.Precio, cant));
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Número {num} inválido para productos o combos.");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Entrada '{sel}' no válida.");
                    }
                }
            }

            return pedidos;
        }

        static void RealizarReserva()
        {
            Console.WriteLine("\n=== Realizar Reserva ===");
            Console.Write("Ingrese su nombre: ");
            string nombre = Console.ReadLine();

            TipoCliente tipoCliente = PedirTipoCliente();

            int salaIndex = PedirSala();
            string horario = PedirHorario(salaIndex);
            var sala = salas[salaIndex];

            sala.DibujarMapa();

            Console.WriteLine("\nSeleccione asiento:");
            int fila = PedirNumero("Fila (1-8): ", 1, Sala.FILAS) - 1;
            int columna = PedirNumero("Columna (1-8): ", 1, Sala.COLUMNAS) - 1;

            if (sala.Asientos[fila, columna].Activa)
            {
                Console.WriteLine("Asiento ocupado, elija otro.");
                return;
            }

            bool esVip = fila >= Sala.FILAS - 2;

            decimal precioBase = esVip ? PrecioVip : PrecioNormal;
            decimal descuento = DescuentosPeliculas[tipoCliente];
            decimal precioFinal = precioBase * (1 - descuento);

            Console.WriteLine($"Precio asiento: {precioBase:C2}");
            Console.WriteLine($"Descuento por tipo cliente ({tipoCliente}): {descuento * 100}%");
            Console.WriteLine($"Precio final asiento: {precioFinal:C2}");

            var pedidoConfiteria = PedirConfiteria(tipoCliente);

            decimal totalConfiteria = 0;
            foreach (var p in pedidoConfiteria)
                totalConfiteria += p.Total;

            decimal totalPagar = precioFinal + totalConfiteria;

            Console.WriteLine("\n=== Resumen de Reserva ===");
            Console.WriteLine($"Cliente: {nombre} ({tipoCliente}) - VIP: {esVip}");
            Console.WriteLine($"Pelicula: {sala.Pelicula} - Horario: {horario}");
            Console.WriteLine($"Asiento: F{fila + 1}C{columna + 1} - Precio Asiento: {precioFinal:C2}");
            if (pedidoConfiteria.Count > 0)
            {
                Console.WriteLine("\nPedido de confitería:");
                foreach (var p in pedidoConfiteria)
                {
                    Console.WriteLine($"- {p.Nombre} x{p.Cantidad} = {p.Total:C2}");
                }
            }
            else
            {
                Console.WriteLine("No se agregó confitería.");
            }
            Console.WriteLine("\nResumen de costos:");
            Console.WriteLine($"Precio Asiento: {precioFinal:C2}");
            if (pedidoConfiteria.Count > 0)
            {
                Console.WriteLine($"Total Confitería: {totalConfiteria:C2}");
            }
            else
            {
                Console.WriteLine("Total Confitería: 0.00");
            }
            Console.WriteLine($"\nTotal a pagar: {totalPagar:C2}");
            Console.Write("Confirmar reserva? (s/n): ");
            string confirmar = Console.ReadLine().ToLower();
            if (confirmar != "s") return;

            var reserva = new Reserva
            {
                NombreCliente = nombre,
                TipoCliente = tipoCliente,
                EsVip = esVip,
                Pelicula = sala.Pelicula,
                Horario = horario,
                Fila = fila,
                Columna = columna,
                PedidoConfiteria = pedidoConfiteria,
                TotalAPagar = totalPagar,
                Activa = true
            };

            reservas.Add(reserva);
            sala.Asientos[fila, columna] = reserva;

            Console.WriteLine("Reserva realizada con éxito.");
        }

        static void MostrarMapaSala()
        {
            int salaIndex = PedirSala();
            var sala = salas[salaIndex];
            Console.WriteLine($"\nMapa de {sala.Nombre} - Película: {sala.Pelicula}");
            sala.DibujarMapa();
        }

        static void MostrarReservas()
        {
            Console.WriteLine("\n=== Reservas Registradas ===");
            if (reservas.Count == 0)
            {
                Console.WriteLine("No hay reservas.");
                return;
            }

            int index = 1;
            foreach (var r in reservas)
            {
                if (!r.Activa) continue;
                Console.WriteLine($"{index++}. {r.NombreCliente} - Sala: {r.Pelicula} - Horario: {r.Horario} - Asiento: F{r.Fila + 1}C{r.Columna + 1} - Tipo: {r.TipoCliente} - Total: {r.TotalAPagar:C2}");
            }
        }

        static void EditarReserva()
        {
            MostrarReservas();
            if (reservas.Count == 0) return;

            Console.Write("Ingrese número de reserva a editar: ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num < 1 || num > reservas.Count)
            {
                Console.WriteLine("Número inválido.");
                return;
            }

            var reserva = reservas[num - 1];
            if (!reserva.Activa)
            {
                Console.WriteLine("Reserva ya cancelada.");
                return;
            }

            Console.WriteLine($"Editando reserva de {reserva.NombreCliente} en {reserva.Pelicula} a las {reserva.Horario}");
            Console.WriteLine("Agregar productos de confitería:");

            var nuevosPedidos = PedirConfiteria(reserva.TipoCliente);
            if (nuevosPedidos.Count == 0)
            {
                Console.WriteLine("No se agregaron productos.");
                return;
            }

            foreach (var p in nuevosPedidos)
            {
                var existente = reserva.PedidoConfiteria.FindIndex(x => x.Nombre == p.Nombre);
                if (existente >= 0)
                {
                    var viejo = reserva.PedidoConfiteria[existente];
                    viejo.Cantidad += p.Cantidad;
                    reserva.PedidoConfiteria[existente] = viejo;
                }
                else
                {
                    reserva.PedidoConfiteria.Add(p);
                }
            }

            decimal totalConfiteria = 0;
            foreach (var p in reserva.PedidoConfiteria)
                totalConfiteria += p.Total;

            decimal precioAsiento = reserva.EsVip ? PrecioVip : PrecioNormal;
            decimal descuento = DescuentosPeliculas[reserva.TipoCliente];
            decimal precioFinalAsiento = precioAsiento * (1 - descuento);

            reserva.TotalAPagar = precioFinalAsiento + totalConfiteria;

            reservas[num - 1] = reserva;

            var sala = salas.Find(s => s.Pelicula == reserva.Pelicula);
            if (sala != null)
            {
                sala.Asientos[reserva.Fila, reserva.Columna] = reserva;
            }

            Console.WriteLine("Reserva actualizada con confitería.");
        }

        static void CancelarReserva()
        {
            MostrarReservas();
            if (reservas.Count == 0) return;

            Console.Write("Ingrese número de reserva a cancelar: ");
            if (!int.TryParse(Console.ReadLine(), out int num) || num < 1 || num > reservas.Count)
            {
                Console.WriteLine("Número inválido.");
                return;
            }

            var reserva = reservas[num - 1];
            if (!reserva.Activa)
            {
                Console.WriteLine("Reserva ya cancelada.");
                return;
            }

            reserva.Activa = false;
            reservas[num - 1] = reserva;

            var sala = salas.Find(s => s.Pelicula == reserva.Pelicula);
            if (sala != null)
            {
                sala.Asientos[reserva.Fila, reserva.Columna] = new Reserva();
            }

            Console.WriteLine("Reserva cancelada exitosamente.");
        }
    }
}
