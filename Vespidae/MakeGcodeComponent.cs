using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ClipperHelper;
using GMaker;


namespace Vespidae
{
    public class MakeGcodeComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MakeGcodeComponent()
          : base("MakeGcodeComponent", "Make Gcode",
            "Converts polylines to gcode and visualizes machine operation (deleteMe)",
            "Vespidae", "undefined")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Polylines", "P", "polylines to convert", GH_ParamAccess.list);
            pManager.AddIntegerParameter("TravelSpeed", "TS", "speed of travels between operations m/s", GH_ParamAccess.item, 4000);
            pManager.AddIntegerParameter("WorkSpeed", "WS", "speed of operation itself m/s", GH_ParamAccess.item, 3000);
            pManager.AddNumberParameter("RetractHeight", "RH", "retraction height between operations", GH_ParamAccess.item, 50);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("gcode", "G", "Output gcode", GH_ParamAccess.list);
            pManager.AddGenericParameter("toolpaths", "TP", "Output toolpaths for visualization", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var curves = new List<Curve>();
            int ts = 0;
            int ws = 0;
            double rh = 0;
            GTools gtools = new GTools();

            if (!DA.GetDataList("Polylines", curves)) return;
            DA.GetData("TravelSpeed", ref ts);
            DA.GetData("WorkSpeed", ref ws);
            DA.GetData<double>("RetractHeight", ref rh);

            List<Polyline> polys = ClipperTools.ConvertCurvesToPolylines(curves);
            List<String> outputGcode = gtools.MakeGcode(polys, ts, ws, rh);

            DA.SetDataList("gcode", outputGcode);
            DA.SetDataList("toolpaths", gtools.outputPaths);

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
            get { return new Guid("8b5cccb4-2352-4a10-a862-3757f188d64e"); }
        }
    }
}
