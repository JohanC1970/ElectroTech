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

            // Configuración y Perfil
            MiPerfil, // Cambiado de Usuarios
            Bitacora
            // Se elimina Configuracion
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

            // Usuario paramétrico
            if (usuario.Nivel == (int)NivelUsuario.Parametrico)
            {
                switch (modulo)
                {
                    case Modulo.MiPerfil: // Todos los usuarios autenticados pueden ver su perfil
                    case Modulo.Productos:
                    case Modulo.Categorias:
                    case Modulo.Proveedores:
                    case Modulo.Clientes:
                    // case Modulo.Empleados: // Paramétrico no accede a Empleados
                    case Modulo.Ventas:
                    case Modulo.Compras:
                    case Modulo.Devoluciones:
                    case Modulo.Inventario:
                    case Modulo.ReporteVentas:
                    case Modulo.ReporteInventario:
                    case Modulo.ReporteProductos:
                    case Modulo.ReporteClientes:
                        return true;
                    case Modulo.Bitacora: // Bitácora solo para admin
                    case Modulo.Empleados: // Empleados solo para admin
                        return false;
                    default:
                        return false;
                }
            }

            // Usuario esporádico
            if (usuario.Nivel == (int)NivelUsuario.Esporadico)
            {
                switch (modulo)
                {
                    case Modulo.MiPerfil: // Todos los usuarios autenticados pueden ver su perfil
                    case Modulo.ReporteVentas:
                    case Modulo.ReporteInventario:
                    case Modulo.ReporteProductos:
                    case Modulo.ReporteClientes:
                        return true;
                    default: // Acceso denegado al resto
                        return false;
                }
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