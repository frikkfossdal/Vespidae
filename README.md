# Vespidae


## What is it? 

*sketch*

Vespidae is a set of tools that you can use to create toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae enables you to draw toolpaths using curves and polylines in Rhino and transform these paths into executable machine actions and gcode. Vespidae includes powerful tools for manupulating curves (offseting and boolean tools, slicing tools) together with different tools for visualizing toolpaths, converting toolpaths into different gcode dialects and finally tools for communicating with machines. 

The tools included in Vespidae is designed with an ethos to provide users with as much flexibility as possible. Modern CAM/slicing tools gives designers great means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for people who are  seeking a specific aesthetics or for people who are building novel tools that does not necessarily fit into a generalized CAM tool. 

## Why does this exist? 

*sketch*

In the good ol' days CNC operators programmed their machines in raw commands (gcode). This off course took a fair amount of time to do and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the machinist of this complexity by automating the generation of toolpaths. Designs represented as virtual 3D models combined with high-level configuration parameters.
This is all great and in many cases efficient and useful, but there are cases when you want to be more specific and have more control. In my world, where I tend to build a custom machines with specialized workflows, a proper CAM tool doesn't really exists. To cope with this Vespidae positions itself as a sort of hybrid between these two paradigms. It uses modern a modern CAD tools ability to draw and visualize shapes and it combines this with a set of flexible tools for generating and manipulating toolpaths based on these shapes.

## How does it work? 

*sketch, explain how tagging with meta-data is super cool and useful*

At its Core Vespidae transforms Rhino Polycurves into encapsulated objects that we call *VespActions*. A VespActions can be anything from a simple move to a cutting move to an extrusino move. These are compiled... 

## How do I use it? 

1. Download Build folder from repository. 
3. Move files from Build folder to Grasshopper Component Folder (Open Grasshopper, then File -> Special Folders -> Component Folder).
4. Restart Rhino and you should be good to go. 

