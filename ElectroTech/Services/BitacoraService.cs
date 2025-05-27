using ElectroTech.DataAccess;
using ElectroTech.Models;
using ElectroTech.Helpers;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    public class BitacoraService
    {
        private readonly BitacoraRepository _bitacoraRepository;

        public BitacoraService()
        {
            _bitacoraRepository = new BitacoraRepository();
        }

        /// <summary>
        /// Obtiene los registros de la bitácora, opcionalmente filtrados por un rango de fechas.
        /// </summary>
        /// <param name="fechaInicio">Fecha de inicio para el filtro (opcional).</param>
        /// <param name="fechaFin">Fecha de fin para el filtro (opcional).</param>
        /// <returns>Una lista de objetos BitacoraUsuario.</returns>
        public List<BitacoraUsuario> ObtenerRegistros(DateTime? fechaInicio, DateTime? fechaFin)
        {
            try
            {
                // Si fechaFin tiene valor, ajustarlo para que incluya todo el día.
                DateTime? fechaFinAjustada = fechaFin.HasValue ? fechaFin.Value.Date.AddDays(1).AddTicks(-1) : (DateTime?)null;

                // Asegurar que la fecha de inicio no sea posterior a la fecha de fin si ambas están presentes
                if (fechaInicio.HasValue && fechaFinAjustada.HasValue && fechaInicio.Value.Date > fechaFinAjustada.Value.Date)
                {
                    // Opcional: lanzar una excepción o simplemente devolver una lista vacía o ajustar fechas.
                    // Por ahora, podríamos intercambiarlas o simplemente proceder, la BD podría manejarlo o devolver vacío.
                    // Para ser más amigable, podrías notificar al usuario o ajustar aquí.
                    // Por simplicidad, pasaremos las fechas como están y dejaremos que la consulta maneje la lógica de rango.
                }

                return _bitacoraRepository.ObtenerRegistros(fechaInicio?.Date, fechaFinAjustada);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error en BitacoraService al obtener registros."); //
                // Podrías devolver una lista vacía o relanzar la excepción dependiendo de cómo quieras manejar los errores en la UI.
                return new List<BitacoraUsuario>();
            }
        }
    }
}