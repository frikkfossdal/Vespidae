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
          : base("ExposePaths", "Expose Paths",
            "ExposePaths description",
            "Vespidae", "3.Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("vespmo", "VObj", "Vespidae action objects", GH_ParamAccess.list); 
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("AllMoves", "allPaths", "all paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddGenericParameter("TravelMoves", "allTravel", "filtered travel paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddGenericParameter("WorkMoves", "allWork", "filtered work paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Speed", "sp", "list of speeds", GH_ParamAccess.list);
            pManager.AddGenericParameter("Arrows", "ar", "direction arrows", GH_ParamAccess.list); 
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GMaker.Action> actions = new List<GMaker.Action>();
            List<Polyline> allPaths = new List<Polyline>();
            var arrows = new List<Mesh>(); 

            if (!DA.GetDataList("vespmo", actions)) return;

            //get all paths
            var allMoves = new List<Polyline>();
            foreach (var move in actions) {
                allMoves.Add(move.path);
                arrows.AddRange(GMaker.Visualization.enterExit(move.path)); 
            }

            var work = actions.Where(m => m.actionType == GMaker.opTypes.extrusion);
            var workMoves = new List<Polyline>();
            foreach (var move in work) {
                workMoves.Add(move.path); 
            }

            var travel = actions.Where(m => m.actionType == GMaker.opTypes.move);
            var travelMoves = new List<Polyline>(); 
            foreach (var move in travel) {
                travelMoves.Add(move.path); 
            }

            DA.SetDataList("AllMoves", allMoves); 
            DA.SetDataList("WorkMoves", workMoves);
            DA.SetDataList("TravelMoves", travelMoves);
            DA.SetDataList("Arrows", arrows); 
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
