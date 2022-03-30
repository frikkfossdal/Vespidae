using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GMaker;
using System.Linq; 

namespace Vespidae
{
    public class ExposePaths : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExposePaths()
          : base("Visualize Paths", "viz_paths",
            "ExposePaths description",
            "Vespidae", "3.Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("actions", "VObj", "Vespidae action objects", GH_ParamAccess.list);
            pManager.AddNumberParameter("arrowScl", "scl", "scale of viz arrows", GH_ParamAccess.item,1.0);
            pManager.AddIntegerParameter("arrowDensity", "dens", "density of viz arrows", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AllMoves", "allPaths", "all paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddGenericParameter("AllTravel", "allTravel", "all paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddGenericParameter("Arrows", "ar", "direction arrows", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var actions = new List<GMaker.Action>();
            var allPaths = new List<Polyline>();
            var arrows = new List<Mesh>();
            double scl = 0;
            int density = 0; 

            if (!DA.GetDataList("actions", actions)) return;

            DA.GetData("arrowScl", ref scl);
            DA.GetData("arrowDensity", ref density); 

            //////get all paths
            var allMoves = new List<Polyline>();
            foreach (var act in actions)
            {
                allMoves.Add(act.path); 
            }

            var travel = actions.Where(act => act.actionType == GMaker.opTypes.travel).ToList();
            var travelMoves = new List<Polyline>();

            foreach (var move in travel)
            {
                travelMoves.Add(move.path);
            }

            var moves = actions.Where(act => act.actionType == GMaker.opTypes.move).ToList();
            foreach (var move in moves) {
                arrows.AddRange(GMaker.Visualization.pathViz(move.path, scl, density)); 
            }
            DA.SetDataList("AllMoves", allMoves);
            DA.SetDataList(1, travelMoves);
            DA.SetDataList(2, arrows); 
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
                return Resources.Resources.visualizePaths;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("89e70fcb-f60f-4d8b-a8b3-0160992003cc"); }
        }
    }
}
