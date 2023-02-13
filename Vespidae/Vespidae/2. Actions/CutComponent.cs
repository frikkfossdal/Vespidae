using System;
using System.Collections.Generic;
using ClipperHelper;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Vespidae.Actions
{
  public class CutComponent : GH_Component
  {
    /// <summary>
    /// Each implementation of GH_Component must provide a public 
    /// constructor without any arguments.
    /// Category represents the Tab in which the component will appear, 
    /// Subcategory the panel. If you use non-existing tab or panel names, 
    /// new tabs/panels will automatically be created.
    /// </summary>
    public CutComponent()
      : base("CutComponent", "VespCut",
            "This is to cut stuff",
            "Vespidae", "2.Actions")
    {
    }

    /// <summary>
    /// Registers all the input parameters for this component.
    /// </summary>
    protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
    {
            pManager.AddCurveParameter("Curve", "c", "curves to extrude", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Feedrate", "feedrate", "cut feedrate in mm/min", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("SpindleSpeed", "s_speed", "speed of spindle in rounds/min. Default 18000rounds/min", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("ToolId", "to", "tool id that performs operation. Defaults to t0", GH_ParamAccess.item, 0);
            pManager.AddTextParameter("GcodeInjection", "gInj", "gcode to inject before operation", GH_ParamAccess.list, "");
            pManager.AddTextParameter("GcodePostInjection", "gPost", "gcode to inject after operation", GH_ParamAccess.list, "");
        }

    /// <summary>
    /// Registers all the output parameters for this component.
    /// </summary>
    protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
    {
          pManager.AddGenericParameter("VespObj", "VObj", "Vespidae action objects", GH_ParamAccess.list); 

    }

    /// <summary>
    /// This is the method that actually does the work.
    /// </summary>
    /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
    /// to store data in output parameters.</param>
    protected override void SolveInstance(IGH_DataAccess DA)
    {
            List<Curve> crv = new List<Curve>();
            List<String> gInj = new List<string>();

            int spindleSpeed = 0;
            int feedrate = 0; 
            double ext = 0;
            double temp = 0;
            int tool = 0;

            if (!DA.GetDataList("Curve", crv)) return;

            DA.GetData("Feedrate", ref feedrate);
            DA.GetData("SpindleSpeed", ref spindleSpeed);
            DA.GetData("ToolId", ref tool);

            DA.GetDataList("GcodeInjection", gInj);


            var pol = ClipperTools.ConvertCurvesToPolylines(crv);

            var actions = VespidaeTools.Operation.createCutOps(pol, feedrate, spindleSpeed, tool, gInj);

            DA.SetDataList("VespObj", actions);
            //ops.createActions();
            //
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
        return Resources.Resources.cut;
      }
    }

    /// <summary>
    /// Each component must have a unique Guid to identify it. 
    /// It is vital this Guid doesn't change otherwise old ghx files 
    /// that use the old ID will partially fail during loading.
    /// </summary>
    public override Guid ComponentGuid
    {
      get { return new Guid("d85e28e0-2b0f-4966-a546-f3ea7578d207"); }
    }
  }
}
