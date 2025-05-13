using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using ClipperHelper;
using Eto.Forms.ThemedControls;
using Eto.Forms;
using static System.Net.Mime.MediaTypeNames;


namespace Vespidae
{
    // NOTE: If you want this to show up in the Vespidae toolbar, you need to make this class public
    class ExampleComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExampleComponent()
          : base("Example", "Example Nickname",
            "Description",
            "Vespidae", "1.Toolpath")
        {
            // The general structure of base is (Name, Nickname, Description,
            // Category (e.g. This will show up in the "Vespidae" toolbar), Subcategory (This will show up in the "1. Toolpath" tab))
        }

        /// <summary>
        /// Registers all the input parameters for this component. This shows up on the left side of the component
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("A", "A", "A list of curves (A) to use", GH_ParamAccess.list); // accepts a list of curves, no default
            pManager.AddIntegerParameter("speed", "speed", "Example, speed", GH_ParamAccess.item, 1000); // accepts a single integer as input, defaults to 1
            // other supported parameters are boolean, 
        }

        /// <summary>
        /// Registers all the output parameters for this component. This shows up on the right side of the component
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            // the final output of the Example Component returns a list of Vespidae Objects with attached metadata
            pManager.AddGenericParameter("VespObj", "VObj", "Vespidae action objects", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        { 
            // First initializes the variables you are planning on using. In this case
            // we have a list of curves and an integer for speed
            List<Curve> crv = new List<Curve>();
            int speed = 0;

            // These functions update the initialized variables with the values that are being input into the left side of the component
            if (!DA.GetDataList("A", crv)) return; // this will return an error if there is no wire connected to A
            DA.GetData("ClipType", ref speed); // this will not return an error even if there is no wire connected to speed

            // converts curves to polylines, which is how Vespidae typically handles geometries
            var pol = ClipperTools.ConvertCurvesToPolylines(crv);

            // creates Vespidae Objects based on backend code that is located in VespidaeTools -> GTools.cs. 
            // Because Vespidae Actions are typically of similar subclasses, most code is handled in there as opposed to within the component. 
            var actions = VespidaeTools.Operation.createExampleOps(pol, speed);


            // once the actions have been assigned metadata, return the actions into the output of the component
            DA.SetDataList(0, actions);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.Resources.boolean;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("AB869523-12EB-43C6-A313-C989C29E929A");
    }

}
