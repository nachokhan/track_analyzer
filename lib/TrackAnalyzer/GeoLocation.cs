namespace TrackAnalyzer
{
    public class GeoLocation
    {
        public GeoLocation()
        {            
        }

        public GeoLocation(double lat, double lon, double elev)
        {
            Latitude = lat;
            Longitude = lon;
            Elevation = elev;
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Elevation { get; set; }
    }
}
