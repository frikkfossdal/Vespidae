using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools; 

namespace Vespidae
{
    public class SetMachineParamsComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SetMachineParamsComponent()
          : base("SetMachineParams", "SetMachineParams",
            "Set printer parameters to generate appropriate headers for the gcode",
            "Vespidae", "4.Export")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("accelXY", "Max XY Accel (mm/s2)", "Maximum XY Acceleration", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("accelZ", "Max Z Accel (mm/s2)", "Maximum Z Acceleration", GH_ParamAccess.item, 20);
            pManager.AddIntegerParameter("accelE", "Max E Accel (mm/s2)", "Maximum E Acceleration", GH_ParamAccess.item, 1500);
            pManager.AddIntegerParameter("feedXY", "Max XY Feedrate (mm/s)", "Maximum XY Feedrate", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("feedZ", "Max Z Feedrate (mm/s)", "Maximum Z Feedrate", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("feedE", "Max E Feedrate (mm/s)", "Maximum E Feedrate", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("accelP", "Starting Print Accel (mm/s2)", "Starting Print Acceleration", GH_ParamAccess.item, 1500);
            pManager.AddIntegerParameter("accelR", "Starting Retract Accel (mm/s2)", "Starting Retract Acceleration", GH_ParamAccess.item, 1500);
            pManager.AddIntegerParameter("accelT", "Starting Travel Accel (mm/s2)", "Starting Travel Acceleration", GH_ParamAccess.item, 1500);
            pManager.AddNumberParameter("jerkXY", "Jerk XY Limits (mm/s)", "Jerk XY limits", GH_ParamAccess.item, 17.0);
            pManager.AddNumberParameter("jerkZ", "Jerk Z Limits (mm/s)", "Jerk Z limits", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("jerkE", "Jerk E Limits (mm/s)", "Jerk E limits", GH_ParamAccess.item, 50.0);
            pManager.AddIntegerParameter("bedTemp", "Bed Temp (degC)", "Set Bed Temperature", GH_ParamAccess.item, 0);
            pManager.AddBooleanParameter("toolchanger", "Toolchanger?", "Boolean for if there are multiple tools", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("absXYZ", "Absolute XYZ Coordinates", "absolute or relative XYZ Coordinates", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("absExt", "Absolute Extrusion", "absolute or relative extrusion", GH_ParamAccess.item, false); 
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
            List<string> gcode = new List<string>();
            int accelXY = 0;
            int accelZ = 0;
            int accelE = 0;
            int feedXY = 0;
            int feedZ = 0;
            int feedE = 0;
            int accelP = 0;
            int accelR = 0;
            int accelT = 0;
            double jerkXY = 0;
            double jerkZ = 0;
            double jerkE = 0;
            int bedTemp = 0;
            bool toolchanger = false;
            bool absXYZ = false;
            bool absE = false;


            DA.GetData("accelXY", ref accelXY);
            DA.GetData("accelZ", ref accelZ);
            DA.GetData("accelE", ref accelE);
            DA.GetData("feedXY", ref feedXY);
            DA.GetData("feedZ", ref feedZ);
            DA.GetData("feedE", ref feedE);
            DA.GetData("accelP", ref accelP);
            DA.GetData("accelR", ref accelR);
            DA.GetData("accelT", ref accelT);
            DA.GetData("jerkXY", ref jerkXY);
            DA.GetData("jerkZ", ref jerkZ);
            DA.GetData("jerkE", ref jerkE);
            DA.GetData("bedTemp", ref bedTemp);
            DA.GetData("toolchanger", ref toolchanger);
            DA.GetData("absXYZ", ref absXYZ);
            DA.GetData("absExt", ref absE);

            gcode.Add($"M201 X{accelXY} Y{accelXY} Z{accelZ} E{accelE}; max accel (mm/s2)");
            gcode.Add($"M203 X{feedXY} Y{feedXY} Z{feedZ} E{feedE}; mas feedrates (mm/s)");
            gcode.Add($"M204 P{accelP} R{accelR} T{accelT}; accel print travel & retract (mm/s2)");
            gcode.Add($"M205 X{jerkXY:F2} Y{jerkXY:F2} Z{jerkZ:F2} E{jerkE:F2}; Jerk limits (mm/s)");
            gcode.Add("M107");
            if (bedTemp > 0) gcode.Add($"M190 S{bedTemp}");
            gcode.Add("M91; Relative Moves \n" +
                "G1 Z1 F900; raise tool 1mm \n" +
                "G90; Absolute Moves");
            if (toolchanger) gcode.Add("T-1; make sure no tool on carriage");
            gcode.Add("G0 X150 Y150 F10000; move to center of print area \n" +
                "M558 F500; set probe speed \n" +
                "G30; single probe \n" +
                "M558 F50; slower probe speed \n" +
                "G30; Second, slower probe \n" +
                "G21; set units to mm");
            if (absXYZ) gcode.Add("G90; use absolute coords");
            else gcode.Add("G91; use relative coords");
            if (absE) gcode.Add("M82; absolute extrusion");
            else gcode.Add("M83; relative extrusion distance");
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
                return Resources.Resources.machine_params;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("B36AFAC3-6751-419A-924A-03A240AEA173"); }
        }
    }
}
