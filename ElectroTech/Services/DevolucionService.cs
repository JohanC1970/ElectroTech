using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de devoluciones.
    /// </summary>
    public class DevolucionService
    {
        private readonly DevolucionRepository _devolucionRepository;
        private readonly VentaRepository _ventaRepository;
        private readonly InventarioRepository _inventarioRepository;

        /// <summary>
        /// Constructor del servicio de devoluciones.
        /// </summary>
        public DevolucionService()
        {
            _devolucionRepository = new DevolucionRepository();
            _ventaRepository = new VentaRepository();
            _inventarioRepository = new InventarioRepository();
        }

        /// <summary>
        /// Obtiene todas las devoluciones.
        /// </summary>
        /// <returns>Lista de devoluciones.</returns>
        public List<Devolucion> ObtenerTodas()
        {
            try
            {
                return _devolucionRepository.ObtenerTodas();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las devoluciones");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una devolución por su ID.
        /// </summary>
        /// <param name="idDevolucion">ID de la devolución.</param>
        /// <returns>La devolución si se encuentra, null en caso contrario.</returns>
        public Devolucion ObtenerPorId(int idDevolucion)
        {
            try
            {
                return _devolucionRepository.ObtenerPorId(idDevolucion);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener devolución con ID {idDevolucion}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las devoluciones de una venta específica.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>Lista de devoluciones de la venta.</returns>
        public List<Devolucion> ObtenerPorVenta(int idVenta)
        {
            try
            {
                return _devolucionRepository.ObtenerPorVenta(idVenta);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener devoluciones de la venta {idVenta}");
                throw;
            }
        }

        /// <summary>
        /// Verifica si una venta ya tiene una devolución asociada.
        /// </summary>
        /// <param name="idVenta">ID de la venta a verificar.</param>
        /// <returns>True si ya existe una devolución, False en caso contrario.</returns>
        public bool ExisteDevolucionParaVenta(int idVenta)
        {
            try
            {
                var devoluciones = _devolucionRepository.ObtenerPorVenta(idVenta);
                return devoluciones.Count > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de devolución para la venta {idVenta}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene los IDs de ventas que ya tienen devoluciones asociadas.
        /// </summary>
        /// <returns>Lista de IDs de ventas con devoluciones.</returns>
        public List<int> ObtenerVentasConDevolucion()
        {
            try
            {
                return _devolucionRepository.ObtenerVentasConDevolucion();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener ventas con devolución");
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva devolución.
        /// </summary>
        /// <param name="devolucion">Devolución a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID de la devolución creada o -1 si falla.</returns>
        public int CrearDevolucion(Devolucion devolucion, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la devolución
                if (!ValidarDevolucion(devolucion, out errorMessage))
                {
                    return -1;
                }

                // Establecer estado inicial "Pendiente"
                devolucion.Estado = 'P';

                // Crear la devolución
                int idDevolucion = _devolucionRepository.Crear(devolucion);
                Logger.LogInfo($"Devolución creada exitosamente para la venta {devolucion.IdVenta} (ID: {idDevolucion})");
                return idDevolucion;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear devolución para la venta {devolucion.IdVenta}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza una devolución existente.
        /// </summary>
        /// <param name="devolucion">Devolución con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarDevolucion(Devolucion devolucion, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la devolución
                if (!ValidarDevolucion(devolucion, out errorMessage))
                {
                    return false;
                }

                // Obtener devolución actual para verificar estado
                Devolucion devolucionActual = _devolucionRepository.ObtenerPorId(devolucion.IdDevolucion);

                if (devolucionActual == null)
                {
                    errorMessage = "No se encontró la devolución especificada.";
                    return false;
                }

                // Solo permitir actualizar devoluciones pendientes
                if (devolucionActual.Estado != 'P')
                {
                    errorMessage = "Solo se pueden actualizar devoluciones pendientes.";
                    return false;
                }

                // Actualizar la devolución
                bool resultado = _devolucionRepository.Actualizar(devolucion);

                if (resultado)
                {
                    Logger.LogInfo($"Devolución actualizada exitosamente: ID {devolucion.IdDevolucion}");
                }
                else
                {
                    errorMessage = "No se pudo actualizar la devolución en la base de datos.";
                    Logger.LogError($"Error al actualizar devolución ID {devolucion.IdDevolucion}");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar devolución ID {devolucion.IdDevolucion}");
                return false;
            }
        }

        /// <summary>
        /// Procesa una devolución (actualiza inventario y cambia estado).
        /// </summary>
        /// <param name="idDevolucion">ID de la devolución a procesar.</param>
        /// <param name="errorMessage">Mensaje de error si el procesamiento falla.</param>
        /// <returns>True si el procesamiento es exitoso, False en caso contrario.</returns>
        public bool ProcesarDevolucion(int idDevolucion, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener la devolución
                Devolucion devolucion = _devolucionRepository.ObtenerPorId(idDevolucion);

                if (devolucion == null)
                {
                    errorMessage = "No se encontró la devolución especificada.";
                    return false;
                }

                // Solo procesar devoluciones pendientes
                if (devolucion.Estado != 'P')
                {
                    errorMessage = "La devolución ya ha sido procesada o rechazada.";
                    return false;
                }

                // Obtener la venta relacionada
                Venta venta = _ventaRepository.ObtenerPorId(devolucion.IdVenta);

                if (venta == null)
                {
                    errorMessage = "No se encontró la venta asociada a la devolución.";
                    return false;
                }

                // Actualizar el inventario (devolver los productos)
                if (venta.Detalles != null && venta.Detalles.Count > 0)
                {
                    foreach (var detalle in venta.Detalles)
                    {
                        // Aumentar el stock para cada producto
                        _inventarioRepository.ActualizarStock(detalle.IdProducto, detalle.Cantidad, 'E');
                    }
                }

                // Cambiar estado de la devolución a "Procesada"
                devolucion.Estado = 'P'; // Procesada
                bool resultado = _devolucionRepository.Actualizar(devolucion);

                if (resultado)
                {
                    Logger.LogInfo($"Devolución procesada exitosamente: ID {idDevolucion}");
                }
                else
                {
                    errorMessage = "No se pudo actualizar el estado de la devolución.";
                    Logger.LogError($"Error al cambiar estado de devolución ID {idDevolucion}");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al procesar devolución ID {idDevolucion}");
                return false;
            }
        }

        /// <summary>
        /// Genera una nota de crédito para una devolución.
        /// </summary>
        /// <param name="idDevolucion">ID de la devolución.</param>
        /// <returns>Texto de la nota de crédito o cadena vacía si falla.</returns>
        public string GenerarNotaCredito(int idDevolucion)
        {
            try
            {
                // Obtener la devolución
                Devolucion devolucion = _devolucionRepository.ObtenerPorId(idDevolucion);

                if (devolucion == null)
                {
                    return string.Empty;
                }

                // Obtener la venta relacionada
                Venta venta = _ventaRepository.ObtenerPorId(devolucion.IdVenta);

                if (venta == null)
                {
                    return string.Empty;
                }

                // Crear el texto de la nota de crédito
                StringBuilder sb = new StringBuilder();

                // Encabezado
                sb.AppendLine("=================================================================");
                sb.AppendLine("                        NOTA DE CRÉDITO                          ");
                sb.AppendLine("=================================================================");
                sb.AppendLine();
                sb.AppendLine($"Número: NC{idDevolucion:D5}");
                sb.AppendLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine($"Venta relacionada: {venta.NumeroFactura}");
                sb.AppendLine();

                // Datos del cliente
                sb.AppendLine("DATOS DEL CLIENTE:");
                sb.AppendLine($"Nombre: {venta.Cliente?.NombreCompleto ?? "Cliente no disponible"}");
                sb.AppendLine($"Documento: {venta.Cliente?.TipoDocumento ?? ""} {venta.Cliente?.NumeroDocumento ?? ""}");
                sb.AppendLine();

                // Datos de la devolución
                sb.AppendLine("DATOS DE LA DEVOLUCIÓN:");
                sb.AppendLine($"Fecha de devolución: {devolucion.Fecha:dd/MM/yyyy}");
                sb.AppendLine($"Motivo: {devolucion.Motivo}");
                sb.AppendLine();

                // Productos devueltos
                sb.AppendLine("PRODUCTOS DEVUELTOS:");
                sb.AppendLine("------------------------------------------------------------------");
                sb.AppendLine("Código   Descripción                Cant.   Precio      Subtotal");
                sb.AppendLine("------------------------------------------------------------------");

                if (venta.Detalles != null && venta.Detalles.Count > 0)
                {
                    foreach (var detalle in venta.Detalles)
                    {
                        sb.AppendLine($"{detalle.Producto?.Codigo?.PadRight(9) ?? "".PadRight(9)}" +
                            $"{detalle.Producto?.Nombre?.Substring(0, Math.Min(28, (detalle.Producto?.Nombre?.Length ?? 0))).PadRight(28) ?? "".PadRight(28)}" +
                            $"{detalle.Cantidad.ToString().PadLeft(5)}   " +
                            $"{detalle.PrecioUnitario.ToString("0.00").PadLeft(10)}   " +
                            $"{detalle.Subtotal.ToString("0.00").PadLeft(10)}");
                    }
                }

                sb.AppendLine("------------------------------------------------------------------");
                sb.AppendLine($"{"IMPORTE TOTAL:".PadLeft(54)} {devolucion.MontoDevuelto.ToString("0.00").PadLeft(10)}");
                sb.AppendLine();

                // Pie de nota
                sb.AppendLine("=================================================================");
                sb.AppendLine("          Esta nota de crédito sirve como comprobante           ");
                sb.AppendLine("          de la devolución de los productos indicados           ");
                sb.AppendLine("=================================================================");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al generar nota de crédito para devolución ID {idDevolucion}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Valida los datos de una devolución.
        /// </summary>
        /// <param name="devolucion">Devolución a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si la devolución es válida, False en caso contrario.</returns>
        private bool ValidarDevolucion(Devolucion devolucion, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar ID de venta
            if (devolucion.IdVenta <= 0)
            {
                errorMessage = "El ID de venta es inválido.";
                return false;
            }

            // Validar fecha
            if (devolucion.Fecha == default(DateTime))
            {
                errorMessage = "La fecha de devolución es obligatoria.";
                return false;
            }

            // Validar motivo
            if (string.IsNullOrWhiteSpace(devolucion.Motivo))
            {
                errorMessage = "El motivo de la devolución es obligatorio.";
                return false;
            }

            // Validar monto
            if (devolucion.MontoDevuelto <= 0)
            {
                errorMessage = "El monto de devolución debe ser mayor que cero.";
                return false;
            }

            // Obtener la venta
            Venta venta = _ventaRepository.ObtenerPorId(devolucion.IdVenta);

            if (venta == null)
            {
                errorMessage = "No se encontró la venta asociada a la devolución.";
                return false;
            }

            // Validar que el monto no sea mayor al total de la venta
            if (devolucion.MontoDevuelto > venta.Total)
            {
                errorMessage = "El monto a devolver no puede ser mayor al total de la venta.";
                return false;
            }

            // Validar que la fecha de devolución no sea anterior a la fecha de venta
            if (devolucion.Fecha < venta.Fecha)
            {
                errorMessage = "La fecha de devolución no puede ser anterior a la fecha de venta.";
                return false;
            }

            // Validar que la fecha de devolución no sea posterior a la fecha actual
            if (devolucion.Fecha > DateTime.Now)
            {
                errorMessage = "La fecha de devolución no puede ser posterior a la fecha actual.";
                return false;
            }

            // Validar plazo de devolución (30 días)
            TimeSpan plazo = devolucion.Fecha - venta.Fecha;
            if (plazo.TotalDays > 30)
            {
                errorMessage = "No se pueden realizar devoluciones después de 30 días de la compra.";
                return false;
            }

            return true;
        }
    }
}