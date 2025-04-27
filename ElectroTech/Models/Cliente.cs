using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un cliente en el sistema.
    /// </summary>
    public class Cliente : INotifyPropertyChanged
    {
        private int _idCliente;
        private string _tipoDocumento;
        private string _numeroDocumento;
        private string _nombre;
        private string _apellido;
        private string _direccion;
        private string _telefono;
        private string _correo;
        private DateTime _fechaRegistro;
        private bool _activo;

        /// <summary>
        /// Identificador único del cliente.
        /// </summary>
        public int IdCliente
        {
            get { return _idCliente; }
            set
            {
                if (_idCliente != value)
                {
                    _idCliente = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tipo de documento de identidad (DNI, RUC, CE, etc.).
        /// </summary>
        public string TipoDocumento
        {
            get { return _tipoDocumento; }
            set
            {
                if (_tipoDocumento != value)
                {
                    _tipoDocumento = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Número de documento de identidad.
        /// </summary>
        public string NumeroDocumento
        {
            get { return _numeroDocumento; }
            set
            {
                if (_numeroDocumento != value)
                {
                    _numeroDocumento = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre del cliente.
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
                    NotifyPropertyChanged(nameof(NombreCompleto));
                }
            }
        }

        /// <summary>
        /// Apellido del cliente.
        /// </summary>
        public string Apellido
        {
            get { return _apellido; }
            set
            {
                if (_apellido != value)
                {
                    _apellido = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(NombreCompleto));
                }
            }
        }

        /// <summary>
        /// Dirección del cliente.
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
        /// Número de teléfono del cliente.
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
        /// Correo electrónico del cliente.
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
        /// Fecha de registro del cliente en el sistema.
        /// </summary>
        public DateTime FechaRegistro
        {
            get { return _fechaRegistro; }
            set
            {
                if (_fechaRegistro != value)
                {
                    _fechaRegistro = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica si el cliente está activo.
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
        /// Nombre completo del cliente (calculado).
        /// </summary>
        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellido}".Trim(); }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Cliente()
        {
            FechaRegistro = DateTime.Now;
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
        /// Obtiene el historial de compras del cliente.
        /// </summary>
        /// <returns>Lista de ventas realizadas al cliente.</returns>
        public List<Venta> ObtenerHistorialCompras()
        {
            // Esta funcionalidad se implementará en el repositorio o servicio correspondiente
            return null;
        }

        /// <summary>
        /// Calcula el total de compras realizadas por el cliente.
        /// </summary>
        /// <returns>Monto total de compras.</returns>
        public double CalcularTotalCompras()
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
            return NombreCompleto;
        }
    }
}