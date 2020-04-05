using System.Collections.Generic;
using System.Linq;

namespace TrackAnalyzer
{
    public class Segmento
    {
        public Segmento()
        {
            Muestras = new List<Muestra>();
        }



        public string Name {get;set;}
        public int ID { get; private set; }
        public GeoLocation StartPoint 
        { 
            get 
            {
                if(Muestras != null) 
                {
                    return Muestras.First().P1;
                }
                return null;
            }
        }
        public GeoLocation EndPoint
        {
            get 
            {
                if(Muestras != null) 
                {
                    return Muestras.Last().P2;
                }
                return null;
            }
        }
        public double Distance 
        {
            get 
            {
                if(Muestras != null) 
                {
                    double a = Muestras.Sum(m => m.Distancia);
                    return a;
                }
                return 0;
            }
        }
        public double PendientePromedio {get; private set; }
        public double PendienteBruta { get; private set; }
        public List<Muestra> Muestras { get; set;}
        public double ElevationGain { get; set;}
        public double Dificultad {get; private set; }
        


    }
}