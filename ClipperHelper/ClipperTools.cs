using System;
using System.Collections.Generic;
using Rhino; 
using Rhino.Geometry;
using ClipperLib;
using System.Linq; 

namespace ClipperHelper
{
    public static class ClipperTools
    {

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
            if (!polylineCurve.TryGetPolyline(out pl)) {
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
        public static List<Polyline> intersection(IEnumerable<Polyline> A, IEnumerable<Polyline> B, int type) {
            List<Polyline> result = new List<Polyline>();

            var clip = new Clipper();
            
            var polyfilltype = PolyFillType.pftEvenOdd;

            foreach (var plA in A) {
                clip.AddPath(ToPath2d(plA), PolyType.ptSubject,plA.IsClosed);
            }

            foreach (var plB in B) {
                clip.AddPath(ToPath2d(plB), PolyType.ptClip,plB.IsClosed);
            }

            PolyTree polytree = new PolyTree();
            var ctype = new ClipType();

            switch (type) {
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
            if (polytree.Childs.Count > 0)
            {
                foreach (var c in polytree.Childs) {
                    output.Add(twoDtothreeD.toPolyline(c.Contour));
                }
            }
            
            return output;
        }

        //perform offset operation on curve 
        //public static List<Polyline> offset(IEnumerable<Polyline> polysToOffset, double distance) {
        //    List<Polyline> output = new List<Polyline>();
        //    List<List<IntPoint>> input = new List<List<IntPoint>>(); 

        //    foreach (Polyline poly in polysToOffset) {
        //        input.Add(ToPath2d(poly)); 
        //    }
        //    off
        //    List<List<IntPoint>> result = Clipper.OffsetPolygons(input, distance);

        //    foreach (List<IntPoint> path in result) {
        //        output.Add(twoDtothreeD.toPolyline(path));
        //    }

        //    return output;
        //}

        //perform series of offset operation on curve


        public static List<IntPoint> ToPath2d(this Polyline pl) {
            var path = new List<IntPoint>();
            foreach (var pt in pl) {
                path.Add(ToIntPoint2d(pt));
            }
            return path;
        }

        private static IntPoint ToIntPoint2d(this Point3d pt)
        {
            var point = new IntPoint(((long)(pt.X/0.01)), ((long)(pt.Y/0.01)));
            return point;
        }
    }

    public static class twoDtothreeD{
        public static Polyline toPolyline(List<IntPoint> path) {
            var result = new Polyline();

            foreach (var pt in path) {
                result.Add(new Point3d(((float)(pt.X*0.01)), ((float)(pt.Y*0.01)), 0));
            }
            return result; 
        }
    }

    public static class brepTools {

        private static double checkAngle(Vector3d check, Vector3d refVector) {
            check.IsPerpendicularTo(refVector);
            check.IsParallelTo(refVector);
            return 3;
        }

        //Creates infill in x direction 
        public static List<Polyline> createInfillLines(Brep b, double gap){

            var output = new List<Polyline>(); 
            var bound = b.GetBoundingBox(true);
            var min = bound.Min;
            var max = bound.Max;

            for (double i = min.X; i <= max.X+gap; i+=gap) {
                var poly = new Polyline();
                poly.Add(i, min.Y, 0);
                poly.Add(i, max.Y, 0);
                output.Add(poly); 
            }
            return output; 
        }

        //flips every second polyline in list
        public static List<Polyline> flipInfillLines(List<Polyline> polys) {
            var shouldFlip = false;

            foreach (var poly in polys) {
                if (shouldFlip)
                {
                    poly.Reverse();
                    shouldFlip = false;
                }
                else {
                    shouldFlip = true; 
                }
            }
            return polys; 
        }

        public static List<Polyline> sortPolys(List<Polyline> polys) {
            return polys.OrderBy(p => p[0].X).ToList(); 
        }

        //connects infillPolys. Needs more logic. I think...
        public static Polyline connectPolys(List<Polyline> polys)
        {
            var output = new Polyline(); 
            foreach (var poly in polys) {
                foreach (var p in poly) {
                    output.Add(p); 
                }
            }
            return output; 
        }
            
    }
}
