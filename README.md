# Vespidae

**NOTE:** Vespidae is currently under heavy development and should be treated accordingly. Check you code before executing anything on your tool / machine. 

Installation instructions are located [here](#installation-instructions-how-do-i-use-it).

## What is it? 

Vespidae is a framework for prototyping and creating toolpaths for computer controlled motion systems (e.g 3D printers, laser cutters, CNC milling machines). At its core, Vespidae includes tools for manipulating and generating toolpaths as curves in Rhino, tools that tags these curves with process specific metadata, and finally tools that converts these actions to executable programs. Vespidae also includes tools for communitation between grasshopper and a machine controller. 

Vespidae is designed with an ethos to provide users with as much flexibility as possible when working with digital fabrication equipment. Modern CAM/slicing tools gives designers good means to create complex and efficient toolpaths, but this power comes at a cost of loosing flexibility and control. Full toolpath control can be particularly important for people who are either looking for specific aesthetics or for people who are building bespoke tools that does not necessarily fit into existing CAM tools. 

## Why does this exist? 

In the good ol' days CNC operators programmed their machines in raw "goto-position" commands (gcode). This took a fair amount of time to do and limited the complexity of what programs a machinist was able to make. However, it also kept the machinist close to what the machine was doing and gave the operator total control of each machine action. 

Todays CAM tools and slicer relieves the machinist/maker of this complexity by automating the generation of toolpaths and gcode. This is all great and in many cases efficient and useful, but there are edge-cases when you want to be more specific and have more control. In my world, where I build custom machines for different weird workflows, a proper CAM tool doesn't really exists. Vespidae positions itself as a sort of hybrid between these two workflows. 

## Installation Instructions: How do I use it? 

1. Open the Build folder from repository. 
![1. Open the Build folder from repository.](img/Installation/build.png?raw=true)
2. Copy all of the files from the Build folder.
![2. Copy all of the files from the Build folder.](img/Installation/copy.png?raw=true)
3. Open the Grasshopper Component Folder (Open Grasshopper, then File -> Special Folders -> Component Folder).
![3. Open Grasshopper.](img/Installation/open_grasshopper.png?raw=true)
4. Paste all copied Build files into the Component Folder.
![4. Paste Build files into Component Folder.](img/Installation/paste.png?raw=true)
5. Restart Rhino and you should see Vespidae as a submenu in your Grasshopper workspace. 
![5. Restart Rhino.](img/Installation/installed.png?raw=true)
6. Have a look in the example folder or the component documentation to familiarize yourself more with the different components of Vespidae. 
![6. Look at examples.](img/Installation/examples.png?raw=true)

