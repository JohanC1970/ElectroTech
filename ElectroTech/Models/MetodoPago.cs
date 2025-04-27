using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un método de pago en el sistema.
    /// </summary>
    public class MetodoPago : INotifyPropertyChanged
    {
        private int _idMetodoPago;
        private string _nombre;
        private string _descripcion;
        private bool _activo;

        /// <summary>
        /// Identificador único del método de pago.
        /// </summary>
        public int IdMetodoPago
        {
            get { return _idMetodoPago; }
            set
            {
                if (_idMetodoPago != value)
                {
                    _idMetodoPago = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre del método de pago.
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
        /// Descripción del método de pago.
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
        /// Indica si el método de pago está activo.
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
        public MetodoPago()
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
        /// Obtiene las ventas realizadas con este método de pago.
        /// </summary>
        /// <returns>Lista de ventas que utilizaron este método de pago.</returns>
        public List<Venta> ObtenerVentasPorMetodo()
        {
            // Esta funcionalidad se implementará en el repositorio o servicio correspondiente
            return null;
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