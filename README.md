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

# Vespidae Component Dictionary 

# 1. CurveTools
*Tools for manipulating geometry in Rhino. This includes clipping tools and soon slicing tools and tools for filling / clearing pockets.*

|:--|:--|
|Boolean|Performs boolean operations, or *Clipping*, on polygons using the ClipperLib. The operations include intersection, union, difference and xor. See [this](http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/ClipType.htm) for more documentation on the different operations. |
|Offset|Offsets polylines using the Clipper Library offset algorithm. Supports multiple offsets[^Only component] in same operation. Solution will be transformed to the XY plane of the first given polyline|
|Sort Curves| Sorts curves in first Y direction and then X direction. The sorting algorithm currently uses the first point of each polyline when sorting. |
|Slicer| *Under development.* Slices Breps into slices of 2D polygons. |

# 2. Actions

_Actions-components converts sets of polylines into Vespidae-actions and tags them with relevant metadata. For example, the ExtrudeAction tags each polyline with an extrusion-parameter that sets the extrusion rate for each move. This metadata is applied by the solvers and visualizers in step 3._

|:--|:--|
| ExtrudeAction | Creates ExtrudeActions that is tagged with relevant metadata. <br> <br> **ex(extrusion)** extrusion flowrate multiplier. Extrusion amount is calculated by: `distance x 0.01 x ex` <br> **s (speed)** - speed of move. Translates to `F_speed_` in gcode. <br> **t(temperature)** - extruder temperature. Translates to `S_temperature` in gcode.|
| MoveAction | *General purpose movement actions.*  <br> <br> **s(speed)**  - speed of move. Translates to `F_speed_` in gcode. <br> **to (tool_id)**  - tool number to execute move with. Translates to `T_toolId_` <br> **gInj (gcodeInjection)** - injects gcode prior to the move. The gcode is added when the action is translated to gcode in step 3.|
| ZpinAction | *under development*|
| Sort Actions| *Sorts action according to input criteria. So far this includes sorting by x-y and z-directions.* <br> <br> **actions** - actions to be sorted. <br> **sort (sort type)** - Sorting options: 0: x-direction, 1: y-direction, 2: z-direction. <br> **flip**- flips the sorted list.| 

# 3. Solve
*Solvers are used to compute and derive programs (sequence of actions) from sets of Actions. Solvers are also used when converting Actions into gcode*

|:--|:--|
| Solver Actions|*Takes lists of actions and transforms them into a list of executable Actions, adding travel moves between each action.* <br><br> **Vobj (Vespidae Actions)** - Input actions to the solver. <br> **rh (retract height)** - Retract height between the moves. <br> **ts (travel speed)** - Travel speed between moves. <br> **pr (partial retract)** - enables partial retract between Actions where possible[^the algorithm checks if the next Actions z-height is the same as the current Actions z-height. If yes, it will do a partial retract currently predefined to .2 mm.]. |
| Solver Gcode| *Takes lists of actions and converts all actions into a single gcode file.* <br><br> **Vobj (Vespidae Actions)** - Input actions to the solver. <br> **h (header)** - inject header gcode. <br> **f (footer)** - inject footer gcode.|
| ExposePaths | *Visualizes Vespidae Actions in the Rhino Workspace.* <br><br>|

# 4. Communicate

|:--|:--|
| UploadGcode | *Uploads gcode directly to a Duet controller. Requires a network connection to the duet and that the duet is running [DWC](https://docs.duet3d.com/en/User_manual/Reference/Duet_Web_Control_Manual)*.|
| | |
| | |
