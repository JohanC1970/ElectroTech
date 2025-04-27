using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un detalle (línea) de una venta.
    /// </summary>
    public class DetalleVenta : INotifyPropertyChanged
    {
        private int _idDetalleVenta;
        private int _idVenta;
        private int _idProducto;
        private int _cantidad;
        private double _precioUnitario;
        private double _descuento;
        private double _subtotal;

        // Propiedades de navegación
        private Venta _venta;
        private Producto _producto;

        /// <summary>
        /// Identificador único del detalle de venta.
        /// </summary>
        public int IdDetalleVenta
        {
            get { return _idDetalleVenta; }
            set
            {
                if (_idDetalleVenta != value)
                {
                    _idDetalleVenta = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador de la venta a la que pertenece este detalle.
        /// </summary>
        public int IdVenta
        {
            get { return _idVenta; }
            set
            {
                if (_idVenta != value)
                {
                    _idVenta = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador del producto vendido.
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
        /// Cantidad de unidades vendidas.
        /// </summary>
        public int Cantidad
        {
            get { return _cantidad; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("La cantidad debe ser mayor que cero.");
                }

                if (_cantidad != value)
                {
                    _cantidad = value;
                    NotifyPropertyChanged();
                    CalcularSubtotal();
                }
            }
        }

        /// <summary>
        /// Precio unitario del producto al momento de la venta.
        /// </summary>
        public double PrecioUnitario
        {
            get { return _precioUnitario; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("El precio unitario no puede ser negativo.");
                }

                if (_precioUnitario != value)
                {
                    _precioUnitario = value;
                    NotifyPropertyChanged();
                    CalcularSubtotal();
                }
            }
        }

        /// <summary>
        /// Descuento aplicado a este detalle (monto).
        /// </summary>
        public double Descuento
        {
            get { return _descuento; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("El descuento no puede ser negativo.");
                }

                if (_descuento != value)
                {
                    _descuento = value;
                    NotifyPropertyChanged();
                    CalcularSubtotal();
                }
            }
        }

        /// <summary>
        /// Subtotal del detalle (precio unitario * cantidad - descuento).
        /// </summary>
        public double Subtotal
        {
            get { return _subtotal; }
            set
            {
                if (_subtotal != value)
                {
                    _subtotal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Venta a la que pertenece este detalle (propiedad de navegación).
        /// </summary>
        public Venta Venta
        {
            get { return _venta; }
            set
            {
                if (_venta != value)
                {
                    _venta = value;
                    if (_venta != null)
                    {
                        IdVenta = _venta.IdVenta;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Producto vendido (propiedad de navegación).
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
                        if (PrecioUnitario <= 0)
                        {
                            PrecioUnitario = (double)_producto.PrecioVenta;
                        }
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public DetalleVenta()
        {
            Cantidad = 1;
            Descuento = 0;
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
        /// Calcula el subtotal del detalle.
        /// </summary>
        private void CalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario - Descuento;
        }

       
        
    }
}