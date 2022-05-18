using System;
using System.Collections; 
using System.Collections.Generic;
using Rhino.Geometry;
using System.Linq;

namespace VespidaeTools
{
    public enum opTypes
    {
        travel,
        move,
        extrusion,
        zPin
    }

    public enum extrudeTypes {
        shell = 0,
        infill = 1
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

    /// <summary>
    /// Static methods for visualizing toolpaths. 
    /// </summary>
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

        public static List<Mesh> pathViz(Polyline poly, double scl, int density)
        {
            var arrows = new List<Mesh>();
            var arrow = createArrow(scl);

            //find points that will be populated with arrows
            Point3d[] points;
            var nurbCurve = poly.ToNurbsCurve();
            nurbCurve.DivideByLength(density, false, out points);

            //check if distance between points is smaller than arrow size
            var prevPoint = poly.First;

            //find planes on each point 
            var plns = new List<Plane>();
            foreach (var p in points)
            {
                if (p.DistanceTo(prevPoint) > scl)
                {
                    var ind = poly.ClosestIndex(p);
                    plns.Add(horizFrame(poly, ind));
                }

                prevPoint = p;
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

    /// <summary>
    /// Static methods for sorting lists of actions.
    /// </summary>
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

    /// <summary>
    /// Static methods for converting lists of polylines 
    /// into lists of Actions. 
    /// </summary>
    public static class Operation
    {
        //static function for creating simple extrusion operation. Could be modified with enumeration of tool 
        public static List<Action> createExtrudeOps(List<Polyline> paths, int speed, double retract, double ext, double temp, int tool, int extType,  List<string> injection)
        {
            List<Action> actions = new List<Action>();

            extrudeTypes tp;
            if (extType == 0)
            {
                tp = extrudeTypes.shell;
            }
            else {
                tp = extrudeTypes.infill; 
            }

            foreach (var p in paths)
            {
                actions.Add(new Extrude(p, temp, ext, speed, retract, tool, tp,  injection));
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

            output.Add(";Vespidae made this program");

            //check if program contains Extruder actions
            if (actions.Where(act => act.actionType == opTypes.extrusion).ToList().Count > 0)
            {
                output.AddRange(new List<string>() { ";program contains extrusion actions", ";setting relative extrusion", "M83" });
            }

            var currentPos = new Point3d();
            foreach (var ac in actions)
            {
                output.AddRange(ac.translate());
            }
            return output;
        }
    }

    /// <summary>
    /// The Operation class consists of functions that take list of actions and converts them
    /// into complete Vespidae programs with travel moves between each Action. 
    /// </summary>
    public static class Solve
    {
        /// <summary>
        /// Generic solves for any sequence of Actions. Takes user defined sequences of Actions
        /// and creates a complete Vespidae program with Travel Actions between each Action.
        /// The solver does not sort or change the input sequence of Actions. 
        /// </summary>
        /// <param name="actions">actions to put in program</param>
        /// <param name="rh">retract height </param>
        /// <param name="sp">travel speed</param>
        /// <param name="pr">enable partial retract where possible</param>
        /// <returns></returns>
        public static List<Action> GenericSolver(List<Action> actions, int rh, int sp, bool pr)
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
                //first move
                if (first)
                {
                    var fm = new Travel(sp, true, rh);

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
                    //check if we need new tool
                    if (act.tool != prevAct.tool) toolChange = true;

                    //check if next is same z-height
                    //if (act.path.First.Z != prevAct.path.Last.Z || pr == false) partial = false;
                    //else partial = true;

                    if (!toolChange)
                    {
                        Travel m = moveBetweenActions(prevAct, act, rh, partial_rh, sp, false);
                        newProgram.Add(m);
                    }
                    else
                    {
                        var tm = makeToolchange(prevAct, act, rh, sp);
                        newProgram.Add(tm);
                    }
                    //m.tool = act.tool;
                }

                //then add action
                newProgram.Add(act);
                prevAct = act;
                toolChange = false;
            }

            //exit move
            var lm = new Travel(6000, false, rh);
            lm.path.Add(prevAct.path.Last);
            lm.path.Add(prevAct.path.Last.X, prevAct.path.Last.Y, rh);
            newProgram.Add(lm);

            return newProgram;
        }

        /// <summary>
        /// Solver for additive operations. Sorts actions by layer and by sort critera on each layer
        /// </summary>
        public static List<Action> AdditiveSolver(List<Action> actions, int rh, int sp, bool pr, int srtType)
        {

            ///hack. filter out all extrude actions.
            var filteredActions = actions.Where(obj => obj.GetType() == typeof(Extrude)).Select(obj => obj as Extrude);
          

            //
            var newProgram = new List<Action>();
            var prHeight = 1; 

            //STEP 1: Sort actions into dictionary with layer height as lookup index.
            //Note: currently uses first point of each actions path as referance value
            //move to separate function?
            SortedDictionary<double, List<Extrude>> layerLookup = new SortedDictionary<double, List<Extrude>>();

            foreach (var action in filteredActions) { 
                double index = action.path.First.Z;
                if (layerLookup.ContainsKey(index))
                {
                    layerLookup[index].Add(action);
                }
                else
                {
                    layerLookup.Add(index, new List<Extrude> { action });
                }
            }

            //STEP 2: flatten dictionary and generate complete program

            bool firstLayerFlag = true;
            bool layerChangeFlag = false;

            var prevAction = layerLookup.First().Value.First();


            foreach (var layer in layerLookup)
            {
                ///first sort layer by tool then by extrusionType e.g shell->infill
                var sortedLayer = layer.Value.OrderBy(l => l.tool).ThenBy(l => l.extType); 

                //sort by x y

                if (firstLayerFlag)
                {
                    //move to first point in layer.
                    var firstPoint = sortedLayer.First().path.First;
                    var trvMove = new Travel(sp, true, rh);
                    trvMove.tool = sortedLayer.First().tool; 
                    trvMove.path.Add(new Point3d(firstPoint.X, firstPoint.Y, rh));
                    trvMove.path.Add(firstPoint);
                    newProgram.Add(trvMove);
                }

                if (layerChangeFlag) {
                    //move between layers. Last action to first action on new layer
                    if (prevAction.tool != sortedLayer.First().tool)
                    {
                        //perform toolchange
                        newProgram.Add(makeToolchange(prevAction, sortedLayer.First(), rh, sp)); 
                    }
                    else {
                        newProgram.Add(moveBetweenActions(prevAction, sortedLayer.First(), rh, prHeight, sp, false));
                    }
                }

                //execute all actions on layer with partial retract height 
                foreach (var action in sortedLayer)
                {
                    if (firstLayerFlag)
                    {
                        firstLayerFlag = false;
                    }
                    else if(layerChangeFlag){
                        layerChangeFlag = false; 
                    }

                    else {
                        if(prevAction.tool != action.tool) newProgram.Add(makeToolchange(prevAction, action, rh, sp));
                        else newProgram.Add(moveBetweenActions(prevAction, action, rh, .5, sp, true));
                    }

                    newProgram.Add(action);
                    prevAction = action;
                }

                //flag layer to layer move next round
                layerChangeFlag = true; 
            }
            //final move of operation. Park tools?


            return newProgram;
        }

        //generates a move between two actions 
        private static Travel moveBetweenActions(Action prev, Action cur, int full_rh, double part_rh, int speed, bool partial)
        {
            var newMove = new Travel(speed, false, full_rh);
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

        private static Travel makeToolchange(Action prev, Action cur, int full_rh, int speed)
        {
            Travel t = new Travel(speed, true, full_rh);
            t.tool = cur.tool;
            t.path.Add(prev.path.Last);
            t.path.Add(prev.path.Last.X, prev.path.Last.Y, full_rh);
            t.path.Add(prev.path.Last.X, -50, full_rh);
            t.path.Add(cur.path.First.X, cur.path.First.Y, full_rh);
            t.path.Add(cur.path.First);
            return t;
        }
    }

    /// <summary>
    /// Abstract class for all Vespidae Actions. 
    /// </summary>
    public abstract class Action
    {
        public Polyline path;
        public int speed;
        public opTypes actionType;
        public List<string> injection;
        public Action() { }
        public int tool;
        public bool toolCh;
        public int retractHeight;

        public abstract List<string> translate();
    }

    public class Travel : Action
    {
        public Travel(int s, bool tc, int rh)
        {
            path = new Polyline();
            speed = s;
            actionType = opTypes.travel;
            tool = -1;
            toolCh = tc;
            retractHeight = rh;
        }

        public override List<string> translate()
        {
            var translation = new List<string>();
            translation.Add("");
            translation.Add($";Action: {actionType}");

            if (toolCh)
            {
                translation.Add(";executing toolChange");
                translation.Add($"G0 Z{retractHeight}");
                translation.Add($"t{tool}");
                translation.Add($"G0 F{speed}");
                translation.Add($"G0 Z{retractHeight}");
                translation.Add(path.Last.toGcode());

                //go to retract height
                //go to last point on path
                //lower 
            }
            else
            {
                translation.Add($"G0 F{speed}");
                foreach (var p in path)
                {
                    translation.Add(p.toGcode());
                }
            }


            return translation;
        }
    }

    /// <summary>
    /// Creates Action object for generic moves. Good for experienting with generic motion. 
    /// </summary>
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

        public override List<string> translate()
        {
            var translation = new List<string>();
            translation.Add("");
            translation.Add($";Action: {actionType}");
            translation.Add($"G0 F{speed}");

            //gcode injection 
            if (injection.Count > 0 && injection.First().Length != 0)
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
    /// <summary>
    /// Creates Action object for extrusion operations. 
    /// </summary>
    public class Extrude : Action
    {
        public double ext;
        public double temperature;
        public double retract;
        public extrudeTypes extType; 

        public Extrude(Polyline p, double t, double e, int s, double r, int to, extrudeTypes _extType, List<string> inj)
        {
            path = p;
            ext = e;
            temperature = t;
            tool = to;
            speed = s;
            retract = r;
            injection = inj;
            toolCh = true;

            actionType = opTypes.extrusion;
            extType = _extType; 
        }

        public override List<string> translate()
        {
            var translation = new List<string>();

            //inital code

            translation.Add("");
            translation.Add($";Action: {actionType}");
            translation.Add($";extrudeType: {extType}");
            translation.Add($"M109 {temperature}");
            translation.Add($"G0 F{speed}");
            //relative extrusion. Don't need to add this everytime

            //gcode injection
            if (injection.Count > 0 && injection.First().Length != 0)
            {
                translation.Add(";>>>>injected gcode start<<<<");
                translation.AddRange(injection);
                translation.Add(";>>>>injected gcode end<<<<");
            }

            //add something to detect change in Z and removes Z if there is no change

            //set previous point to first point of path. 
            Point3d prev = path.First;

            translation.Add($"G0 E{retract}");

            foreach (var p in path)
            {
                double distToPrev = p.DistanceTo(prev);

                //0.01 is experimental value. Check cura / slicer for scale 
                double extrude = distToPrev * .01 * ext;

                translation.Add(p.toGcode() + $" E{Math.Round(extrude, 5)}");
                prev = p;
            }

            //retract filement 
            translation.Add($"G0 E{-retract}");

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

        public override List<string> translate()
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

