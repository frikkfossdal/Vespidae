using System;
using System.Collections.Generic;
using Rhino.Geometry;
namespace ClipperTools
{
    public class ClipperGeometry
    {
        public ClipperGeometry()
        {
        }

        public static bool ConvertCurveToPolyline(Curve crv, out Polyline pl)
        {
            if (crv.TryGetPolyline(out pl))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //Converts collection of curves to polylines
        //return can be changed to IEnumerable and use yield??
        public static IEnumerable<Polyline> ConvertCurvesToPolylines(IEnumerable<Curve> crvs) {
            foreach (var c in crvs) {
                if (ConvertCurveToPolyline(c, out var pol)) {
                    yield return pol;    
                }  
            }
        }
    }
}
