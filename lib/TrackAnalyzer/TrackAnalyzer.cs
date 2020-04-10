using System;
using System.Collections.Generic;
using System.Linq;

using Gpx;

namespace TrackAnalyzer
{    
    public class TrackAnalyzer
    {
        public static List<Segmento> Get_SegmentsFromSamples(List<Muestra> samples, double min_dist = 0)
        {
            List<Segmento> segments = new List<Segmento>();

            var info = Get_SlopeChangesPositions(samples);
            info = Filter_SlopeChangesPositions(info);

          
           /* double dist = samples.Sum(m => m.Distancia);
            min_dist = 0.00846 * dist;*/


            segments = GetSegments_By_MetodoDeLasPendientesContrarias(samples, info, min_dist);

                        
            return segments;
        }

        private static List<Segmento> GetSegments_By_MetodoDeLaDistanciaMinima(List<Muestra> samples, List<Tuple<int, int>> info, double min_dist)
        {
            List<Segmento> segments = new List<Segmento>();
            Segmento segment; 

            int segment_id = 0;

            foreach(var t in info)
            {
                segment = new Segmento();
                var sub = samples.GetRange(t.Item1, t.Item2);
                segment.Muestras.AddRange(sub);
                segment.ID = segment_id;
                
                if(segment.Distance <= min_dist && segments.Count>0)
                {
                   segments.Last().Muestras.AddRange(sub);
                }
                else 
                {
                    segments.Add(segment);
                    segment_id++;
                }
            }          

            return segments;
        }

        private static List<Segmento> GetSegments_By_MetodoDeLasPendientesContrarias(List<Muestra> samples, List<Tuple<int, int>> info, double min_dist)
        {
            List<Segmento> segments = new List<Segmento>();
            Segmento segment; 

            int segment_id = 0;

            // Get the slope of the first segment.            
            var last_slope = (new Segmento() {Muestras = samples.GetRange(info[0].Item1, info[0].Item2)}).PendientePromedio;


            foreach(var t in info)
            {
                segment = new Segmento();
                var sub = samples.GetRange(t.Item1, t.Item2);
                segment.Muestras = sub;               
                
                if(segments.Count > 0)
                {
                    if(Math.Sign(segment.PendientePromedio) != Math.Sign(last_slope))
                    {
                        if(segment.Distance <= min_dist)
                        {
                            segments.Last().Muestras.AddRange(sub);
                        }
                        else
                        {
                            last_slope = segment.PendientePromedio;
                            segment.ID = ++segment_id;
                            segments.Add(segment);
                        }
                    }
                    else
                    {
                        segments.Last().Muestras.AddRange(sub);
                    }
                }
                else
                {
                    segments.Add(segment);
                }             
            }

            return segments;
        }

        private static List<Tuple<int, int>> Filter_SlopeChangesPositions(List<Tuple<int, int>> positions, double error=0)
        {
            List<Tuple<int, int>> new_positions = new List<Tuple<int, int>>();

            new_positions = positions.ToArray().ToList();

            for(int i = 1; i < new_positions.Count-1; i++)
            {
                var t = new_positions[i];
                if(t.Item2 <= error)
                {                    
                    int saved_samples = new_positions[i].Item2 + new_positions[i+1].Item2;

                    var prev_position = new_positions[i-1];

                    var new_prevPosition = new Tuple<int, int>(
                        prev_position.Item1, 
                        prev_position.Item2+saved_samples
                    );
                    
                    new_positions.RemoveAt(i+1);
                    new_positions.RemoveAt(i);
                    new_positions.RemoveAt(i-1);

                    new_positions.Insert(i-1, new_prevPosition);

                    i=1;
                }
            }

            return new_positions;
        }

        private static List<Tuple<int, int>> Get_SlopeChangesPositions(List<Muestra> samples)
        {
            List<Tuple<int, int>> positions = new List<Tuple<int, int>>();
            eTipoPendiente prev_slope, actual_slope;
            int actual_pos;
            int same_slope_count;

            prev_slope = Get_TipoPendiente(samples[0].Pendiente);
            actual_pos = 0;
            same_slope_count = 0;

            foreach(Muestra s in samples)            
            {
                actual_slope = Get_TipoPendiente(s.Pendiente);

                if(prev_slope == actual_slope)
                {
                    same_slope_count++;
                }
                else
                {
                    positions.Add(new Tuple<int, int>(actual_pos, same_slope_count));
                    actual_pos = samples.IndexOf(s);
                    same_slope_count=1;
                    prev_slope = actual_slope;
                }
            }
            positions.Add(new Tuple<int, int>(actual_pos, same_slope_count));

            return positions;
        }

        private static eTipoPendiente Get_TipoPendiente(double pendiente)
        {
            if(pendiente <= 0)
                return eTipoPendiente.BAJADA;
            
            return eTipoPendiente.SUBIDA;
        }

        /// Given a GPX Segment, take all the samples with distances and slopes.
        public static List<Muestra> Get_SamplesFromSegment(GpxTrackSegment segment, int muestreo=1)
        {
            List<Muestra> muestras = new List<Muestra>();

            var gpxPoints = segment.TrackPoints;

            for(int i = 0; i < gpxPoints.Count-muestreo; i+=muestreo)
            {
                var p1 = gpxPoints[i];
                var p2 = gpxPoints[i+muestreo];
                var geo1 = new GeoLocation(p1.Latitude, p1.Longitude, (double)p1.Elevation);
                var geo2 = new GeoLocation(p2.Latitude, p2.Longitude, (double)p2.Elevation);

                var m = new Muestra(geo1, geo2);
                muestras.Add(m);
            }

            return muestras;

        }

        // Delete samples where there was no movement (it means we stayed at the same place)
        public static List<Muestra> Filter_DeadTimes(List<Muestra> lista, double error)
        {
                List<Muestra> nuevaLista = new List<Muestra>();

                foreach(Muestra m in lista)
                {
                    if (m.Distancia >= error)
                        nuevaLista.Add(m);
                }
                return nuevaLista;
        }

        // Apply Moving Average Filter to delete noise and unespected signals.
        public static List<Muestra> Filter_Slopes(List<Muestra> lista, int ventana)
        {
            
            double promedio;

            List<double> buffer = new List<double>();
           
            List<Muestra> nueva = new List<Muestra>();

            promedio = lista[0].Pendiente;

            
            foreach(var m in lista)
            {
                buffer.Add(m.Pendiente);

                m.CorregirPendiente(promedio);

                if(buffer.Count > ventana) 
                    buffer.RemoveAt(0);

                promedio = buffer.Average();

                nueva.Add(m);
            }

            return nueva;
        }
    }   
}