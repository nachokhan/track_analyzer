using System.Collections.Generic;
using System.Linq;

namespace TrackAnalyzer.Logger
{ 
    public class SegmentsListCombination
    {       
        public SegmentsListCombination()
        {
            Segments = new List<SegmentInfo>();
        }        

        public void Add_NewSegmentsList(string name, List<Segmento> segmentsList)
        {
            Segments = new List<SegmentInfo>();

            Name = name;
            SegmentsCount = segmentsList.Count;

            int samples_counter = 0;
            int first, last;

            foreach (Segmento s in segmentsList)
            {
                first = samples_counter;
                last = first + s.Muestras.Count;
                SegmentInfo elem = new SegmentInfo()
                {
                    FirstSample = first,
                    LastSample = last,
                    SamplesCount = last - first,
                    Distance = s.Distance,
                    Slope = s.PendientePromedio
                };
                Segments.Add(elem);
                samples_counter += s.Muestras.Count + 1;
            }
        }

        public string Name { get; set; }        
        public List<SegmentInfo> Segments {get; private set;}
        public int SegmentsCount {get;set;}
    }
}