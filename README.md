# Vespidae


## What is it? 


Vespidae is a framework for prototyping and creating toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae takes curves and polylines in Rhino and transform these into executable machine actions and gcode. Vespidae includes tools for manupulating curves (offseting and boolean tools, slicing tools) together with different tools for visualizing toolpaths and toosl for converting toolpaths into different gcode dialects. Vespidae also includes tools for communitation between grasshopper and a machine contorller. 

Vespidae is designed with an ethos to provide users with as much flexibility as possible. Modern CAM/slicing tools gives designers great means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for people who are either specific aesthetics or for people who are building novel tools that does not necessarily fit into a generalized CAM tool. 

## Why does this exist? 

In the good ol' days CNC operators programmed their machines in raw commands (gcode). This off course took a fair amount of time to do and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the machinist of this complexity by automating the generation of toolpaths and gcode. This is all great and in many cases efficient and useful, but there are cases when you want to be more specific and have more control. In my world (read Frikk), where I build custom machines for different weird workflows, a proper CAM tool doesn't really exists. Vespidae positions itself as a sort of hybrid between these two paradigms. *How?*

## How do I use it? 

1. Download Build folder from repository. 
3. Move files from Build folder to Grasshopper Component Folder (Open Grasshopper, then File -> Special Folders -> Component Folder).
4. Restart Rhino and you should see Vespidae as a submenu in your Grasshopper workspace. 

## Getting familiar

*This example shows how Vespidae is used to convert curves into gcode and upload them to a machine running a Duet2 or 3 controller and connected to a client machine over ethernet. See [here](https://www.jubilee3d.com/index.php?title=Duet3_Raspberry_Pi_Provisioning) for documentation about how to setup a similar connection to the Duet.* 

![Exercise: curves to vespidae objects to gcode. Find example [here]()](img/vespidae_intro_example.png)

To get familiar with Vespidae open [example_00]() in your grasshoppen environment. This file will take all curves from *Default* layer in Rhino, convert them into *Vespidae Move*-objects, run these objects through a *Vespidae-solver*, and finally convert all the objects to gcode and upload them to the Jobs folder on the Duet. 
