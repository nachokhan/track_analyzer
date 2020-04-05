using System;
using System.Collections.Generic;
using System.IO;


using Gpx;
using TrackAnalyzer;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            double MIN_DT = 2;
            double MAX_DT = 14;
            int MIN_WIN = 5;
            int MAX_WIN = 40;
            int MIN_PERM = 5;
            int MAX_PERM = 40;

            double INC_DT = 2;
            int INC_WIN = 2;
            int INC_PERM = 5;



            HacerPruebas("rocas", MIN_DT, MAX_DT, MIN_WIN, MAX_WIN, MIN_PERM, MAX_PERM, INC_DT, INC_WIN, INC_PERM);

            //ProbarDistancias

        }

        static private void HacerPruebas(
            String FileName, 
            double min_dt, double max_dt, 
            int min_win, int max_win, 
            int min_per, int max_per, 
            double inc_dt, int inc_win, int inc_per)
        {

            FileStream fileStream = new FileStream("other/" + FileName + ".gpx", FileMode.Open);
            Gpx.GpxReader gpxReader = new GpxReader(fileStream);

            while(gpxReader.Read());

            var a = gpxReader.Track.Name;
            Console.WriteLine("TRACK NAME: " + a);

            GpxTrack t = gpxReader.Track;

            List<Muestra> all_muestras = new List<Muestra>();
            List<Muestra> muestras;
            List<Segmento> segmentos;
            
                     
            foreach(GpxTrackSegment seg in t.Segments)
            {
                all_muestras = TrackAnalyzer.TrackAnalyzer.Get_SamplesFromSegment(seg, 1);
            }

            for(double dead_times = min_dt; dead_times < max_dt; dead_times+= inc_dt)
            {
                muestras = TrackAnalyzer.TrackAnalyzer.Filter_DeadTimes(all_muestras, dead_times);

                for(int win = min_win; win <= max_win; win += inc_win)
                {
                    muestras = TrackAnalyzer.TrackAnalyzer.Filter_Slopes(muestras, win);

                    for (int perm = min_per; perm <= max_per; perm += inc_per)
                    {
                        segmentos = TrackAnalyzer.TrackAnalyzer.Get_SegmentsFromSamples(muestras, perm);

                        var file = String.Format("other/output/{0}_d_{1}_v_{2}_s_{3}.txt", FileName, dead_times, win, perm);
                        //WriteDown_Segments(segmentos, file);
                    }
                }
            }
            
            
            /*
            string lala = "Dist, Pend\n";

            string vega_values = "";
          
            
            foreach(Muestra m in muestras)
            {
                total += m.Distancia;                
                lala += Math.Round(total,2) + ", " + Math.Round(m.Pendiente,3) + "\n";
                

                vega_values += @"{""u"":" + Math.Round(total,2);
                vega_values += @",""v"":" + Math.Round(m.Pendiente,3) +"},";
            }
            vega_values = vega_values.Remove(vega_values.Length-1);
            
            string todo = File.ReadAllText("vegav/pat3.json");

            todo = todo.Replace("values_here", vega_values);

            File.WriteAllText("other/output/"+FileName+"_excel.txt", lala);
            File.WriteAllText("vegav/chart", todo);
            */

            Console.WriteLine("FIN" );
        }

        private static void ProbarDistancias()
        {
            double lat1 = -32.950992;
            double lon1 = -68.854515;
            double lat2 = -32.953204;
            double lon2 = -68.910102;

            var d1 = TrackAnalyzer.GeoMaths.GetDistance(lon1, lat1, lon2, lat2, 0, 0)/1000;            
            var d2 = TrackAnalyzer.GeoMaths.GetDistance2(lon1, lat1, lon2, lat2, 0, 0)/1000;
            var d3 = TrackAnalyzer.GeoMaths.GetDistance(lon1, lat1, lon2, lat2, 2749.6, 2980.7)/1000;
            var d4 = TrackAnalyzer.GeoMaths.GetDistance2(lon1, lat1, lon2, lat2, 2749.6, 2980.7)/1000;            

            Console.WriteLine("DIST:\n---------\n{0}\n{1}\n{2}\n{3}\n", d1, d2, d3, d4);
        }

        public static void WriteDown_Segments(List<Segmento> segments, string fileName)
        {
            const string segmentHeader = "type,latitude,longitude,alt,name,color\n";

            string segmentText;

            List<string> colors = new List<string>()
                {"green","red","blue","black","orange","yellow","grey","brown","white","rose"}; 

            int nextColor = 0;


            segmentText = "";

            foreach(Segmento s in segments)
            {
                segmentText += segmentHeader;
                foreach(Muestra m in s.Muestras)
                {
                    segmentText += String.Format("T,{0},{1},{2},{3},{4}\n", 
                        m.P1.Latitude,
                        m.P2.Longitude,
                        m.Elevacion,
                        s.Name, colors[nextColor]);
                }

                nextColor++;
                if(nextColor >= 10)
                    nextColor = 0;
            }

            var todo = segmentText;

            System.IO.File.WriteAllText(fileName, todo);
        }
    }
}
