using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace GMaker
{
    public enum opTypes
    {
        travel,
        move,
        extrusion,
        zPin
    }

    public static class Extension{
        public static string toGcode(this Point3d p) {
            double x = Math.Round(p.X, 3);
            double y = Math.Round(p.Y, 3);
            double z = Math.Round(p.Z, 3);

            return $"G0 X{x} Y{y} Z{z}"; 
        }
}

    public static class Visualization {
        public static List<Mesh> enterExit(Polyline poly, double scl) {

            var enter = createArrow(scl);
            var exit = createArrow(scl);

            enter.Transform(Transform.PlaneToPlane(Plane.WorldXY, horizFrame(poly, 0)));
            var indexOfLastPoint = poly.IndexOf(poly.Last);
            exit.Transform(Transform.PlaneToPlane(Plane.WorldXY, horizFrame(poly, indexOfLastPoint)));

            return new List<Mesh>() { enter, exit }; 
        }
        //creates a mesh arrow 
        private static Mesh createArrow(double scl) {
            var returnMesh = new List<Mesh>();

            var arrow = new Mesh();
            var pnts = new List<Point3d>();
            pnts.Add(new Point3d(0, (double)-scl / 2, 0));
            pnts.Add(new Point3d(0, (double)scl / 2, 0));
            pnts.Add(new Point3d((double)scl, 0, 0));

            
            arrow.Vertices.AddVertices(pnts);
            arrow.Faces.AddFace(new MeshFace(0, 1, 2));
            return arrow; 
        }

        private static Plane horizFrame(Polyline C, double t) {
            Vector3d Tangent = C.TangentAt(t);

            if (Tangent.IsParallelTo(Vector3d.ZAxis) == 0)
            {
                Vector3d Perp = Vector3d.CrossProduct(Vector3d.ZAxis, Tangent);
                Plane frame = new Plane(C.PointAt(t), Tangent, Perp);
                return frame;
            }
            else
            {
                var frame = new Plane(C.PointAt(t), Tangent, Vector3d.XAxis);
                return frame;
            }
        }
    }

    public static class Operation
    {
        //static functino for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int speed, double ext, double temp, string tool)
        {
            List<Action> actions = new List<Action>();

            foreach (var p in paths)
            {
                actions.Add(new Extrude(p, temp, ext, speed, tool));
            }
            return actions;
        }

        public static List<Action> createMoveOps(List<Polyline> paths, int speed, string tool, List<string> injection) {
            List<Action> actions = new List<Action>();
            foreach (var p in paths) {
                actions.Add(new Move(p, speed, tool,injection));
            }
            return actions; 
        }

        public static List<Action> createZpinOps(List<Polyline> paths, double amount, double temp, string tool) {
            List<Action> actions = new List<Action>();

            foreach (var p in paths) {
                actions.Add(new zPin(p, temp, amount, tool)); 
            }

            return actions; 
        }

        public static List<string> translateToGcode(List<Action> actions)
        {
            var output = new List<string>();
            //can add something that checks if Action is Extrude type or not 
            double extrusion = 0;

            //hack fix this 
            output.Add("G0 E0 F1200");

            foreach (var ac in actions)
            {
                output.AddRange(ac.translate(ref extrusion));
            }
            return output;
        }

        public static List<Polyline> sortPolys(List<Polyline> polys)
        {
            //A = polylines.OrderBy(c => c.ElementAt(0).Y).ToList();
            return polys.OrderBy(p => p[0].Y).ToList();
        }
    }

    public static class Solve{
        //generates a move between two actions 
        private static Travel moveBetweenActions(Action prev, Action cur, double rh, int speed, bool partial) {
            var newMove = new Travel(speed);
            if (partial) {
                newMove.path.Add(prev.path.Last);
                newMove.path.Add(prev.path.Last.X, prev.path.Last.Y, cur.path.First.Z + rh);
                newMove.path.Add(cur.path.First.X, cur.path.First.Y, cur.path.First.Z + rh);
                newMove.path.Add(cur.path.First);
            }
            else {
                newMove.path.Add(prev.path.Last);
                newMove.path.Add(prev.path.Last.X, prev.path.Last.Y, rh);
                newMove.path.Add(cur.path.First.X, cur.path.First.Y, rh);
                newMove.path.Add(cur.path.First);
            }
                return newMove;
        }
        public static List<Action> GenerateProgram(List<Action> actions, int rh, int sp, bool pr) {
            var newProgram = new List<Action>();

            //var prevPo = actions.First().path.First;
            var prevAct = actions.First(); 
            double partialRh = 0.2; //partial retract height

            //add sorting?
            //when do we add moves between actions. We can add tons of checks here
            //should we check for planar vs non-planar? 

            bool first = true; 

            foreach (var act in actions) {
                //first move
                if (first)
                {
                    var fm = new Travel(sp);
                    fm.path.Add(act.path.First.X, act.path.First.Y, rh);
                    fm.path.Add(act.path.First);
                    newProgram.Add(fm);

                }
                //Check if last z height is not same as current z or partial is false
                //full retract
                else if (act.path.First.Z != prevAct.path.Last.Z || pr ==false)
                {
                    Travel m = moveBetweenActions(prevAct, act, rh, sp, false);
                    newProgram.Add(m);
                }
                //same z-height and partial is true. Partial retract 
                else if (pr == true)
                {
                    Travel m = moveBetweenActions(prevAct, act, partialRh, sp, true);
                    newProgram.Add(m);
                }
                
                //then add action
                newProgram.Add(act);
                prevAct = act; 
                first = false; 
            }

            //exit move
            var lm = new Travel(6000);
            lm.path.Add(prevAct.path.Last);
            lm.path.Add(prevAct.path.Last.X, prevAct.path.Last.Y, rh); 
            newProgram.Add(lm); 

            return newProgram; 
        }
}

    public abstract class Action
    {
        public Polyline path;
        public int speed;
        public opTypes actionType;
        public List<string> injection; 
        public Action() { }

        public abstract List<string> translate(ref double ex);
    }

    public class Travel : Action
    {
        public Travel(int s)
        {
            path = new Polyline();
            speed = s;
            actionType = opTypes.travel;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");
            translation.Add($"G0 F{speed}"); 

            foreach (var p in path)
            {
                translation.Add(p.toGcode());
            }

            return translation;
        }
    }

    public class Move : Action
    {
        public string tool;

        public Move(Polyline p,  int s, string to, List<string> inj)
        {
            path = p;
            speed = s;
            actionType = opTypes.move;
            tool = to;
            injection = inj; 
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");
            translation.Add(tool);
            translation.Add($"G0 F{speed}");

            if (injection.First().Length > 0) {
                translation.Add(";>>>>injected gcode start<<<<");
                translation.AddRange(injection);
                translation.Add(";>>>>injected gcode end<<<<");
            }

            foreach (var p in path)
            {
                translation.Add(p.toGcode());
            }

            return translation;
        }
    }

    public class Extrude : Action
    {
        public double ext;
        public double temperature;
        public string tool;

        public Extrude(Polyline p, double t, double e, int s, string to)
        {
            path = p;
            ext = e;
            temperature = t;
            tool = to;
            speed = s; 

            actionType = opTypes.extrusion;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();

            //inital code
            translation.Add($";{actionType} Speed:{speed} Ex.Mult: {ex} Temp: {temperature}");
            translation.Add(tool);
            translation.Add($"M109 {temperature}");
            translation.Add($"G0 F{speed}");

            //add something to detect change in Z and removes Z if there is no change
            Point3d prev = path.First;

            foreach (var p in path)
            {
                double distToPrev = p.DistanceTo(prev);

                //0.01 is experimental value
                double extrude = distToPrev * .01 * ext;

                translation.Add(p.toGcode() + $" E{Math.Round(extrude+ex,4)}");
                ex += extrude;
                prev = p; 
            }

            //retract filement 

            return translation;
        }
    }

    //main difference from extrude is that we disconnect extrude amount from move speed
    //and that we work
    //future could also include some type of oscillation and maybe also smearing on
    //top layer 
    public class zPin : Action {

        public double amount;
        public double temperature;
        string tool;

        public zPin(Polyline p, double t, double a, string to) {
            path = p;
            amount = a;
            temperature = t;
            tool = to;

            actionType = opTypes.zPin;
        }

        private int calculateExtrusion() {
            return 1; 
        }
        
        public override List<string> translate(ref double ex) {
            var translation = new List<string>();

            translation.Add($";{actionType}");
            translation.Add(tool);
            translation.Add($"M109 {temperature}");

            Point3d prev = path.First;

            foreach (var p in path) {

            }

            return new List<string>(); 
        }

    }
}

