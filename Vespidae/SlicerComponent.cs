    using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using ClipperHelper;
using SlicerTool;

namespace Vespidae
{
    public class SlicerComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public SlicerComponent()
          : base("SlicerComponent", "Slice",
            "SlicerComponent description",
            "Vespidae", "Toolpath")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBrepParameter("Brep", "G", "Brep geometry to be sliced", GH_ParamAccess.item);
            pManager.AddNumberParameter("LayerHeight", "LH", "Slicing layer height", GH_ParamAccess.item,1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Vespidae_Object_out", "VESPO", "Prior Vespidae operations", GH_ParamAccess.item);
            pManager.AddGenericParameter("SlicedPolys", "P", "sliced polys as list", GH_ParamAccess.list);
            pManager.AddGenericParameter("SlicingPlanes", "pln", "slicing planes", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Slicer slc = new Slicer();

            Brep geo = new Brep();
            Plane pl = new Plane();
            double lh = 0.4;

            if (!DA.GetData("Brep", ref geo)) return;
            DA.GetData("LayerHeight", ref lh);
            
            List<Curve> lst = new List<Curve>();
            lst.AddRange(Brep.CreateContourCurves(geo, new Point3d(0, 0, 0), new Point3d(0, 0, 30), 1));

            //List<Polyline> polys = ClipperTools.ConvertCurvesToPolylines(lst);

            var bound = geo.GetBoundingBox(true);

            var infill = brepTools.createInfillLines(geo, 0.3);

            infill = brepTools.sortPolys(infill);

            slc.layerHeight = lh;
            slc.model = geo;
            slc.slice();

            DA.SetData("Vespidae_Object_out", slc);
            DA.SetDataList("SlicedPolys", slc.exposeShells());
            //DA.SetDataList("SlicingPlanes", slc.exposePlanes());
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
            get { return new Guid("30c8db20-681c-472d-90a0-845347998b8e"); }
        }
    }
}
