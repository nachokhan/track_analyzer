using Gpx;

namespace TrackAnalyzer
{
    public class Muestra
    {
        public Muestra(GeoLocation p1, GeoLocation p2)
        {
            P1 = p1;
            P2 = p2;
            PendienteCorregida = false;
            
            MakeCalculations();
        }
        public void CorregirPendiente(double value)
        {
            OldPendiente = Pendiente;
            Pendiente = value;
            PendienteCorregida = true;
        }

        private void MakeCalculations()
        {
            // Enviamos 0 como elevaciones, porque buscamos la distancia sobre el EJE X. Esta distancia es necesaria
            // para calcular la pendiente más adelante.
            // Y por ende,debemos calcular la distancia real (teniendo en cuenta las altitudes "a mano" )
            var Delta_X = GeoMaths.GetDistance2(P1.Longitude, P1.Latitude, P2.Longitude, P2.Latitude, 0, 0);
            var Delta_X_2 = System.Math.Pow(Delta_X, 2);

            var Delta_Y = (double)P2.Elevation - (double)P1.Elevation;            
            var Delta_Y_2 = System.Math.Pow(Delta_Y, 2);

            Distancia = System.Math.Sqrt(Delta_X_2+Delta_Y_2);

            var X = System.Math.Sqrt( System.Math.Abs(Delta_X_2 - Delta_Y_2) );

            Pendiente = Delta_Y / X * 100;
           
            Elevacion = ( (double)P2.Elevation + (double)P1.Elevation) / 2;
        }

        public GeoLocation P1 {get;set;}
        public GeoLocation P2 {get;set;}
        public double Pendiente { get; private set;}
        public double Distancia { get; private set;}
        public double Elevacion { get; private set;}
        public bool PendienteCorregida {get; private set;}
        public double OldPendiente {get; private set;}
    }

    public enum eTipoPendiente
    {
        BAJADA = -1,
        NEUTRA = 0,
        SUBIDA = 1
    }
}