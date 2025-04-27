using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un usuario del sistema.
    /// </summary>
    public class Usuario : INotifyPropertyChanged
    {
        private int _idUsuario;
        private string _nombreUsuario;
        private string _clave;
        private int _nivel;
        private string _nombreCompleto;
        private string _correo;
        private char _estado;
        private DateTime _fechaCreacion;
        private DateTime? _ultimaConexion;

        /// <summary>
        /// Identificador único del usuario.
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
        /// Nombre de usuario para autenticación.
        /// </summary>
        public string NombreUsuario
        {
            get { return _nombreUsuario; }
            set
            {
                if (_nombreUsuario != value)
                {
                    _nombreUsuario = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Contraseña del usuario (hash).
        /// </summary>
        public string Clave
        {
            get { return _clave; }
            set
            {
                if (_clave != value)
                {
                    _clave = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nivel de acceso del usuario (1: Administrador, 2: Paramétrico, 3: Esporádico).
        /// </summary>
        public int Nivel
        {
            get { return _nivel; }
            set
            {
                if (_nivel != value)
                {
                    _nivel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre completo del usuario.
        /// </summary>
        public string NombreCompleto
        {
            get { return _nombreCompleto; }
            set
            {
                if (_nombreCompleto != value)
                {
                    _nombreCompleto = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Correo electrónico del usuario.
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
        /// Estado del usuario (A: Activo, I: Inactivo).
        /// </summary>
        public char Estado
        {
            get { return _estado; }
            set
            {
                if (_estado != value)
                {
                    _estado = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fecha de creación del usuario.
        /// </summary>
        public DateTime FechaCreacion
        {
            get { return _fechaCreacion; }
            set
            {
                if (_fechaCreacion != value)
                {
                    _fechaCreacion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fecha y hora de la última conexión del usuario.
        /// </summary>
        public DateTime? UltimaConexion
        {
            get { return _ultimaConexion; }
            set
            {
                if (_ultimaConexion != value)
                {
                    _ultimaConexion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Propiedad calculada que devuelve el nombre del nivel de usuario.
        /// </summary>
        public string NombreNivel
        {
            get
            {
                switch (Nivel)
                {
                    case 1:
                        return "Administrador";
                    case 2:
                        return "Paramétrico";
                    case 3:
                        return "Esporádico";
                    default:
                        return "Desconocido";
                }
            }
        }

        /// <summary>
        /// Propiedad calculada que devuelve el nombre del estado del usuario.
        /// </summary>
        public string NombreEstado
        {
            get
            {
                return Estado == 'A' ? "Activo" : "Inactivo";
            }
        }

        /// <summary>
        /// Propiedad calculada que indica si el usuario es administrador.
        /// </summary>
        public bool EsAdministrador
        {
            get { return Nivel == 1; }
        }

        /// <summary>
        /// Propiedad calculada que indica si el usuario puede realizar modificaciones.
        /// </summary>
        public bool PuedeModificar
        {
            get { return Nivel <= 2; } // Administrador y Paramétrico
        }

        /// <summary>
        /// Propiedad calculada que indica si el usuario está activo.
        /// </summary>
        public bool EstaActivo
        {
            get { return Estado == 'A'; }
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
        /// Constructor por defecto.
        /// </summary>
        public Usuario()
        {
            FechaCreacion = DateTime.Now;
            Estado = 'A'; // Por defecto, activo
        }

        /// <summary>
        /// Método para verificar si el usuario tiene permiso para acceder a funcionalidad.
        /// </summary>
        /// <param name="nivelRequerido">Nivel mínimo requerido para acceder.</param>
        /// <returns>True si tiene permiso, False en caso contrario.</returns>
        public bool TienePermiso(int nivelRequerido)
        {
            // Los niveles son: 1-Administrador, 2-Paramétrico, 3-Esporádico
            // Menor número significa mayor nivel de acceso
            return Nivel <= nivelRequerido && EstaActivo;
        }

        /// <summary>
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return $"{NombreUsuario} - {NombreCompleto} ({NombreNivel})";
        }
    }
}