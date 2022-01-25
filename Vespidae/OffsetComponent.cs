using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using ClipperHelper;

namespace Vespidae
{
    public class OffsetComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OffsetComponent()
          : base("Offset", "Offset",
            "Offsets polylines",
            "Vespidae", "ClipperTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curve or curves to offset", GH_ParamAccess.list); 
            pManager.AddNumberParameter("Distance", "D", "Distance to offset", GH_ParamAccess.item);
            pManager.AddNumberParameter("Number of offsets", "NO", "Number of times to offset curve. Default 1", GH_ParamAccess.item, 1); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("result", "r", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> offsetCurves = new List<Curve>();
            double distance = 0;

            if (!DA.GetDataList("Curve", offsetCurves))return;
            if (!DA.GetData("Distance", ref distance))return;

            List<Polyline> offsetPolylines = ClipperTools.ConvertCurvesToPolylines(offsetCurves);
            List<Polyline> result = ClipperTools.offset(offsetPolylines, distance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);

            DA.SetDataList(0, result); 
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
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0f98fefb-135d-4e93-999b-30d501057087"); }
        }
    }
}
