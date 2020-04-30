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
            string lastBody_01 = ", ,={0}";
            string lastBody_02 = " ${0}${1}-{2}{3} + ${4}${5}-{6}{7} +";
            string lastBody = "";

            string totalHeader = "";
            string totalBody = "";
            string totalText = "";
         
            var ordered_list = combinations.Combinations.GroupBy(p => p.SegmentsCount).Select(p => p.ToList()).ToList();

            int actual_combinationList = 0;
            int perfect_row = 0;
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
                    int alph_index = 2;
                    actual_combination++;
                    foreach(SegmentInfo segment in combination.Segments)
                    {
                        totalBody += String.Format(secondBody, segment.FirstSample, segment.LastSample, segment.SamplesCount);                        
                        var letra1 = Alphabet(alph_index);
                        var letra2 = Alphabet(alph_index+1);
                        lastBody += string.Format(lastBody_02, 
                            letra1, total_combinations+5 + actual_combinationList, letra1, actual_combination + actual_combinationList, 
                            letra2, total_combinations+5 + actual_combinationList, letra2, actual_combination + actual_combinationList);
                        
                        alph_index+=3;
                    }
                    lastBody = lastBody.Remove(lastBody.Count()-1);
                    lastBody = string.Format(lastBody_01, lastBody);
                    totalBody += lastBody + "\n";                   
                }
                totalText += totalHeader + "\n" + totalBody + "\n";

                // Ingresar HEADER PERFECTO
                totalText += "Perfecto:\n";
                totalText += totalHeader + "\n";
                //INGRESAR BODY PERFECTO
                actual_combinationList += 1 + total_combinations +  7;
                perfect_row = actual_combinationList - 5;
                for (int i = 1; i <= segments_count*3+1; i++)
                    totalText += String.Format("=INDIRECT(\"{0}\"&B${1}),", Alphabet(i), perfect_row);
                    // =INDIRECT("C"&B85)
                totalText += "\n\n\n\n";
                
            }

            System.IO.File.WriteAllText(FileName, totalText);

        }

        public static string Alphabet(int column)
        {
            string columnString = "";
            decimal columnNumber = column;
            while (columnNumber > 0)
            {
                decimal currentLetterNumber = (columnNumber - 1) % 26;
                char currentLetter = (char)(currentLetterNumber + 65);
                columnString = currentLetter + columnString;
                columnNumber = (columnNumber - (currentLetterNumber + 1)) / 26;
            }
            return columnString;
        }

        public static void WriteDown_SamplesForVisualize(List<Segmento> segments, string fileName)
        {
            const string segmentHeader = "type,latitude,longitude,alt,name,color\n";

            string segmentText;                             // Para generar todo el track completo
            string partial_segment_text = "";               // Para generar segmentos aislados

            List<string> colors = new List<string>()
                {"green","red","blue","black","orange","yellow","grey","brown"}; 

            int nextColor = 0;

            int file_counter = 0;
            int max_segments = 9;
            int max_samples = 130;
            int samples_count = 0;
            
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

                // Cada 'max_segments' o cada 'max_samples' generamos un nuevo archivo 
                // para limitar la cantidad de puntos que vemos
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