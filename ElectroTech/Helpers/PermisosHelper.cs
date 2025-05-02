using ElectroTech.Models;
using System;

namespace ElectroTech.Helpers
{
    /// <summary>
    /// Clase para manejar la verificación de permisos según el nivel de usuario.
    /// </summary>
    public static class PermisosHelper
    {
        /// <summary>
        /// Niveles de usuario en el sistema.
        /// </summary>
        public enum NivelUsuario
        {
            Administrador = 1,
            Parametrico = 2,
            Esporadico = 3
        }

        /// <summary>
        /// Módulos de la aplicación.
        /// </summary>
        public enum Modulo
        {
            // Entidades
            Productos,
            Categorias,
            Proveedores,
            Clientes,
            Empleados,

            // Transacciones
            Ventas,
            Compras,
            Devoluciones,
            Inventario,

            // Reportes
            ReporteVentas,
            ReporteInventario,
            ReporteProductos,
            ReporteClientes,

            // Configuración
            Usuarios,
            Bitacora,
            Configuracion
        }

        /// <summary>
        /// Verifica si un usuario tiene permisos para acceder a un módulo específico.
        /// </summary>
        /// <param name="usuario">Usuario a verificar.</param>
        /// <param name="modulo">Módulo a verificar.</param>
        /// <returns>True si tiene permiso, False en caso contrario.</returns>
        public static bool TienePermiso(Usuario usuario, Modulo modulo)
        {
            if (usuario == null || !usuario.EstaActivo)
            {
                return false;
            }

            // Administrador tiene acceso a todo
            if (usuario.Nivel == (int)NivelUsuario.Administrador)
            {
                return true;
            }

            // Usuario paramétrico tiene acceso a todo excepto configuración
            if (usuario.Nivel == (int)NivelUsuario.Parametrico)
            {
                return modulo != Modulo.Usuarios &&
                       modulo != Modulo.Bitacora &&
                       modulo != Modulo.Configuracion &&
                       modulo != Modulo.Empleados;
            }

            // Usuario esporádico solo tiene acceso a reportes
            if (usuario.Nivel == (int)NivelUsuario.Esporadico)
            {
                return modulo == Modulo.ReporteVentas ||
                       modulo == Modulo.ReporteInventario ||
                       modulo == Modulo.ReporteProductos ||
                       modulo == Modulo.ReporteClientes;
            }

            return false;
        }

        /// <summary>
        /// Verifica si un usuario tiene permiso para realizar acciones de escritura (crear, modificar, eliminar).
        /// </summary>
        /// <param name="usuario">Usuario a verificar.</param>
        /// <returns>True si tiene permiso, False en caso contrario.</returns>
        public static bool PuedeEscribir(Usuario usuario)
        {
            return usuario != null &&
                   usuario.EstaActivo &&
                   (usuario.Nivel == (int)NivelUsuario.Administrador ||
                    usuario.Nivel == (int)NivelUsuario.Parametrico);
        }

        /// <summary>
        /// Verifica si un usuario es administrador.
        /// </summary>
        /// <param name="usuario">Usuario a verificar.</param>
        /// <returns>True si es administrador, False en caso contrario.</returns>
        public static bool EsAdministrador(Usuario usuario)
        {
            return usuario != null &&
                   usuario.EstaActivo &&
                   usuario.Nivel == (int)NivelUsuario.Administrador;
        }
    }
}