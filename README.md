# unity-utf8exporter
Exporter toolset for creating utf8 mesh (and material) files for use with ThreeJS

The current Unity to UTF8 process is using a number of tools, some patched with minor fixes.
The process currently:
1. Export the scene using OBJExporter in Unity.
2. Run the OBJ to UTF8 convertor taken from Google Code with patches. (See patches folder)
   Preferably capture the json output if you wish to use materials like so:
   UTF8Convertor [OBJ filename] [UTF8 filename] > [Material .json filename]
3. Copy the generated textures folder, the UTF8 and material file (json) to your ThreeJS model folder
4. Load in using the UTF8Loader for ThreeJS with the modified patch (See patches folder)

There are a number of limitations with this system. 
- No animation supported
- Only diffuse textures currently supported
- No complex sub materials etc.
- No Atlasing (although thats being added soon)
- Fixed quantisation of coords, uvs, normals.

# Future Work
Move the whole conversion system into Unity C# and throw away the OBJExporter entirely.
A number of major changes need to be done to best execute this.
- C -> C# conversion (should be relatively ok, since its mostly math based).
- A decent Window panel in Unity to set UTF8 configuration (like quantization levels and so on).
- An improved UTF8 loader to support various features.
