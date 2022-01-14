using System;
using System.Collections.Generic;
using Rhino; 
using Rhino.Geometry;
using ClipperLib;

namespace ClipperHelper
{
    public static class ClipperTools
    {
        public static Point3d test() {
            return new Point3d(5, 5, 0);

        }

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

        public static List<Polyline> boolean(IEnumerable<Polyline> A, IEnumerable<Polyline> B) {
            List<Polyline> result = new List<Polyline>();

            var clip = new Clipper();
            var polyfilltype = PolyFillType.pftEvenOdd;

            foreach (var plA in A) {
                clip.AddPolygon(ToPath2d(plA), PolyType.ptSubject);
            }

            foreach (var plB in B) {
                clip.AddPolygon(ToPath2d(plB), PolyType.ptClip);
            }

            PolyTree polytree = new PolyTree();

            clip.Execute(ClipType.ctUnion, polytree, polyfilltype, polyfilltype);

            if (polytree.Childs.Count > 0)
            {
                
                var output = twoDtothreeD.toPolyline(polytree.Childs[0].Contour);
                result.Add(output);
            }
            
            return result;
        }

        public static List<Polyline> offset(IEnumerable<Polyline> polysToOffset, double distance) {
            List<Polyline> output = new List<Polyline>();
            List<List<IntPoint>> input = new List<List<IntPoint>>(); 

            foreach (Polyline poly in polysToOffset) {
                input.Add(ToPath2d(poly)); 
            }

            List<List<IntPoint>> result = Clipper.OffsetPolygons(input, distance);

            foreach (List<IntPoint> path in result) {
                output.Add(twoDtothreeD.toPolyline(path));
            }

            return output;
        }

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
}
