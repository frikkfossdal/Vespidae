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
        public static List<Polyline> intersection(IEnumerable<Polyline> A,  IEnumerable<Polyline> B, Plane pln, double tolerance, int type) {
            List<Polyline> result = new List<Polyline>();

            var clip = new Clipper();
            var polyfilltype = PolyFillType.pftEvenOdd;

            foreach (var plA in A) {
                clip.AddPath(ToPath2d(plA,tolerance), PolyType.ptSubject,plA.IsClosed);
            }

            foreach (var plB in B) {
                clip.AddPath(ToPath2d(plB,tolerance), PolyType.ptClip,true);
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

            foreach (var pn in polytree.Iterate()) {
                if (pn.Contour.Count > 1) {
                    output.Add(ToPolyline(pn.Contour, pln, tolerance, !pn.IsOpen));
                }
            }

            return output;
        }

        //perform offset operation on curve 
        public static List<Polyline> offset(IEnumerable<Polyline> polysToOffset, Plane pln, double distance, double tolerance)
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
                if (poly.IsClosed) {
                    clipOfs.AddPath(ToPath2d(poly, tolerance), JoinType.jtRound, EndType.etClosedLine);
                }
                else {
                    clipOfs.AddPath(ToPath2d(poly, tolerance), JoinType.jtRound, EndType.etOpenRound);
                }
            }

            PolyTree polytree = new PolyTree();
            clipOfs.Execute(ref polytree, distance/tolerance);

            //potential hack. Needs revision
            foreach (var path in polytree.Iterate()) {
                
                if (path.IsHole)
                {
                    if (path.Contour.Count > 1)
                    {
                        output.Add(ToPolyline(path.Contour, pln, tolerance, !path.IsOpen));
                    }
                }
                else {

                }
       
            }
          
            return output;
        }

        //perform series of offset operation on curve


        public static List<IntPoint> ToPath2d(this Polyline pl, double tolerance) {
            var path = new List<IntPoint>();
            foreach (var pt in pl) {
                path.Add(ToIntPoint2d(pt,tolerance));
            }
            return path;
        }

        private static IntPoint ToIntPoint2d(Point3d pt, double tolerance)
        {
            var point = new IntPoint(((long)(pt.X/tolerance)), ((long)(pt.Y/tolerance)));
            return point;
        }

        public static Polyline ToPolyline(List<IntPoint> path, Plane pln, double tolerance,  bool closed)
        {
            var polylinepts = new List<Point3d>();

            foreach (var pt in path)
            {
                polylinepts.Add(pln.PointAt(pt.X*tolerance, pt.Y*tolerance));
                //polylinepts.Add(new Point3d(((float)(pt.X * tolerance)), ((float)(pt.Y * tolerance)),0));
            }

            if (closed && path.Count > 0)
            {
                polylinepts.Add(polylinepts[0]);
            }
            var poly = new Polyline(polylinepts);

            return poly;
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
