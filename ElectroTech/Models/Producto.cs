using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un producto en el sistema.
    /// </summary>
    public class Producto : INotifyPropertyChanged
    {
        private int _idProducto;
        private string _codigo;
        private string _nombre;
        private string _descripcion;
        private int _idCategoria;
        private int? _idMarca;
        private string _modelo;
        private decimal _precioCompra;
        private decimal _precioVenta;
        private int _stockMinimo;
        private string _ubicacionAlmacen;
        private bool _activo;
        private byte[] _imagenBytes;

        // Propiedades adicionales no almacenadas directamente en la tabla Producto
        private int _cantidadDisponible;
        private string _nombreCategoria;
        private DateTime? _ultimaActualizacion;

        /// <summary>
        /// Identificador único del producto.
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
        /// Código del producto (único).
        /// </summary>
        public string Codigo
        {
            get { return _codigo; }
            set
            {
                if (_codigo != value)
                {
                    _codigo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public string Nombre
        {
            get { return _nombre; }
            set
            {
                if (_nombre != value)
                {
                    _nombre = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Descripción detallada del producto.
        /// </summary>
        public string Descripcion
        {
            get { return _descripcion; }
            set
            {
                if (_descripcion != value)
                {
                    _descripcion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador de la categoría a la que pertenece el producto.
        /// </summary>
        public int IdCategoria
        {
            get { return _idCategoria; }
            set
            {
                if (_idCategoria != value)
                {
                    _idCategoria = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre de la categoría a la que pertenece el producto.
        /// </summary>
        public string NombreCategoria
        {
            get { return _nombreCategoria; }
            set
            {
                if (_nombreCategoria != value)
                {
                    _nombreCategoria = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador de la marca del producto (opcional).
        /// </summary>
        public int? IdMarca
        {
            get { return _idMarca; }
            set
            {
                if (_idMarca != value)
                {
                    _idMarca = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Modelo del producto.
        /// </summary>
        public string Modelo
        {
            get { return _modelo; }
            set
            {
                if (_modelo != value)
                {
                    _modelo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Precio de compra del producto.
        /// </summary>
        public decimal PrecioCompra
        {
            get { return _precioCompra; }
            set
            {
                if (_precioCompra != value)
                {
                    _precioCompra = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Margen));
                    NotifyPropertyChanged(nameof(PorcentajeMargen));
                }
            }
        }

        /// <summary>
        /// Precio de venta del producto.
        /// </summary>
        public decimal PrecioVenta
        {
            get { return _precioVenta; }
            set
            {
                if (_precioVenta != value)
                {
                    _precioVenta = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(Margen));
                    NotifyPropertyChanged(nameof(PorcentajeMargen));
                }
            }
        }

        /// <summary>
        /// Cantidad mínima que debe haber en stock (para alertas).
        /// </summary>
        public int StockMinimo
        {
            get { return _stockMinimo; }
            set
            {
                if (_stockMinimo != value)
                {
                    _stockMinimo = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(RequiereReposicion));
                }
            }
        }

        /// <summary>
        /// Ubicación física del producto en el almacén.
        /// </summary>
        public string UbicacionAlmacen
        {
            get { return _ubicacionAlmacen; }
            set
            {
                if (_ubicacionAlmacen != value)
                {
                    _ubicacionAlmacen = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica si el producto está activo.
        /// </summary>
        public bool Activo
        {
            get { return _activo; }
            set
            {
                if (_activo != value)
                {
                    _activo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Bytes de la imagen del producto.
        /// </summary>
        public byte[] ImagenBytes
        {
            get { return _imagenBytes; }
            set
            {
                if (_imagenBytes != value)
                {
                    _imagenBytes = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(TieneImagen));
                }
            }
        }

        /// <summary>
        /// Cantidad disponible en inventario.
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
        /// Fecha de la última actualización del inventario.
        /// </summary>
        public DateTime? UltimaActualizacion
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
        /// Propiedad calculada: Margen de ganancia (precio venta - precio compra).
        /// </summary>
        public decimal Margen
        {
            get { return PrecioVenta - PrecioCompra; }
        }

        /// <summary>
        /// Propiedad calculada: Porcentaje de margen de ganancia.
        /// </summary>
        public decimal PorcentajeMargen
        {
            get
            {
                if (PrecioCompra > 0)
                    return Math.Round((Margen / PrecioCompra) * 100, 2);
                return 0;
            }
        }

        /// <summary>
        /// Propiedad calculada: Indica si el producto requiere reposición.
        /// </summary>
        public bool RequiereReposicion
        {
            get { return CantidadDisponible < StockMinimo; }
        }

        /// <summary>
        /// Propiedad calculada: Valor del inventario para este producto.
        /// </summary>
        public decimal ValorInventario
        {
            get { return PrecioCompra * CantidadDisponible; }
        }

        /// <summary>
        /// Propiedad calculada: Indica si el producto tiene imagen.
        /// </summary>
        public bool TieneImagen
        {
            get { return ImagenBytes != null && ImagenBytes.Length > 0; }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Producto()
        {
            Activo = true;
            StockMinimo = 5;
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
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return $"{Codigo} - {Nombre}";
        }
    }
}