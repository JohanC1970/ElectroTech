using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa una compra a proveedores en el sistema.
    /// </summary>
    public class Compra : INotifyPropertyChanged
    {
        private int _idCompra;
        private string _numeroOrden;
        private DateTime _fecha;
        private int _idProveedor;
        private double _subtotal;
        private double _impuestos;
        private double _total;
        private char _estado; // 'P' = Pendiente, 'R' = Recibida, 'C' = Cancelada
        private string _observaciones;

        // Propiedades de navegación
        private Proveedor _proveedor;
        private List<DetalleCompra> _detalles;

        /// <summary>
        /// Identificador único de la compra.
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
        /// Número de orden de compra.
        /// </summary>
        public string NumeroOrden
        {
            get { return _numeroOrden; }
            set
            {
                if (_numeroOrden != value)
                {
                    _numeroOrden = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fecha de la compra.
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
        /// ID del proveedor al que se realiza la compra.
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
        /// Subtotal de la compra (antes de impuestos).
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
                    RecalcularTotal();
                }
            }
        }

        /// <summary>
        /// Monto de impuestos aplicados.
        /// </summary>
        public double Impuestos
        {
            get { return _impuestos; }
            set
            {
                if (_impuestos != value)
                {
                    _impuestos = value;
                    NotifyPropertyChanged();
                    RecalcularTotal();
                }
            }
        }

        /// <summary>
        /// Monto total de la compra.
        /// </summary>
        public double Total
        {
            get { return _total; }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Estado actual de la compra (P: Pendiente, R: Recibida, C: Cancelada).
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
        /// Propiedad calculada: Indica si la compra está pendiente.
        /// </summary>
        public bool EstadoPendiente
        {
            get { return Estado == 'P'; }
        }

        /// <summary>
        /// Observaciones o notas adicionales sobre la compra.
        /// </summary>
        public string Observaciones
        {
            get { return _observaciones; }
            set
            {
                if (_observaciones != value)
                {
                    _observaciones = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Proveedor al que se realiza la compra (propiedad de navegación).
        /// </summary>
        public Proveedor Proveedor
        {
            get { return _proveedor; }
            set
            {
                if (_proveedor != value)
                {
                    _proveedor = value;
                    if (_proveedor != null)
                    {
                        IdProveedor = _proveedor.IdProveedor;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Detalles de la compra (productos comprados).
        /// </summary>
        public List<DetalleCompra> Detalles
        {
            get { return _detalles; }
            set
            {
                if (_detalles != value)
                {
                    _detalles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Descripción textual del estado de la compra.
        /// </summary>
        public string EstadoDescripcion
        {
            get
            {
                switch (Estado)
                {
                    case 'P':
                        return "Pendiente";
                    case 'R':
                        return "Recibida";
                    case 'C':
                        return "Cancelada";
                    default:
                        return "Desconocido";
                }
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Compra()
        {
            Fecha = DateTime.Now;
            Estado = 'P'; // Pendiente por defecto
            Detalles = new List<DetalleCompra>();
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
        /// Recalcula el total de la compra.
        /// </summary>
        private void RecalcularTotal()
        {
            Total = Subtotal + Impuestos;
        }

        /// <summary>
        /// Calcula el total de la compra a partir de los detalles.
        /// </summary>
        /// <returns>Total calculado.</returns>
        public double CalcularTotal()
        {
            if (Detalles == null || Detalles.Count == 0)
            {
                return 0;
            }

            Subtotal = 0;
            foreach (var detalle in Detalles)
            {
                Subtotal += detalle.Subtotal;
            }

            return Total;
        }

        /// <summary>
        /// Marca la compra como recibida.
        /// </summary>
        /// <returns>True si la operación es exitosa, False en caso contrario.</returns>
        public bool RecibirProductos()
        {
            // Esta funcionalidad se implementará en el servicio correspondiente
            return false;
        }

        /// <summary>
        /// Cancela la orden de compra.
        /// </summary>
        /// <returns>True si la cancelación es exitosa, False en caso contrario.</returns>
        public bool CancelarOrden()
        {
            // Esta funcionalidad se implementará en el servicio correspondiente
            return false;
        }

        /// <summary>
        /// Convierte el objeto a una cadena de texto.
        /// </summary>
        /// <returns>Representación en texto del objeto.</returns>
        public override string ToString()
        {
            return $"Orden #{NumeroOrden} - {Fecha:dd/MM/yyyy} - {EstadoDescripcion}";
        }
    }
}