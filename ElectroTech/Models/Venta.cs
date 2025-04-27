using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa una venta en el sistema.
    /// </summary>
    public class Venta : INotifyPropertyChanged
    {
        private int _idVenta;
        private string _numeroFactura;
        private DateTime _fecha;
        private int _idCliente;
        private int _idEmpleado;
        private int _idMetodoPago;
        private double _subtotal;
        private double _descuento;
        private double _impuestos;
        private double _total;
        private string _observaciones;
        private char _estado; // 'C' = Completada, 'A' = Anulada, 'P' = Pendiente

        // Propiedades de navegación
        private Cliente _cliente;
        private Empleado _empleado;
        private string _metodoPagoNombre;
        private List<DetalleVenta> _detalles;

        /// <summary>
        /// Identificador único de la venta.
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
        /// Número de factura o comprobante.
        /// </summary>
        public string NumeroFactura
        {
            get { return _numeroFactura; }
            set
            {
                if (_numeroFactura != value)
                {
                    _numeroFactura = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Fecha y hora de la venta.
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
        /// ID del cliente al que se realizó la venta.
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
        /// ID del empleado que registró la venta.
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
        /// ID del método de pago utilizado.
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
        /// Subtotal de la venta (antes de descuentos e impuestos).
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
                    // Recalcular el total cuando cambia el subtotal
                    RecalcularTotal();
                }
            }
        }

        /// <summary>
        /// Monto de descuento aplicado.
        /// </summary>
        public double Descuento
        {
            get { return _descuento; }
            set
            {
                // Validar que el descuento no supere el 30% del subtotal
                if (value > _subtotal * 0.3)
                {
                    throw new ArgumentException("El descuento no puede superar el 30% del subtotal.");
                }

                if (_descuento != value)
                {
                    _descuento = value;
                    NotifyPropertyChanged();
                    // Recalcular el total cuando cambia el descuento
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
                    // Recalcular el total cuando cambian los impuestos
                    RecalcularTotal();
                }
            }
        }

        /// <summary>
        /// Monto total de la venta.
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
        /// Observaciones o notas adicionales sobre la venta.
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
        /// Estado actual de la venta (C: Completada, A: Anulada, P: Pendiente).
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
        /// Cliente al que se realizó la venta (propiedad de navegación).
        /// </summary>
        public Cliente Cliente
        {
            get { return _cliente; }
            set
            {
                if (_cliente != value)
                {
                    _cliente = value;
                    if (_cliente != null)
                    {
                        IdCliente = _cliente.IdCliente;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Empleado que registró la venta (propiedad de navegación).
        /// </summary>
        public Empleado Empleado
        {
            get { return _empleado; }
            set
            {
                if (_empleado != value)
                {
                    _empleado = value;
                    if (_empleado != null)
                    {
                        IdEmpleado = _empleado.IdEmpleado;
                    }
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nombre del método de pago utilizado.
        /// </summary>
        public string MetodoPagoNombre
        {
            get { return _metodoPagoNombre; }
            set
            {
                if (_metodoPagoNombre != value)
                {
                    _metodoPagoNombre = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Detalles de la venta (productos vendidos).
        /// </summary>
        public List<DetalleVenta> Detalles
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
        /// Descripción textual del estado de la venta.
        /// </summary>
        public string EstadoDescripcion
        {
            get
            {
                switch (Estado)
                {
                    case 'C':
                        return "Completada";
                    case 'A':
                        return "Anulada";
                    case 'P':
                        return "Pendiente";
                    default:
                        return "Desconocido";
                }
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Venta()
        {
            Fecha = DateTime.Now;
            Estado = 'P'; // Pendiente por defecto
            Detalles = new List<DetalleVenta>();
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
        /// Recalcula el total de la venta.
        /// </summary>
        private void RecalcularTotal()
        {
            Total = Subtotal - Descuento + Impuestos;
        }

        /// <summary>
        /// Calcula el total de la venta a partir de los detalles.
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
        /// Genera el texto de la factura.
        /// </summary>
        /// <returns>Texto de la factura.</returns>
        public string GenerarFactura()
        {
            // Esta funcionalidad se implementará en el servicio correspondiente
            return null;
        }

        /// <summary>
        /// Anula la venta.
        /// </summary>
        /// <returns>True si la anulación es exitosa, False en caso contrario.</returns>
        public bool Anular()
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
            return $"Venta #{NumeroFactura} - {Fecha:dd/MM/yyyy} - {Total:C}";
        }
    }
}