
# Vespidae Develop Log

# Prior log lost in time 

Today is all about simple toolpaths and visualization. I need scripts that takes curves as input and translates this into usable machine code. Simultaneously it needs to visualize the machine movement (retract moves, travel moves, etc). I'm not sure if these should be separated or how the data should be formmated, but I figure this will become clear if I work with the data for a while. Here goes. 

**first challenge - sort curves**

I need to sort the input curves. I have to create my own specialized sorting function to do this.  I dont fully understand this in C#, so I should return to this point and refine in near future. I'm using the LINQ  library with a custom sorting function to do this. This looks like this: 

	A = polylines.OrderBy(c => c.ElementAt(0).Y).ToList();
	
![curves sorted in y-direction](./img/sort_curves.gif)

I will have to revisit this once I'm doing toolpaths layer-vise.

**adding travel-moves**


**NOTE:** A nice feature would be the ability to separate *move-types* - e.g operation moves, travel-moves, etc. I'm thinking that this will require a custom class à la this: 

	public class Work{
	    public Polyline line;
	    public string Type;
	}

I think the proper way of doing this will be to create some custom components in Grasshopper in Visual Studio. I'll return to this if needed. For now I think I can handle this by dispatching the output list and assign different colors to the different moves. 

![Travel moves with curve-sorting in y-direction. Think about how this would be if I have multiple layers](./img/sorting_in_y.gif)

![](./img/travel_moves_viz.png)

**Toolpaths on 3D surfaces**

Simple exercise. Use Grasshopper's Contour component to "slice" 3D surface. Feed the resulting contours to Vespidae's Gcode generator. 

![Surface sliced with Contour in Grasshopper](./img/sliced_surface.png)

![Sliced surface with Vespidae Moves](./img/sliced_with_moves.png)

![Dynamic 3D toolpahts](./img/3D_surface_toolpaths.gif)


# Vinh Battery Printing

Ok so we are printing batteries. This is quite a unique workflow and will be a good showcase for dynamic CAM tools. There are lots of challenges that needs to be addressed. I think I finally need to get a proper-ish slicer up and running in Grasshopper. I'm thinking clipper makes sense here to generate infill toolpaths. I also need a way to test this in real world. For this I will use my trusty Ultimaker as it has two extruders (this compliment nicely with Vinh and his tool-changing Jubilee. 

As I understand it Vinh wants to print the base geometry of the battery (a box-geometry full of holes) and park the nozzle over each whole and fill it with plastic using the extruder. The geometry looks like this. 

![3D printed battery exploded view](./img/battery_geometry.png)


![Goo extruding into hole](./img/goo.gif)

Note to self: investigate how objects can be tagged with metadata using *setUserText* in Rhino. 



# 1310_2022

Starting (b)logging a bit late in the game here but I spent some time getting comfortable with the whole Visual Studio experience again. I've had a hard time setting everything up to run and compile properly, but I'm at a better place now. A highlight from this is how to create solutions that consists of multiple projects in one solution (think one project that holds all the Grasshopper components combined with projects that holds all the supporting code and libraries, for example the logic for Clipper). Following includes a short recipe for setting things up. I'm linking documentation for osx, but equivalent documentation exists for windows on McNeels developer sites. Here goes: 

## **Guide for setting things up in Visual Studio**

## *Install McNeel tools in Visual Studio*

First of all install the template tools from McNeel as documented [here](https://developer.rhino3d.com/guides/rhinocommon/installing-tools-mac/).

## *Create Solution*

Follow this guide to create first Grasshopper (or Rhino) component and to understand how they should be set up. 

## *Creating Custom Libraries / Projects*

To create class libraries for supporting code right click solution and add new project *(Add->New Project)*. In the following dialog select Library *(.NET->Library)*. After library folder has been generated and added to the solution right click top library folder and select options. Check that build settings are set to Library *(Build->General->Compile Target*. 

Now comes the weird part. If you want to include Rhino or Grasshopper namespaces, which you probably do in most cases, you have to go through a couple of extra steps to ensure that it compiles as a library. I'm not sure if this is the proper way to do things, but this is how I did it. 

1. First, I changed the target framework of the project to match that of the Rhino/Grasshopper-template generated project. In my case this was .NET Framework 4.8. (Build->General->Target Framework).
2. I then use the NuGet package manager to add Rhino namespace to the project. Right click project folder->Manage NuGet Packages. I then search and add Grasshopper to the project. 
	3. This changes the build settings and adds a new tab called Rhino. Go to this tab (right click project/Options/Build/Rhino). Then set the Plugin Type to Library. 
	4. Thats it. Library should be ready for use. Final thing to do is to add a reference to this project from the main Project (again, the one that Rhino/Grasshopper-template generated for you). Right click Rhino/Grasshopper-project/Add Reference. Your new project should be under the Projects-tab. Add it. 
	5. Build everything and hopefully you will be ok! 

## Back to logging 

This now includes Clipper. Look [here](http://angusj.com/delphi/clipper.php) for examples and documentation. 

![Offset tool in all its glory ](./img/vespidae_offset.gif)

# 1701_2022

Started updating the offset component to prepare it for slicing. As of now it only offsets a curve once, but I think it is more useful to enable it to offset a curve multiple times based on a given input number. I added the structure to do this component wise, but It's missing the logic to actually compute this. I'll wrap this up next session. 

Today I'm also implementing tools for converting polylines into gcode. Initially I'm doing this bruteforce and separating the "toolpahts" into two separate structures; one with raw gcode and one with polylines that represents the travel moves between each individual operation. I'm thinking that I really should make a better data structure here, where both gcode and polylines are linked in a single data structure / class. A cool thing this could be used for is to visualize the gcode commands in context of the toolpaths they represents in Rhino. I'm returning to this point in the future when I know more about what I'm after. For now, brute force is the way. 

I've also added a separate sorting component to the GcodeTool-shelf. Right now it only sorts in Y-direction but it should include some sort options (x,y,z, or combination). I'm also thinking that the Gcode generator should include a sort option. 

# 1801_2022

Working on adding infill algorithms to the slicer component. I'm starting out with low complexity for infill patterns. My strategy is to use the bounding box of a Brep, rotate it 45 degrees in regards to the brep to be sliced and use the edges to compute infill lines. I did a short test with Cura and it seems they are using the same strategy. 

Puh(!) I have to return to this. After some prototyping I've decided to initially create infill lines in x direction. This is a point I can totally return to, but I'm not so sure how important it really is. Method is extract bounding box of brep, find the min and max points of the bounding box and use these bounds to generate lines with given interval through a for-loop. Here is what it looks like: 

![Generating infill](./img/infill_lines.gif)

Next thing to do is to find the intersection between the contour of the Brep and the infill pattern and convert this into a toolpath. I scripted a prototype of this in Grasshopper for a single layer. First I extract the contour of the Brep at layer height 0. I then use clipper to compute the intersection between the contour and the infill lines. This gives me a list of polylines that look like this: 

![clipped polys](./img/clipped_polys.png)

For toolpaths I first sort every polyline in x-direction (I totally need to return to this point and create some kind of vector-based sorting but bare with me). I then flip every second curve (to make the path go "front-and-back"). Finally I iterate over all the points in the list of polylines and connect them. I will need tons of more logic here. This setup will fail if the direction of the infill is too angled in regards to x (because sorting fails). Returning soon. Result looks like this: 

![infill toolpath](./img/infill_toolpath.png)

I think I'm ready to put this into my code. First thing to do is to clean up my Clipper implementation. I'm trying to create a one-function-serves-them-all function that can handle any clip type (intersection, boolean, etc). I might need a more specialized function to handle slicing when my contour consists of more then just an outer contour (think pockets in the Brep), but I will return to this. Right now my "boolean" function only does just this, a boolean operation. I'm changing this to giving the intersection type as argument and running this through a switch-case. The function also only return a single polyline. Changing this to return as many polylines as Clipper computes. 

Another problem! I'm using wrong clipper. NuGet only provides Clipper 6.4.0(?). Manually adding clipper and changing the version to 6.4.2.

**Things I need to figure out with Clipper:** When I perform a clipping operation, for example difference, the resulting polytree keeps track of outer and inner polygons by nesting them as childs in the polytree. It does this by using a winding number algorithm as documented [here](http://www.angusj.com/delphi/clipper/documentation/Docs/Units/ClipperLib/Types/PolyFillType.htm). Does the same happen with the offset operation?

# 2401-2022

Logging fast today since I'm already behind. Clipper core functionality should now be properly implemented for doing both boolean operations and offsetting. Still, there are some minor things that should be addressed. Most important I need to keep track on height. Clipper computes everything in 2D. When I convert into "clipper-points" I need to keep track of the height and re-apply it when I convert back from clipper to rhino polylines. I'll add this when I need it. 

# 2501-2022

I need to think about how I want to structure the data and what a real case user journey looks like. A big goal I'm aiming for is to expose all the steps of a CAM processor and provide simple entry points where we can modify and customize its output and behaviour. For 3D printing for exampel this would entail exposure of the computing of the shells and infills. I'm thinking more and more that I want to create my own data object that contains all the data, and different components that allows step-wise manipulation of this object. I made a quick doodle for reference: 

![doodle of data structure](./img/philosophy_sketch.jpg)

A quick thought on this point: I could go at this from a higher abstraction point. Rather then segmenting the data into *layers*, as in layers in 3d printing, I could segment everything into operations or actions. These would represent any action that I want the machine to perform. I could then create a class that holds all of these actions, and this class would be shared between each Vespidae Component. I would also need a component that translate the class into Rhino-compatible stuff like lines and gcode and so on. I need to think more about this, but since I already created a slicer class that contains all layers I will move forward with a similar structure and see how it feels. 

As for slicing I think I want the complete slicer operation to consist of several separate components in Grasshopper; one for contouring the brep, one for creating infill, etc. This both exposes the data that each of these component generates and enables us to manipulate this data as it is flowing through the slicing pipeline. 

I've implemented a slicing component and a infill component according to this philosophy. Now we return to the problem of my clipping tool not factoring in height of the polylines its working on. The infill component uses clipping to calculate the intersection between the generated infill lines and the contours of the brep. This all works fine, but the calculated infill lines are all outputted on the same height (z=0). Fixing this is a big todo tomorrow! 

![Preview of infill clipping working. Layer is manually moved as the clipper height is wrong.](./img/infill_preview.png)

I'm getting close and I'm starting to like the feel of this. Hopefully shooting for a physical test on the ultimaker this week! 

# 2701_2022

Spent some time today to try to figure out how to connect clipped infill lines into connected toolpaths. This is a bigger struggle than I imagined it to be and I didn't really arrive at a good solution. A brief overview of my problem: 

![I want to go from this](./img/infill_notconnected.png)

![To this](./img/infill_connected.png)

I think there are several ways to go about this, but utlimately this is a np-hard problem and I have to go through some kind of search tree. I tried to develop a couple of different strategies here, namely creating a dictionary of points with connected point in polyline and using this to search and check for intersections. I think this could work but I need to define better logic. This is not critical for my slicer to work, the printing will just be a bit slower since I have to lift the nozzle between each line. I'll return to this point when I'm ready to optimize. 

Write about plans for top / bottom shells and infill density. 

# 0402_2022

Been bad at logging progress. I've shifted a lot of stuff around this week and I think I'm finally arriving at a structure I feel good about. 

**Clipper offset:** I revisited my offset component after a talk with Jens. Jens is missing two features: ability to offset multiple times and better / more logical sorting of inside / outside polygons in context of slicing. This is also something I totally need for slicing. I've modified the Offset-component with a new input that lets us set number of offsets in addition to the offset delta. What would be neat to add on this note is an option that just offsets until it cant offset anymore (think generating infill). 

# 0602_2022

~~Extracting extrusion values for z pinning from Cura. By default Cura gcode moves to a corner and extrudes a set amount of plastic to clean the extruder before printing starts (prime blob). The code for this looks like this:~~ 

The code for prime blob is not included in the gcode as this is done through firmware. 

Did first round of "real" testing on Ultimaker and exposed several problems. 

- How does a move object extrude? How do I add extrusion to a path? It might make sense to create a class for each move type. If not, I need to add some new parameters and logic. 
- How should the machine be set up intitally before all the Vespidae operations are performed? For now I think this can be handled manually using headers. 
- Creating moves between operations: Do I need to do this? It is totally necassary for visualization. When should I compute travel moves? Should I wait till all Vespidae operations are generated and then generate? Or should I generate on each operation? 


# 0802_2022

Adding extrusion. From Cura: 

	G1 X112.282 Y103.215 E25.98213
	G1 X120.852 Y103.215 E26.07617

From this: \
$120.852 - 112.282 = 8.57$\
$26.07617 - 25.98213 = 0.09404$

So per unit this is: $0.09404 / 8.57 = 0.010973$ 

# 1402_2022 

Restructure for toolpathing philosophy. I know call everything **Actions**, and I've implemented this as an abstract class. We can have different Actions like Move, Extrude, Cut, and these actions all inherit from the Base class but can have additional special parameters. The cool thing about this is that I can chain all Actions into one List which makes everything pretty and understandable. 

**Missing and todo next time:** 
- I need a good way to keep track on extrusion. Either I can reset extrusion value for each action by sending a G92 E, or I need to define a better logic to keep track on this.  What I should think about here is if gcode is generated as Actions are created or if gcode is created one time running over all actions (makes more sense?)
- Fix categories 
- Modify sorting component to take vector input. 
- Start thinking about toolpath texture. Think about how displacement and bump maps are used in texturing. 

Finally I need to start about creating some tutorials / documentation for how this can be used and what a user journey might look like. 

# 1502_2022

Added proper extrusion like discussed in last entry. Gcode is not computed before all actions are ready chained together. This means I can keep track of extrusion across different sets of Actions. I also cleaned up the categories a bit. 

Vinh shared his [project page](https://github.com/invinhtory/GrasshopperPrinting) where he is using Xylinus for creating code a Aerotech machine. Aerotech uses a different flavour of gcode - for example moves are defined as Linear X Y Z instead of G0. How can you adapt your translation components to handle this and what does this interaction look like in Grasshopper? 

Apart from that I think I'm good to go to start testing. I've set up a Rhino document with 3 different tests - 1. A simple test that just print some 2D lines, 2. Z-pinning, 3. Spiral-vase which will be first testing ground for texture component. 

