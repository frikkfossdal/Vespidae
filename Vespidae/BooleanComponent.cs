using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using ClipperHelper; 


namespace Vespidae
{
    public class BooleanComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public BooleanComponent()
          : base("Boolean", "Vespidae",
            "Test of clipper library",
            "Vespidae", "ClipperTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("A", "A", "", GH_ParamAccess.list);
            pManager.AddCurveParameter("B", "B", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("ClipType", "CT", "Clipping type. 0 : difference, 1: intersection, 2: union, 3: xor", GH_ParamAccess.item,0);
            pManager.AddPlaneParameter("OutputPlane", "pln", "Plane to output solution to", GH_ParamAccess.item, new Plane()); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("result1", "r1", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        { 

            List<Curve> curvesA = new List<Curve>();
            List<Curve> curvesB = new List<Curve>();
            int clipNumber = 0;
            Plane pln = new Plane(); 

            if (!DA.GetDataList("A", curvesA)) return;
            if (!DA.GetDataList("B", curvesB))return;
            DA.GetData("ClipType", ref clipNumber);
            DA.GetData("OutputPlane", ref pln); 

            List<Polyline> test1 = ClipperTools.ConvertCurvesToPolylines(curvesA);
            List<Polyline> test2 = ClipperTools.ConvertCurvesToPolylines(curvesB);

            var result = ClipperTools.intersection(test1, test2, pln,RhinoDoc.ActiveDoc.ModelAbsoluteTolerance,clipNumber); 

            DA.SetDataList(0, result);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("AB869523-12EB-43C6-A313-C989C29E929A");
    }

}
