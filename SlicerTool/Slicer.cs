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
                var contours = Brep.CreateContourCurves(m, new Plane(new Point3d(0, 0, lh), new Vector3d(0, 0, 1)));
                var nl = new Layer();
                nl.height = i;
                
                List<Polyline> converted = ClipperTools.ConvertCurvesToPolylines(contours);
                foreach (var p in converted) {
                    nl.shells.Add(p); 
                }
                layers.Add(nl);
            }
        }

        public void createInfill() {

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
    }

    public class Layer {
        public List<Polyline> shells;
        public double height;

        public Layer() {
            shells = new List<Polyline>();
            height = 0; 
        }
    }
}
