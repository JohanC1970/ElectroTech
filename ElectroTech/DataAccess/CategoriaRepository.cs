using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace ElectroTech.DataAccess
{
    /// <summary>
    /// Repositorio para operaciones de base de datos relacionadas con la entidad Categoría.
    /// </summary>
    public class CategoriaRepository : DatabaseRepository
    {
        /// <summary>
        /// Obtiene todas las categorías activas.
        /// </summary>
        /// <returns>Lista de categorías activas.</returns>
        public List<Categoria> ObtenerTodas()
        {
            try
            {
                string query = @"
                    SELECT idCategoria, nombre, descripcion, activa
                    FROM Categoria
                    WHERE activa = 'S'
                    ORDER BY nombre";

                DataTable dataTable = ExecuteQuery(query);
                List<Categoria> categorias = new List<Categoria>();

                foreach (DataRow row in dataTable.Rows)
                {
                    categorias.Add(ConvertirDataRowACategoria(row));
                }

                return categorias;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las categorías");
                throw new Exception("Error al obtener categorías.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        /// <param name="idCategoria">ID de la categoría.</param>
        /// <returns>El objeto Categoria si se encuentra, null en caso contrario.</returns>
        public Categoria ObtenerPorId(int idCategoria)
        {
            try
            {
                string query = @"
                    SELECT idCategoria, nombre, descripcion, activa
                    FROM Categoria
                    WHERE idCategoria = :idCategoria";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCategoria", idCategoria }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);

                if (dataTable.Rows.Count > 0)
                {
                    return ConvertirDataRowACategoria(dataTable.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener categoría con ID {idCategoria}");
                throw new Exception("Error al obtener categoría por ID.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Busca categorías según un término de búsqueda.
        /// </summary>
        /// <param name="termino">Término de búsqueda.</param>
        /// <returns>Lista de categorías que coinciden con el término.</returns>
        public List<Categoria> Buscar(string termino)
        {
            try
            {
                string terminoBusqueda = $"%{termino}%";

                string query = @"
                    SELECT idCategoria, nombre, descripcion, activa
                    FROM Categoria
                    WHERE (UPPER(nombre) LIKE UPPER(:termino) OR 
                           UPPER(descripcion) LIKE UPPER(:termino))
                    AND activa = 'S'
                    ORDER BY nombre";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":termino", terminoBusqueda }
                };

                DataTable dataTable = ExecuteQuery(query, parameters);
                List<Categoria> categorias = new List<Categoria>();

                foreach (DataRow row in dataTable.Rows)
                {
                    categorias.Add(ConvertirDataRowACategoria(row));
                }

                return categorias;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar categorías con término '{termino}'");
                throw new Exception("Error al buscar categorías.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Crea una nueva categoría en la base de datos.
        /// </summary>
        /// <param name="categoria">Categoría a crear.</param>
        /// <returns>ID de la categoría creada.</returns>
        public int Crear(Categoria categoria)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe una categoría con el mismo nombre
                if (ExisteNombre(categoria.Nombre))
                {
                    throw new Exception("Ya existe una categoría con el nombre especificado.");
                }

                // Obtener el próximo ID de categoría
                int idCategoria = GetNextSequenceValue("SEQ_CATEGORIA");

                // Insertar la categoría
                string query = @"
                    INSERT INTO Categoria (idCategoria, nombre, descripcion, activa)
                    VALUES (:idCategoria, :nombre, :descripcion, :activa)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCategoria", idCategoria },
                    { ":nombre", categoria.Nombre },
                    { ":descripcion", categoria.Descripcion ?? (object)DBNull.Value },
                    { ":activa", categoria.Activa ? "S" : "N" }
                };

                ExecuteNonQuery(query, parameters);
                CommitTransaction();
                return idCategoria;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al crear categoría {categoria.Nombre}");
                throw new Exception("Error al crear categoría.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        /// <param name="categoria">Categoría con los datos actualizados.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool Actualizar(Categoria categoria)
        {
            try
            {
                BeginTransaction();

                // Verificar si ya existe otra categoría con el mismo nombre
                if (ExisteOtraCategoriaConNombre(categoria.Nombre, categoria.IdCategoria))
                {
                    throw new Exception("Ya existe otra categoría con el nombre especificado.");
                }

                string query = @"
                    UPDATE Categoria SET 
                        nombre = :nombre,
                        descripcion = :descripcion,
                        activa = :activa
                    WHERE idCategoria = :idCategoria";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", categoria.Nombre },
                    { ":descripcion", categoria.Descripcion ?? (object)DBNull.Value },
                    { ":activa", categoria.Activa ? "S" : "N" },
                    { ":idCategoria", categoria.IdCategoria }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                CommitTransaction();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Logger.LogException(ex, $"Error al actualizar categoría {categoria.Nombre}");
                throw new Exception("Error al actualizar categoría.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Elimina una categoría (marcándola como inactiva).
        /// </summary>
        /// <param name="idCategoria">ID de la categoría a eliminar.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool Eliminar(int idCategoria)
        {
            try
            {
                // Verificar si la categoría está siendo utilizada por productos
                if (TieneProductosAsociados(idCategoria))
                {
                    throw new Exception("No se puede eliminar la categoría porque tiene productos asociados.");
                }

                // En lugar de eliminar físicamente, marcamos como inactiva
                string query = "UPDATE Categoria SET activa = 'N' WHERE idCategoria = :idCategoria";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCategoria", idCategoria }
                };

                int rowsAffected = ExecuteNonQuery(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al eliminar categoría {idCategoria}");
                throw new Exception("Error al eliminar categoría.", ex);
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// Verifica si ya existe una categoría con el nombre especificado.
        /// </summary>
        /// <param name="nombre">Nombre a verificar.</param>
        /// <returns>True si ya existe, False en caso contrario.</returns>
        private bool ExisteNombre(string nombre)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Categoria WHERE UPPER(nombre) = UPPER(:nombre)";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", nombre }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre {nombre}");
                throw new Exception("Error al verificar existencia de nombre.", ex);
            }
        }

        /// <summary>
        /// Verifica si ya existe otra categoría con el mismo nombre.
        /// </summary>
        /// <param name="nombre">Nombre a verificar.</param>
        /// <param name="idCategoria">ID de la categoría actual (para excluirla de la verificación).</param>
        /// <returns>True si ya existe otra categoría con el mismo nombre, False en caso contrario.</returns>
        private bool ExisteOtraCategoriaConNombre(string nombre, int idCategoria)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Categoria WHERE UPPER(nombre) = UPPER(:nombre) AND idCategoria != :idCategoria";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":nombre", nombre },
                    { ":idCategoria", idCategoria }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar existencia de nombre en otra categoría {nombre}");
                throw new Exception("Error al verificar existencia de nombre en otra categoría.", ex);
            }
        }

        /// <summary>
        /// Verifica si la categoría tiene productos asociados.
        /// </summary>
        /// <param name="idCategoria">ID de la categoría a verificar.</param>
        /// <returns>True si tiene productos asociados, False en caso contrario.</returns>
        private bool TieneProductosAsociados(int idCategoria)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM Producto WHERE idCategoria = :idCategoria";

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { ":idCategoria", idCategoria }
                };

                object result = ExecuteScalar(query, parameters);
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al verificar si categoría {idCategoria} tiene productos asociados");
                throw new Exception("Error al verificar si categoría tiene productos asociados.", ex);
            }
        }

        /// <summary>
        /// Convierte un DataRow a un objeto Categoria.
        /// </summary>
        /// <param name="row">DataRow con los datos de la categoría.</param>
        /// <returns>Objeto Categoria con los datos del DataRow.</returns>
        private Categoria ConvertirDataRowACategoria(DataRow row)
        {
            var categoria = new Categoria
            {
                IdCategoria = Convert.ToInt32(row["idCategoria"]),
                Nombre = row["nombre"].ToString(),
                Descripcion = row["descripcion"] != DBNull.Value ? row["descripcion"].ToString() : null,
                Activa = row["activa"].ToString() == "S"
            };

            return categoria;
        }
    }
}