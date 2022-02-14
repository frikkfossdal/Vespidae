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
        public static List<Polyline> sortPolys(List<Polyline> polys)
        {
            //A = polylines.OrderBy(c => c.ElementAt(0).Y).ToList();
            return polys.OrderBy(p => p[0].Y).ToList();
        }
    }

    //temporary class for handling VESPMO objects
    public static class GConvert
    {
        public static List<String> convertMoveObject(Move move)
        {

            List<String> gcode = new List<string>();
            gcode.Add($";type: {move.type}");
            gcode.Add($"G0 F{move.speed}");

            foreach (var m in move.path)
            {
                gcode.Add($"G0 X{m.X} Y{m.Y} Z{m.Z}");
            }


            return gcode;
        }

        public static List<string> convertExtrusionObject(Move move, ref double ext)
        {
            List<String> gcode = new List<string>();
            Point3d lastPoint = move.path.First;

            gcode.Add($";type: {move.type}");
            gcode.Add($"G0 F{move.speed}");

            foreach (var p in move.path)
            {
                double distance = p.DistanceTo(lastPoint);
                ext = ext + distance * 0.010973 * move.val;
                gcode.Add($"G0 X{p.X} Y{p.Y} Z{p.Z} E{ext}");
                lastPoint = p;
            }

            return gcode;
        }

        public static List<String> convertOperation(List<Move> moves)
        {
            List<string> gcode = new List<string>();
            double ext = 0;

            //value to keep track on extrusion amount 

            gcode.Add(";new operation");

            if (moves.Any(p => p.type == opTypes.extrusion))
            {
                gcode.Add(";hack! resets extrusion for each operation");
                gcode.Add("G92 E0");
            }

            foreach (var move in moves)
            {
                if (move.type == opTypes.extrusion)
                {
                    gcode.AddRange(convertExtrusionObject(move, ref ext));
                }
                else {
                    gcode.AddRange(convertMoveObject(move));
                }
            }
            return gcode;
        }
    }

    public static class Operations
    {

        public static List<Move> normalOps(List<Polyline> polys, int rh, opTypes type, int speed, int val)
        {

            List<Move> output = new List<Move>();

            //set first point in operation 
            Point3d prev = new Point3d(polys[0].First.X, polys[0].First.Y, rh);
            bool first = true;
            int travelSpeed = 5000;

            foreach (var pol in polys)
            {
                //Something is wrong here. Check output gcode 
                Move tr = new Move(opTypes.move, travelSpeed);

                //first point
                if (first)
                {

                    tr.path.Add(prev);
                    tr.path.Add(pol.First);
                    first = false;
                }

                else
                {
                    tr.path.Add(prev);
                    tr.path.Add(prev.X, prev.Y, rh);
                    tr.path.Add(pol.First.X, pol.First.Y, rh);
                    tr.path.Add(pol.First);
                }

                output.Add(tr);

                //add ops -> Move.type = extrusion / cut / etc
                Move wo = new Move();
                wo.type = type;
                wo.val = val;
                wo.speed = speed;
                wo.path = pol;
                output.Add(wo);

                prev = wo.path.Last;
            }

            //add exit move
            Point3d lp = polys.Last().Last;
            Move exit = new Move(opTypes.move, 5000);
            exit.path.Add(lp);
            exit.path.Add(lp.X, lp.Y, rh);
            output.Add(exit);

            return output;
        }

        public static List<Move> zPinning(List<Point3d> pnts, int start, int stop, int amount, int rh, int speed, Point3d initPoint)
        {
            List<Move> output = new List<Move>();

            Point3d prev = initPoint;

            foreach (Point3d p in pnts)
            {
                //travel
                Move travel = new Move(opTypes.move, 4000);
                travel.speed = 4000;
                travel.path.Add(prev);
                travel.path.Add(prev.X, prev.Y, rh);
                travel.path.Add(p.X, p.Y, rh);
                travel.path.Add(p.X, p.Y, start);
                output.Add(travel);

                //pinning
                Move pinOp = new Move(opTypes.extrusion, speed);
                pinOp.val = amount;
                pinOp.path.Add(p.X, p.Y, start);
                pinOp.path.Add(p.X, p.Y, stop);
                output.Add(pinOp);

                prev = pinOp.path.Last;
            }

            return output;
        }
    }

    public class Move
    { 
        public opTypes type;
        public Polyline path;
        public int speed;
        public double val;

        public Move()
        {
            //todo: enumerate type
            type = opTypes.move;
            path = new Polyline();
            speed = 100;
            val = 0;
        }

        public Move(opTypes t, int s)
        {
            type = t;
            speed = s;
            path = new Polyline();
            val = 0;
        }
    }
}
