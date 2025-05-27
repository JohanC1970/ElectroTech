using ElectroTech.Helpers;
using ElectroTech.Models; // Asegúrate que Models.Enums esté accesible o usa el using completo
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    public class BitacoraRepository : DatabaseRepository
    {
        public List<BitacoraUsuario> ObtenerRegistros(DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var registros = new List<BitacoraUsuario>();
            try
            {
                string query = @"
                    SELECT b.idBitacora, b.idUsuario, b.tipoAccion, b.fechaHora, b.ipAcceso,
                           u.nombreUsuario, u.nombreCompleto AS nombreCompletoUsuario
                    FROM BitacoraUsuario b
                    JOIN Usuario u ON b.idUsuario = u.idUsuario";

                var parameters = new Dictionary<string, object>();
                List<string> conditions = new List<string>();

                if (fechaInicio.HasValue)
                {
                    conditions.Add("TRUNC(b.fechaHora) >= :fechaInicio");
                    parameters.Add(":fechaInicio", fechaInicio.Value.Date);
                }
                if (fechaFin.HasValue)
                {
                    conditions.Add("TRUNC(b.fechaHora) <= :fechaFin");
                    parameters.Add(":fechaFin", fechaFin.Value.Date);
                }

                if (conditions.Count > 0)
                {
                    query += " WHERE " + string.Join(" AND ", conditions);
                }

                query += " ORDER BY b.fechaHora DESC";

                DataTable dataTable = ExecuteQuery(query, parameters);

                foreach (DataRow row in dataTable.Rows)
                {
                    Models.Enums.TipoAccion tipoAccionEnum;
                    string tipoAccionChar = row["tipoAccion"].ToString();

                    // CORRECCIÓN AQUÍ: Mapear el char al enum
                    if (tipoAccionChar == "E")
                    {
                        tipoAccionEnum = Models.Enums.TipoAccion.Entrada;
                    }
                    else if (tipoAccionChar == "S")
                    {
                        tipoAccionEnum = Models.Enums.TipoAccion.Salida;
                    }
                    else
                    {
                        // Manejar un valor inesperado, quizás lanzar una excepción o un valor por defecto
                        Logger.LogWarning($"Valor inesperado para tipoAccion en Bitacora: '{tipoAccionChar}' con ID {row["idBitacora"]}"); //
                        // Podrías asignar un valor por defecto o seguir con una excepción si es crítico
                        tipoAccionEnum = default; // o lanzar una nueva excepción
                    }

                    var registro = new BitacoraUsuario
                    {
                        IdBitacora = Convert.ToInt32(row["idBitacora"]),
                        IdUsuario = Convert.ToInt32(row["idUsuario"]),
                        TipoAccion = tipoAccionEnum, // Usar el enum mapeado
                        FechaHora = Convert.ToDateTime(row["fechaHora"]),
                        IpAcceso = row["ipAcceso"]?.ToString(),
                        Usuario = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(row["idUsuario"]),
                            NombreUsuario = row["nombreUsuario"].ToString(),
                            NombreCompleto = row["nombreCompletoUsuario"].ToString()
                        }
                    };
                    registros.Add(registro);
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener registros de la bitácora"); //
                throw new Exception("Error al obtener registros de la bitácora.", ex);
            }
            finally
            {
                EnsureConnectionIsClosed();
            }
            return registros;
        }
    }
}