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
            Point3d lastPoint = new Point3d();

            foreach (Polyline poly in polys) {

                //make function for viz? 
                Polyline vizPoly = new Polyline();
                vizPoly.Add(lastPoint);
                vizPoly.Add(lastPoint.X, lastPoint.Y, rh);
                vizPoly.Add(poly[0].X, poly[0].Y, rh);
                vizPoly.Add(poly[0].X, poly[0].Y, poly[0].Z);
                outputPaths.Add(vizPoly);
                lastPoint = poly[poly.Count - 1];

                makeTravelCode(output, poly[0], ws, rh);
                makeTravelMove(output, poly, ws);
            }

            return output;
        }

        private void makeTravelCode(List<string> gcode, Point3d fp, int ts, double rh) {
            gcode.Add(";Travel move");
            gcode.Add($"G0 Z{rh} F{ts}");
            gcode.Add($"G0 X{fp.X} Y{fp.Y}");
            gcode.Add($"G0 Z{fp.Z}");
            gcode.Add("");
        }

        private void makeTravelMove(List<string> gcode, Polyline poly, int ws) {
            gcode.Add(";Work move");
            gcode.Add($"G0 F{ws}");
            foreach (Point3d p in poly) {
                gcode.Add($"G0 X{p.X} Y{p.Y} Z{p.Z}");
            }
            gcode.Add("");
        }

        //sort polys. Needs flexible sort direction 
        public static List<Polyline> sortPolys(List<Polyline> polys) {
            //A = polylines.OrderBy(c => c.ElementAt(0).Y).ToList();
            return polys.OrderBy(p => p[0].Y).ToList();
            
        }



        //sort polylines
        //public static List<Polylines> sortPolys(List<Polyline> polys) {

        //    return polys.OrderBy(c => c.ElementAt(0).Y).ToList());
        //}
    }
}
