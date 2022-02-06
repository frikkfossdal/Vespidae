using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GMaker; 

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
          : base("Vespmo_Gcode_Component", "VespMoGcode",
            "Converts VESPMO object to gcode files",
            "Vespidae", "Gcode Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("VESPMO", "VESPMO", "Vespida Move objects", GH_ParamAccess.list);
            pManager.AddTextParameter("header", "h", "optional gcode header", GH_ParamAccess.list);
            pManager.AddTextParameter("footer", "f", "optional gcode footer", GH_ParamAccess.list); 
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
            List<Move> moves = new List<Move>();
            List<String> gcode = new List<string>();
            List<String> header = new List<string>();
            List<String> footer = new List<string>();

            if (!DA.GetDataList("VESPMO", moves)) return;
            DA.GetDataList("header", header);
            DA.GetDataList("footer", footer);

            if (header.Count > 0) {
                gcode.AddRange(header);
            }

            gcode = GConvert.convertOperation(moves);

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
            get { return new Guid("7b2fa908-e881-4bf6-96bd-68d5ef549b09"); }
        }
    }
}
