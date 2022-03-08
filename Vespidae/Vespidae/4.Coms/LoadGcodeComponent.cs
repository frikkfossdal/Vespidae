using System;
using System.Collections.Generic;
using Coms; 
using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using GrasshopperAsyncComponent;




namespace Vespidae.Coms
{
    public class LoadGcodeComponent : GH_AsyncComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public LoadGcodeComponent()
          : base("LoadGcodeComponent", "UploadGcode",
            "UNDER DEVELOPMENT",
            "Vespidae", "4.Coms")
        {
            BaseWorker = new PrimeCalculatorWorker();
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("N", "N", "Which n-th prime number. Minimum 1, maximum one million. Take care, it can burn your CPU.", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Cancel", "cnl", "cancel current operation", GH_ParamAccess.item,false);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Output", "O", "The n-th prime number.", GH_ParamAccess.item);
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
            get { return new Guid("648fc90e-7b95-4681-8263-f9a5abbe7850"); }
        }

    }

    public class PrimeCalculatorWorker : WorkerInstance
    {
        int TheNthPrime { get; set; } = 100;
        long ThePrime { get; set; } = -1;

        public PrimeCalculatorWorker() : base(null) { }

        public override void DoWork(Action<string, double> ReportProgress, Action Done)
        {
            // 👉 Checking for cancellation!
            if (CancellationToken.IsCancellationRequested) { return; }

            int count = 0;
            long a = 2;

            // Thanks Steak Overflow (TM) https://stackoverflow.com/a/13001749/
            while (count < TheNthPrime)
            {
                // 👉 Checking for cancellation!
                if (CancellationToken.IsCancellationRequested) { return; }

                long b = 2;
                int prime = 1;// to check if found a prime
                while (b * b <= a)
                {
                    // 👉 Checking for cancellation!
                    if (CancellationToken.IsCancellationRequested) { return; }

                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }

                ReportProgress(Id, ((double)count) / TheNthPrime);

                if (prime > 0)
                {
                    count++;
                }
                a++;
            }

            ThePrime = --a;
            Done();
        }

        public override WorkerInstance Duplicate() => new PrimeCalculatorWorker();

        public override void GetData(IGH_DataAccess DA, GH_ComponentParamServer Params)
        {
            int _nthPrime = 100;
            DA.GetData(0, ref _nthPrime);
            if (_nthPrime > 1000000) _nthPrime = 1000000;
            if (_nthPrime < 1) _nthPrime = 1;

            TheNthPrime = _nthPrime;
        }

        public override void SetData(IGH_DataAccess DA)
        {
            // 👉 Checking for cancellation!
            if (CancellationToken.IsCancellationRequested) { return; }

            DA.SetData(0, ThePrime);
        }
    }
}
