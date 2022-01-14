using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;

namespace Vespidae
{
    public class VespidaeInfo : GH_AssemblyInfo
    {
        public override string Name => "Vespidae";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("3900F949-B192-47F7-9F84-8FEACB6CF387");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}
