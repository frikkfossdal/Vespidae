﻿using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Geometry;
using ClipperLib;
using System.Linq;

namespace ClipperHelper
{
    public static class ClipperTools
    {

        public static bool ConvertCurveToPolyline(Curve crv, out Polyline pl)
        {
            if (crv.TryGetPolyline(out pl))
            {
                return true;
            }

            var polylineCurve = crv.ToPolyline(0, 0, 0.1, 0, 0, 0, 0, 0, true);
            if (polylineCurve == null)
            {
                return false;
            }
            if (!polylineCurve.TryGetPolyline(out pl))
            {
                return false;
            }

            return pl.IsValid && !(pl.Length < RhinoMath.ZeroTolerance);
        }

        //Converts collection of curves to polylines
        //return can be changed to IEnumerable and use yield??
        public static List<Polyline> ConvertCurvesToPolylines(IEnumerable<Curve> crvs)
        {
            var polys = new List<Polyline>();

            foreach (var c in crvs)
            {
                Polyline poly;
                if (ConvertCurveToPolyline(c, out poly))
                {
                    polys.Add(poly);
                }
            }
            return polys;
        }

        //perform boolean operation on curves 
        public static List<Polyline> boolean(IEnumerable<Polyline> A, IEnumerable<Polyline> B, Plane pln, double tolerance, int type)
        {
            List<Polyline> result = new List<Polyline>();

            var clip = new Clipper();
            var polyfilltype = PolyFillType.pftEvenOdd;

            foreach (var plA in A)
            {
                clip.AddPath(ToPath2d(plA, tolerance), PolyType.ptSubject, plA.IsClosed);
            }

            foreach (var plB in B)
            {
                clip.AddPath(ToPath2d(plB, tolerance), PolyType.ptClip, true);
            }

            PolyTree polytree = new PolyTree();
            var ctype = new ClipType();

            switch (type)
            {
                case 0:
                    ctype = ClipType.ctDifference;
                    break;
                case 1:
                    ctype = ClipType.ctIntersection;
                    break;
                case 2:
                    ctype = ClipType.ctUnion;
                    break;
                case 3:
                    ctype = ClipType.ctXor;
                    break;
            }

            clip.Execute(ctype, polytree, polyfilltype, polyfilltype);

            var output = new List<Polyline>();

            foreach (var pn in polytree.Iterate())
            {
                if (pn.Contour.Count > 1)
                {
                    output.Add(ToPolyline(pn.Contour, pln, tolerance, !pn.IsOpen));
                }
            }

            return output;
        }

        //perform offset operation on curve 
        public static List<Polyline> offset(IEnumerable<Polyline> polysToOffset, int amount, Plane pln, double distance, double tolerance)
        {
            /*
                    How do we handle not-closed polygons?

                    see: http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/EndType.htm

                    etClosedPolygon: Ends are joined using the JoinType value and the path filled as a polygon
                    etClosedLine: Ends are joined using the JoinType value and the path filled as a polyline
                    etOpenSquare: Ends are squared off and extended delta units
                    etOpenRound: Ends are rounded off and extended delta units
                    etOpenButt: Ends are squared off with no extension
            */

            List<Polyline> output = new List<Polyline>();
            ClipperOffset clipOfs = new ClipperOffset();

            //keep height hack. Needs better logic

            foreach (Polyline poly in polysToOffset)
            {
                if (poly.IsClosed)
                {
                    clipOfs.AddPath(ToPath2d(poly, tolerance), JoinType.jtRound, EndType.etClosedLine);
                }
                else
                {
                    clipOfs.AddPath(ToPath2d(poly, tolerance), JoinType.jtRound, EndType.etOpenRound);
                }
            }

            PolyTree polytree = new PolyTree();
            double delta = distance;

            clipOfs.Execute(ref polytree, delta / tolerance);

            for (int i = 0; i < amount; i++)
            {
                clipOfs.Execute(ref polytree, delta / tolerance);
                output.AddRange(outerFunc(polytree,amount));
                delta += distance;
            }   

            return output;
        }

        public static List<Polyline> outerFunc(PolyNode n, int amount )
        {
            int index = 0;
            var flatSolution = new List<Polyline>();
            var solution = new Dictionary<int, List<Polyline>>();
            /// we loop through all outer "shells" run recursive function
            /// on all "sub-childs"
            foreach (var cn in n.Childs)
            {
                Rhino.RhinoApp.WriteLine($"new list: {index}");
                solution[index] = new List<Polyline>();

                iteratev2(cn, 0, solution[index], 1, false);
                index++;
            }
            foreach (var group in solution)
            {
                foreach (var poly in group.Value)
                {
                    flatSolution.Add(poly);
                }
            }
            return flatSolution;
        }

        public static void iteratev2(PolyNode n, int depth, List<Polyline> lst, int goalDepth, bool bigSmall)
        {
            if (depth == goalDepth)
            {
                lst.Add(ToPolyline(n.Contour, Plane.WorldXY, .001, true));
                if (bigSmall)
                {
                    goalDepth = depth + 3;
                }
                else {
                    goalDepth = depth + 1; 
                }
                bigSmall = !bigSmall;
            }

            foreach (var pn in n.Childs)
            {
                Rhino.RhinoApp.WriteLine($"new level: {depth}");
                iteratev2(pn, depth+1, lst, goalDepth, bigSmall);
            }
        }

        //borrowed from original grasshopper clipper lib 
        public static IEnumerable<PolyNode> Iterate(this PolyNode node)
        {
            yield return node;
            foreach (var childNode in node.Childs)
            {
                foreach (var childNodeItem in childNode.Iterate())
                {
                    yield return childNodeItem;
                }
            }
        }



        public static List<Polyline> f_iterate(PolyNode n, int depth, double tolerance, Plane pln)
        {
            Rhino.RhinoApp.WriteLine(depth.ToString());

            var output = new List<Polyline>();

            //Needs better logic to handle more depth 
            if (depth == 2 || depth == 3)
            {
                var poly = ToPolyline(n.Contour, pln, tolerance, true);
                if (poly.Length > 0)
                {
                    output.Add(poly);
                }
            }

            foreach (var child in n.Childs)
            {
                output.AddRange(f_iterate(child, depth + 1, tolerance, pln));
            }

            return output;
        }

        //extracts polylines from a polytree. This should be sensitive to inside/outside ref slicerFriendliness 
        private static List<Polyline> extractSolution(PolyTree sol, Plane pln, double tolerance)
        {
            List<Polyline> output = new List<Polyline>();

            return f_iterate(sol, 0, tolerance, pln);
        }

        //converts list of Clipper Intpoints to Polylines
        public static Polyline ToPolyline(List<IntPoint> path, Plane pln, double tolerance, bool closed)
        {
            var polyline = new Polyline();

            foreach (var pt in path)
            {
                polyline.Add(pt.X * tolerance, pt.Y * tolerance, pln.OriginZ);
            }

            if (closed && path.Count > 0)
            {
                polyline.Add(polyline.First);
            }

            return polyline;
        }

        public static List<IntPoint> ToPath2d(this Polyline pl, double tolerance)
        {
            var path = new List<IntPoint>();
            foreach (var pt in pl)
            {
                path.Add(ToIntPoint2d(pt, tolerance));
            }
            return path;
        }

        private static IntPoint ToIntPoint2d(Point3d pt, double tolerance)
        {
            var point = new IntPoint(((long)(pt.X / tolerance)), ((long)(pt.Y / tolerance)));
            return point;
        }
    }

    /// <summary>
    /// Static class for creating infill patterns. Most functions take closed
    /// polygons as input and outputs infill patterns. See documentation
    /// for more information
    /// </summary>
    public static class Infill
    {

        static public List<Polyline> simpleInfill(Polyline pol, double gap)
        {
            var output = new List<Polyline>();
            var bound = pol.BoundingBox;
            var min = bound.Min;
            var max = bound.Max;

            //iterate and create infill lines 
            for (double i = min.X; i <= max.X + gap; i += gap)
            {
                var poly = new Polyline();
                poly.Add(i, min.Y, 0);
                poly.Add(i, max.Y, 0);
                output.Add(poly);
            }
            return output;
        }

        /// <summary>
        /// Generates infill lines on closed polygons.
        /// </summary>
        /// <param name="pol">closed polyline to be infilled</param>
        /// <param name="gap">distance between infill lines</param>
        /// <param name="angle">direction angle in degrees of infill lines. </param>
        /// <param name="pln">output plane.</param>
        /// <returns></returns>
        static public List<Polyline> contInfill(Polyline pol, double gap, double angle, Plane pln)
        {
            var output = new List<Polyline>();
            var bound = pol.BoundingBox;

            //convert from degree to radians
            double rad = RhinoMath.ToRadians(angle);

            //get length of polygon bounding box diagonal
            double length = bound.Diagonal.Length;

            //solution dictionary
            //var solution = new SortedDictionary<int, Polyline>();
            var solution2 = new SortedDictionary<int, List<Polyline>>();
            int numIntersections = -10;
            int dictIndex = 0;
            bool flip = true;

            //iterate and create infill lines 
            for (double i = -length / 2; i <= length + gap; i += gap)
            {
                //create line
                //var line = new Polyline();
                //line.Add(i, min.Y, 0);
                //line.Add(i, max.Y, 0);

                var newLine = new Polyline();
                newLine.Add(bound.Center.X + i, bound.Center.Y + length / 2, bound.Center.Z);
                newLine.Add(bound.Center.X + i, bound.Center.Y - length / 2, bound.Center.Z);
                var tr = Rhino.Geometry.Transform.Rotation(rad, bound.Center);
                newLine.Transform(tr);


                //check intersection
                var intersectLines = ClipperTools.boolean(new List<Polyline> { newLine }, new List<Polyline> { pol }, pln, 0.001, 1);

                if (intersectLines.Count != numIntersections)
                {
                    numIntersections = intersectLines.Count;
                    foreach (var p in intersectLines)
                    {
                        //solution.Add(dictIndex++, p);
                        solution2.Add(dictIndex++, new List<Polyline> { p });
                    }
                }

                else
                {
                    int reverseIndex = numIntersections;
                    foreach (var p in intersectLines)
                    {
                        solution2[dictIndex - reverseIndex].Add(p);
                        //if (flip) {

                        //    //solution[dictIndex - reverseIndex].AddRange(p);

                        //    flip = false; 
                        //}
                        //else {
                        //    //solution[dictIndex - reverseIndex].AddRange(p);
                        //    solution2[dictIndex - reverseIndex].Add(p);
                        //    flip = true; 
                        //}

                        reverseIndex--;
                    }
                }
            }



            foreach (var s in solution2)
            {
                var tempList = brepTools.flipInfillLines(s.Value);
                var path = new Polyline();
                foreach (var p in s.Value)
                {
                    path.AddRange(p);
                }

                output.Add(path);
            }

            return output;
        }

    }

    public static class brepTools
    {

        private static double checkAngle(Vector3d check, Vector3d refVector)
        {
            check.IsPerpendicularTo(refVector);
            check.IsParallelTo(refVector);
            return 3;
        }


        //flips every second polyline in list
        public static List<Polyline> flipInfillLines(List<Polyline> polys)
        {
            var shouldFlip = false;

            foreach (var poly in polys)
            {
                if (shouldFlip)
                {
                    poly.Reverse();
                    shouldFlip = false;
                }
                else
                {
                    shouldFlip = true;
                }
            }
            return polys;
        }

        public static List<Polyline> sortPolys(List<Polyline> polys)
        {
            return polys.OrderBy(p => p[0].X).ToList();
        }

        //connects infillPolys. Needs more logic. I think...
        public static Polyline connectPolys(List<Polyline> polys)
        {
            var output = new Polyline();
            foreach (var poly in polys)
            {
                foreach (var p in poly)
                {
                    output.Add(p);
                }
            }
            return output;
        }

    }
}
