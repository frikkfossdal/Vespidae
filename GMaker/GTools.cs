using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace GMaker
{
    public enum opTypes
    {
        move,
        extrusion
    }

    public class GTools
    {
        public List<String> outputG;
        public List<Polyline> outputPaths;

        public GTools()
        {
            outputG = new List<string>();
            outputPaths = new List<Polyline>();
        }

        public List<String> MakeGcode(List<Polyline> polys, int ts, int ws, double rh)
        {
            List<String> output = new List<string>();
            Point3d lastPoint = new Point3d();

            foreach (Polyline poly in polys)
            {

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

        private void makeTravelCode(List<string> gcode, Point3d fp, int ts, double rh)
        {
            gcode.Add(";Travel move");
            gcode.Add($"G0 Z{rh} F{ts}");
            gcode.Add($"G0 X{fp.X} Y{fp.Y}");
            gcode.Add($"G0 Z{fp.Z}");
            gcode.Add("");
        }

        private void makeTravelMove(List<string> gcode, Polyline poly, int ws)
        {
            gcode.Add(";Work move");
            gcode.Add($"G0 F{ws}");
            foreach (Point3d p in poly)
            {
                gcode.Add($"G0 X{p.X} Y{p.Y} Z{p.Z}");
            }
            gcode.Add("");
        }

        //sort polys. Needs flexible sort direction 

    }
    //UPDATE TO NEW ABSTRACT CLASS STRUCTURE OR REMOVE 
    //    public static List<Move> zPinning(List<Point3d> pnts, int start, int stop, int amount, int rh, int speed, Point3d initPoint)
    //    {
    //        List<Move> output = new List<Move>();

    //        Point3d prev = initPoint;

    //        foreach (Point3d p in pnts)
    //        {
    //            //travel
    //            Move travel = new Move(opTypes.move, 4000);
    //            travel.speed = 4000;
    //            travel.path.Add(prev);
    //            travel.path.Add(prev.X, prev.Y, rh);
    //            travel.path.Add(p.X, p.Y, rh);
    //            travel.path.Add(p.X, p.Y, start);
    //            output.Add(travel);

    //            //pinning
    //            Move pinOp = new Move(opTypes.extrusion, speed);
    //            pinOp.val = amount;
    //            pinOp.path.Add(p.X, p.Y, start);
    //            pinOp.path.Add(p.X, p.Y, stop);
    //            output.Add(pinOp);

    //            prev = pinOp.path.Last;
    //        }

    //        return output;
    //    }
    //}

    //NEW STRUCTURE

    public static class Operation {

        //static functino for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int rh, int speed, double ext, double temp, string tool) {
            List<Action> actions = new List<Action>();
            bool first = true;
            var prev = paths.First().First;

            //loop through all paths
            foreach (var p in paths) {
                var mv = new Move2();
                if (first)
                {
                    mv.path.Add(prev.X, prev.Y, rh);
                    mv.path.Add(prev);
                    first = false;
                }
                else {
                    mv.path.Add(prev);
                    mv.path.Add(prev.X, prev.Y, rh);
                    mv.path.Add(p.First.X, p.First.Y, rh);
                    mv.path.Add(p.First); 
                }
                actions.Add(mv); 
                
                actions.Add(new Extrude(p, temp, 1, tool));
                prev = p.Last; 
            }

            //create end move 
            return actions; 
        }

        public static List<string> translateToGcode(List<Action> actions) {
            var output = new List<string>();

            foreach (var ac in actions) {
                output.AddRange(ac.translate());
            }

            return output; 
        }

        public static List<Polyline> sortPolys(List<Polyline> polys)
        {
            //A = polylines.OrderBy(c => c.ElementAt(0).Y).ToList();
            return polys.OrderBy(p => p[0].Y).ToList();
        }
    }

    public abstract class Action {
        public Polyline path;
        public int speed;
        public opTypes actionType;
        public Action() {

        }

        public abstract List<string> translate(); 
}

    public class Move2 : Action {
        public Move2() {
            path = new Polyline();
            speed = 5000;
            actionType = opTypes.move; 
        }

        public override List<string> translate()
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");

            foreach (var p in path) {
                translation.Add($"G0 X{p.X} Y{p.Y} Z{p.Z}");
            }

            return translation; 
        }
    }

    public class Extrude : Action {
        public int amount;
        public double temperature;
        string tool; 

        public Extrude(Polyline p, double t, int a, string to) {
            path = p;
            amount = a;
            temperature = t;
            tool = to; 

            actionType = opTypes.extrusion;
        }

        public override List<string> translate() {
            var translation = new List<string>();

            //inital code
            translation.Add($";{actionType}");
            translation.Add(tool);
            translation.Add($"M109 {temperature}");

            //add something to detect change in Z
            foreach (var p in path) {
                translation.Add($"G0 X{p.X} Y{p.Y}, Z{p.Z}");  
            }
            return translation; 
        }
    }

    //public class Cut : Action {
    //    public int spindleSpeed;

    //    public override void translate()
    //    {
    //        throw new NotImplementedException();
    //    }

    //}

}
