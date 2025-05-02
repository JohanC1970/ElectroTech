using ElectroTech.DataAccess;
using ElectroTech.Helpers;
using ElectroTech.Models;
using System;
using System.Collections.Generic;

namespace ElectroTech.Services
{
    /// <summary>
    /// Servicio para la gestión de categorías.
    /// </summary>
    public class CategoriaService
    {
        private readonly CategoriaRepository _categoriaRepository;

        /// <summary>
        /// Constructor del servicio de categorías.
        /// </summary>
        public CategoriaService()
        {
            _categoriaRepository = new CategoriaRepository();
        }

        /// <summary>
        /// Obtiene todas las categorías activas.
        /// </summary>
        /// <returns>Lista de categorías activas.</returns>
        public List<Categoria> ObtenerTodas()
        {
            try
            {
                return _categoriaRepository.ObtenerTodas();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error al obtener todas las categorías");
                throw;
            }
        }

        /// <summary>
        /// Obtiene una categoría por su ID.
        /// </summary>
        /// <param name="idCategoria">ID de la categoría.</param>
        /// <returns>La categoría si se encuentra, null en caso contrario.</returns>
        public Categoria ObtenerPorId(int idCategoria)
        {
            try
            {
                return _categoriaRepository.ObtenerPorId(idCategoria);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al obtener categoría con ID {idCategoria}");
                throw;
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
                return _categoriaRepository.Buscar(termino);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, $"Error al buscar categorías con término '{termino}'");
                throw;
            }
        }

        /// <summary>
        /// Crea una nueva categoría.
        /// </summary>
        /// <param name="categoria">Categoría a crear.</param>
        /// <param name="errorMessage">Mensaje de error si la creación falla.</param>
        /// <returns>ID de la categoría creada o -1 si falla.</returns>
        public int CrearCategoria(Categoria categoria, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la categoría
                if (!ValidarCategoria(categoria, out errorMessage))
                {
                    return -1;
                }

                // Crear la categoría
                int idCategoria = _categoriaRepository.Crear(categoria);
                Logger.LogInfo($"Categoría creada exitosamente: {categoria.Nombre} (ID: {idCategoria})");
                return idCategoria;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al crear categoría {categoria.Nombre}");
                return -1;
            }
        }

        /// <summary>
        /// Actualiza una categoría existente.
        /// </summary>
        /// <param name="categoria">Categoría con los datos actualizados.</param>
        /// <param name="errorMessage">Mensaje de error si la actualización falla.</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarCategoria(Categoria categoria, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Validar datos de la categoría
                if (!ValidarCategoria(categoria, out errorMessage))
                {
                    return false;
                }

                // Actualizar la categoría
                bool resultado = _categoriaRepository.Actualizar(categoria);

                if (resultado)
                {
                    Logger.LogInfo($"Categoría actualizada exitosamente: {categoria.Nombre} (ID: {categoria.IdCategoria})");
                }
                else
                {
                    errorMessage = "No se pudo actualizar la categoría en la base de datos.";
                    Logger.LogError($"Error al actualizar categoría {categoria.Nombre} (ID: {categoria.IdCategoria})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al actualizar categoría {categoria.Nombre} (ID: {categoria.IdCategoria})");
                return false;
            }
        }

        /// <summary>
        /// Elimina una categoría (marca como inactiva).
        /// </summary>
        /// <param name="idCategoria">ID de la categoría a eliminar.</param>
        /// <param name="errorMessage">Mensaje de error si la eliminación falla.</param>
        /// <returns>True si la eliminación es exitosa, False en caso contrario.</returns>
        public bool EliminarCategoria(int idCategoria, out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                // Obtener la categoría para registrar en el log
                Categoria categoria = _categoriaRepository.ObtenerPorId(idCategoria);

                if (categoria == null)
                {
                    errorMessage = "No se encontró la categoría especificada.";
                    return false;
                }

                // Eliminar la categoría
                bool resultado = _categoriaRepository.Eliminar(idCategoria);

                if (resultado)
                {
                    Logger.LogInfo($"Categoría eliminada (marcada como inactiva) exitosamente: {categoria.Nombre} (ID: {idCategoria})");
                }
                else
                {
                    errorMessage = "No se pudo eliminar la categoría.";
                    Logger.LogError($"Error al eliminar categoría {categoria.Nombre} (ID: {idCategoria})");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                Logger.LogException(ex, $"Error al eliminar categoría {idCategoria}");
                return false;
            }
        }

        /// <summary>
        /// Valida los datos de una categoría.
        /// </summary>
        /// <param name="categoria">Categoría a validar.</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla.</param>
        /// <returns>True si la categoría es válida, False en caso contrario.</returns>
        private bool ValidarCategoria(Categoria categoria, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validar nombre
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                errorMessage = "El nombre de la categoría es obligatorio.";
                return false;
            }

            // Validar longitud máxima del nombre
            if (categoria.Nombre.Length > 50)
            {
                errorMessage = "El nombre de la categoría no puede exceder los 50 caracteres.";
                return false;
            }

            // Validar longitud máxima de la descripción
            if (categoria.Descripcion != null && categoria.Descripcion.Length > 200)
            {
                errorMessage = "La descripción de la categoría no puede exceder los 200 caracteres.";
                return false;
            }

            return true;
        }
    }
}