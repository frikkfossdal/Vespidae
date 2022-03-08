using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Coms;
using System.Threading.Tasks; 

namespace Vespidae.Coms
{
    public class UploadGcodeComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public UploadGcodeComponent()
          : base("UploadGcodeComponent", "Upload Gcode",
            "UploadGcodeComponent description",
            "Vespidae", "4.Coms")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("gcode", "gcode", "gcode to be sent to controller. File will be located under /Vespidae", GH_ParamAccess.list,"default");
            pManager.AddTextParameter("filename", "fn", "filename on the controller, /Vespidae/<filename>.gcode", GH_ParamAccess.item, "protomolecule"); 
            pManager.AddTextParameter("ip", "ip", "ip adress of controller", GH_ParamAccess.item, "127.0.0.1"); 
            pManager.AddBooleanParameter("sendCode", "snd", "description", GH_ParamAccess.item, false); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("response", "res", "controller response", GH_ParamAccess.item); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool send = false;
            var gcode = new List<string>(); 
            string filename = "";
            string ip = ""; 

            DA.GetDataList("gcode", gcode);
            DA.GetData("filename", ref filename);
            DA.GetData("ip", ref ip);
            DA.GetData("sendCode", ref send);

            if (send) {
                var comsTask = Task.Run(() => httpComs.sendGcodeTask(gcode,filename,ip));
                comsTask.Wait();
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
                return Resources.Resources.uploadG;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4e823ccb-aee6-42f4-a363-39d82e0e7758"); }
        }
    }
}
