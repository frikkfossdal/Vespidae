using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq; 

namespace GMaker
{
    public class GTools
    {
        public List<String> outputG;
        public List<Polyline> outputPaths;

        public GTools() {
            outputG = new List<string>();
            outputPaths = new List<Polyline>(); 
        }


        public List<String> MakeGcode(List<Polyline> polys, int ts, int ws, double rh) {
            List<String> output = new List<string>();

            foreach (Polyline poly in polys) {
                //add travel
                foreach (Point3d p in poly) {
                    outputG.Add($"G0 X{p.X} Y{p.Y} Z{p.Z}"); 
                }
                //add move 
            }

            return output;   
        }



        //sort polylines
        //public static List<Polylines> sortPolys(List<Polyline> polys) {

        //    return polys.OrderBy(c => c.ElementAt(0).Y).ToList());
        //}
    }
}
