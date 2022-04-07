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

    public static class Extension
    {
        public static string toGcode(this Point3d p)
        {
            double x = Math.Round(p.X, 3);
            double y = Math.Round(p.Y, 3);
            double z = Math.Round(p.Z, 3);

            return $"G0 X{x} Y{y} Z{z}";
        }
    }

    public static class Visualization
    {
        public static List<Mesh> enterExit(Polyline poly, double scl)
        {
            var enter = createArrow(scl);
            var exit = createArrow(scl);

            enter.Transform(Transform.PlaneToPlane(Plane.WorldXY, horizFrame(poly, 0)));
            var indexOfLastPoint = poly.IndexOf(poly.Last) / 2;
            exit.Transform(Transform.PlaneToPlane(Plane.WorldXY, horizFrame(poly, indexOfLastPoint)));

            return new List<Mesh>() { enter, exit };
        }

        public static List<Mesh> pathViz(Polyline poly, double scl, int density) {
            var arrows = new List<Mesh>();
            var arrow = createArrow(scl);

            //find points that will be populated with arrows
            Point3d[] points;
            var nurbCurve = poly.ToNurbsCurve();
            nurbCurve.DivideByLength(density, false, out points);

            //find planes on each point 
            var plns = new List<Plane>();
            foreach (var p in points)
            {
                var ind = poly.ClosestIndex(p);
                plns.Add(horizFrame(poly, ind));
            }

            foreach (var p in plns)
            {
                var newMesh = new Mesh();
                newMesh.CopyFrom(arrow);

                newMesh.Transform(Transform.PlaneToPlane(Plane.WorldXY, p));
                arrows.Add(newMesh);
            }

            return arrows; 
        }

        //creates a mesh arrow 
        private static Mesh createArrow(double scl)
        {
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

        private static Plane horizFrame(Polyline C, double t)
        {
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

    //static class for sorting lists of actions
    public static class Sort
    {
        public static List<Action> sortByX(List<Action> actions, bool f)
        {
            actions = actions.OrderBy(act => act.path.First().X).ToList();
            if (f) actions.Reverse();
            return actions;
        }

        public static List<Action> sortByY(List<Action> actions, bool f)
        {
            actions = actions.OrderBy(act => act.path.First().Y).ToList();
            if (f) actions.Reverse();
            return actions;
        }

        public static List<Action> sortByZ(List<Action> actions, bool f)
        {
            actions = actions.OrderBy(act => act.path.First().Z).ToList();
            if (f) actions.Reverse();
            return actions;
        }

        public static List<Action> sortByTool(List<Action> actions, bool f)
        {
            actions = actions.OrderBy(act => act.tool).ToList();
            if (f) actions.Reverse();
            return actions;
        }
    }

    public static class Operation
    {
        //static functino for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int speed, double ext, double temp, int tool)
        {
            List<Action> actions = new List<Action>();

            foreach (var p in paths)
            {
                actions.Add(new Extrude(p, temp, ext, speed, tool));
            }
            return actions;
        }

        public static List<Action> createMoveOps(List<Polyline> paths, int speed, int tool, List<string> injection)
        {
            List<Action> actions = new List<Action>();
            foreach (var p in paths)
            {
                actions.Add(new Move(p, speed, tool, injection));
            }
            return actions;
        }

        public static List<Action> createZpinOps(List<Polyline> paths, double amount, double temp, int tool)
        {
            List<Action> actions = new List<Action>();

            foreach (var p in paths)
            {
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
            //output.Add("G0 E0 F1200");

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

    public static class Solve
    {
        //generates a move between two actions 
        private static Travel moveBetweenActions(Action prev, Action cur,double full_rh, double part_rh, int speed, bool partial)
        {
            var newMove = new Travel(speed, true);
            if (partial)
            {
                newMove.path.Add(prev.path.Last);
                newMove.path.Add(prev.path.Last.X, prev.path.Last.Y, cur.path.First.Z + part_rh);
                newMove.path.Add(cur.path.First.X, cur.path.First.Y, cur.path.First.Z + part_rh);
                newMove.path.Add(cur.path.First);
            }
            else
            {
                newMove.path.Add(prev.path.Last);
                newMove.path.Add(prev.path.Last.X, prev.path.Last.Y, full_rh);
                newMove.path.Add(cur.path.First.X, cur.path.First.Y, full_rh);
                newMove.path.Add(cur.path.First);
            }
            return newMove;
        }
        public static List<Action> GenerateProgram(List<Action> actions, int rh, int sp, bool pr)
        {
            var newProgram = new List<Action>();

            //var prevPo = actions.First().path.First;
            var prevAct = actions.First();
            double partial_rh = 0.2; //partial retract height

            //add sorting?
            //when do we add moves between actions. We can add tons of checks here
            //should we check for planar vs non-planar? 

            bool first = true;
            bool toolChange = true;
            bool partial = false; 

            foreach (var act in actions)
            {
                //check if we need new tool
                if (act.tool != prevAct.tool) toolChange = true;

                //first move
                if (first)
                {
                    var fm = new Travel(sp, true);

                    //pick up first action's tool
                    fm.tool = act.tool;

                    //go to position of first action 
                    fm.path.Add(act.path.First.X, act.path.First.Y, rh);
                    fm.path.Add(act.path.First);
                    newProgram.Add(fm);

                    first = false;
                }

                //perform check if we are doing full or partial retraction  
                //1. sameStartingPoint check. if yes dont full retract
                //2. same z height. if yes partial retract.

                //Check if last z height is not same as current z or partial is false
                //full retract
                else
                {
                    //check if next is same z-height
                    if (act.path.First.Z != prevAct.path.Last.Z || pr == false) partial = false;
                    else partial = true; 
                    
                    Travel m = moveBetweenActions(prevAct, act, rh, partial_rh, sp, partial);
                    m.tool = act.tool;
                    newProgram.Add(m);
                }

                //then add action
                newProgram.Add(act);
                prevAct = act;
            }

            ////exit move
            //var lm = new Travel(6000, false);
            ////lm.path.Add(prevAct.path.Last);
            //lm.path.Add(prevAct.path.Last.X, prevAct.path.Last.Y, rh);
            //newProgram.Add(lm);

            return newProgram;
        }

        public static List<Action> multiToolSolver(List<Action> actions, double lh)
        {
            var newProgram = new List<Action>();

            //precheck
            //set correct tool
            //go to first position

            //foreach actions
            foreach (var act in actions)
            {
                newProgram.Add(act);
            }
            //1.execute
            //2.checkTool 
            //2.moveToNext

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
        public int tool;
        public bool toolCh; 

        public abstract List<string> translate(ref double ex);
    }

    public class Travel : Action
    {
        public Travel(int s, bool tc)
        {
            path = new Polyline();
            speed = s;
            actionType = opTypes.travel;
            tool = -1;
            toolCh = tc; 
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");

            if (toolCh) {
                translation.Add($"t{tool}");
            }
            
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
        public Move(Polyline p, int s, int to, List<string> inj)
        {
            path = p;
            speed = s;
            actionType = opTypes.move;
            tool = to;
            injection = inj;
            toolCh = true; 
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");
            translation.Add($"G0 F{speed}");

            if (injection.First().Length > 0)
            {
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

        public Extrude(Polyline p, double t, double e, int s, int to)
        {
            path = p;
            ext = e;
            temperature = t;
            tool = to;
            speed = s;
            toolCh = true; 

            actionType = opTypes.extrusion;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();

            //inital code
            translation.Add($";{actionType} Speed:{speed} Ex.Mult: {ex} Temp: {temperature}");
            translation.Add(";MISSING: tool precheck & preheat");
            translation.Add($"M109 {temperature}");
            translation.Add($"G0 F{speed}");

            //add something to detect change in Z and removes Z if there is no change
            Point3d prev = path.First;

            foreach (var p in path)
            {
                double distToPrev = p.DistanceTo(prev);

                //0.01 is experimental value
                double extrude = distToPrev * ext;

                translation.Add(p.toGcode() + $" E{Math.Round(extrude, 4)}");
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
    public class zPin : Action
    {

        public double amount;
        public double temperature;

        public zPin(Polyline p, double t, double a, int to)
        {
            path = p;
            amount = a;
            temperature = t;
            tool = to;

            actionType = opTypes.zPin;
        }

        private int calculateExtrusion()
        {
            return 1;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();

            translation.Add($";{actionType}");
            translation.Add($"t{tool}");
            translation.Add($"M109 {temperature}");

            Point3d prev = path.First;

            foreach (var p in path)
            {

            }

            return new List<string>();
        }

    }
}

