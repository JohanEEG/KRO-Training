using System;
using System.Collections.Generic;

namespace KROTraining.Models
{
    public class EvolucionMedidasViewModel
    {
        public List<PuntoEvolucionMedidasViewModel> Registros { get; set; }
            = new List<PuntoEvolucionMedidasViewModel>();
    }

    public class PuntoEvolucionMedidasViewModel
    {
        public DateTime Fecha { get; set; }

        public decimal Peso { get; set; }

        public decimal Estatura { get; set; }

        public decimal? Cintura { get; set; }

        public decimal? Cadera { get; set; }

        public decimal? Brazo { get; set; }
    }
}
