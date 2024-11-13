using System;
using System.Collections.Generic;
using VespidaeTools;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Vespidae.Solve
{
    public class AdditiveSolver : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public AdditiveSolver()
          : base("AdditiveSolver", "AdditiveSolver",
            "Solver for additive operations. Sorts actions in ascending order based on z height",
            "Vespidae", "3.Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Actions", "VObj", "Actions to be solved", GH_ParamAccess.list);
            pManager.AddIntegerParameter("RetractHeight", "rh", "retract height between moves", GH_ParamAccess.item, 15);
            pManager.AddIntegerParameter("TravelSpeed", "ts", "travel speed between moves", GH_ParamAccess.item, 5000);
            pManager.AddBooleanParameter("PartialRetract", "pr", "partial retract when possible", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("SortCriteria", "sort", "layer sort criteria. 0: x-direction, 1: y-direction, 2: by tool. Default x", GH_ParamAccess.item, 0); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("OutputActions", "VObj", "New list of actions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<VespidaeTools.Action> actions = new List<VespidaeTools.Action>();
            int rh = 0;
            int ts = 0;
            int srt = 0; 
            bool pr = false;

            if (!DA.GetDataList("Actions", actions)) return;
            DA.GetData("RetractHeight", ref rh);
            DA.GetData("TravelSpeed", ref ts);
            DA.GetData("PartialRetract", ref pr);
            DA.GetData("SortCriteria", ref srt);

            var output = VespidaeTools.Solve.AdditiveSolver(actions, rh, ts, true,srt);
            DA.SetDataList("OutputActions", output);
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
                return Resources.Resources.additive_solver;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8b3e2417-5e4c-44e8-8f59-8c21691c1050"); }
        }
    }
}
