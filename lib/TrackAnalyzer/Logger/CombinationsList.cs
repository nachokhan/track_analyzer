using System.Collections.Generic;

namespace TrackAnalyzer.Logger
{
    public class CombinationsList
    {
        public CombinationsList()
        {
            Combinations = new List<SegmentsListCombination>();
        }

        public void Add_NewCombination (string name, List<Segmento> segmentList)
        {
            SegmentsListCombination new_combi = new SegmentsListCombination();
            new_combi.Add_NewSegmentsList(name, segmentList);

            Combinations.Add(new_combi);
        }

        public List<SegmentsListCombination> Combinations {get; private set;}
    }
}