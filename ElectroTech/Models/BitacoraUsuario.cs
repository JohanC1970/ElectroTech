using ElectroTech.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa un registro en la bitácora de acceso de usuarios.
    /// </summary>
    public class BitacoraUsuario : INotifyPropertyChanged
    {
        private int _idBitacora;
        private int _idUsuario;
        private TipoAccion _tipoAccion;  // 'E' = Entrada, 'S' = Salida
        private DateTime _fechaHora;
        private string _ipAcceso;

        // Propiedad de navegación
        private Usuario _usuario;

        /// <summary>
        /// Identificador único del registro de bitácora.
        /// </summary>
        public int IdBitacora
        {
            get { return _idBitacora; }
            set
            {
                if (_idBitacora != value)
                {
                    _idBitacora = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador del usuario que realizó la acción.
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
        /// Tipo de acción registrada (E: Entrada, S: Salida).
        /// </summary>
        public TipoAccion TipoAccion
        {
            get { return _tipoAccion; }
            set
            {
                if (_tipoAccion != value)
                {
                    _tipoAccion = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(_tipoAccion));
                }
            }
        }

        /// <summary>
        /// Fecha y hora del registro.
        /// </summary>
        public DateTime FechaHora
        {
            get { return _fechaHora; }
            set
            {
                if (_fechaHora != value)
                {
                    _fechaHora = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Dirección IP desde la que se accedió.
        /// </summary>
        public string IpAcceso
        {
            get { return _ipAcceso; }
            set
            {
                if (_ipAcceso != value)
                {
                    _ipAcceso = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Usuario asociado al registro de bitácora (propiedad de navegación).
        /// </summary>
        public Usuario Usuario
        {
            get { return _usuario; }
            set
            {
                if (_usuario != value)
                {
                    _usuario = value;
                    if (_usuario != null)
                    {
                        IdUsuario = _usuario.IdUsuario;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public BitacoraUsuario()
        {
            FechaHora = DateTime.Now;
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
        /// Registra la entrada de un usuario en el sistema.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que ingresa.</param>
        /// <param name="ip">Dirección IP desde la que accede.</param>
        public void RegistrarEntrada(int idUsuario, string ip)
        {
            IdUsuario = idUsuario;
            IpAcceso = ip;
            TipoAccion = TipoAccion.Entrada;
            FechaHora = DateTime.Now;
        }

        /// <summary>
        /// Registra la salida de un usuario del sistema.
        /// </summary>
        /// <param name="idUsuario">ID del usuario que sale.</param>
        public void RegistrarSalida(int idUsuario)
        {
            IdUsuario = idUsuario;
            TipoAccion = TipoAccion.Salida;
            FechaHora = DateTime.Now;
        }

        /// <summary>
        /// Obtiene los registros de bitácora para un usuario específico.
        /// </summary>
        /// <param name="idUsuario">ID del usuario.</param>
        /// <returns>Lista de registros de bitácora del usuario.</returns>
        public static List<BitacoraUsuario> ObtenerRegistros(int idUsuario)
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
            return $"{FechaHora:dd/MM/yyyy HH:mm:ss} - {TipoAccion} - Usuario ID: {IdUsuario} - IP: {IpAcceso}";
        }
    }
}