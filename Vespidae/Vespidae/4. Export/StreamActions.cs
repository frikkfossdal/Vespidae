using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Coms;
using VespidaeTools;

namespace Vespidae.Export
{
    public class StreamActions : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear,
        /// Subcategory the panel. If you use non-existing tab or panel names,
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StreamActions()
            : base("StreamActions", "Nickname", "StreamActions description", "Vespidae", "4.Export")
        { }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter(
                "Actions",
                "VObj",
                "Actions to be solved",
                GH_ParamAccess.list
            );
            pManager.AddBooleanParameter(
                "send",
                "send",
                "toggle for executing POST request",
                GH_ParamAccess.item
            );
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(
            GH_Component.GH_OutputParamManager pManager
        ) { }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<VespidaeTools.Action> actions = new List<VespidaeTools.Action>();

            bool send = false;
            if (!DA.GetDataList("Actions", actions))
                return;
            DA.GetData("send", ref send);

            if (send)
            {
                httpComs.streamActions(actions);
            }
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
            get { return new Guid("4c7bffde-1318-40dc-a52d-f0fd1155b795"); }
        }
    }
}
