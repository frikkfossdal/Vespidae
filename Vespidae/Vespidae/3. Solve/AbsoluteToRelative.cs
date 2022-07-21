using System;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools;

namespace Vespidae
{
    public class Absolute_to_Relative : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Absolute_to_Relative()
          : base("AbsoluteToRelative", "AbsToRel",
            "Converts Gcode/Aerobasic from absolute to relative coords",
            "Vespidae", "3.Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("toolpaths", "toolpaths", "Absolute toolpaths", GH_ParamAccess.list);
            pManager.AddPointParameter("startPoint", "startPoint", "starting point", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("relToolpaths", "relToolpaths", "Relative toolpaths", GH_ParamAccess.list);
            pManager.AddGenericParameter("test", "test", "test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<String> toolpaths = new List<string>();
            bool gCode = new bool();
            List<int> indices = new List<int>();
            List<string> test = new List<string>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> z = new List<double>();
            List<double> f = new List<double>();
            List<String> relToolpaths = new List<string>();
            Point3d start = new Point3d();


            if (!DA.GetDataList("toolpaths", toolpaths)) return;
            DA.GetDataList("toolpaths", relToolpaths);

            if (toolpaths.Contains("GCode"))
                gCode = true;
            else
                gCode = false;
            
            // stores x, y, z coordinates and index of each LINEAR/G0
            for (int i = 0; i<toolpaths.Count; i++)
            {
                if (toolpaths[i].Contains("LINEAR") || toolpaths[i].Contains("G0"))
                {
                    indices.Add(i);
                    
                    List<string> toolpath = splitCommand(toolpaths[i]);
                    
                    foreach (string part in toolpath)
                    {
                        if (part.Contains("X"))
                        {
                            x.Add(Convert.ToDouble(part.Substring(1)));
                            // x.Add(0);
                        }
                        else
                            x.Add(0);

                        if (part.Contains("Y"))
                            //y.Add(0);
                            y.Add(Convert.ToDouble(part.Substring(1)));
                        else
                            y.Add(0);

                        if (part.Contains ("Z"))
                            //z.Add(0);
                            z.Add(Convert.ToDouble(part.Substring(1)));
                        else
                            z.Add(0);

                        if (part.Contains("F"))
                            //f.Add(0);
                            f.Add(Convert.ToDouble(part.Substring(1)));
                        else
                            f.Add(0);
                    
                    }
                    
                }
            }
            
            // if a start point exists, use that. If not, very first point extracted becomes the start point to base relative coords
            if (!DA.GetData("startPoint", ref start))
            {
                start.X = x[0];
                start.Y = y[0];
                start.Z = z[0];
            }    

            // given initial start point, calculate relative movement between all points
            for (int i = 0; i < indices.Count; i++)
            {
                if (i == 0)
                {
                    relToolpaths[i] = relCommand(start, x[i], y[i], z[i], f[i], gCode);
                }
                else
                {
                    relToolpaths[i] = relCommand(new Point3d(x[i - 1], y[i - 1], z[i - 1]), x[i], y[i], z[i], f[i], gCode);
                }
            }
            
            DA.SetDataList("relToolpaths", relToolpaths);
            DA.SetDataList("test", test);
        }

        protected List<string> splitCommand(string toolpath)
        {
            List<string> split = new List<string>(toolpath.Split());

            return split;
        }

        protected string relCommand(Point3d prev, double x, double y, double z, double f, bool gcode)
        {
            double relX = Math.Round(x - prev.X, 3);
            double relY = Math.Round(y - prev.Y, 3);
            double relZ = Math.Round(z - prev.Z, 3);

            if (gcode)
                if (f == 0)
                    return $"G0 X{relX} Y{relY} Z{relY}";
                else
                    return $"G0 X{relX} Y{relY} Z{relY} F{f}";
            else
                if (f == 0)
                    return $"LINEAR X{relX} Y{relY} Z{relY}";
                else
                    return $"LINEAR X{relX} Y{relY} Z{relY} F{f}";
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
            get { return new Guid("c70fdb28-70f9-4e43-a25b-93e357a6a512"); }
        }
    }
}
