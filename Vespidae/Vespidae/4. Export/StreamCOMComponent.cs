using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Coms;
using System.Threading.Tasks;
using System.Drawing.Printing;
using System.IO.Ports;
using System.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using Rhino.UI;


namespace Vespidae.Coms
{
    public class StreamCOMComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public StreamCOMComponent()
          : base("StreamCOMComponent", "Stream GCode via COM",
            "Streams GCode via COM device (i.e. USB). Right click the component to choose the COM port.",
            "Vespidae", "4.Export")
        {
        }

        public string comPort;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("gcode", "gcode", "GCode to be sent to the printer", GH_ParamAccess.list,"default");
            pManager.AddBooleanParameter("sendCode", "snd", "description", GH_ParamAccess.item, false); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("response", "res", "controller response", GH_ParamAccess.item); 
        }

        public override bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            writer.SetString("ComPort", "Unset");
            return base.Write(writer);
        }

        public override bool Read(GH_IO.Serialization.GH_IReader reader)
        {

            comPort = reader.GetString("ComPort");
            return base.Read(reader);
        }

        protected override void AppendAdditionalComponentMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            List<string> availablePorts = SerialPort.GetPortNames().ToList();
            base.AppendAdditionalComponentMenuItems(menu);
            foreach(var port in availablePorts)
            {
                Menu_AppendItem(menu, port.ToString(), Menu_COMSelected, true, port == this.comPort).Tag = port;
            }

        }
                
        private void Menu_COMSelected(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string)
            {
                this.comPort = (string)item.Tag;
                ExpireSolution(true);
            }
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
            string com = "6";
            

            DA.GetDataList("gcode", gcode);
            DA.GetData("sendCode", ref send);
            com = this.comPort;

            List<string> availablePorts = SerialPort.GetPortNames().ToList();
            Debug.WriteLine(availablePorts);
            if (send) {
                httpComs.streamGcodeCOM(gcode, com, send);
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
                return Resources.Resources.com;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("76A3CBF4-4D1E-4DD8-8113-5070674796A9"); }
        }
    }
}
