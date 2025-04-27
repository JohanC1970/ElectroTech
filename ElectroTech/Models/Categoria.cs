using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ElectroTech.Models
{
    /// <summary>
    /// Modelo que representa una categoría de productos.
    /// </summary>
    public class Categoria : INotifyPropertyChanged
    {
        private int _idCategoria;
        private string _nombre;
        private string _descripcion;
        private bool _activa;

        /// <summary>
        /// Identificador único de la categoría.
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
        /// Nombre de la categoría.
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
        /// Descripción de la categoría.
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
        /// Indica si la categoría está activa.
        /// </summary>
        public bool Activa
        {
            get { return _activa; }
            set
            {
                if (_activa != value)
                {
                    _activa = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Constructor por defecto.
        /// </summary>
        public Categoria()
        {
            Activa = true;
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
        /// Obtiene los productos de esta categoría.
        /// </summary>
        /// <returns>Lista de productos de la categoría.</returns>
        public List<Producto> ObtenerProductos()
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