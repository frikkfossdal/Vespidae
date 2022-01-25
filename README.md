# Vespidae


## What is it? 

Vespidae is a set of tools that you can use to create toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae enables users to draw raw toolpaths with curves and polylines in Rhino and visualize these as machine actions in the Rhino viewport. Native tools like arrays and copy/pasting can be used to create complex patterns. 

The tools included in Vespidae is designed with an ethos to provide users with as much flexibility as possible. Modern CAM/slicing tools gives designers great means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for designers seeking a specific aesthetics or for people who are building novel tools that does not necessarily fit into a generalized CAM tool. 

## Why does this exist? 

In the good ol' days CNC operators programmed their machines in raw sequential commands (gcode). This off course took a fair amount of time and experience and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was actually doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the user of this complexity by automating the generation of toolpaths based on an input 3d model together with user defined high-level parameters. This is all great and in many cases efficient and useful, but there are cases when you want to be more specific and have more control. In my world, where I tend to build a custom machines with specialized workflows, a proper CAM tool doesn't really exists. To cope with this Vespidae positions itself as a sort of hybrid between these two paradigms. It uses modern a modern CAD tools ability to draw and visualize shapes and it combines this with a set of flexible tools for generating and manipulating toolpaths based on these shapes.

## How do I use it? 

# Curated Examples (will move to separe page)

## Spiral vase on the Ultimaker 

Ultimaker uses a flavor of Griffin as gcode. Important note here is that the CuraEngine(?) relies on a custom header in the gcode. Without this header, and without this header being correct, the ultimaker will flat-out refuse your gcode. I had a frustrating time figuring this out (turned out I used the wrong object-envelope in the header). The header is documented-ish [here](https://community.ultimaker.com/topic/15555-inside-the-ultimaker-3-day-1-gcode/). 

I made a simple script that uses the uv-coordinates to wrap a spiral around a surface in Rhino. The script is attached in [the repo](add link).

![Spirals. Visualized with 0.4 radius for the nozzle diameter. Spiral needs adjustment.](./img/spiral1.png)

Next challenge is calculating extrusion rate. Main train of though coming into this is that I need to check the length between each point that is translated to gcode and use this to determine how much material that should be extruded between these two points. 


## Probing script on Clank 

## Microscope Scan on Clank 

## Pen-plotting on Clank 
