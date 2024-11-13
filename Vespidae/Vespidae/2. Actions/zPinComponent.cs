using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ClipperHelper; 

namespace Vespidae.Ops
{
    public class zPinComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public zPinComponent()
          : base("zPinComponent", "zPin",
            "zPinComponent description",
            "Vespidae", "2.Actions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "c", "curves to extrude", GH_ParamAccess.list);
            pManager.AddNumberParameter("Amount", "am", "extrusion flowrate", GH_ParamAccess.item, .6);
            pManager.AddIntegerParameter("Speed", "s", "speed of move in mm/min", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Temperature", "t", "extrusion temperature", GH_ParamAccess.item, 205);
            pManager.AddIntegerParameter("ToolId", "to", "tool id that performs operation. Defaults to t0", GH_ParamAccess.item, 0);
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
            int speed = 0;
            double amount = 0;
            // int rh = 0;
            double temp = 0;
            int tool = 0;

            if (!DA.GetDataList("Curve", crv)) return;

            DA.GetData("Amount", ref amount);
            DA.GetData("Speed", ref speed);
            DA.GetData("Temperature", ref temp);
            DA.GetData("ToolId", ref tool);

            var pol = ClipperTools.ConvertCurvesToPolylines(crv);
            var actions = VespidaeTools.Operation.createZpinOps(pol, amount, temp, tool);

            DA.SetData("VespObj", actions); 
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
                return Resources.Resources.zpin;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("acbfc3d4-d6c4-4f87-b590-ce9dbbda7fcf"); }
        }
    }
}
