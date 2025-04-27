using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ElectroTech.Models;

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
                string query = @"SELECT idUsuario, nombreUsuario, nivel, nombreCompleto, correo, estado, 
                                fechaCreacion, ultimaConexion 
                                FROM Usuario 
                                WHERE nombreUsuario = :nombreUsuario AND clave = :clave";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreUsuario", nombreUsuario },
                    { ":clave", clave }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    DataRow row = dataTable.Rows[0];
                    Usuario usuario = new Usuario
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

                    // Registrar la entrada en la bitácora
                    RegistrarAcceso(usuario.IdUsuario, 'E');

                    // Actualizar la última conexión
                    ActualizarUltimaConexion(usuario.IdUsuario);

                    return usuario;
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
        /// Crea un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="usuario">El usuario a crear.</param>
        /// <returns>ID del usuario creado.</returns>
        public int Crear(Usuario usuario)
        {
            try
            {
                // Verificar si ya existe un administrador si el nuevo usuario es nivel 1
                if (usuario.Nivel == 1 && ExisteAdministrador())
                {
                    throw new Exception("Solo puede existir un usuario administrador en el sistema.");
                }

                // Verificar si el nombre de usuario ya existe
                if (ExisteNombreUsuario(usuario.NombreUsuario))
                {
                    throw new Exception("El nombre de usuario ya existe en el sistema.");
                }

                BeginTransaction();

                int idUsuario = GetNextSequenceValue("SEQ_USUARIO");

                string query = @"INSERT INTO Usuario (idUsuario, nombreUsuario, clave, nivel, nombreCompleto, correo, estado, fechaCreacion) 
                               VALUES (:idUsuario, :nombreUsuario, :clave, :nivel, :nombreCompleto, :correo, :estado, SYSDATE)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idUsuario", idUsuario },
                    { ":nombreUsuario", usuario.NombreUsuario },
                    { ":clave", usuario.Clave },
                    { ":nivel", usuario.Nivel },
                    { ":nombreCompleto", usuario.NombreCompleto },
                    { ":correo", usuario.Correo },
                    { ":estado", usuario.Estado.ToString() }
                };

                ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return idUsuario;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw new Exception("Error al crear usuario.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="usuario">El usuario con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Usuario usuario)
        {
            try
            {
                // Verificar si ya existe un administrador si el usuario actualizado es nivel 1
                if (usuario.Nivel == 1)
                {
                    int idAdmin = ObtenerIdAdministrador();
                    if (idAdmin != -1 && idAdmin != usuario.IdUsuario)
                    {
                        throw new Exception("Solo puede existir un usuario administrador en el sistema.");
                    }
                }

                BeginTransaction();

                string query = @"UPDATE Usuario 
                               SET nombreCompleto = :nombreCompleto, 
                                   correo = :correo, 
                                   nivel = :nivel, 
                                   estado = :estado 
                               WHERE idUsuario = :idUsuario";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombreCompleto", usuario.NombreCompleto },
                    { ":correo", usuario.Correo },
                    { ":nivel", usuario.Nivel },
                    { ":estado", usuario.Estado.ToString() },
                    { ":idUsuario", usuario.IdUsuario }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                throw new Exception("Error al actualizar usuario.", ex);
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
    }
}