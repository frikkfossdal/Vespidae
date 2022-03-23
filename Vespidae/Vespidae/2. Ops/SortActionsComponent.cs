using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GMaker; 

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
            pManager.AddIntegerParameter("SortType", "sort", "Sorting options. 0: x-direction, 1: y-direction, 2: z-direction", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("Flip", "flip", "Flips direction of list of acitons. Default false", GH_ParamAccess.item, false); 
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
            var actions = new List<GMaker.Action>();
            int option = 0;
            bool flip = false; 

            if(!DA.GetDataList("Actions", actions))return;
            DA.GetData("SortType", ref option);
            DA.GetData("Flip", ref flip); 

            switch (option) {
                
                case 0:
                    //sort in x direction
                    actions = GMaker.Sort.sortByX(actions,flip); 
                    break;

                case 1:
                    //sort in y direction
                    actions = GMaker.Sort.sortByY(actions,flip);
                    break;

                case 2:
                    //sort in z direction
                    actions = GMaker.Sort.sortByZ(actions,flip);
                    break;
                case 3:
                    //sort by tool
                    actions = GMaker.Sort.sortByTool(actions, flip);
                    break;

                default:
                    //no sorting
                    break; 
            }
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
            get { return new Guid("5d769569-7564-40e3-a497-57a1a7b03e28"); }
        }
    }
}
