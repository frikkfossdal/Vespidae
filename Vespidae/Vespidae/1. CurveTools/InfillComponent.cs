﻿using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino; 
using Rhino.Geometry;
using SlicerTool; 

namespace Vespidae
{
    public class InfillComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public InfillComponent()
          : base("InfillComponent", "Infill",
            "Create infill operations on top of slice",
            "Vespidae", "1.CurveTools")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Vespidae_Operations", "VESPO", "Prior Vespidae operations", GH_ParamAccess.item);
            pManager.AddIntegerParameter("density", "D", "infill density", GH_ParamAccess.item, 1); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Vespidae_Object_out", "VESO", "Prior Vespidae operations", GH_ParamAccess.item);
            pManager.AddGenericParameter("InfillPoly", "Inf", "Preview infill", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var vespo = new Slicer(); 
            int density = 0; 

            if (!DA.GetData("Vespidae_Operations", ref vespo)) return;

            DA.GetData("density", ref density);

            vespo.createInfill(density, RhinoDoc.ActiveDoc.ModelAbsoluteTolerance);
            DA.SetDataList("InfillPoly", vespo.exposeInfill());
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
            get { return new Guid("ef5e632d-bdec-4094-92b7-d11d73c53198"); }
        }
    }
}