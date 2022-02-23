# Vespidae


## What is it? 

Vespidae is a set of tools that you can use to create toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae enables users to draw raw toolpaths with curves and polylines in Rhino and visualize these as machine actions in the Rhino viewport. Native tools like arrays and copy/pasting can be used to create more patterns and programs. 

The tools included in Vespidae is designed with an ethos to provide users with as much flexibility as possible. Modern CAM/slicing tools gives designers great means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for people who are  seeking a specific aesthetics or for people who are building novel tools that does not necessarily fit into a generalized CAM tool. 

## Why does this exist? 

In the good ol' days CNC operators programmed their machines in raw sequential commands (gcode). This off course took a fair amount of time and experience and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was actually doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the user of this complexity by automating the generation of toolpaths based on an input 3d model together with user defined high-level parameters. This is all great and in many cases efficient and useful, but there are cases when you want to be more specific and have more control. In my world, where I tend to build a custom machines with specialized workflows, a proper CAM tool doesn't really exists. To cope with this Vespidae positions itself as a sort of hybrid between these two paradigms. It uses modern a modern CAD tools ability to draw and visualize shapes and it combines this with a set of flexible tools for generating and manipulating toolpaths based on these shapes.

## How does it work? 

At its Core Vespidae transforms Rhino Polycurves into encapsulated objects that we call *VespActions*. A VespActions can be anything from a simple move to a 

## How do I use it? 

We are currently working on setting up a set of curated examples for different workflows. 
