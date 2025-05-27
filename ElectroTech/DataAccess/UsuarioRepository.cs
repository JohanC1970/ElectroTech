using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ElectroTech.Helpers;
using ElectroTech.Models;
using Oracle.ManagedDataAccess.Client;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Usuario.
    /// </summary>
    public class UsuarioRepository : DatabaseRepository
    {
        /// <summary>
        /// Autentica a un usuario en el sistema.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario.</param>
        /// <param name="clave">Contraseña del usuario.</param>
        /// <returns>El objeto Usuario si la autenticación es exitosa, null en caso contrario.</returns>
        public Usuario Autenticar(string nombreUsuario, string clave)
        {
            try
            {
                // Obtener todos los usuarios con el nombre de usuario proporcionado
                string query = @"SELECT idUsuario, nombreUsuario, clave, nivel, nombreCompleto, correo, estado,   
                               fechaCreacion, ultimaConexion   
                         FROM Usuario   
                         WHERE nombreUsuario = :nombreUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { ":nombreUsuario", nombreUsuario }
        };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        // Imprimir las contraseñas
                        Console.WriteLine($"Contraseña en base de datos: {row["clave"]}");
                        Console.WriteLine($"Contraseña proporcionada: {clave}");


                        // Validar la contraseña

                        if (PasswordValidator.VerifyPassword(clave, row["clave"].ToString()))
                        //if (true)
                        {
                            Usuario usuario = new Usuario
                            {
                                IdUsuario = Convert.ToInt32(row["idUsuario"]),
                                NombreUsuario = row["nombreUsuario"].ToString(),
                                Nivel = Convert.ToInt32(row["nivel"]),
                                NombreCompleto = row["nombreCompleto"].ToString(),
                                Correo = row["correo"].ToString(),
                                Estado = row["estado"].ToString()[0],
                                FechaCreacion = row["fechaCreacion"] != DBNull.Value ? Convert.ToDateTime(row["fechaCreacion"]) : DateTime.MinValue,
                                UltimaConexion = row["ultimaConexion"] != DBNull.Value ? Convert.ToDateTime(row["ultimaConexion"]) : (DateTime?)null
                            };

                            // Registrar la entrada en la bitácora
                            RegistrarAcceso(usuario.IdUsuario, 'E');

                            // Actualizar la última conexión
                            ActualizarUltimaConexion(usuario.IdUsuario);

                            return usuario;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al autenticar usuario.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }


        /// <summary>
        /// Registra el acceso de un usuario en la bitácora.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <param name="tipoAccion">Tipo de acción ('E' para entrada, 'S' para salida).</param>
        /// <returns>True si el registro es exitoso, False en caso contrario.</returns>
        public bool RegistrarAcceso(int idUsuario, char tipoAccion)
        {
            try
            {
                string query = @"INSERT INTO BitacoraUsuario (idBitacora, idUsuario, tipoAccion, fechaHora, ipAcceso) 
                               VALUES (SEQ_BITACORA.NEXTVAL, :idUsuario, :tipoAccion, SYSDATE, :ipAcceso)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idUsuario", idUsuario },
                    { ":tipoAccion", tipoAccion.ToString() },
                    { ":ipAcceso", GetClientIPAddress() }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar acceso en bitácora.", ex);
            }
        }

        /// <summary>
        /// Actualiza la fecha de última conexión de un usuario.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        private bool ActualizarUltimaConexion(int idUsuario)
        {
            try
            {
                string query = "UPDATE Usuario SET ultimaConexion = SYSDATE WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idUsuario", idUsuario }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar última conexión del usuario.", ex);
            }
        }

        /// <summary>
        /// Obtiene la dirección IP del cliente.
        /// </summary>
        /// <returns>La dirección IP como string.</returns>
        private string GetClientIPAddress()
        {
            // En una aplicación WPF, normalmente sería la IP local
            return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                .AddressList.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString() ?? "127.0.0.1";
        }

        /// <summary>
        /// Obtiene todos los usuarios del sistema.
        /// </summary>
        /// <returns>Lista de usuarios.</returns>
        public List<Usuario> ObtenerTodos()
        {
            try
            {
                string query = @"SELECT idUsuario, nombreUsuario, nivel, nombreCompleto, correo, estado, 
                                fechaCreacion, ultimaConexion 
                                FROM Usuario";

                DataTable dataTable = ExecuteQuery(query);
                List<Usuario> usuarios = new List<Usuario>();

                foreach (DataRow row in dataTable.Rows)
                {
                    usuarios.Add(new Usuario
                    {
                        IdUsuario = Convert.ToInt32(row["idUsuario"]),
                        NombreUsuario = row["nombreUsuario"].ToString(),
                        Nivel = Convert.ToInt32(row["nivel"]),
                        NombreCompleto = row["nombreCompleto"].ToString(),
                        Correo = row["correo"].ToString(),
                        Estado = row["estado"].ToString()[0],
                        FechaCreacion = Convert.ToDateTime(row["fechaCreacion"]),
                        UltimaConexion = Convert.ToDateTime(row["ultimaConexion"])
                    });
                }

                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuarios.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns>El objeto Usuario si se encuentra, null en caso contrario.</returns>
        public Usuario ObtenerPorId(int idUsuario)
        {
            try
            {
                string query = @"SELECT idUsuario, nombreUsuario, nivel, nombreCompleto, correo, estado, 
                                fechaCreacion, ultimaConexion 
                                FROM Usuario 
                                WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idUsuario", idUsuario }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    return new Usuario
                    {
                        IdUsuario = Convert.ToInt32(row["idUsuario"]),
                        NombreUsuario = row["nombreUsuario"].ToString(),
                        Nivel = Convert.ToInt32(row["nivel"]),
                        NombreCompleto = row["nombreCompleto"].ToString(),
                        Correo = row["correo"].ToString(),
                        Estado = row["estado"].ToString()[0],
                        FechaCreacion = Convert.ToDateTime(row["fechaCreacion"]),
                        UltimaConexion = Convert.ToDateTime(row["ultimaConexion"])
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener usuario por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea un nuevo usuario en la base de datos.
        /// </summary>
        /// <param name="usuario">Usuario a crear.</param>
        /// <param name="hashContrasena">Hash de la contraseña.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>True si la creación es exitosa, False en caso contrario.</returns>
        public bool Crear(Usuario usuario, string hashContrasena, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                BeginTransaction();

                // Verificar si ya existe el nombre de usuario
                if (ExisteNombreUsuario(usuario.NombreUsuario))
                {
                    errorMessage = "El nombre de usuario ya existe.";
                    RollbackTransaction();
                    return false;
                }

                // Obtener el próximo ID de usuario
                int idUsuario = GetNextSequenceValue("SEQ_USUARIO");

                string query = @"
            INSERT INTO Usuario (
                idUsuario, 
                nombreUsuario, 
                clave, 
                nivel, 
                nombreCompleto, 
                correo, 
                estado, 
                fechaCreacion, 
                ultimaConexion
            ) VALUES (
                :idUsuario, 
                :nombreUsuario, 
                :clave, 
                :nivel, 
                :nombreCompleto, 
                :correo, 
                :estado, 
                :fechaCreacion, 
                NULL
            )";

                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { ":idUsuario", idUsuario },
            { ":nombreUsuario", usuario.NombreUsuario },
            { ":clave", hashContrasena },
            { ":nivel", usuario.Nivel },
            { ":nombreCompleto", usuario.NombreCompleto },
            { ":correo", usuario.Correo },
            { ":estado", usuario.Estado.ToString() },
            { ":fechaCreacion", DateTime.Now }
        };

                int rowsAffected = ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    usuario.IdUsuario = idUsuario;
                    usuario.FechaCreacion = DateTime.Now;
                    CommitTransaction();
                    return true;
                }
                else
                {
                    errorMessage = "No se pudo crear el usuario en la base de datos.";
                    RollbackTransaction();
                    return false;
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear usuario {usuario.NombreUsuario}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un usuario existente sin cambiar su contraseña.
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        // En UsuarioRepository.cs
        public bool Actualizar(Usuario usuario, out string errorMessage)
        {
            errorMessage = string.Empty;
            // OpenConnection(); // La conexión se abrirá con BeginTransaction o ExecuteNonQuery si es necesario
            try
            {
                BeginTransaction();

                // Verificar si el nombre de usuario ya existe para otro usuario
                // Esta verificación debería usar ExecuteScalar, que ahora maneja su conexión si no hay transacción
                if (ExisteNombreUsuarioOtroUsuario(usuario.NombreUsuario, usuario.IdUsuario))
                {
                    errorMessage = "El nombre de usuario ya existe para otro usuario.";
                    RollbackTransaction(); // Importante hacer rollback antes de salir si la transacción se inició
                    return false;
                }

                string query = @"
            UPDATE Usuario SET 
                nombreUsuario = :nombreUsuario,
                nivel = :nivel,
                nombreCompleto = :nombreCompleto,
                correo = :correo,
                estado = :estado
            WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreUsuario", usuario.NombreUsuario },
                    { ":nivel", usuario.Nivel },
                    { ":nombreCompleto", usuario.NombreCompleto },
                    { ":correo", usuario.Correo },
                    { ":estado", usuario.Estado.ToString() },
                    { ":idUsuario", usuario.IdUsuario }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters); // Usa la transacción y conexión abiertas

                if (rowsAffected > 0)
                {
                    CommitTransaction();
                    return true;
                }
                else
                {
                    errorMessage = "No se encontró el usuario a actualizar o no se realizaron cambios.";
                    RollbackTransaction();
                    return false;
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction(); // Intenta el rollback
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar usuario con ID {usuario.IdUsuario}"); //
                return false;
            }
            finally
            {
                EnsureConnectionIsClosed(); // Este método se encargará de cerrar la conexión y limpiar la transacción.
            }
        }

        /// <summary>
        /// Actualiza un usuario incluyendo su contraseña.
        /// </summary>
        /// <param name="usuario">Usuario con los datos actualizados.</param>
        /// <param name="hashContrasena">Hash de la nueva contraseña.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarConContrasena(Usuario usuario, string hashContrasena, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                BeginTransaction();

                string query = @"
            UPDATE Usuario SET 
                nombreUsuario = :nombreUsuario,
                clave = :clave, 
                nivel = :nivel,
                nombreCompleto = :nombreCompleto,
                correo = :correo,
                estado = :estado
            WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { ":nombreUsuario", usuario.NombreUsuario },
            { ":clave", hashContrasena },
            { ":nivel", usuario.Nivel },
            { ":nombreCompleto", usuario.NombreCompleto },
            { ":correo", usuario.Correo },
            { ":estado", usuario.Estado.ToString() },
            { ":idUsuario", usuario.IdUsuario }
        };

                int rowsAffected = ExecuteNonQuery(query, parameters);

                if (rowsAffected > 0)
                {
                    CommitTransaction();
                    return true;
                }
                else
                {
                    RollbackTransaction();
                    errorMessage = "No se encontró el usuario a actualizar.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar usuario con ID {usuario.IdUsuario}");
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un nombre de usuario para otro usuario diferente.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar.</param>
        /// <param name="idUsuarioActual">ID del usuario actual (para excluirlo de la verificación).</param>
        /// <returns>True si el nombre de usuario ya existe para otro usuario, False en caso contrario.</returns>
        public bool ExisteNombreUsuarioOtroUsuario(string nombreUsuario, int idUsuarioActual)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Usuario WHERE nombreUsuario = :nombreUsuario AND idUsuario <> :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { ":nombreUsuario", nombreUsuario },
            { ":idUsuario", idUsuarioActual }
        };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre de usuario '{nombreUsuario}' para otro usuario");
                throw new Exception("Error al verificar existencia de nombre de usuario para otro usuario.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <param name="nuevaClave">Nueva contraseña.</param>
        /// <returns>True si el cambio es exitoso, False en caso contrario.</returns>
        public bool CambiarClave(int idUsuario, string nuevaClave)
        {
            try
            {
                string query = "UPDATE Usuario SET clave = :clave WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":clave", nuevaClave },
                    { ":idUsuario", idUsuario }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al cambiar clave de usuario.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe un usuario administrador en el sistema.
        /// </summary>
        /// <returns>True si existe un administrador, False en caso contrario.</returns>
        public bool ExisteAdministrador()
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Usuario WHERE nivel = 1";
                object result = ExecuteScalar(query);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar existencia de administrador.", ex);
            }
        }

        /// <summary>
        /// Obtiene el ID del usuario administrador (nivel 1).
        /// </summary>
        /// <returns>ID del administrador o -1 si no existe.</returns>
        public int ObtenerIdAdministrador()
        {
            try
            {
                string query = "SELECT idUsuario FROM Usuario WHERE nivel = 1";
                object result = ExecuteScalar(query);
                return result != null ? Convert.ToInt32(result) : -1;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener ID del administrador.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe un nombre de usuario en el sistema.
        /// </summary>
        /// <param name="nombreUsuario">Nombre de usuario a verificar.</param>
        /// <returns>True si el nombre de usuario ya existe, False en caso contrario.</returns>
        public bool ExisteNombreUsuario(string nombreUsuario)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Usuario WHERE nombreUsuario = :nombreUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreUsuario", nombreUsuario }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar existencia de nombre de usuario.", ex);
            }
        }

        public int CrearConTransaccion(Usuario usuario, string hashContrasena, OracleConnection connection, OracleTransaction transaction)
        {
            // Obtener SEQ
            int idUsuario;
            using (OracleCommand seqCmd = new OracleCommand("SELECT SEQ_USUARIO.NEXTVAL FROM DUAL", connection))
            {
                seqCmd.Transaction = transaction;
                idUsuario = Convert.ToInt32(seqCmd.ExecuteScalar());
            }

            usuario.IdUsuario = idUsuario;

            // Insertar
            string query = @"
                INSERT INTO Usuario (idUsuario, nombreUsuario, clave, nivel, nombreCompleto, correo, estado, fechaCreacion)
                VALUES (:idUsuario, :nombreUsuario, :clave, :nivel, :nombreCompleto, :correo, :estado, :fechaCreacion)";

            using (OracleCommand cmd = new OracleCommand(query, connection))
            {
                cmd.Transaction = transaction;
                cmd.Parameters.Add(":idUsuario", OracleDbType.Int32, usuario.IdUsuario, ParameterDirection.Input);
                cmd.Parameters.Add(":nombreUsuario", OracleDbType.Varchar2, usuario.NombreUsuario, ParameterDirection.Input);
                cmd.Parameters.Add(":clave", OracleDbType.Varchar2, hashContrasena, ParameterDirection.Input);
                cmd.Parameters.Add(":nivel", OracleDbType.Int32, usuario.Nivel, ParameterDirection.Input);
                cmd.Parameters.Add(":nombreCompleto", OracleDbType.Varchar2, usuario.NombreCompleto, ParameterDirection.Input);
                cmd.Parameters.Add(":correo", OracleDbType.Varchar2, usuario.Correo, ParameterDirection.Input);
                cmd.Parameters.Add(":estado", OracleDbType.Char, usuario.Estado.ToString(), ParameterDirection.Input);
                cmd.Parameters.Add(":fechaCreacion", OracleDbType.Date, usuario.FechaCreacion, ParameterDirection.Input);

                if (cmd.ExecuteNonQuery() == 0) throw new Exception("Error al insertar usuario.");
            }
            return idUsuario;
        }

    }

}