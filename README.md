# Vespidae

**NOTE:** Vespidae is currently under heavy development and should be treated accordingly. Check you code before executing anything on your tool / machine. 

## What is it? 


Vespidae is a framework for prototyping and creating toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae includes tools for manipulating and generating toolpaths as curves in Rhino, tools that tags these curves with process specific metadata, and finally tools that converts these actions to executable programs. Vespidae also includes tools for communitation between grasshopper and a machine controller. 

Vespidae is designed with an ethos to provide users with as much flexibility as possible when working with digital fabrication equipment. Modern CAM/slicing tools gives designers good means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for people who are either looking for specific aesthetics or for people who are building bespoke tools that does not necessarily fit into excisting CAM tools. 

## Why does this exist? 

In the good ol' days CNC operators programmed their machines in raw commands (gcode). This off course took a fair amount of time to do and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the machinist of this complexity by automating the generation of toolpaths and gcode. This is all great and in many cases efficient and useful, but there are cases when you want to be more specific and have more control. In my world (read Frikk), where I build custom machines for different weird workflows, a proper CAM tool doesn't really exists. Vespidae positions itself as a sort of hybrid between these two paradigms. 

## How do I use it? 

1. Download Build folder from repository. 
3. Move files from Build folder to Grasshopper Component Folder (Open Grasshopper, then File -> Special Folders -> Component Folder).
4. Restart Rhino and you should see Vespidae as a submenu in your Grasshopper workspace. 
5. Have a look in the example folder or the component documentation to familiarize yourself more with the different components of Vespidae. 

