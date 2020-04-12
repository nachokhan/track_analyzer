using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TrackAnalyzer.Logger
{
    public class TrackFileSaver
    {

        public static void WriteDown_CombinationSegments(CombinationsList combinations, string FileName)
        {
            string mainHeader = "SegID";
            string secondHeader = ",{0}_I, {0}_F, {0}_Ct"; 
            string mainBody = "{0}";
            string secondBody = ",{0},{1},{2}";
            string alphabet = "ABCEFHIKLNOQRTUWXZ";
            string lastBody_01 = ", ,={0}";
            string lastBody_02 = " ${0}${1}-{2}{3} + ${4}${5}-{6}{7} +";
            string lastBody = "";

            string totalHeader = "";
            string totalBody = "";
            string totalText = "";
         
            var ordered_list = combinations.Combinations.GroupBy(p => p.SegmentsCount).Select(p => p.ToList()).ToList();

            foreach (List<SegmentsListCombination> combinationList in ordered_list)
            {
                int total_combinations = combinationList.Count;
                int segments_count = combinationList[0].SegmentsCount;

                totalHeader = mainHeader;
                for(int cont=0; cont < segments_count; cont++)
                    totalHeader += String.Format(secondHeader, cont);                

                totalBody = "";
                int actual_combination = 1;
                foreach (SegmentsListCombination combination in combinationList)
                {

                    totalBody += String.Format(mainBody, combination.Name);
                    
                    lastBody = "";
                    int alph_index = 1;
                    actual_combination++;
                    foreach(SegmentInfo segment in combination.Segments)
                    {
                        totalBody += String.Format(secondBody, segment.FirstSample, segment.LastSample, segment.SamplesCount);
                        char letra1 = alphabet[alph_index++];
                        char letra2 = alphabet[alph_index++];
                        lastBody += string.Format(lastBody_02, 
                            letra1, total_combinations+5, letra1, actual_combination, 
                            letra2, total_combinations+5, letra2, actual_combination);
                    }
                    lastBody = lastBody.Remove(lastBody.Count()-1);
                    lastBody = string.Format(lastBody_01, lastBody);
                    totalBody += lastBody + "\n";
                }
                totalText += totalHeader + "\n" + totalBody + "\n\n\n";

                // Ingresar PERFECTO
                totalText += "\n";
                totalText += totalHeader + "\n";
                totalText += "\n\n";

            }

            System.IO.File.WriteAllText(FileName, totalText);

        }

        public static void WriteDown_SamplesForVisualize(List<Segmento> segments, string fileName)
        {
            const string segmentHeader = "type,latitude,longitude,alt,name,color\n";

            string segmentText;

            List<string> colors = new List<string>()
                {"green","red","blue","black","orange","yellow","grey","brown"}; 

            int nextColor = 0;

            int file_counter = 0;
            int max_segments = 9;
            int max_samples = 130;
            int samples_count = 0;
            string partial_segment_text = "";
            int index_barra = fileName.LastIndexOf('/');
            int index_punto = fileName.LastIndexOf('.');
            string nombre = fileName.Substring(index_barra+1, index_punto-index_barra-1);
            String outputDirectory = fileName.Substring(0, index_barra) + "/VIEWS/" + nombre +"/";
            Directory.CreateDirectory(outputDirectory);



            segmentText = "";

            foreach(Segmento s in segments)
            {
                segmentText += segmentHeader;
                partial_segment_text += segmentHeader;
                foreach(Muestra m in s.Muestras)
                {
                    segmentText += String.Format("T,{0},{1},{2},{3},{4}\n", 
                        m.P1.Latitude,
                        m.P2.Longitude,
                        m.Elevacion,
                        s.Name, colors[nextColor]);

                    partial_segment_text += String.Format("T,{0},{1},{2},{3},{4}\n", 
                        m.P1.Latitude,
                        m.P2.Longitude,
                        m.Elevacion,
                        s.Name, colors[nextColor]);
                    samples_count++;
                }

                nextColor++;
                if(nextColor >= colors.Count)
                    nextColor = 0;


                if( max_segments == 0 || samples_count >= max_samples)
                {                    
                    var fn = String.Format("{0}/_{1:00}.txt", outputDirectory, file_counter++);
                    System.IO.File.WriteAllText(fn, partial_segment_text);
                    partial_segment_text =  "";
                    max_segments = 9;
                    samples_count = 0;
                }
                max_segments--;
            }

            var todo = segmentText;

            System.IO.File.WriteAllText(fileName, todo);
        }

        public static void WriteDown_SamplesInfo(List<Segmento> segments, string fileName)
        {
            const string segmentHeader = "N(T), N(S), Dist, Lat1, Long1, Elev1, Lat2, Long2, Elev2, S_ID, S_Slope, First/Last\n";

            string segmentText;            

            segmentText = "";

            double total = 0;
            int num_total=0;
            int num_seg;

            foreach(Segmento s in segments)
            {
                num_seg = 0;
                foreach(Muestra m in s.Muestras)
                {
                    double a = 0;
                    if( !double.IsNaN(s.PendientePromedio))
                        a =  Math.Sign(s.PendientePromedio);
                   
                    segmentText += String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}\n", 
                        num_total,
                        num_seg,
                        Math.Round(total, 2),
                        m.P1.Latitude,
                        m.P1.Longitude,
                        m.P1.Elevation,
                        m.P2.Latitude,
                        m.P2.Longitude,
                        m.P2.Elevation,
                        s.ID,
                        s.PendientePromedio,
                        a);

                    total += m.Distancia;
                    num_total++;
                    num_seg++;
                }
            }

            var todo = segmentHeader + segmentText;

            System.IO.File.WriteAllText(fileName, todo);
        }

        public static void WriteDown_SegmentsInfo(List<Segmento> segmentos, string FileName)
        {
            const String header =  "ID, Samples, Dist, Slope\n";

            string text = header;

            foreach(Segmento s in segmentos)
            {
                text += String.Format("{0},{1},{2:0.00},{3:0.00}\n", s.ID, s.Muestras.Count, s.Distance, s.PendientePromedio);
            }

            System.IO.File.WriteAllText(FileName, text);
        }
    }
}