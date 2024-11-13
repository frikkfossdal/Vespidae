using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools; 

namespace Vespidae.Ops
{
    public class SortActionsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SortActionsComponent()
          : base("SortActionsComponent", "SortActions",
            "SortActionsComponent description",
            "Vespidae", "2.Actions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Actions", "actions", "actions to sort", GH_ParamAccess.list);
            pManager.AddGenericParameter("Sort", "sort_string", "Sort options: X, Y, Z, T(ool). Write the order that you'd like to sort by. " +
                "Any variables left unwritten will be sorted by default order: TZXY", GH_ParamAccess.item);
            //pManager.AddIntegerParameter("SortType", "sort", "Sorting options. 0: x-direction, 1: y-direction, 2: z-direction, 3: by tool", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("FlipX", "flipX", "Flips X direction of list of acitons. Default false", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("FlipY", "flipY", "Flips Y direction of list of acitons. Default false", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("FlipZ", "flipZ", "Flips Z direction of list of acitons. Default false", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("FlipT", "flipT", "Flips tool order of list of acitons. Default false", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("SortedActions", "actions", "sorted actions", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var actions = new List<VespidaeTools.Action>();
            string sort_order = "TZXY";
            //int option = 0;
            bool flipX = false;
            bool flipY = false;
            bool flipZ = false;
            bool flipT = false;

            if (!DA.GetDataList("Actions", actions)) return;
            DA.GetData("Sort", ref sort_order);
            //DA.GetData("SortType", ref option);
            DA.GetData("FlipX", ref flipX);
            DA.GetData("FlipY", ref flipY);
            DA.GetData("FlipZ", ref flipZ);
            DA.GetData("FlipT", ref flipT);

            List<bool> flips = new List<bool> { flipX, flipY, flipZ, flipT };

            List<char> order = sort_order.ToCharArray().ToList();
            if (!sort_order.Contains("X") || !sort_order.Contains("Y") || !sort_order.Contains("Z") || 
                !sort_order.Contains("T") || order.Count != 4)
            {
                throw new ArgumentException("Sort order must contain X,Y,Z, and T exactly once each. Cannot use other characters.");
            }

            actions = VespidaeTools.Sort.SortByString(actions, order, flips);

            //switch (option) {
                
            //    case 0:
            //        //sort in x direction
            //        actions = VespidaeTools.Sort.sortByX(actions,flip); 
            //        break;

            //    case 1:
            //        //sort in y direction
            //        actions = VespidaeTools.Sort.sortByY(actions,flip);
            //        break;

            //    case 2:
            //        //sort in z direction
            //        actions = VespidaeTools.Sort.sortByZ(actions,flip);
            //        break;
            //    case 3:
            //        //sort by tool
            //        actions = VespidaeTools.Sort.sortByTool(actions, flip);
            //        break;

            //    default:
            //        //no sorting
            //        break; 
            //}
            DA.SetDataList("SortedActions", actions); 
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
                return Resources.Resources.sortActions;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5d769569-7564-40e3-a497-57a1a7b03e28"); }
        }
    }
}
