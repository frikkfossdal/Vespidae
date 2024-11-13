using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino; 
using Rhino.Geometry;
using ClipperHelper;

namespace Vespidae
{
    public class InfillComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public InfillComponent()
          : base("InfillComponent", "Infill",
            "Create infill operations on top of slice",
            "Vespidae", "1.Toolpath")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("curve", "crv", "closed polygon to be filled", GH_ParamAccess.list);
            pManager.AddNumberParameter("density", "den", "infill density. Hard limit on 0.05", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("offset", "offset", "infill offset", GH_ParamAccess.item, 0.2);
            pManager.AddNumberParameter("infillAngle", "ang", "direciion angle of infill lines. Default value: 0", GH_ParamAccess.item,0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("curves", "crvs", "infill polygons", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var inputCurves = new List<Curve>();
            double density = 0;
            double offset = 0;
            double angle = 0; 

            if (!DA.GetDataList("curve", inputCurves)) return;
            DA.GetData("density", ref density);
            DA.GetData("offset", ref offset);
            DA.GetData("infillAngle", ref angle); 

            //sanity check on input density to stop infinite computation
            if (density < 0.05) {
                density = 0.05; 
            }

            var outputCurves = new List<Polyline>();
            var newCurve = new List<Polyline>();

            var layerIndex = ClipperTools.createLayerLookup(inputCurves);
            var solutionPlane = Plane.WorldXY;

            var offsetPolygons = new List<Polyline>();

                //needs check for non-closed polygons 
                foreach (var layer in layerIndex)
                {
                    solutionPlane.OriginZ = layer.Key;
                    offsetPolygons = ClipperTools.offset(layer.Value, 1, solutionPlane, offset, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
                    outputCurves.AddRange(Infill.contInfill(offsetPolygons, density, angle, solutionPlane));
                }

            ////convert curves to polylines and remove open curves
            //var infillPolys = new List<Polyline>() ; 
            //foreach (var crv in inputCurves) {
            //    Polyline pol;
            //    Plane pln;
            //    if (ClipperHelper.ClipperTools.ConvertCurveToPolyline(crv, out pol) && crv.TryGetPlane(out pln)) {
            //        infillPolys.Add(pol); 
            //    } 
            //}

            ////get dictionaries
            //var infillShapes = ClipperHelper.ClipperTools.offsetForInfill(infillPolys, 1, Plane.WorldXY, offset, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance, newCurve);

            //foreach (var group in infillShapes) {
            //    outputCurves.AddRange(ClipperHelper.Infill.contInfill(group.Value, density, angle, Plane.WorldXY)); 
            //}

            DA.SetDataList("curves", outputCurves); 
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                return Resources.Resources.infill;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("ef5e632d-bdec-4094-92b7-d11d73c53198"); }
        }
    }
}
