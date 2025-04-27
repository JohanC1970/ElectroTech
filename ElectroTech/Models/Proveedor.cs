using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un proveedor en el sistema.
    /// </summary>
    public class Proveedor : INotifyPropertyChanged
    {
        private int _idProveedor;
        private string _nombre;
        private string _direccion;
        private string _telefono;
        private string _correo;
        private string _contacto;
        private string _condicionesPago;
        private bool _activo;

        /// <summary>
        /// Identificador único del proveedor.
        /// </summary>
        public int IdProveedor
        {
            get { return _idProveedor; }
            set
            {
                if (_idProveedor != value)
                {
                    _idProveedor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre o razón social del proveedor.
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
        /// Dirección del proveedor.
        /// </summary>
        public string Direccion
        {
            get { return _direccion; }
            set
            {
                if (_direccion != value)
                {
                    _direccion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Número de teléfono del proveedor.
        /// </summary>
        public string Telefono
        {
            get { return _telefono; }
            set
            {
                if (_telefono != value)
                {
                    _telefono = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Correo electrónico del proveedor.
        /// </summary>
        public string Correo
        {
            get { return _correo; }
            set
            {
                if (_correo != value)
                {
                    _correo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre de la persona de contacto.
        /// </summary>
        public string Contacto
        {
            get { return _contacto; }
            set
            {
                if (_contacto != value)
                {
                    _contacto = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Condiciones de pago acordadas con el proveedor.
        /// </summary>
        public string CondicionesPago
        {
            get { return _condicionesPago; }
            set
            {
                if (_condicionesPago != value)
                {
                    _condicionesPago = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica si el proveedor está activo.
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
        /// Constructor por defecto.
        /// </summary>
        public Proveedor()
        {
            Activo = true;
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
        /// Obtiene los productos suministrados por este proveedor.
        /// </summary>
        /// <returns>Lista de productos del proveedor.</returns>
        public List<Producto> ObtenerProductos()
        {
            // Esta funcionalidad se implementará en el repositorio o servicio correspondiente
            return null;
        }

        /// <summary>
        /// Calcula el tiempo promedio de entrega en días.
        /// </summary>
        /// <returns>Tiempo promedio de entrega en días.</returns>
        public int CalcularTiempoEntregaPromedio()
        {
            // Esta funcionalidad se implementará en el repositorio o servicio correspondiente
            return 0;
        }

        /// <summary>
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return Nombre;
        }
    }
}