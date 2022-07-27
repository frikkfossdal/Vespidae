using System;
using System.Collections.Generic;
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using VespidaeTools;
using ClipperHelper;

namespace Vespidae.Ops
{
    public class NonPlanarSyringeComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public NonPlanarSyringeComponent()
          : base("nonPlanarSyringeComponent", "nonPlanarSyringe",
            "nonPlanarSyringe description",
            "Vespidae", "2.Actions")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "c", "curves to project & extrude", GH_ParamAccess.list);
            pManager.AddBrepParameter("Surface", "srf", "nonplanar surface to project & extrude", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Speed", "s", "speed of move in mm/min", GH_ParamAccess.item, 1000);
            pManager.AddIntegerParameter("ToolId", "to", "tool id that performs operation. Defaults to t0", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("ToolLength", "tl", "length of tool from center of rotation", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Projection", "project?", "does the item need to be projected onto surface", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("PathTolerance", "tolerance", "tolerance in mm when projecting & converting curves", GH_ParamAccess.item, 0.005);
            pManager.AddNumberParameter("LayerOffset", "offset", "layer offset", GH_ParamAccess.item, 0.01);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("VespObj", "VObj", "Vespidae action objects", GH_ParamAccess.list);
            pManager.AddBrepParameter("PrintSurface", "PrintSrf", "Passing through print surface", GH_ParamAccess.item);
            //pManager.AddGenericParameter("speedCheck", "speedCheck", "makes sure converted speeds don't exceed max values", GH_ParamAccess.item);
            //pManager.AddGenericParameter("testList2", "test", "test", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Curve> crv = new List<Curve>();
            Brep srf = new Brep();
            int speed = 0;
            int tool = 0;
            double length = 0.0;
            bool pro = true;
            double tol = 0.005;
            double offset = 0.0;

            if (!DA.GetDataList("Curve", crv)) return;

            DA.GetData("Speed", ref speed);
            DA.GetData("ToolId", ref tool);

            DA.GetData("Surface", ref srf);
            DA.GetData("ToolLength", ref length);
            DA.GetData("Projection", ref pro);
            DA.GetData("PathTolerance", ref tol);
            DA.GetData("LayerOffset", ref offset);
            
            List<Curve> proj = projectCurves(crv, srf);

            var pol = ClipperTools.ConvertCurvesToPolylines(proj, tol);

            Rhino.Runtime.HostUtils.SendDebugToCommandLine = true;

            List<double[]> angleA, angleC;
            List<Point3d[]> tooltipCoords, machineCoords;
            calcInvKinematics(pol, srf, offset, length, out angleA, out angleC, 
                out tooltipCoords, out machineCoords);

            List<Polyline> toolLines = PointsToPolyline(tooltipCoords);
            List<Polyline> machineLines = PointsToPolyline(machineCoords);

            List<double[]> sFactor = speedFactor(toolLines, machineLines);

            Rhino.Runtime.HostUtils.DebugString($"Elements in angleA:{angleA[0].Length}, angleC:{angleC[0].Length} \n" +
                $"tooltipCoords:{tooltipCoords[0].Length} machineCoords{machineCoords[0].Length}");

            var actions = VespidaeTools.Operation.createNonPlanarSyringeOps(machineLines, tool, speed, sFactor, angleA, angleC);

            //List<string> test = new List<string>();
            //var actions = VespidaeTools.Operation.createMoveOps(machineLines, speed, tool, test, test);

            DA.SetDataList("VespObj", actions);
            DA.SetData("PrintSurface", srf);
        }

        // helper functions
        protected List<Curve> projectCurves(List<Curve> crv, Brep srf)
        {
            Vector3d unitZ = new Vector3d(0, 0, 1);

            List<Curve> proj = new List<Curve>();
            foreach (Curve c in crv)
            {
                // it will project onto all surfaces of Brep
                Curve[] projections = Rhino.Geometry.Curve.ProjectToBrep(c, srf, unitZ, 0.01);
                // need to figure out how to take the tallest curve, since that's what I'll be printing on
                // placeholder, just taking first curve
                Curve highest = projections[0];

                // takes the tallest one (presumably the top one but TBD)
                proj.Add(highest);
            }

            return proj;
        }
        protected void calcInvKinematics(List<Polyline> paths, Brep srf, double offset, 
            double toolLength, out List<double[]> invA, out List<double[]> invC,
            out List<Point3d[]> tooltipCoords, out List<Point3d[]> machineCoords)
        {
            // find line segments and vertices of each of the curves in the list
            List<Point3d[]> crvEndPoints = new List<Point3d[]>();
            var crvSegments = new List<Line[]>();
            segmentPolyline(paths, ref crvEndPoints, ref crvSegments);

            List<Point3d[]> projPoints; List<Vector3d[]> projNormals;
            findNormals(crvEndPoints, srf, out projPoints, out projNormals);

            List<double[]> angleA, angleC;
            calcAllAngles(projNormals, out angleA, out angleC);
            invA = angleA;
            invC = angleC;

            // calcultes tooltip coords (offset, or layer height, off from surface)
            tooltipCoords = calcInvCoords(projPoints, projNormals, offset);
            // calculates machine coords (tool length away from tooltip coords)
            machineCoords = calcInvCoords(projPoints, projNormals, toolLength);
        }

        protected List<Polyline> PointsToPolyline(List<Point3d[]> points)
        {
            List<Polyline> polys = new List<Polyline>();
            foreach (var p in points)
            {
                Polyline poly = new Polyline(p);
                polys.Add(poly);
            }
            return polys;
        }

        protected List<double[]> speedFactor(List<Polyline> originalCurves, 
            List<Polyline> machineCurves)
        {
            List<double[]> factor = new List<double[]>();
            for (int i = 0; i < originalCurves.Count; i++)
            {
                Line[] origSeg = originalCurves[i].GetSegments();
                Line[] machineSeg = machineCurves[i].GetSegments();
                double[] segFactor = new double[origSeg.Length];
                for (int j = 0; j < origSeg.Length; j++)
                {
                    segFactor[j] = machineSeg[j].Length/origSeg[j].Length;
                }
                factor.Add(segFactor);
            }
            return factor;
        }

        //helper functions for the helper functions
        protected void segmentPolyline(List<Polyline> paths, 
            ref List<Point3d[]> endPoints, ref List<Line[]> segments)
        {
            foreach (var p in paths)
            {
                // get each line segment and creates a list of vertices
                Line[] seg = p.GetSegments();
                //Curve[] segments = new Curve[lines.Length];
                Point3d[] endPs = new Point3d[seg.Length + 1];
                for (int i = 0; i < seg.Length; i++)
                {
                    //segments[i] = new LineCurve(lines[i]);
                    if (i == 0)
                        endPs[i] = seg[i].From;
                    endPs[i + 1] = seg[i].To;
                }
                endPoints.Add(endPs);
                segments.Add(seg);
            }
        }

        protected void findNormals(List<Point3d[]> verticesList, Brep srf, 
            out List<Point3d[]> points, out List<Vector3d[]> normals)
        {
            points = new List<Point3d[]>();
            normals = new List<Vector3d[]>();
            foreach (Point3d[] vertices in verticesList)
            {
                Point3d[] singleCurvePoints = new Point3d[vertices.Length];
                Vector3d[] singleCurveNormals = new Vector3d[vertices.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    Point3d point; Vector3d normal;
                    srf.ClosestPoint(vertices[i], out point, out _, out _, out _, 0, 
                        out normal);
                    singleCurveNormals[i] = normal;
                    singleCurvePoints[i] = point;
                }
                points.Add(singleCurvePoints);
                normals.Add(singleCurveNormals);
            }
        }

        protected private void calcAngles(Vector3d normal, out double angleA, 
            out double angleC)
        {
            angleA = new double();
            angleC = new double();
            double radToDeg = 180 / Math.PI;

            // first calculates thetaC, which is angle of rotation along Z axis based on given normal
            // angle of rotation around Z = angle difference of X axes
            double thetaC = Math.Acos(normal.X);
            double thetaCDeg = thetaC * radToDeg;
            // offsets by -90deg so that answers are between -90 to 90 instead of 0 to 180
            thetaCDeg = Math.Round(thetaCDeg - 90, 6);
            angleC = thetaCDeg;

            // calculates thetaA based off the remaining x and y components of normal
            double thetaA = Math.Acos(normal.Z / Math.Sin(thetaC));
            double thetaADeg = thetaA * radToDeg;
            // determines sign of thetaA based on direction of Y
            thetaADeg = thetaADeg * Math.Sign(normal.Y);
            angleA = thetaADeg;
        }

        protected private void calcAllAngles(List<Vector3d[]> projNormals, 
            out List<double[]> allAngleA, out List<double[]> allAngleC)
        {
            allAngleA = new List<double[]>();
            allAngleC = new List<double[]>();

            foreach (var normals in projNormals)
            {
                double[] singleAngleA = new double[normals.Length];
                double[] singleAngleC = new double[normals.Length];

                for (int i = 0; i < normals.Length; i++)
                {
                    double angleA, angleC;
                    calcAngles(normals[i], out angleA, out angleC);
                    singleAngleA[i] = angleA;
                    singleAngleC[i] = angleC;
                }

                allAngleA.Add(singleAngleA);
                allAngleC.Add(singleAngleC);
            }
        }

        protected private List<Point3d[]> calcInvCoords(List<Point3d[]> allCoords, 
            List<Vector3d[]> allNormals, double offset)
        {
            List<Point3d[]> allInvCoords = new List<Point3d[]>();
            for (int i = 0; i < allCoords.Count; i++)
            {
                Point3d[] curvePoints = allCoords[i];
                Vector3d[] curveNormals = allNormals[i];
                Point3d[] invCoords = new Point3d[curvePoints.Length];
                for (int j = 0; j < curvePoints.Length; j++)
                {
                    Line extension = new Line(curvePoints[j], curveNormals[j], offset);
                    invCoords[j] = extension.To;
                }
                allInvCoords.Add(invCoords);
            }
            return allInvCoords;
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
            
            get { return new Guid("F38BA121-EABD-4A07-9D28-8C4774A0D306"); }
        }
    }
}
