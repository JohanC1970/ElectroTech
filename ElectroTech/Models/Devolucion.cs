using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa una devolución de venta en el sistema.
    /// </summary>
    public class Devolucion : INotifyPropertyChanged
    {
        private int _idDevolucion;
        private int _idVenta;
        private DateTime _fecha;
        private string _motivo;
        private double _montoDevuelto;
        private char _estado; // 'P' = Procesada, 'R' = Rechazada

        // Propiedad de navegación
        private Venta _venta;

        /// <summary>
        /// Identificador único de la devolución.
        /// </summary>
        public int IdDevolucion
        {
            get { return _idDevolucion; }
            set
            {
                if (_idDevolucion != value)
                {
                    _idDevolucion = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Identificador de la venta asociada a esta devolución.
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
        /// Fecha de la devolución.
        /// </summary>
        public DateTime Fecha
        {
            get { return _fecha; }
            set
            {
                if (_fecha != value)
                {
                    _fecha = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Motivo de la devolución.
        /// </summary>
        public string Motivo
        {
            get { return _motivo; }
            set
            {
                if (_motivo != value)
                {
                    _motivo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Monto a devolver al cliente.
        /// </summary>
        public double MontoDevuelto
        {
            get { return _montoDevuelto; }
            set
            {
                if (_montoDevuelto != value)
                {
                    _montoDevuelto = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Estado de la devolución (P: Procesada, R: Rechazada).
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
                    NotifyPropertyChanged(nameof(EstadoDescripcion));
                }
            }
        }

        /// <summary>
        /// Venta asociada a esta devolución (propiedad de navegación).
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
        /// Descripción textual del estado de la devolución.
        /// </summary>
        public string EstadoDescripcion
        {
            get
            {
                switch (Estado)
                {
                    case 'P':
                        return "Procesada";
                    case 'R':
                        return "Rechazada";
                    default:
                        return "Desconocido";
                }
            }
        }

        /// <summary>
        /// Indica si la devolución está dentro del plazo permitido (30 días).
        /// </summary>
        public bool EstaDentroDePlazo
        {
            get
            {
                if (Venta == null)
                {
                    return false;
                }

                TimeSpan diferencia = Fecha - Venta.Fecha;
                return diferencia.TotalDays <= 30;
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Devolucion()
        {
            Fecha = DateTime.Now;
            Estado = 'P'; // Procesada por defecto
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
        /// Procesa la devolución.
        /// </summary>
        /// <returns>True si el procesamiento es exitoso, False en caso contrario.</returns>
        public bool ProcesarDevolucion()
        {
            // Esta funcionalidad se implementará en el servicio correspondiente
            return false;
        }

        /// <summary>
        /// Genera una nota de crédito para la devolución.
        /// </summary>
        /// <returns>Texto de la nota de crédito.</returns>
        public string GenerarNotaCredito()
        {
            // Esta funcionalidad se implementará en el servicio correspondiente
            return null;
        }

        /// <summary>
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return $"Devolución #{IdDevolucion} - Venta #{IdVenta} - {Fecha:dd/MM/yyyy} - {MontoDevuelto:C}";
        }
    }
}