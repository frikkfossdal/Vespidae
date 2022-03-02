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
            pManager.AddCurveParameter("Curve", "c", "curves to extrude", GH_ParamAccess.list);
            pManager.AddNumberParameter("Extrusion", "ex", "extrusion flowrate multiplier (distance x 0.01 x multiplier)", GH_ParamAccess.item,1);
            pManager.AddIntegerParameter("Speed", "s", "speed of move in mm/min", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Temperature", "t", "extrusion temperature", GH_ParamAccess.item, 205);
            pManager.AddTextParameter("ToolId", "to", "tool id that performs operation. Defaults to t0", GH_ParamAccess.item, "t0");
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
            int speed = 0;
            double ext = 0;
            int rh = 0;
            double temp = 0;
            string tool = ""; 

            if (!DA.GetDataList("Curve", crv)) return;

            DA.GetData("Extrusion", ref ext);
            DA.GetData("Speed", ref speed);
            DA.GetData("Temperature", ref temp);
            DA.GetData("ToolId", ref tool); 

            var pol = ClipperTools.ConvertCurvesToPolylines(crv);

            var actions = GMaker.Operation.createExtrudeOps(pol, speed, ext, temp, tool);

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
