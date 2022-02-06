using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GMaker; 

namespace Vespidae
{
    public class ZPinning : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ZPinning()
          : base("ZPinning", "ZPin",
            "ZPinning description",
            "Vespidae", "Operations")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("inputPoints", "pnts", "point to zpin", GH_ParamAccess.list);
            pManager.AddIntegerParameter("startHeight", "start", "start extrusion height", GH_ParamAccess.item,5);
            pManager.AddIntegerParameter("stopHeight", "stop", "stop extrusion height", GH_ParamAccess.item,10);
            pManager.AddIntegerParameter("amount", "a", "extrusion amount", GH_ParamAccess.item, 15);
            pManager.AddIntegerParameter("retract", "rh", "retract height", GH_ParamAccess.item, 20);
            pManager.AddIntegerParameter("speed", "s", "pin speed", GH_ParamAccess.item, 50);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("moves", "VESPMO", "Vespidae Moves", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Move> output = new List<Move>(); 

            List<Point3d> inpPoints = new List<Point3d>();
            int start = 0;
            int stop = 0;
            int rh = 0;
            int amount = 0;
            int speed = 0; 

            if (!DA.GetDataList("inputPoints", inpPoints)) return;
            DA.GetData("startHeight", ref start);
            DA.GetData("stopHeight", ref stop);
            DA.GetData("amount", ref amount);
            DA.GetData("retract", ref rh);
            DA.GetData("speed", ref speed);

            output = Operations.zPinning(inpPoints, start, stop, amount, rh,speed,  new Point3d(0,0,3));
            DA.SetDataList("moves", output); 
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
            get { return new Guid("e507b6c8-ad82-4cdb-90c9-440f2f8e7195"); }
        }
    }
}
