PSX Carnival Kit
================

132 PS1-style pieces to build a carnival shut for the season: the entrance arch, the ticket
booth, the midway's light poles and pennants, game booths and their prizes, the high striker,
the fortune teller, the food stand and carts, the carousel and the ferris wheel, the ghost train
and the fun house to walk through, the big top, the backlot's trailer, generator and power, the
clowns, the dead, and PSX Forest's woods and ground. Low-poly, one 2048x2048 point-filtered texture
atlas, the shading baked into vertex colours, real-world scale (1 unit = 1 metre). It fits PSX
Hospital Kit's grid and PSX Forest's ground:
https://heyheythere.itch.io/psx-hospital-kit
https://heyheythere.itch.io/psx-forest

What's in the zip
-----------------
glb/        one file per piece, with its animations (the best format for Godot, Blender, three.js)
fbx/        one file per piece, with its animations as takes (Unreal, Unity, most 3D apps)
obj/        one file per piece, still (any 3D app)
textures/   atlas.png, the one texture every piece uses, and the canvas, plywood, swirl, checker,
            deck, midway earth and sawdust textures it tiles from, 128x128 a metre, for your own
            geometry
icons/      an icon per piece, 256x256 PNG with transparency
props.json  every piece: title, group, size, triangles, parts, animations and what using it
            does; "layout" is the demo level
blender/    every piece in one .blend, the atlas packed in; its Level scene is the demo level
addons/     the Godot addon, psx_carnival_kit/: this folder is a Godot 4.3+ project that opens
            on its gallery (.gdignore keeps Godot out of the other folders)
unity/      a .unitypackage for Unity 6.3+ (Built-in and URP): prefabs, Machine.cs, a PSX
            shader, the demo level and a demo scene

The grid
--------
The ghost train and the fun house snap to a 2 m grid. A wall is 2 m long and 0.12 m thick,
3.6 m high, centred on a cell's edge, its front facing -Y in Blender (+Z in Godot, -Z in Unity):
the ghost train's weathered purple outside and black (or a painted ghoul) inside, the fun house's
spirals both sides. A floor or a roof covers one 2 x 2 m cell, the roof at the walls' top. The
fronts are 8 m, four cells, on the line of the walls; the ghost train's doors are 2 m either side
of its middle, each on a row of track. The track is 2 m a piece: a straight from a cell's near
edge to its far one, a corner from its near edge to its right.

The big top's twelve walls go round its middle 7.46 m out, each turned to face out (the way in one
of them), the roof and the king pole on the middle, the ring's curb in eighths on the middle too,
each turned 45 more. The ferris wheel goes on its frame's spot. The carousel, the booths, the
rides and the trailer stand on the ground at 0.

The ground is PSX Forest's: 4 x 4 m tiles centred on multiples of 4, their tops at 0. The midway's
edge tiles turn as the forest's edge does, grass to their front. A wall piece (a poster, a mirror,
the dressing mirror) has its back on the wall's face.

Godot 4.3 and later
-------------------
Copy addons/psx_carnival_kit/ into your project's addons/ folder. Each piece is a scene in
props/: drag it into your level and turn on grid snapping at 1 m. It has a box collider
(StaticBody3D) on each of its parts: a doorway's leave the way through clear and a door's moves
with it. A piece with animations has an AnimationPlayer:

    high_striker.get_node("AnimationPlayer").play("strike")   # high_striker: the piece in your level

The doors, the trunk, the high striker, the turnstile, the generator, the power board, the ride
panel and everything that lights, turns or lunges are machines: their scene root has machine.gd,
set up in the inspector. What runs or lights does it while what it needs is live (with nothing to
need, a use toggles it); the generator, the ticket booth and the ride panel take their keys. Wire
what each needs there (needs), or in code, and listen for its signals:

    power_panel.needs = [power_panel.get_path_to(generator_trailer)]
    generator_trailer.use(carried)            # takes its key from the player's items

demo/gallery.tscn shows every piece (drag to turn, arrows to browse, A for all of them), and the
demo level: L to see it, W to walk it (WASD and the mouse, Shift to run, E uses things, F for a
torch, Esc to stop). The fair's dark: get the generator running, the power on, and open the ghost
train.
The catalog and the level overview are lit without distance fog; walking keeps the level lighting.
The pieces use one material, props.tres: the atlas times the vertex colours, lit, so your lights
fall on them. For the full PS1 look (vertex snap, affine textures, 240p, dither), PSX Look comes with it, off as it starts.
With PSX Look in res://addons/psx_look/, the gallery shows the pieces through it (P toggles it).

Unity 6.3 and later
-------------------
Import the .unitypackage in unity/. Every piece is a prefab in Assets/PSXCarnivalKit/Prefabs/
with box colliders and an Animation component with its clips; the machines carry Machine.cs.
Documentation.txt there has the details; Demo/L_Level.unity is the demo level to walk and solve
and Demo/L_Demo.unity shows every piece.

Other engines and apps
----------------------
Use fbx/, glb/ or obj/ with textures/atlas.png. Set the texture's filtering to nearest/point and
turn off mipmaps for the crisp PS1 texels, and multiply it by the vertex colours (the baked
shading) in your material.

License: see LICENSE.txt. Made by heyheythere: https://heyheythere.itch.io
