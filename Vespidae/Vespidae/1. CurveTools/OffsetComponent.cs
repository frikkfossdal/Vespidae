﻿using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using ClipperHelper;

namespace Vespidae
{
    public class OffsetComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public OffsetComponent()
          : base("SlicerFriendlyOffset", "SlicerOffset",
            "Offsets polylines",
            "Vespidae", "1.CurveTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("curve", "crv", "Curve or curves to offset", GH_ParamAccess.list); 
            pManager.AddNumberParameter("Distance", "dist", "Distance to offset", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Amount", "amo", "Amount of times to offset curve. Default 1", GH_ParamAccess.item, 1);
            //pManager.AddPlaneParameter("OutputPlane", "pln", "Plane to output solution to", GH_ParamAccess.item, Plane.WorldXY);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("result", "res", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            List<Curve> offsetCurves = new List<Curve>();
            double distance = 0;
            int amount = 0;
            var pln = new Plane();

            if (!DA.GetDataList("curve", offsetCurves))return;
            DA.GetData("Distance", ref distance);
            DA.GetData("Amount", ref amount);
            //DA.GetData("OutputPlane", ref pln);

            //hack: extract plane from first input curve

            var output = new List<Polyline>(); 

            foreach (var crv in offsetCurves) {
                crv.TryGetPlane(out pln);
                List<Polyline> offsetPolylines = ClipperTools.ConvertCurvesToPolylines(offsetCurves);
                output.AddRange(ClipperTools.offset(offsetPolylines, amount, pln, distance, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance));
            }


            DA.SetDataList(0, output); 
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
                return Resources.Resources.offset;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0f98fefb-135d-4e93-999b-30d501057087"); }
        }
    }
}