# Vespidae Examples 

## Example_00 - Getting started: Pattern Coaster



## Spiral vase on the Ultimaker 

Ultimaker uses a flavor of Griffin as gcode. Important note here is that the CuraEngine(?) relies on a custom header in the gcode. Without this header, and without this header being correct, the ultimaker will flat-out refuse your gcode. I had a frustrating time figuring this out (turned out I used the wrong object-envelope in the header). The header is documented-ish [here](https://community.ultimaker.com/topic/15555-inside-the-ultimaker-3-day-1-gcode/). 

I made a simple script that uses the uv-coordinates to wrap a spiral around a surface in Rhino. The script is attached in [the repo](add link).

Next challenge is calculating extrusion rate. Main train of though coming into this is that I need to check the length between each point that is translated to gcode and use this to determine how much material that should be extruded between these two points. 


## Probing script on Clank 

## Microscope Scan on Clank 

## Pen-plotting on Clank 
