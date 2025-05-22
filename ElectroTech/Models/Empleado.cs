using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un empleado en el sistema.
    /// </summary>
    public class Empleado : INotifyPropertyChanged
    {
        private int _idEmpleado;
        private string _tipoDocumento;
        private string _numeroDocumento;
        private string _nombre;
        private string _apellido;
        private string _direccion;
        private string _telefono;
        private DateTime _fechaContratacion;
        private double _salarioBase;
        private int _idUsuario;
        private bool _activo;

        /// <summary>
        /// Identificador único del empleado.
        /// </summary>
        public int IdEmpleado
        {
            get { return _idEmpleado; }
            set
            {
                if (_idEmpleado != value)
                {
                    _idEmpleado = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Tipo de documento de identidad (DNI, CE, etc.).
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
        /// Nombre del empleado.
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
        /// Apellido del empleado.
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
        /// Dirección del empleado.
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
        /// Número de teléfono del empleado.
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
        /// Fecha de contratación del empleado.
        /// </summary>
        public DateTime FechaContratacion
        {
            get { return _fechaContratacion; }
            set
            {
                if (_fechaContratacion != value)
                {
                    _fechaContratacion = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(AntiguedadEnAnios));
                }
            }
        }

        /// <summary>
        /// Salario base del empleado.
        /// </summary>
        public double SalarioBase
        {
            get { return _salarioBase; }
            set
            {
                if (_salarioBase != value)
                {
                    _salarioBase = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// ID del usuario asociado al empleado en el sistema.
        /// </summary>
        public int IdUsuario
        {
            get { return _idUsuario; }
            set
            {
                if (_idUsuario != value)
                {
                    _idUsuario = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Indica si el empleado está activo.
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
        /// Nombre completo del empleado (calculado).
        /// </summary>
        public string NombreCompleto
        {
            get { return $"{Nombre} {Apellido}".Trim(); }
        }

        /// <summary>
        /// Antigüedad del empleado en años (calculado).
        /// </summary>
        public int AntiguedadEnAnios
        {
            get
            {
                DateTime hoy = DateTime.Today;
                int años = hoy.Year - FechaContratacion.Year;
                if (FechaContratacion.Date > hoy.AddYears(-años)) años--;
                return años;
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Empleado()
        {
            FechaContratacion = DateTime.Now;
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
        /// Calcula las comisiones del empleado en un período determinado.
        /// </summary>
        /// <param name="inicio">Fecha de inicio del período.</param>
        /// <param name="fin">Fecha de fin del período.</param>
        /// <returns>Monto total de comisiones.</returns>
        public double CalcularComisiones(DateTime inicio, DateTime fin)
        {
            // Esta funcionalidad se implementará en el repositorio o servicio correspondiente
            return 0;
        }

        /// <summary>
        /// Obtiene las ventas realizadas por el empleado.
        /// </summary>
        /// <returns>Lista de ventas realizadas por el empleado.</returns>
        public List<Venta> ObtenerVentasRealizadas()
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
            return NombreCompleto;
        }

    }
}