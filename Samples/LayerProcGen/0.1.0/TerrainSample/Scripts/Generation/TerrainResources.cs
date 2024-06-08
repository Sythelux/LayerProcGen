using Godot;
using Terrain3DBindings;

[Tool]
[GlobalClass]
public partial class TerrainResources : SingletonAsset<TerrainResources>
{

	[Export(PropertyHint.ResourceType, nameof(Terrain3DMaterial))]
	protected Resource material;
	[Export(PropertyHint.ResourceType, nameof(Terrain3DStorage))]
	protected Resource terrainData;
	[Export(PropertyHint.ResourceType, nameof(Terrain3DTextureList))]
	protected Resource textureList;
	
	public Terrain3DMaterial Material => new(material);
	public Terrain3DStorage TerrainData => new(terrainData);
	public Terrain3DTextureList TextureList => new(textureList);
}
