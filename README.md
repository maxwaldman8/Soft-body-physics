# Soft body physics
Ok-ish 3d (and if you want 2d) mesh (only convex) softbody physics that I made for unity. 

Explanation:

It first splits the mesh into "nodes", which are just small spheres with rigidbodies. Then it connects each of those nodes to its "neighbors" (which are the nodes it is connected to by an edge) with unity spring joints. Each node is also connected to the center (the parent rigidbody) with a spring joint. All of the settings on the spring joints, along with all of the settings of the ridgidbody, are all adjustable, and there is even a 2D mode that turns sprites into (convex) meshes. 

WARNING: DO NOT USE WITH LARGE MESHES (it gets VERY laggy)

Instructions: 

1. Unzip

For just using the prefabs:

1. Put all of the files into your project
2. Just use the prefab (instructions on how to change the settings are later)

For making your own (convex) softbody:

1. Only import the SoftBody script, Node prefab, and if you want to make something with a lot of vertices, you can also import the SphereNode prefab (for spheres and things with a lot of vertices)
2. Create a game object with a mesh filter (set to the mesh you want to become softbody), mesh renderer, mesh collider (you need to turn on convex), rigidbody (start with mass 1, drag 0.1, and angular drag 5 but you will probably need to change the settings later) with gravity on, and the softbody script (for now with default settings, but if there is nothing in the "Node" field then put the node/spherenode prefab in it)
3. Adjust the settings

Adjustable options:

On script: 

Spring, Damper, and Tolerance: The corresponding settings on the spring joints that connect each node to each other
Center Spring, Center Damper, and Center Tolerance: The corresponding settings on the spring joints that connect each node to the center (the parent rigidbody)
Sprite Mode (2D mode): Whether or not to use a sprite to make the mesh instead of the one on the mesh filter
Sprite: If sprite mode is on, the sprite to use for the mesh
Node: The node prefab for the softbody (note: there are no 2D node prefabs included (but you can disable x and y rotation and z movement for the effect), but you can make one if you want, but you might need to edit the script to work with full 2D)

On rigidbody: 

The normal rigidbody settings (I have found that there are normally better results when the values are set to the same values as the nodes, except for with spheres or shapes with a lot of vertices)

On node:

You can adjust the rigidbody settings (just keep rotation disabled) to fit your needs; generally, smaller mass is better for more vertices

You can also adjust the sphere collider radius if you really need too
