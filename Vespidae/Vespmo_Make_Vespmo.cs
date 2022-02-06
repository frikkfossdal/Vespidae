﻿using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GMaker;
using ClipperHelper; 

namespace Vespidae
{
    public class Vespmo_Make_Vespmo : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Vespmo_Make_Vespmo()
          : base("Vespmo_Make_Vespmo", "Nickname",
            "Vespmo_Make_Vespmo description",
            "Vespidae", "Gcode Tools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "crv", "polyline to convert", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Type", "t", "type of move 0 = travel, 1 = work", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Speed", "s", "speed of move in mm/min", GH_ParamAccess.item,1000); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("move", "VESPMO", "Vespidae Moves", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Curve crv = new PolyCurve();
            int type = 0;
            int speed = 0; 

            if (!DA.GetData("Curve", ref crv)) return;
            DA.GetData("Type", ref type);
            DA.GetData("Speed", ref speed); 

            Polyline pol = new Polyline(); 
            ClipperTools.ConvertCurveToPolyline(crv, out pol); 

            Move mv = new Move();
            mv.path = pol;
            mv.speed = speed;

            //come back and fix this 
            mv.type = "travel"; 

            DA.SetData("move", mv); 
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
            get { return new Guid("a10d1ea2-cd5f-4e31-8f35-ac77d5fa7b33"); }
        }
    }
}
