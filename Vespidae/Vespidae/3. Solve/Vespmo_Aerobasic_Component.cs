using System;
using System.Linq;
using System.Collections.Generic;

using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools;

namespace Vespidae
{
    public class Vespmo_Aerobasic_Component : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Vespmo_Aerobasic_Component()
          : base("Vespmo_Aerobasic_Component", "VespMoAerobasic",
            "Converts VESPMO object to Aerobasic",
            "Vespidae", "3.Solver")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("actions", "VObj", "Vespidae action objects", GH_ParamAccess.list);
            pManager.AddTextParameter("header", "h", "optional Aerobasic header", GH_ParamAccess.list, "");
            pManager.AddTextParameter("footer", "f", "optional aerobasic footer", GH_ParamAccess.list, "");
            pManager.AddBooleanParameter("movement", "Absolute?", "absolute or relative movement",
                GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("invertZ", "Invert Z?", 
                "whether direction of Z axes is inverted", GH_ParamAccess.item, true);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("aerobasic", "Aerobasic", "output gcode", GH_ParamAccess.list);
            pManager.AddGenericParameter("test", "test", "test", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<VespidaeTools.Action> actions = new List<VespidaeTools.Action>();
            List<String> aerobasic = new List<string>();
            List<String> header = new List<string>();
            List<String> footer = new List<string>();
            List<string> test = new List<string>();
            bool absMove = true;
            bool invertZ = true;

            if (!DA.GetDataList("actions", actions)) return;
            DA.GetDataList("header", header);
            DA.GetDataList("footer", footer);
            DA.GetData("movement", ref absMove);
            DA.GetData("invertZ", ref invertZ);

            aerobasic.Add($"; AeroBasic");

            if (header.Count > 0)
            {
                aerobasic.AddRange(header);
            }

            Rhino.Runtime.HostUtils.DebugString($"Input into translation: {actions}");
            string debug = DebugActions(actions);
            Rhino.Runtime.HostUtils.DebugString(debug);
            List<string> body = VespidaeTools.Operation.translateToAerobasic(actions);
            if (!absMove)
            {
                (body, test) = AbsoluteToRelative(body, invertZ);
            }
            aerobasic.AddRange(body);

            if (footer.Count > 0)
            {
                aerobasic.AddRange(footer);
            }
            DA.SetDataList("aerobasic", aerobasic);
            DA.SetDataList("test", test);
        }

        protected string DebugActions(List<VespidaeTools.Action> actions)
        {
            string debug = "";
            foreach (var ac in actions)
            {
                debug += $"\n This actions has {ac.path.Count} polyline points";
                //debug += $"\n This actions has {ac.path.Count} polyline points, {ac.angleA.Length} angles, {ac.speedFactor.Length} speed factors";
            }
            return debug;
        }

        protected (List<string>, List<string>) AbsoluteToRelative(List<string> toolpaths, bool invertZ)
        {
            bool gCode = new bool();
            List<int> indices = new List<int>();
            List<string> test = new List<string>();
            List<double> x = new List<double>();
            List<double> y = new List<double>();
            List<double> z = new List<double>();
            List<double> a = new List<double>();
            List<double> f = new List<double>();
            List<String> relToolpaths = new List<string>();

            relToolpaths.AddRange(toolpaths);

            if (toolpaths.Contains("GCode"))
                gCode = true;
            else
                gCode = false;

            // stores x, y, z coordinates and index of each LINEAR/G0
            for (int i = 0; i < toolpaths.Count; i++)
            {
                if (toolpaths[i].Contains("LINEAR") || toolpaths[i].Contains("G0"))
                {
                    indices.Add(i);
                    
                    List<string> toolpath = splitCommand(toolpaths[i]);
                    
                    if (toolpaths[i].Contains("X"))
                    {
                        x.Add(Convert.ToDouble(toolpath.Find(q => q.Contains("X")).Substring(1)));
                    }
                    else if (x.Count() == 0)
                        x.Add(0);
                    else
                        x.Add(x.Last());

                    if (toolpaths[i].Contains("Y"))
                    {
                        y.Add(Convert.ToDouble(toolpath.Find(q => q.Contains("Y")).Substring(1)));
                    }
                    else if (y.Count() == 0)
                        y.Add(0);
                    else
                        y.Add(y.Last());

                    if (toolpaths[i].Contains("Z"))
                    {
                        z.Add(Convert.ToDouble(toolpath.Find(q => q.Contains("Z")).Substring(1)));
                    }
                    else if (z.Count() == 0)
                        z.Add(0);
                    else
                        z.Add(z.Last());

                    // " A" instead of "A" to avoid hitting on "LINEAR"
                    if (toolpaths[i].Contains(" A"))
                    {
                        a.Add(Convert.ToDouble(toolpath.FindLast(q => q.Contains("A")).Substring(1)));
                    }
                    else if (a.Count() == 0)
                        a.Add(0);
                    else
                        a.Add(a.Last());

                    if (toolpaths[i].Contains("F"))
                    {
                        f.Add(Convert.ToDouble(toolpath.Find(q => q.Contains("F")).Substring(1)));
                    }
                    else if (f.Count() == 0)
                        f.Add(0);
                    else
                        f.Add(f.Last());
                }
            }
            
            // given initial start point, calculate relative movement between all points
            for (int index = 0; index < indices.Count; index++)
            {
                test.Add($"X{x[index]}, Y{y[index]}, Z{z[index]}, F{f[index]}");
                if (index == 0)
                {
                    Point3d prev = new Point3d(x[index], y[index], z[index]);
                    relToolpaths[indices[index]] = relCommand(prev, a[index],
                        x[index], y[index], z[index], a[index], f[index], gCode, invertZ);
                }
                else
                {
                    Point3d prev = new Point3d(x[index - 1], y[index - 1], z[index - 1]);
                    relToolpaths[indices[index]] = relCommand(prev, a[index-1],
                        x[index], y[index], z[index], a[index], f[index], gCode, invertZ);
                }
            }
            
            return (relToolpaths,test);
        }

        protected List<string> splitCommand(string toolpath)
        {
            List<string> split = new List<string>(toolpath.Split(' '));

            return split;
        }

        protected string relCommand(Point3d prev, double prevA, double x, double y, double z, double a, double f, bool gcode, bool invert)
        {
            double relX = Math.Round(x - prev.X, 3);
            double relY = Math.Round(y - prev.Y, 3);
            double relZ = Math.Round(z - prev.Z, 3);
            double relA = Math.Round(a - prevA, 3);
            if (invert)
                relZ = -relZ;
            string command = "";

            if (relX != 0)
                command += $" X{relX}";
            if (relY != 0)
                command += $" Y{relY}";
            if (relZ != 0)
                command += $" Z{relZ}";
            if (relZ != 0)
                command += $" A{relA}";
            if (f != 0)
                command += $" F{f}";

            if (gcode)
                return $"G0" + command;
            else
                return $"LINEAR" + command;
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
            get { return new Guid("3b2fb908-e881-4bf6-96bd-68d1ef549b08"); }
        }
    }
}
