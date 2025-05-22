using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de métodos de pago.
    /// </summary>
    public class MetodoPagoService
    {
        private readonly MetodoPagoRepository _metodoPagoRepository;

        /// <summary>
        /// Constructor del servicio de métodos de pago.
        /// </summary>
        public MetodoPagoService()
        {
            _metodoPagoRepository = new MetodoPagoRepository();
        }

        /// <summary>
        /// Obtiene todos los métodos de pago activos.
        /// </summary>
        /// <returns>Lista de métodos de pago activos.</returns>
        public List<MetodoPago> ObtenerTodos()
        {
            try
            {
                // Simulamos la obtención de métodos de pago
                // En una implementación real, esto se obtendría de la base de datos
                List<MetodoPago> metodosPago = new List<MetodoPago>
                {
                    new MetodoPago { IdMetodoPago = 1, Nombre = "Efectivo", Descripcion = "Pago en efectivo", Activo = true },
                    new MetodoPago { IdMetodoPago = 2, Nombre = "Tarjeta de Crédito", Descripcion = "Pago con tarjeta de crédito", Activo = true },
                    new MetodoPago { IdMetodoPago = 3, Nombre = "Tarjeta de Débito", Descripcion = "Pago con tarjeta de débito", Activo = true },
                    new MetodoPago { IdMetodoPago = 4, Nombre = "Transferencia Bancaria", Descripcion = "Pago por transferencia bancaria", Activo = true }
                };

                return metodosPago;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todos los métodos de pago");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un método de pago por su ID.
        /// </summary>
        /// <param name="idMetodoPago">ID del método de pago.</param>
        /// <returns>El método de pago si se encuentra, null en caso contrario.</returns>
        public MetodoPago ObtenerPorId(int idMetodoPago)
        {
            try
            {
                // En una implementación real, esto se obtendría de la base de datos
                var metodosPago = ObtenerTodos();
                return metodosPago.Find(m => m.IdMetodoPago == idMetodoPago);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener método de pago con ID {idMetodoPago}");
                throw;
            }
        }
    }
}