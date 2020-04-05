using System;

namespace TrackAnalyzer
{
    public class GeoMaths
    {
        const double PIx = 3.141592653589793;
        const double RADIUS = 6378.16;

        /// <summary>
        /// Convert degrees to Radians
        /// </summary>
        /// <param name="x">Degrees</param>
        /// <returns>The equivalent in radians</returns>
        public static double Radians(double x)
        {
            return x * PIx / 180;
        }

        /// <summary>
        /// Calculate the distance between two places.
        /// </summary>        
        public static double GetDistance(double lon1, double lat1, double lon2, double lat2, double elev1, double elev2)
        {
            double dlon = Radians(lon2 - lon1);
            double dlat = Radians(lat2 - lat1);

            double a = (Math.Sin(dlat / 2) * Math.Sin(dlat / 2))
                + Math.Cos(Radians(lat1))
                * Math.Cos(Radians(lat2))
                * (Math.Sin(dlon / 2) * Math.Sin(dlon / 2));

            double angle = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var dist =  angle * RADIUS * 1000;

            var real_dist = GetDistanceWithPitagoras(dist, elev2-elev1);

            return real_dist;

        }

        public static double GetDistance2(double lon1, double lat1, double lon2, double lat2, double elev1, double elev2)
        {
            
            double rlat1 = Math.PI*lat1/180;
            double rlat2 = Math.PI*lat2/180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI*theta/180;
            double dist =
                Math.Sin(rlat1)*Math.Sin(rlat2) + Math.Cos(rlat1)*
                Math.Cos(rlat2)*Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist*180/Math.PI;
            dist = dist*60*1.1515;

            dist *= 1609.344;

            var real_dist = GetDistanceWithPitagoras(dist, elev2-elev1);

            return real_dist;
        }

        public static double GetDistance(GeoLocation p1 , GeoLocation p2)
        {
           return GetDistance(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude, p1.Elevation, p2.Elevation);
        }
        public static double GetDistance2(GeoLocation p1 , GeoLocation p2)
        {
           return GetDistance2(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude, p1.Elevation, p2.Elevation);
        }

        private static double GetDistanceWithPitagoras(double distance, double delta_elevation)
        {
            return Math.Sqrt(Math.Pow(distance,2) + Math.Pow(delta_elevation,2));
        }
    }
}