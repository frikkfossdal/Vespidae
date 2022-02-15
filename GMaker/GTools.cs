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

    public static class Operation
    {

        //static functino for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int rh, int speed, double ext, double temp, string tool)
        {
            List<Action> actions = new List<Action>();
            bool first = true;
            var prev = paths.First().First;

            //loop through all paths
            foreach (var p in paths)
            {
                var mv = new Move();
                if (first)
                {
                    mv.path.Add(prev.X, prev.Y, rh);
                    mv.path.Add(prev);
                    first = false;
                }
                else
                {
                    mv.path.Add(prev);
                    mv.path.Add(prev.X, prev.Y, rh);
                    mv.path.Add(p.First.X, p.First.Y, rh);
                    mv.path.Add(p.First);
                }
                actions.Add(mv);

                actions.Add(new Extrude(p, temp, ext, tool));
                prev = p.Last;
            }

            //create end move 
            return actions;
        }

        public static List<string> translateToGcode(List<Action> actions)
        {
            var output = new List<string>();
            //can add something that checks if Action is Extrude type or not 
            double extrusion = 0;

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

    public abstract class Action
    {
        public Polyline path;
        public int speed;
        public opTypes actionType;
        public Action()
        {

        }

        public abstract List<string> translate(ref double ex);
    }

    public class Move : Action
    {
        public Move()
        {
            path = new Polyline();
            speed = 5000;
            actionType = opTypes.move;
        }

        public override List<string> translate(ref double ex)
        {
            var translation = new List<string>();
            translation.Add($";{actionType}");

            foreach (var p in path)
            {
                translation.Add($"G0 X{p.X} Y{p.Y} Z{p.Z}");
            }

            return translation;
        }
    }

    public class Extrude : Action
    {
        public double amount;
        public double temperature;
        string tool;

        public Extrude(Polyline p, double t, double a, string to)
        {
            path = p;
            amount = a;
            temperature = t;
            tool = to;

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

                translation.Add($"G0 X{p.X} Y{p.Y}, Z{p.Z} E{extrude+ex}");
                ex += extrude;
                prev = p; 
            }

            //retract filement 

            return translation;
        }
    }
}

