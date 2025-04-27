using System;
using System.Collections.Generic;

namespace ElectroTech.Models
{
    public class ReporteVentas
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public Dictionary<string, object> GenerarReportePeriodo()
        {
            // Generar un reporte completo de ventas para el período especificado
            var reporte = new Dictionary<string, object>();

            // Cálculo de ventas totales
            double totalVentas = CalcularTotalVentas();
            reporte.Add("TotalVentas", totalVentas);

            // Cálculo de ventas promedio diarias
            int diasPeriodo = (int)(FechaFin - FechaInicio).TotalDays + 1;
            if (diasPeriodo > 0)
            {
                reporte.Add("PromedioVentasDiario", totalVentas / diasPeriodo);
            }

            // Otros cálculos y datos para el reporte
            reporte.Add("Crecimiento", CalcularCrecimiento());
            reporte.Add("ProductosTopVendidos", ProductosTopVendidos(10));

            return reporte;
        }

        public double CalcularCrecimiento()
        {
            // Calcular el porcentaje de crecimiento respecto al período anterior

            // Calcular la duración del período actual
            int diasPeriodo = (int)(FechaFin - FechaInicio).TotalDays + 1;

            // Definir el período anterior (mismo número de días)
            DateTime fechaFinAnterior = FechaInicio.AddDays(-1);
            DateTime fechaInicioAnterior = fechaFinAnterior.AddDays(-diasPeriodo + 1);

            // Calcular ventas en ambos períodos
            double ventasActual = CalcularTotalVentas();

            // Guardar fechas actuales
            DateTime fechaInicioTemp = FechaInicio;
            DateTime fechaFinTemp = FechaFin;

            // Configurar fechas para período anterior
            FechaInicio = fechaInicioAnterior;
            FechaFin = fechaFinAnterior;

            double ventasAnterior = CalcularTotalVentas();

            // Restaurar fechas originales
            FechaInicio = fechaInicioTemp;
            FechaFin = fechaFinTemp;

            // Calcular crecimiento
            if (ventasAnterior == 0)
                return 100; // Si no hubo ventas anteriores, es 100% crecimiento

            return ((ventasActual - ventasAnterior) / ventasAnterior) * 100;
        }

        public List<Producto> ProductosTopVendidos(int limite)
        {
            // Obtener los productos más vendidos en el período
            return new List<Producto>(); // Placeholder
        }

        private double CalcularTotalVentas()
        {
            // Calcular el total de ventas para el período actual
            return 0; // Placeholder
        }
    }
}