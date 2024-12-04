using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools; 

namespace Vespidae
{
    public class Vespmo_Gcode_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Vespmo_Gcode_Component()
          : base("VespidaeGcode", "Vespidae_to_Gcode",
            "Converts Vespidae Actions to Gcode",
            "Vespidae", "4.Export")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("actions", "VObj", "Vespidae action objects", GH_ParamAccess.list);
            pManager.AddTextParameter("header", "h", "optional gcode header", GH_ParamAccess.list, "");
            pManager.AddTextParameter("footer", "f", "optional gcode footer", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("absolute", "abs", "absolute or relative extrusion", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("toolchange", "toolchange?", "sets if it is possible to change tools on printer", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("gcode", "Gcode", "output gcode", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<VespidaeTools.Action> actions = new List<VespidaeTools.Action>();
            List<String> gcode = new List<string>();
            List<String> header = new List<string>();
            List<String> footer = new List<string>();
            bool abs = false;
            bool tc = false;

            if (!DA.GetDataList("actions", actions)) return;
            DA.GetDataList("header", header);
            DA.GetDataList("footer", footer);
            DA.GetData("absolute", ref abs);
            DA.GetData("toolchange", ref tc);

            if (header.Count > 0) {
                gcode.AddRange(header);
            }

            
            gcode.AddRange(VespidaeTools.Operation.translateToGcode(actions,abs, tc));

            if (footer.Count > 0) {
                gcode.AddRange(footer); 
            }
            DA.SetDataList("gcode", gcode);
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
                return Resources.Resources.gcode;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7b2fa908-e881-4bf6-96bd-68d5ef549b09"); }
        }
    }
}
