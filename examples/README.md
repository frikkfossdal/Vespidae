# Vespidae Examples 

This list of examples will help you get familiarized with how Vespidae works, starting from understanding the basic workflow to building more complex geometries using multiple tools. To run the examples, go to each examples specific folder. If there is both a .3dm file and a .gh file, open both to run the example. All gcode outputs are meant to work with printers running Marlin firmware and were run on a Jubilee open-source toolchanger. 

## Example 00 - Getting started: Workflow Introduction with Movement

Simple exercise to familiarize ourself with the Vespidae workflow. In this example, we outline the basic workflow for turning curves in Rhino into GCode that a printer can run. This example only has simple movements of the toolhead without printing. 

## Example 01 - Modifying Parameters with Vespidae Actions

This example still works with movement using a single tool, but outlines how you can start changing different parameters for each curve. With Vespidae, you can create very detailed parameters that are specific to each curve. 

## Example 02 - Working with Curves in Multiple Layers, Sorting

This example explores how we can work with curves when they are in multiple layers. This opens up the space to set different parameters for each layer of curves, and allows for multiple tools to be used at once. This example also outlines how the sorting components of Vespidae work. 

## Example 03 - Extrusion Introduction with Multiple Layers

In this example, we move on to generating Extrude components. This lets us start assigning parameters so that we can start printing!
This still builds on using multiple layers and multiple tools from the previous example, but you can also just use one layer for simplicity.

![Example 3, with the Rhino model on the left and the printed example on the right](img/Example03.png?raw=true)

## Example 04 - Pulse Extrusion with Parameterized Spiral

This example walks through how to directly generate custom curve geometries through Grasshopper. Since all curves are generated in Rhino, an accompanying Rhino file is not needed, unlike previous examples. We also walk through the Pulse Extrude component, and how it is different from the Extrude component.

![Example 4, with the Rhino model on the left, a printed example using a pulse extrude factor of 2 in the middle, and a printed example using a pulse extrude factor of 3 on the right. The print in the middle is ridged, with some gaps throughout the print. The print on the right is ridged and much more solid, with minimal gaps. ](img/Example04.png?raw=true)

## Example 05 - Nonplanar Vase Printing

This example walks through how to play around with Vespidae to create some unique shapes with nonplanar printing. The curve is generated in Grasshopper, so no accompanying Rhino file is needed. In this example, we define a custom script that allows users to play around with the shape of a vase with varying nonplanar ridges at the lip of the vase. 

![Example 5, a vase with nonplanar ridges at the lip printed using Vespidae.](img/Example05.png?raw=true)