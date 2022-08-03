using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools;
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
            pManager.AddGenericParameter("allMoves", "all", "all paths of Vespidae object", GH_ParamAccess.list);
            pManager.AddGenericParameter("moves", "mv", "Vespidae generic actions", GH_ParamAccess.list);
            pManager.AddGenericParameter("extrude", "ext", "Vespidae extrude actions", GH_ParamAccess.list);
            pManager.AddGenericParameter("travel", "trv", "Vespidae travel actions", GH_ParamAccess.list);
            pManager.AddGenericParameter("syringe", "syr", "Vespidae syringe actions", GH_ParamAccess.list);
            pManager.AddGenericParameter("Arrows", "ar", "direction arrows", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //output
            var actions = new List<VespidaeTools.Action>();

            var arrows = new List<Mesh>();
            double scl = 0;
            int density = 0; 

            if (!DA.GetDataList("actions", actions)) return;

            DA.GetData("arrowScl", ref scl);
            DA.GetData("arrowDensity", ref density); 

            //sort out actions 
            var travelActions = actions.Where(act => act.actionType == VespidaeTools.opTypes.travel || act.actionType ==VespidaeTools.opTypes.nonplanarTravel).ToList();
            var moveActions = actions.Where(act => act.actionType == VespidaeTools.opTypes.move).ToList();
            var extrudeActions = actions.Where(act => act.actionType == VespidaeTools.opTypes.extrusion).ToList();
            var syringeActions = actions.Where(act => act.actionType == VespidaeTools.opTypes.nonplanarSyringe).ToList();

            //get paths from sorted actions

            var allPaths = convertActionsToPaths(actions); 
            var travelPaths = convertActionsToPaths(travelActions);
            var movePaths = convertActionsToPaths(moveActions);
            var extrudePaths = convertActionsToPaths(extrudeActions);
            var syringePaths = convertActionsToPaths(syringeActions);

            DA.SetDataList("allMoves", allPaths);
            DA.SetDataList("moves", movePaths);
            DA.SetDataList("extrude", extrudePaths);
            DA.SetDataList("travel", travelPaths); 
            DA.SetDataList("syringe", syringePaths);
        }

        //temp function for converting Actions into polylines 
        static List<Polyline> convertActionsToPaths(List<VespidaeTools.Action> actions) {
            var returnPolylines = new List<Polyline>(); 
            foreach (var act in actions) {
                returnPolylines.Add(act.path); 
            }

            return returnPolylines; 
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
