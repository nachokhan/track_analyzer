using System;
using System.Collections.Generic;
using System.IO;

using System.Linq;


using Gpx;
using TrackAnalyzer;
using TrackAnalyzer.Logger;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            double MIN_DT = 4;
            double MAX_DT = 10;
            int MIN_WIN = 3;
            int MAX_WIN = 6;
            int MIN_PERM = 200;
            int MAX_PERM = 500;
   
            double INC_DT = 2;
            int INC_WIN = 2;
            int INC_PERM = 50;

            var a = (int) (MAX_DT-MIN_DT)/INC_DT + 1 ;
            var b = (int) (MAX_WIN-MIN_WIN)/INC_WIN + 1;
            var c = (int) (MAX_PERM-MIN_PERM) / INC_PERM + 1;

            Console.WriteLine("Atención: Se generaran {0} archivos por TRACK", a*b*c+1);

            //HacerPruebas("other/perilago.gpx", MIN_DT, MAX_DT, MIN_WIN, MAX_WIN, MIN_PERM, MAX_PERM, INC_DT, INC_WIN, INC_PERM);
            HacerMuchasPruebas("rincon1___", MIN_DT, MAX_DT, MIN_WIN, MAX_WIN, MIN_PERM, MAX_PERM, INC_DT, INC_WIN, INC_PERM);
            //HacerPruebas("other/rincon1___.gpx");

            //ProbarDistancias
        }

        private static List<Muestra> Read_XML(string FileName)
        {
            List<Muestra> muestras = new List<Muestra>();

            FileStream fileStream = new FileStream(FileName, FileMode.Open);            
            Gpx.GpxReader gpxReader = new GpxReader(fileStream);

            String outputDirectory = "other/output/" + FileName;
            Directory.CreateDirectory(outputDirectory);

            while(gpxReader.Read());

            GpxTrack t = gpxReader.Track;
            foreach(GpxTrackSegment seg in t.Segments)
            {
                muestras = TrackAnalyzer.TrackAnalyzer.Get_SamplesFromSegment(seg, 1);
            }

            return muestras;
        }

        static private void HacerPruebas(String FileName)
        {            
            List<Muestra> muestras;
            List<Segmento> segmentos;

            string visualizationFile;
            string informationFile;
            
            String FileName_SinExt = FileName.Substring(FileName.LastIndexOf("/")+1, FileName.LastIndexOf(".") - FileName.LastIndexOf("/")-1);
            String outputDirectory = "other/output/" + FileName_SinExt;
            Directory.CreateDirectory(outputDirectory);

            muestras = Read_XML(FileName);
            muestras = TrackAnalyzer.TrackAnalyzer.Filter_DeadTimes(muestras, 5);
            muestras = TrackAnalyzer.TrackAnalyzer.Filter_Slopes(muestras, 5);             
            segmentos = TrackAnalyzer.TrackAnalyzer.Get_SegmentsFromSamples(muestras, 0);
                    
            // GPX Visualizer
            visualizationFile = String.Format("{4}/VIEW_{1:00}_{2:00}_{3:00}.txt", FileName_SinExt, 4, 3, 0, outputDirectory);
            TrackFileSaver.WriteDown_SamplesForVisualize(segmentos, visualizationFile);                        

            // Excel ANALYSIS
            informationFile = String.Format("{3}/INFO_{1:00}_{2:00}_00.txt", FileName_SinExt, 4, 3, outputDirectory);                    
            TrackFileSaver.WriteDown_SamplesInfo(segmentos, informationFile + "_SAMPLES");
            TrackFileSaver.WriteDown_SegmentsInfo(segmentos, informationFile + "_SEGMENTS" );           
            
            Console.WriteLine("FIN at " + DateTime.Now.ToString() );
        }

        static private void HacerMuchasPruebas(
            String FileName, 
            double min_dt, double max_dt, int min_win, int max_win, int min_per, int max_per, 
            double inc_dt, int inc_win, int inc_per)
        {
            List<Muestra> muestras;
            List<Muestra> all_muestras = new List<Muestra>();
            List<Segmento> segmentos;

            FileStream fileStream = new FileStream("other/" + FileName + ".gpx", FileMode.Open);            
            Gpx.GpxReader gpxReader = new GpxReader(fileStream);

            String outputDirectory = "other/output/" + FileName;
            Directory.CreateDirectory(outputDirectory);

            while(gpxReader.Read());

            GpxTrack t = gpxReader.Track;
            foreach(GpxTrackSegment seg in t.Segments)
            {
                all_muestras = TrackAnalyzer.TrackAnalyzer.Get_SamplesFromSegment(seg, 1);
            }

            var a = gpxReader.Track.Name;
            Console.WriteLine("TRACK NAME: " + a);

            string visualizationFile;
            string informationFile;

            int samples_01_initial = all_muestras.Count;
            int samples_02_withourDTs = 0;
            int samples_03_Filtered_Slopes = 0;
            string combinations_Header = "DeadTimes, Window, Permitted, Segments\n";
            string combinations_Body = "";

            CombinationsList combinations = new CombinationsList();
            string combin_name;

            for(double dt = min_dt; dt < max_dt; dt+= inc_dt)
            {
                muestras = TrackAnalyzer.TrackAnalyzer.Filter_DeadTimes(all_muestras, dt);               

                samples_02_withourDTs = muestras.Count;

                for(int win = min_win; win <= max_win; win += inc_win)
                {                    
                    muestras = TrackAnalyzer.TrackAnalyzer.Filter_Slopes(muestras, win);

                    samples_03_Filtered_Slopes = muestras.Count;

                    segmentos = TrackAnalyzer.TrackAnalyzer.Get_SegmentsFromSamples(muestras, 0);
                    
                    combinations_Body += String.Format("{0},{1},{2},{3}\n", dt, win, 0, segmentos.Count);
                        
                    // GPX Visualizer
                    visualizationFile = String.Format("{4}/_VIEW_{1:00}_{2:00}_{3:00}.txt", FileName, dt, win, 0, outputDirectory);
                    TrackFileSaver.WriteDown_SamplesForVisualize(segmentos, visualizationFile);                        

                    // Excel ANALYSIS
                    informationFile = String.Format("{3}/INFO_{1:00}_{2:00}_00.txt", FileName, dt, win, outputDirectory);                    
                    TrackFileSaver.WriteDown_SamplesInfo(segmentos, informationFile + "_SAMPLES");
                    TrackFileSaver.WriteDown_SegmentsInfo(segmentos, informationFile + "_SEGMENTS" );


                    for (int perm = min_per; perm <= max_per; perm += inc_per)
                    {
                        segmentos = TrackAnalyzer.TrackAnalyzer.Get_SegmentsFromSamples(muestras, perm);

                        // COMBINATIONS SEGMENTS ANALYSIS
                        combin_name = String.Format("{0:00}_{1:00}_{2:00}", dt, win, perm);
                        combinations.Add_NewCombination(combin_name, segmentos);

                        if(segmentos.Count < 15)
                        {                            
                            combinations_Body += String.Format("{0},{1},{2},{3}\n", dt, win, perm, segmentos.Count);

                            // GPX Visualizer
                            visualizationFile = String.Format("{4}/VIEW_{1:00}_{2:00}_{3:00}.txt", FileName, dt, win, perm, outputDirectory);
                            // TrackFileSaver.WriteDown_SamplesForVisualize(segmentos, visualizationFile);
                        }
                    }
                }
            }

            String combiListFile = String.Format("{0}/_COMBI_EXCEL.txt", outputDirectory);
            TrackFileSaver.WriteDown_CombinationSegments(combinations, combiListFile);

            var combinations_text = combinations_Header + combinations_Body;
            var combinations_file = String.Format("{0}/_Combinations.txt", outputDirectory);
            System.IO.File.WriteAllText(combinations_file, combinations_text);

            Console.WriteLine("FIN at " + DateTime.Now.ToString() );
        }

        private static List<Tuple<int, int, Muestra, Muestra>> GetSegmentsList(List<Segmento> segments)
        {
            List<Tuple<int, int, Muestra, Muestra>> lista = new List<Tuple<int, int, Muestra, Muestra>>();
            Tuple<int, int, Muestra, Muestra> tupla;

            foreach(Segmento s in segments)
            {
                tupla = new Tuple<int, int, Muestra, Muestra>(s.ID, s.Muestras.Count, s.Muestras[0], s.Muestras[s.Muestras.Count-1]);
                lista.Add(tupla);
            }

            return lista;
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

           
        
    }
}
