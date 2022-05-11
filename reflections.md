# Are you creating a toolpath API?

A thought that occurred to me was that Vespidae could be a codebase / API for creating custom CAM software. I actually think that this could be a really valuable thing for a lot of people in the machine building and digital fabrication community. Does any good alternatives exist here? The Vespidae API would take care of all the scary stuff like mesh slicing, path planning algorithms, etc. A good way to think about this could be how Clipper is setup. Clipper forces you to convert your polygons into *Clipper polygons*. Then it lets you feed these polygons into Clippers stack and use all the functionality of Clipper. 

Right now you are developing this in Rhino / Grasshopper. Thats not really a problem and it just helps you visualize things. As Vespidae matures I think the real value of it lies in the VespidaeTools class. A valuable thing to keep in mind as we more forward is how Vespidae could be a made available for other languages, for example javascript. 

*Some loose notes on this subject:* 

- Think about interacting with Vespidae without Grasshopper but through the Rhino terminal. What functionallity do you need to add to enable this? What does this interaction look like? 
- There are a couple of commercial toolpath/CAM apis. Notably [Fusion 360's api](https://help.autodesk.com/view/fusion360/ENU/?guid=GUID-7F3F9D48-ED88-451A-907C-82EAE67DEA93) or [dyndrite's](https://www.dyndrite.com/technology/vector-toolpathing-api). They are however behind subscription walls.

Searching through the web and reading through different forums there doesnt seem to exist a proper open-source library for working with CAM. 

# Is there a need for a new open-source geometry kernel? 

*Vespidae is developed on top of McNeel's/Rhino's geometry kernel. What would my options be if I wanted to go outside of this kernel?*

Wikipedia keeps a good overview of existing geometry kernels [here](https://en.wikipedia.org/wiki/Geometric_modeling_kernel).

# CAD model to battery printing exercise

This is a reflection based no a conversation I had with Leo McElroy the other day where he made some interesting points as I was describing the research project. *Why cant we model battery geometry in CAD and create toolpaths from this model?* 

# 