This voxel prototype was done based on many hints of:
http://www.blockstory.net/node/56
http://forum.unity3d.com/threads/63149-After-playing-minecraft
https://gist.github.com/jstanden


PLATFORMS
=========
This demo works on iOS, Android, Mac, Win and Webplayer.
The main bottleneck is the CPU while the player is creating and destroying blocks. In the code I left some hints of things that I would have done differently now, or links to articles that offers a better solution for what I did.


IMPORTANT CLASSES
=================
The most important classes are inside Assets/Scripts/World. I suggest to you give a look in the following order:
- WorldController
- World
- Chunk
- ChunkObject
- ChunkRenderer
- Block
Almost all character classes came from StandardAssets. The only one that I added was CharacterSkills that handle with the block creation/destruction.


DEBUG OPTIONS
=============
In the worldScene, look for an gameObject called "WorldController".
There you will find some flags to show some gizmos and lightning stuff.

To see the world being generated, try enable the following flags:
Show height map gizmos
Show Chunks Bounding Box
Async World Creation 

To improve the performance disable the light attenuation and smooth light.


CONTACT
=======
Charles Marcel de Barros
charlesbarros@gmail.com
http://gamecoderbr.blogspot.com.br/
