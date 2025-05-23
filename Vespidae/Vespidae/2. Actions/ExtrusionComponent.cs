﻿using System;
using System.Collections.Generic;
using ClipperHelper;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Vespidae.Ops
{
    public class ExtrusionComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExtrusionComponent()
          : base("ExtrusionComponent", "VespExtrusion",
            "ExtrusionComponent description",
            "Vespidae", "2.Actions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crv", "curves to extrude", GH_ParamAccess.list);
            pManager.AddNumberParameter("Extrusion", "ex", "relative extrusion multiplier. Extrusion is calculated by distance_between_points * ext", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Speed", "speed", "speed of move in mm/min. Default 1000mm/min", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Temperature", "temp", "extrusion temperature", GH_ParamAccess.item, 205);
            pManager.AddNumberParameter("Retract", "re", "retract length", GH_ParamAccess.item, 2);
            pManager.AddIntegerParameter("ToolId", "to", "tool id that performs operation. Defaults to t0", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("extType", "extType", "Extrusion type. Used for sorting by solvers. 0: shell (default), 1: infill", GH_ParamAccess.item, 0); 
            pManager.AddTextParameter("GcodeInjection", "gInj", "gcode to inject before operation", GH_ParamAccess.list, "");
            pManager.AddNumberParameter("Layer Height", "lh", "layer height. Default 0.1mm", GH_ParamAccess.item, 0.1);
            pManager.AddNumberParameter("Nozzle Diameter", "dNoz", "nozzle diameter. Default 0.4mm", GH_ParamAccess.item, 0.4);
            pManager.AddNumberParameter("Filament Diameter", "dFil", "filament diameter. Default 1.75mm", GH_ParamAccess.item, 1.75);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VespObj", "VObj", "Vespidae action objects", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> crv = new List<Curve>();
            List<String> gInj = new List<string>();

            int speed = 0;
            double ext = 0;
            double temp = 0;
            double retract = 0; 
            int tool = 0;
            int extType = 0;
            double lh = 0;
            double dNoz = 0;
            double dFil = 0;

            if (!DA.GetDataList("Curve", crv)) return;

            DA.GetData("Extrusion", ref ext);
            DA.GetData("Speed", ref speed);
            DA.GetData("Temperature", ref temp);
            DA.GetData("ToolId", ref tool);
            DA.GetData("Retract", ref retract);
            DA.GetData("extType", ref extType); 
            DA.GetDataList("GcodeInjection", gInj);
            DA.GetData("Layer Height", ref lh);
            DA.GetData("Nozzle Diameter", ref dNoz);
            DA.GetData("Filament Diameter", ref dFil);


            var pol = ClipperTools.ConvertCurvesToPolylines(crv);

            var actions = VespidaeTools.Operation.createExtrudeOps(pol, speed, retract, ext, lh, dNoz, dFil, temp, tool, extType, gInj);

            DA.SetDataList("VespObj", actions); 
            //ops.createActions();
            //
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
                return Resources.Resources.extrude; 
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a8615386-e873-4790-bbf9-7aed568e318f"); }
        }
    }
}
