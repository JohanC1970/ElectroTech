using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un registro de inventario para un producto.
    /// </summary>
    public class Inventario : INotifyPropertyChanged
    {
        private int _idInventario;
        private int _idProducto;
        private int _cantidadDisponible;
        private DateTime _ultimaActualizacion;

        // Propiedad de navegación
        private Producto _producto;

        /// <summary>
        /// Identificador único del registro de inventario.
        /// </summary>
        public int IdInventario
        {
            get { return _idInventario; }
            set
            {
                if (_idInventario != value)
                {
                    _idInventario = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador del producto asociado a este registro de inventario.
        /// </summary>
        public int IdProducto
        {
            get { return _idProducto; }
            set
            {
                if (_idProducto != value)
                {
                    _idProducto = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Cantidad disponible del producto en inventario.
        /// </summary>
        public int CantidadDisponible
        {
            get { return _cantidadDisponible; }
            set
            {
                if (_cantidadDisponible != value)
                {
                    _cantidadDisponible = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(RequiereReposicion));
                    NotifyPropertyChanged(nameof(ValorInventario));
                }
            }
        }

        /// <summary>
        /// Fecha y hora de la última actualización del inventario.
        /// </summary>
        public DateTime UltimaActualizacion
        {
            get { return _ultimaActualizacion; }
            set
            {
                if (_ultimaActualizacion != value)
                {
                    _ultimaActualizacion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Producto asociado a este registro de inventario (propiedad de navegación).
        /// </summary>
        public Producto Producto
        {
            get { return _producto; }
            set
            {
                if (_producto != value)
                {
                    _producto = value;
                    if (_producto != null)
                    {
                        IdProducto = _producto.IdProducto;
                    }
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(RequiereReposicion));
                    NotifyPropertyChanged(nameof(ValorInventario));
                }
            }
        }

        /// <summary>
        /// Indica si el producto requiere reposición (cantidad disponible menor al stock mínimo).
        /// </summary>
        public bool RequiereReposicion
        {
            get
            {
                if (Producto == null)
                {
                    return false;
                }

                return CantidadDisponible < Producto.StockMinimo;
            }
        }

        /// <summary>
        /// Valor monetario del inventario (cantidad * precio de compra).
        /// </summary>
        public decimal ValorInventario
        {
            get
            {
                if (Producto == null)
                {
                    return 0;
                }

                return Producto.PrecioCompra * CantidadDisponible;
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Inventario()
        {
            CantidadDisponible = 0;
            UltimaActualizacion = DateTime.Now;
        }

        /// <summary>
        /// Evento que se dispara cuando una propiedad cambia su valor.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Método para notificar que una propiedad ha cambiado.
        /// </summary>
        /// <param name="propertyName">Nombre de la propiedad que ha cambiado.</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Actualiza el stock del producto.
        /// </summary>
        /// <param name="cantidad">Cantidad a agregar (positiva) o restar (negativa).</param>
        /// <param name="tipoMovimiento">Tipo de movimiento: E (Entrada), S (Salida).</param>
        /// <returns>True si la actualización es exitosa, False en caso contrario.</returns>
        public bool ActualizarStock(int cantidad, char tipoMovimiento)
        {
            if (tipoMovimiento == 'S' && CantidadDisponible < cantidad)
            {
                return false; // No hay suficiente stock para la salida
            }

            if (tipoMovimiento == 'E')
            {
                CantidadDisponible += cantidad;
            }
            else if (tipoMovimiento == 'S')
            {
                CantidadDisponible -= cantidad;
            }
            else
            {
                return false; // Tipo de movimiento no válido
            }

            UltimaActualizacion = DateTime.Now;
            return true;
        }

        /// <summary>
        /// Verifica si hay disponibilidad suficiente para una cantidad requerida.
        /// </summary>
        /// <param name="cantidad">Cantidad requerida.</param>
        /// <returns>True si hay suficiente disponibilidad, False en caso contrario.</returns>
        public bool VerificarDisponibilidad(int cantidad)
        {
            return CantidadDisponible >= cantidad;
        }

        /// <summary>
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return $"Inventario #{IdInventario} - Producto: {(Producto != null ? Producto.Nombre : IdProducto.ToString())} - Disponible: {CantidadDisponible}";
        }
    }
}