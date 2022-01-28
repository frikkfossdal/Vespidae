using System;
using System.Collections.Generic;
using Rhino.Geometry;
using ClipperHelper; 

namespace SlicerTool
{
    public class Slicer
    {
        public List<Layer> layers;
        public Brep model;
        public double layerHeight;
        double nozzleDiam;
        int numberOfLayers;
       

        public Slicer()
        {
            layers = new List<Layer>();
        }

        public void slice() {
            var bb = model.GetBoundingBox(true);

            //check for impossible values 
            if (layerHeight < 0.1) {
                layerHeight = 0.1;
            }

            int numberOfLayers = ((int)((bb.Max.Z - bb.Min.Z) / layerHeight));
            computeShells(model, bb, layerHeight);
        }

        private BoundingBox getBoundingBox() {
            return model.GetBoundingBox(true);
        }

        private int getNumberofLayers(double lh, BoundingBox b) {
            return ((int)((b.Max.Z - b.Min.Z) / lh));
        }

        public void computeShells(Brep m, BoundingBox b, double lh) {
            for (double i = b.Min.Z; i <= b.Max.Z; i += lh) {
                var contours = Brep.CreateContourCurves(m, new Plane(new Point3d(0, 0, i), new Vector3d(0, 0, 1)));
                var nl = new Layer();
                nl.height = i;

                List<Polyline> converted = ClipperTools.ConvertCurvesToPolylines(contours);

                //calculate additional shells and add all shell polys to layer

                converted.AddRange(ClipperTools.offset(converted, new Plane(new Point3d(0, 0, i), new Vector3d(0, 0, 1)), 5, 0.01));

                foreach (Polyline p in converted) {
                    
                    nl.shells.Add(p);
                }
                nl.pln = new Plane(new Point3d(0,0,i), new Vector3d(0,0,1));
                layers.Add(nl);
            }
            
        }


        public void createInfill(double density, double tolerance) {
            List<Polyline> infillLines =  brepTools.createInfillLines(model, density);

            foreach (Layer l in this.layers) {
                List<Polyline> inf =  ClipperTools.intersection(infillLines, l.shells, l.pln, tolerance, 1);
                l.infill.AddRange(inf);
            }
        }

        public List<Polyline> exposeShells() {
            var output = new List<Polyline>();
            foreach (var l in layers) {
                foreach (var s in l.shells) {
                    output.Add(s); 
                } 
            }
            return output; 
        }

        public List<Polyline> exposeInfill()
        {
            var output = new List<Polyline>();
            foreach (var l in layers)
            {
                foreach (var s in l.infill)
                {
                    output.Add(s);
                }
            }
            return output;
        }

        public List<Plane> exposePlanes() {
            var output = new List<Plane>();
            foreach (var l in layers) {
                output.Add(l.pln);
            }
            return output; 
        }
    }

    public class Layer {
        public List<Polyline> shells;
        public List<Polyline> infill;
        public double height;
        public Plane pln;

        public Layer() {
            shells = new List<Polyline>();
            infill = new List<Polyline>(); 
            height = 0;
            pln = new Plane(); 
        }
    }
}
