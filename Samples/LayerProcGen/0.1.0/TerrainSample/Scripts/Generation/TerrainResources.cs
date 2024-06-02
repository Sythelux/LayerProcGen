using Godot;
using Terrain3DBindings;

[Tool]
[GlobalClass]
public partial class TerrainResources : SingletonAsset<TerrainResources> {
	[Export]
	public Texture2D grassTex;
	[Export]
	public Texture2D cliffTex;
	[Export]
	public Texture2D pathTex;
	[Export]
	public Texture2D grassDetail;

	[ExportCategory("Material")]
	[Export]
	public Resource material;
	[Export]
	private Resource terrainData;
	
	public Terrain3DMaterial Material => new(material);
	public Terrain3DStorage TerrainData => new(terrainData);
}
