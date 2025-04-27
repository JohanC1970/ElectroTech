using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un detalle (línea) de una compra a proveedor.
    /// </summary>
    public class DetalleCompra : INotifyPropertyChanged
    {
        private int _idDetalleCompra;
        private int _idCompra;
        private int _idProducto;
        private int _cantidad;
        private double _precioUnitario;
        private double _subtotal;

        // Propiedades de navegación
        private Compra _compra;
        private Producto _producto;

        /// <summary>
        /// Identificador único del detalle de compra.
        /// </summary>
        public int IdDetalleCompra
        {
            get { return _idDetalleCompra; }
            set
            {
                if (_idDetalleCompra != value)
                {
                    _idDetalleCompra = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador de la compra a la que pertenece este detalle.
        /// </summary>
        public int IdCompra
        {
            get { return _idCompra; }
            set
            {
                if (_idCompra != value)
                {
                    _idCompra = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador del producto comprado.
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
        /// Cantidad de unidades compradas.
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
        /// Precio unitario de compra.
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
        /// Subtotal del detalle (precio unitario * cantidad).
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
        /// Compra a la que pertenece este detalle (propiedad de navegación).
        /// </summary>
        public Compra Compra
        {
            get { return _compra; }
            set
            {
                if (_compra != value)
                {
                    _compra = value;
                    if (_compra != null)
                    {
                        IdCompra = _compra.IdCompra;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Producto comprado (propiedad de navegación).
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
                            PrecioUnitario = (double)_producto.PrecioCompra;
                        }
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public DetalleCompra()
        {
            Cantidad = 1;
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
            Subtotal = Cantidad * PrecioUnitario;
        }

        
    }
}