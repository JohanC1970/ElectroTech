using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de ventas.
    /// </summary>
    public class VentaService
    {
        private readonly VentaRepository _ventaRepository;
        private readonly ProductoService _productoService;

        /// <summary>
        /// Constructor del servicio de ventas.
        /// </summary>
        public VentaService()
        {
            _ventaRepository = new VentaRepository();
            _productoService = new ProductoService();
        }

        /// <summary>
        /// Obtiene todas las ventas.
        /// </summary>
        /// <returns>Lista de ventas.</returns>
        public List<Venta> ObtenerTodas()
        {
            try
            {
                return _ventaRepository.ObtenerTodas();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las ventas");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las ventas entre dos fechas.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio.</param>
        /// <param name="fechaFin">Fecha de fin.</param>
        /// <returns>Lista de ventas en el rango de fechas especificado.</returns>
        public List<Venta> ObtenerPorFechas(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                return _ventaRepository.ObtenerPorFechas(fechaInicio, fechaFin);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas entre {fechaInicio} y {fechaFin}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las ventas de un cliente específico.
        /// </summary>
        /// <param name="idCliente">ID del cliente.</param>
        /// <returns>Lista de ventas del cliente.</returns>
        public List<Venta> ObtenerPorCliente(int idCliente)
        {
            try
            {
                return _ventaRepository.ObtenerPorCliente(idCliente);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener ventas del cliente {idCliente}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una venta por su ID.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>La venta si existe, null en caso contrario.</returns>
        public Venta ObtenerPorId(int idVenta)
        {
            try
            {
                return _ventaRepository.ObtenerPorId(idVenta);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener venta {idVenta}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una venta por su número de factura.
        /// </summary>
        /// <param name="numeroFactura">Número de factura.</param>
        /// <returns>La venta si se encuentra, null en caso contrario.</returns>
        public Venta ObtenerPorNumeroFactura(string numeroFactura)
        {
            try
            {
                return _ventaRepository.ObtenerPorNumeroFactura(numeroFactura);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener venta con número de factura {numeroFactura}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene las ventas completadas (estado 'C').
        /// </summary>
        /// <returns>Lista de ventas completadas.</returns>
        public List<Venta> ObtenerVentasCompletadas()
        {
            try
            {
                return _ventaRepository.ObtenerVentasPorEstado('C');
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener ventas completadas");
                throw;
            }
        }

        /// <summary>
        /// Realiza una venta.
        /// </summary>
        /// <param name="venta">Venta a realizar.</param>
        /// <param name="errorMessage">Mensaje de error en caso de fallo.</param>
        /// <returns>ID de la venta creada, o -1 si falla.</returns>
        public int CrearVenta(Venta venta, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar la venta y sus detalles
                if (!ValidarVenta(venta, out errorMessage))
                {
                    return -1;
                }

                // Verificar stock disponible para cada detalle
                foreach (var detalle in venta.Detalles)
                {
                    var producto = _productoService.ObtenerPorId(detalle.IdProducto);
                    if (producto == null)
                    {
                        errorMessage = $"El producto con ID {detalle.IdProducto} no existe.";
                        return -1;
                    }

                    // Verificar stock disponible
                    if (producto.CantidadDisponible < detalle.Cantidad)
                    {
                        errorMessage = $"No hay suficiente stock del producto {producto.Nombre}. " +
                            $"Disponible: {producto.CantidadDisponible}, Solicitado: {detalle.Cantidad}.";
                        return -1;
                    }
                }

                // Calcular totales (por si no están calculados)
                venta.CalcularTotal();

                // Crear la venta en la base de datos
                int idVenta = _ventaRepository.Crear(venta);

                if (idVenta > 0)
                {
                    // Actualizar el stock de cada producto vendido
                    foreach (var detalle in venta.Detalles)
                    {
                        _productoService.ActualizarStock(detalle.IdProducto, detalle.Cantidad, 'S', out string stockErrorMessage);
                        if (!string.IsNullOrEmpty(stockErrorMessage))
                        {
                            Logger.LogError($"Error al actualizar stock para producto {detalle.IdProducto}: {stockErrorMessage}");
                        }
                    }

                    // Registrar comisión del empleado (2% sobre el total de venta)
                    double comision = venta.Total * 0.02;
                    _ventaRepository.RegistrarComision(venta.IdEmpleado, idVenta, comision);

                    Logger.LogInfo($"Venta {idVenta} creada exitosamente por el empleado {venta.IdEmpleado}");
                }

                return idVenta;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, "Error al crear venta");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza una venta existente.
        /// </summary>
        /// <param name="venta">Venta con datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error en caso de fallo.</param>
        /// <returns>True si se actualiza correctamente, False en caso contrario.</returns>
        public bool ActualizarVenta(Venta venta, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Solo se pueden modificar ventas en estado Pendiente
                if (venta.Estado != 'P')
                {
                    errorMessage = "Solo se pueden modificar ventas en estado Pendiente.";
                    return false;
                }

                // Validar la venta y sus detalles
                if (!ValidarVenta(venta, out errorMessage))
                {
                    return false;
                }

                // Obtener venta original para comparar cambios
                var ventaOriginal = _ventaRepository.ObtenerPorId(venta.IdVenta);
                if (ventaOriginal == null)
                {
                    errorMessage = "No se encontró la venta original.";
                    return false;
                }

                // Calcular cambios en detalles para actualizar stock
                var detallesOriginales = ventaOriginal.Detalles;
                var detallesNuevos = venta.Detalles;

                // Calcular totales
                venta.CalcularTotal();

                // Actualizar la venta en la base de datos
                bool resultado = _ventaRepository.Actualizar(venta);

                if (resultado)
                {
                    // Actualizar stock por cada detalle modificado
                    foreach (var detalleNuevo in detallesNuevos)
                    {
                        var detalleOriginal = detallesOriginales.FirstOrDefault(d => d.IdProducto == detalleNuevo.IdProducto);

                        if (detalleOriginal == null)
                        {
                            // Producto nuevo: restar stock
                            _productoService.ActualizarStock(detalleNuevo.IdProducto, detalleNuevo.Cantidad, 'S', out string stockErrorMessage);
                        }
                        else if (detalleNuevo.Cantidad > detalleOriginal.Cantidad)
                        {
                            // Incremento de cantidad: restar la diferencia
                            int diferencia = detalleNuevo.Cantidad - detalleOriginal.Cantidad;
                            _productoService.ActualizarStock(detalleNuevo.IdProducto, diferencia, 'S', out string stockErrorMessage);
                        }
                        else if (detalleNuevo.Cantidad < detalleOriginal.Cantidad)
                        {
                            // Decremento de cantidad: sumar la diferencia
                            int diferencia = detalleOriginal.Cantidad - detalleNuevo.Cantidad;
                            _productoService.ActualizarStock(detalleNuevo.IdProducto, diferencia, 'E', out string stockErrorMessage);
                        }
                    }

                    // Productos eliminados de la venta: restaurar stock
                    foreach (var detalleOriginal in detallesOriginales)
                    {
                        var seMantiene = detallesNuevos.Any(d => d.IdProducto == detalleOriginal.IdProducto);
                        if (!seMantiene)
                        {
                            // Producto eliminado: restaurar stock
                            _productoService.ActualizarStock(detalleOriginal.IdProducto, detalleOriginal.Cantidad, 'E', out string stockErrorMessage);
                        }
                    }

                    // Actualizar comisión si cambió el total
                    if (venta.Total != ventaOriginal.Total)
                    {
                        double comision = venta.Total * 0.02;
                        _ventaRepository.ActualizarComision(venta.IdEmpleado, venta.IdVenta, comision);
                    }

                    Logger.LogInfo($"Venta {venta.IdVenta} actualizada exitosamente");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar venta {venta.IdVenta}");
                return false;
            }
        }

        /// <summary>
        /// Completa una venta pendiente.
        /// </summary>
        /// <param name="idVenta">ID de la venta a completar.</param>
        /// <param name="errorMessage">Mensaje de error en caso de fallo.</param>
        /// <returns>True si se completa correctamente, False en caso contrario.</returns>
        public bool CompletarVenta(int idVenta, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener la venta
                var venta = _ventaRepository.ObtenerPorId(idVenta);
                if (venta == null)
                {
                    errorMessage = "No se encontró la venta especificada.";
                    return false;
                }

                // Verificar que esté en estado Pendiente
                if (venta.Estado != 'P')
                {
                    errorMessage = "Solo se pueden completar ventas en estado Pendiente.";
                    return false;
                }

                // Cambiar estado a Completada
                venta.Estado = 'C';

                // Actualizar en la base de datos
                bool resultado = _ventaRepository.ActualizarEstado(idVenta, 'C');

                if (resultado)
                {
                    Logger.LogInfo($"Venta {idVenta} completada exitosamente");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al completar venta {idVenta}");
                return false;
            }
        }

        /// <summary>
        /// Anula una venta.
        /// </summary>
        /// <param name="idVenta">ID de la venta a anular.</param>
        /// <param name="errorMessage">Mensaje de error en caso de fallo.</param>
        /// <returns>True si se anula correctamente, False en caso contrario.</returns>
        public bool AnularVenta(int idVenta, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener la venta
                var venta = _ventaRepository.ObtenerPorId(idVenta);
                if (venta == null)
                {
                    errorMessage = "No se encontró la venta especificada.";
                    return false;
                }

                // No se puede anular una venta ya anulada
                if (venta.Estado == 'A')
                {
                    errorMessage = "La venta ya ha sido anulada.";
                    return false;
                }

                // Cambiar estado a Anulada
                venta.Estado = 'A';

                // Actualizar en la base de datos
                bool resultado = _ventaRepository.ActualizarEstado(idVenta, 'A');

                if (resultado)
                {
                    // Restaurar stock de los productos
                    foreach (var detalle in venta.Detalles)
                    {
                        _productoService.ActualizarStock(detalle.IdProducto, detalle.Cantidad, 'E', out string stockErrorMessage);
                        if (!string.IsNullOrEmpty(stockErrorMessage))
                        {
                            Logger.LogError($"Error al restaurar stock para producto {detalle.IdProducto}: {stockErrorMessage}");
                        }
                    }

                    // Anular comisión del empleado
                    _ventaRepository.AnularComision(venta.IdEmpleado, idVenta);

                    Logger.LogInfo($"Venta {idVenta} anulada exitosamente");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al anular venta {idVenta}");
                return false;
            }
        }

        /// <summary>
        /// Genera el número de factura para una nueva venta.
        /// </summary>
        /// <returns>Número de factura en formato "F-YYYYMMDD-XXXX".</returns>
        public string GenerarNumeroFactura()
        {
            try
            {
                // Obtener el último número de secuencia
                int ultimaSecuencia = _ventaRepository.ObtenerUltimaSecuencia();

                // Generar número con formato F-YYYYMMDD-XXXX
                string fecha = DateTime.Now.ToString("yyyyMMdd");
                string secuencia = (ultimaSecuencia + 1).ToString("D4");

                return $"F-{fecha}-{secuencia}";
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al generar número de factura");
                // En caso de error, generar un número único basado en timestamp
                return $"F-{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            }
        }

        /// <summary>
        /// Genera una factura para una venta.
        /// </summary>
        /// <param name="idVenta">ID de la venta.</param>
        /// <returns>Contenido de la factura como string.</returns>
        public string GenerarFactura(int idVenta)
        {
            try
            {
                var venta = _ventaRepository.ObtenerPorId(idVenta);
                if (venta == null)
                {
                    return "No se encontró la venta especificada.";
                }

                // Generar factura (formato simple)
                var factura = new System.Text.StringBuilder();

                factura.AppendLine("=====================================================");
                factura.AppendLine("                    ELECTROTECH                     ");
                factura.AppendLine("                FACTURA DE VENTA                    ");
                factura.AppendLine("=====================================================");
                factura.AppendLine();
                factura.AppendLine($"Número de Factura: {venta.NumeroFactura}");
                factura.AppendLine($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}");
                factura.AppendLine();
                factura.AppendLine($"Cliente: {venta.Cliente?.NombreCompleto ?? "Cliente General"}");
                factura.AppendLine($"Documento: {venta.Cliente?.TipoDocumento ?? ""} {venta.Cliente?.NumeroDocumento ?? ""}");
                factura.AppendLine($"Vendedor: {venta.Empleado?.NombreCompleto ?? ""}");
                factura.AppendLine($"Método de Pago: {venta.MetodoPagoNombre ?? ""}");
                factura.AppendLine();
                factura.AppendLine("-----------------------------------------------------");
                factura.AppendLine("Producto                  Cant   Precio     Subtotal ");
                factura.AppendLine("-----------------------------------------------------");

                foreach (var detalle in venta.Detalles)
                {
                    string nombreProducto = detalle.Producto?.Nombre ?? $"Producto {detalle.IdProducto}";
                    if (nombreProducto.Length > 24) nombreProducto = nombreProducto.Substring(0, 21) + "...";

                    factura.AppendLine(string.Format("{0,-24} {1,5} {2,10:C} {3,10:C}",
                        nombreProducto,
                        detalle.Cantidad,
                        detalle.PrecioUnitario,
                        detalle.Subtotal));
                }

                factura.AppendLine("-----------------------------------------------------");
                factura.AppendLine($"Subtotal:                               {venta.Subtotal:C}");
                factura.AppendLine($"Descuento:                              {venta.Descuento:C}");
                factura.AppendLine($"Impuestos:                              {venta.Impuestos:C}");
                factura.AppendLine($"TOTAL:                                  {venta.Total:C}");
                factura.AppendLine("=====================================================");
                factura.AppendLine();
                factura.AppendLine("            Gracias por su compra!                   ");
                factura.AppendLine("            ElectroTech © 2025                       ");

                return factura.ToString();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al generar factura para venta {idVenta}");
                return "Error al generar la factura.";
            }
        }

        /// <summary>
        /// Obtiene el resumen de ventas para el dashboard.
        /// </summary>
        /// <returns>Total de ventas del mes actual.</returns>
        public double ObtenerTotalVentasMes()
        {
            try
            {
                DateTime inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                DateTime finMes = inicioMes.AddMonths(1).AddDays(-1);

                return _ventaRepository.ObtenerTotalVentas(inicioMes, finMes);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener total de ventas del mes");
                return 0;
            }
        }

        /// <summary>
        /// Valida una venta y sus detalles.
        /// </summary>
        /// <param name="venta">Venta a validar.</param>
        /// <param name="errorMessage">Mensaje de error en caso de que la validación falle.</param>
        /// <returns>True si la venta es válida, False en caso contrario.</returns>
        private bool ValidarVenta(Venta venta, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar cliente
            if (venta.IdCliente <= 0)
            {
                errorMessage = "Debe seleccionar un cliente para la venta.";
                return false;
            }

            // Validar empleado
            if (venta.IdEmpleado <= 0)
            {
                errorMessage = "Debe seleccionar un empleado para la venta.";
                return false;
            }

            // Validar método de pago
            if (venta.IdMetodoPago <= 0)
            {
                errorMessage = "Debe seleccionar un método de pago.";
                return false;
            }

            // Validar detalles de venta
            if (venta.Detalles == null || venta.Detalles.Count == 0)
            {
                errorMessage = "La venta debe tener al menos un producto.";
                return false;
            }

            // Validar cada detalle
            foreach (var detalle in venta.Detalles)
            {
                if (detalle.IdProducto <= 0)
                {
                    errorMessage = "Todos los detalles deben tener un producto seleccionado.";
                    return false;
                }

                if (detalle.Cantidad <= 0)
                {
                    errorMessage = "La cantidad de cada producto debe ser mayor que cero.";
                    return false;
                }

                if (detalle.PrecioUnitario <= 0)
                {
                    errorMessage = "El precio unitario de cada producto debe ser mayor que cero.";
                    return false;
                }

                // Validar que el descuento no sea mayor al precio total
                if (detalle.Descuento < 0 || detalle.Descuento > (detalle.PrecioUnitario * detalle.Cantidad))
                {
                    errorMessage = "El descuento no puede ser negativo ni mayor al precio total del producto.";
                    return false;
                }
            }

            // Validar descuento total (no más del 30% del subtotal)
            if (venta.Descuento < 0 || venta.Descuento > (venta.Subtotal * 0.3))
            {
                errorMessage = "El descuento total no puede ser negativo ni mayor al 30% del subtotal.";
                return false;
            }

            return true;
        }
    }
}