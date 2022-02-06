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
    }

    //temporary class for handling VESPMO objects
    public static class GConvert {

        public static List<String> convertMoveObject(Move move) {
            List<String> gcode = new List<string>();
            gcode.Add($";type: {move.type}");
            gcode.Add($"G0 F{move.speed}");

            foreach (var m in move.path) {
                gcode.Add($"G0 X{m.X} Y{m.Y} Z{m.Z}"); 
            }
            return gcode; 
        }

        public static List<String> convertOperation(List<Move> moves) {
            List<string> gcode = new List<string>();
            gcode.Add(";new operation");
            foreach (var m in moves) {
                gcode.AddRange(convertMoveObject(m));
            }
            return gcode; 
        }
    }

    public static class Operations {
        public static List<Move> zPinning(List<Point3d> pnts, int start, int stop, int amount, int rh, Point3d initPoint) {
            List<Move> output = new List<Move>();

            Point3d prev = initPoint; 
            foreach (Point3d p in pnts) {
                //travel
                Move travel = new Move("travel", 4000);
                travel.type = "travel";
                travel.speed = 4000; 
                travel.path.Add(prev);
                travel.path.Add(prev.X, prev.Y, rh);
                travel.path.Add(p.X, p.Y, rh);
                travel.path.Add(p.X, p.Y, start);
                output.Add(travel);

                //pinning
                Move pinOp = new Move("work", 1200);
                pinOp.val = amount; 
                pinOp.path.Add(p.X, p.Y, start);
                pinOp.path.Add(p.X, p.Y, stop);
                output.Add(pinOp);

                prev = pinOp.path.Last; 
            }

            return output; 
        }
    } 

    public class Move {
        public string type;
        public Polyline path;
        public int speed;
        public double val;

        public Move() {
            //todo: enumerate type
            type = "";
            path = new Polyline();
            speed = 100;
            val = 0; 
        }

        public Move(string t, int s) {
            type = t;
            speed = s;
            path = new Polyline();
            val = 0;
        }
    }
}
