using System;
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace GMaker
{
    public enum opTypes
    {
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
    public static class Operation
    {
        //static functino for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int speed, double ext, double temp, string tool)
        {
            List<Action> actions = new List<Action>();
            var first = paths.First().First;
            var last = paths.Last().Last;

            //move to firstPoint
            var mv = new Move(6000);
            //mv.path.Add(first.X, first.Y, rh);
            //mv.path.Add(first);
            //mv.speed = 5000;
            //actions.Add(mv);

            //do all paths


            //loop through all paths
            foreach (var p in paths)
            {
                actions.Add(new Extrude(p, temp, ext, speed, tool));
            }

            //add exit move
            //var exit = new Move(6000);
            //exit.path.Add(last);
            //exit.path.Add(last.X, last.Y, rh);
            //actions.Add(exit);

            //create end move 
            return actions;
        }

        public static List<Action> createZpinOps(List<Polyline> paths, double amount, double temp, string tool) {
            List<Action> actions = new List<Action>();

            foreach (var p in paths) {
                
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
        public static List<Action> GenerateProgram(List<Action> actions, int rh) {
            var newProgram = new List<Action>();

            var prevPo = actions.First().path.First;
            double currentZ = prevPo.Z; //to keep track of when we should add a move. 

            //add sorting?
            //when do we add moves between actions. We can add tons of checks here 

            //move to first action
            var fm = new Move(6000);
            fm.path.Add(prevPo.X, prevPo.Y,rh);
            fm.path.Add(prevPo);
            newProgram.Add(fm);

            foreach (var act in actions) {
                if (act.path.First.Z != currentZ)
                {
                    var newMove = new Move(6000);
                    newMove.path.Add(prevPo);
                    newMove.path.Add(prevPo.X, prevPo.Y, rh);
                    newMove.path.Add(act.path.First.X, act.path.First.Y, rh);
                    newMove.path.Add(act.path.First); 
                    newProgram.Add(newMove);
                    
                }
                newProgram.Add(act);
                prevPo = act.path.Last;
                currentZ = act.path.Last.Z;
            }

            //exit move
            var lm = new Move(6000);
            lm.path.Add(prevPo);
            lm.path.Add(prevPo.X, prevPo.Y, rh); 
            newProgram.Add(lm); 

            return newProgram; 
        }
}

    public abstract class Action
    {
        public Polyline path;
        public int speed;
        public opTypes actionType;
        public Action() { }


        public abstract List<string> translate(ref double ex);
    }

    public class Move : Action
    {
        public Move(int s)
        {
            path = new Polyline();
            speed = s;
            actionType = opTypes.move;
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
    public class Extrude : Action
    {
        public double amount;
        public double temperature;
        string tool;

        public Extrude(Polyline p, double t, double a, int s, string to)
        {
            path = p;
            amount = a;
            temperature = t;
            tool = to;
            speed = s; 

            actionType = opTypes.extrusion;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();

            //inital code
            translation.Add($";{actionType}");
            translation.Add(tool);
            translation.Add($"M109 {temperature}");
            translation.Add($"G0 F{speed}");

            //add something to detect change in Z and removes Z if there is no change
            Point3d prev = path.First;

            foreach (var p in path)
            {
                double distToPrev = p.DistanceTo(prev);

                double extrude = distToPrev * amount;
                p.toGcode(); 
                translation.Add(p.toGcode() + $"E{Math.Round(extrude+ex,4)}");
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

