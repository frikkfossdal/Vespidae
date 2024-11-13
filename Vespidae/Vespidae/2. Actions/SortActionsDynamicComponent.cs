using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using VespidaeTools; 

namespace Vespidae.Ops
{
    public class SortActionsDynamicComponent : GH_Component, IGH_VariableParameterComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SortActionsDynamicComponent()
          : base("SortActionsDynamicComponent", "SortActionsDynamic",
            "Sorts Actions based on the order of actions input",
            "Vespidae", "2.Actions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            int i = 0;
            List<int> indices = new List<int>();
            i = pManager.AddGenericParameter("Sort", "sort_string", "Sort options: X, Y, Z, T(ool). Write the order that you'd like to sort by. " +
                "Any variables left unwritten will be sorted by default order: TZXY", GH_ParamAccess.item);
            this.Params.Input[i].Optional = false;         
            i = pManager.AddBooleanParameter("FlipX", "flipX", "Flips X direction of list of acitons. Default false", GH_ParamAccess.item, false);
            this.Params.Input[i].Optional = false;
            i = pManager.AddBooleanParameter("FlipY", "flipY", "Flips Y direction of list of acitons. Default false", GH_ParamAccess.item, false);
            this.Params.Input[i].Optional = false;
            i = pManager.AddBooleanParameter("FlipZ", "flipZ", "Flips Z direction of list of acitons. Default false", GH_ParamAccess.item, false);
            this.Params.Input[i].Optional = false; 
            i = pManager.AddBooleanParameter("FlipT", "flipT", "Flips tool order of list of acitons. Default false", GH_ParamAccess.item, false);
            this.Params.Input[i].Optional = false; 
            i = pManager.AddGenericParameter("Actions", "Actions0", "Action to sort. Can add additional action streams.", GH_ParamAccess.list);
            this.Params.Input[i].Optional = false;
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
            bool flipX = false;
            bool flipY = false;
            bool flipZ = false;
            bool flipT = false;
            var extra_actions = new List<VespidaeTools.Action>();
            var list_actions = new List<List<VespidaeTools.Action>>();
            var sorted_actions = new List<VespidaeTools.Action>();

            if (!DA.GetDataList("Actions", actions)) return;
            DA.GetData("Sort", ref sort_order);
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
            int num_params = this.Params.Input.Count;
            int num_actions = num_params - 6;
            sorted_actions = VespidaeTools.Sort.SortByString(actions, order, flips);

            for (int i = 6; i < this.Params.Input.Count; i++)
            {
                if (DA.GetDataList(i, extra_actions))
                {
                    sorted_actions.AddRange(VespidaeTools.Sort.SortByString(extra_actions, order, flips));
                }
            }

            DA.SetDataList("SortedActions", sorted_actions); 
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            // Only insert parameters on input side. This can be changed if you like/need
            // side== GH_ParameterSide.Output
            if (side == GH_ParameterSide.Input && index == this.Params.Input.Count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && this.Params.Input[index].Optional == true)
            {
                return true;
            }
            else { return false; }

        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_GenericObject param = new Param_GenericObject();

            // Has to return a parameter object!


            int count = Params.Input.Count - 5;
            

            param.Name = "Actions" + count.ToString();
            param.NickName = param.Name;
            param.Description = "A stream for more actions";
            param.Optional = true;
            param.Access = GH_ParamAccess.list;
            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                return Params.UnregisterInputParameter(Params.Input[index]);
            }
            else
            {
                return false;
            }

        }

        public void VariableParameterMaintenance()
        {

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
                return Resources.Resources.sortActionsDynamic;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("703A12F6-3492-4500-BA11-BCBC8CC3E6CB"); }
        }
    }
}
