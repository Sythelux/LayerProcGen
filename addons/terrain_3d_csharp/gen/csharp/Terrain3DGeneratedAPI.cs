/// <summary>
/// for Version: 0.9.1
/// this API is actually not generated at all. Until this is a thing this file is a placeholder with specific functions implemented as needed.
/// https://github.com/j20001970/GDMP-demo/discussions/6#discussioncomment-7008945
/// </summary>

using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = Godot.Collections.Array;

namespace Terrain3DBindings;

// ReSharper disable once InconsistentNaming
public class _Terrain3DInstanceWrapper_ : IDisposable
{
	public _Terrain3DInstanceWrapper_(GodotObject instance)
	{
		if (instance == null) throw new ArgumentNullException(nameof(instance));
		if (!ClassDB.IsParentClass(instance.GetClass(), GetType().Name)) throw new ArgumentException("\"_instance\" has the wrong type.");
		Instance = instance;
	}

	public GodotObject Instance { get; protected set; }

	public void Dispose()
	{
		Instance?.Dispose();
		Instance = null!;
	}

	public void ClearNativePointer()
	{
		Instance = null!;
	}
}

public class Terrain3D : _Terrain3DInstanceWrapper_
{
	private static readonly StringName storage_name = "storage";
	private static readonly StringName terrainlayers_name = "texture_list";
	private static readonly StringName material_name = "material";
	private static readonly StringName meshlods_name = "mesh_lods";
	private static readonly StringName collision_enabled_name = "collision_enabled";
	private static readonly StringName collision_layer_name = "collision_layer";
	private static readonly StringName collision_mask_name = "collision_mask";
	private static readonly StringName collision_priority_name = "collision_priority";
	private static readonly StringName debug_level_name = "debug_level";
	private static readonly StringName debug_show_collision_name = "debug_show_collision";
	private static readonly StringName mesh_size_name = "mesh_size";
	private static readonly StringName mesh_vertex_spacing_name = "mesh_vertex_spacing";
	private static readonly StringName render_cast_shadows_name = "render_cast_shadows";
	private static readonly StringName render_cull_margin_name = "render_cull_margin";
	private static readonly StringName render_layers_name = "render_layers";
	private static readonly StringName render_mouse_layer_name = "render_mouse_layer";
	private static readonly StringName version_name = "version";
	private static readonly StringName bake_mesh_name = "bake_mesh";
	private static readonly StringName generate_nav_mesh_source_geometry_name = "generate_nav_mesh_source_geometry";
	private static readonly StringName get_camera_name = "get_camera";
	private static readonly StringName get_intersection_name = "get_intersection";
	private static readonly StringName get_plugin_name = "get_plugin";
	private static readonly StringName set_camera_name = "set_camera";
	private static readonly StringName set_plugin_name = "set_plugin";
	private static readonly StringName update_aabbs_name = "update_aabbs";

	private Terrain3DMaterial? material;
	private Terrain3DStorage? storage;
	private Terrain3DTextureList? textureList;

	public Terrain3D(GodotObject instance) : base(instance)
	{
	}

	public Terrain3D() : base(ClassDB.Instantiate(nameof(Terrain3D)).AsGodotObject())
	{
	}

	public Node3D AsNode3D => (Node3D)Instance;

	public bool Visible
	{
		get => AsNode3D.Visible;
		set => AsNode3D.Visible = value;
	}

	public Vector3 Position
	{
		get => AsNode3D.Position;
		set => AsNode3D.Position = value;
	}

	public bool CollisionEnabled
	{
		get => Instance.Get(collision_enabled_name).AsBool();
		set => Instance?.Set(collision_enabled_name, value);
	}

	public int CollisionLayer
	{
		get => Instance.Get(collision_layer_name).AsInt32();
		set => Instance.Set(collision_layer_name, value);
	}

	public int CollisionMask
	{
		get => Instance.Get(collision_mask_name).AsInt32();
		set => Instance.Set(collision_mask_name, value);
	}

	public float CollisionPriority
	{
		get => Instance.Get(collision_priority_name).AsSingle();
		set => Instance.Set(collision_priority_name, value);
	}

	public int DebugLevel
	{
		get => Instance.Get(debug_level_name).AsInt32();
		set => Instance.Set(debug_level_name, value);
	}

	public bool DebugShowCollision
	{
		get => Instance.Get(debug_show_collision_name).AsBool();
		set => Instance.Set(debug_show_collision_name, value);
	}

	public Terrain3DMaterial Material
	{
		get
		{
			material ??= new Terrain3DMaterial(Instance.Get(material_name).AsGodotObject());
			return material;
		}
		set => Instance.Set(material_name, value.Instance); //TODO: maybe cleanup the old one
	}

	public int MeshLods
	{
		get => Instance.Get(meshlods_name).AsInt32();
		set => Instance.Set(meshlods_name, value);
	}

	public int MeshSize
	{
		get => Instance.Get(mesh_size_name).AsInt32();
		set => Instance.Set(mesh_size_name, value);
	}

	public float MeshVertexSpacing
	{
		get => Instance.Get(mesh_vertex_spacing_name).AsSingle();
		set => Instance.Set(mesh_vertex_spacing_name, value);
	}

	public GeometryInstance3D.ShadowCastingSetting RenderCastShadows
	{
		get => (GeometryInstance3D.ShadowCastingSetting)Instance.Get(render_cast_shadows_name).AsInt64();
		set => Instance.Set(render_cast_shadows_name, (long)value);
	}

	public float RenderCullMargin
	{
		get => Instance.Get(render_cull_margin_name).AsSingle();
		set => Instance.Set(render_cull_margin_name, value);
	}

	public uint RenderLayers
	{
		get => Instance.Get(render_layers_name).AsUInt32();
		set => Instance.Set(render_layers_name, value);
	}

	public uint RenderMouseLayer
	{
		get => Instance.Get(render_mouse_layer_name).AsUInt32();
		set => Instance.Set(render_mouse_layer_name, value);
	}

	public Terrain3DStorage Storage
	{
		get
		{
			storage ??= new Terrain3DStorage(Instance.Get(storage_name).AsGodotObject());
			return storage;
		}
		set => Instance.Set(storage_name, value.Instance); //TODO: maybe cleanup the old one
	}

	public Terrain3DTextureList TextureList
	{
		get
		{
			textureList ??= new Terrain3DTextureList(Instance.Get(terrainlayers_name).AsGodotObject());
			return textureList;
		}
		set => Instance.Set(terrainlayers_name, value.Instance); //TODO: maybe cleanup the old one
	}

	public string Version => Instance.Get(version_name).AsString();

	public Mesh BakeMesh(int lod, HeightFilter filter)
	{
		return Instance.Call(bake_mesh_name, lod, (long)filter).As<Mesh>();
	}

	public Vector3[] GenerateNavMeshSourceGeometry(Aabb globalAabb, bool requireNav = true)
	{
		return Instance.Call(generate_nav_mesh_source_geometry_name, globalAabb, requireNav).As<Vector3[]>();
	}

	public Camera3D GetCamera()
	{
		return Instance.Call(get_camera_name).As<Camera3D>();
	}

	public static Image GetFilledImage(Vector2I size, Color color, bool createMipmaps, Image.Format format)
	{
		throw new NotImplementedException();
		// return Instance.Call("get_filled_image", size, color, createmipmaps, (int)format).As<Image>();
	}

	public Vector3 GetIntersection(Vector3 srcPos, Vector3 direction)
	{
		return Instance.Call(get_intersection_name, srcPos, direction).As<Vector3>();
	}

	public static Vector2 GetMinMax(Image image)
	{
		throw new NotImplementedException();
		// return Instance.Call("get_min_max", image).As<Vector2>();
	}

	public EditorPlugin GetPlugin()
	{
		return Instance.Call(get_plugin_name).As<EditorPlugin>();
	}

	public static Image GetThumbnail(Image image, Vector2I? size)
	{
		size ??= new Vector2I(256, 256);
		throw new NotImplementedException();
		// return Instance.Call("get_thumbnail", image, (Vector2I)size).As<Image>();
	}

	public static Image PackImage(Image srcRgb, Image srcR, bool invertGreenChannel = false)
	{
		throw new NotImplementedException();
		//     return Instance.Call("pack_image", srcRgb, srcR, invertGreenChannel).As<Image>();
	}

	public void SetCamera(Camera3D camera)
	{
		Instance.Call(set_camera_name, camera);
	}

	public void SetPlugin(EditorPlugin plugin)
	{
		Instance.Call(set_plugin_name, plugin);
	}

	public void UpdateAabbs()
	{
		Instance.Call(update_aabbs_name);
	}
}

public class Terrain3DMaterial : _Terrain3DInstanceWrapper_
{
	private static readonly StringName shader_parameters = "_shader_parameters";
	private static readonly StringName auto_shader_name = "auto_shader";
	private static readonly StringName dual_scaling_name = "dual_scaling";
	private static readonly StringName shader_override_name = "shader_override";
	private static readonly StringName shader_override_enabled_name = "shader_override_enabled";
	private static readonly StringName show_autoshader_name = "show_autoshader";
	private static readonly StringName show_checkered_name = "show_checkered";
	private static readonly StringName show_colormap_name = "show_colormap";
	private static readonly StringName show_control_blend_name = "show_control_blend";
	private static readonly StringName show_control_texture_name = "show_control_texture";
	private static readonly StringName show_grey_name = "show_grey";
	private static readonly StringName show_heightmap_name = "show_heightmap";
	private static readonly StringName show_navigation_name = "show_navigation";
	private static readonly StringName show_roughmap_name = "show_roughmap";
	private static readonly StringName show_texture_height_name = "show_texture_height";
	private static readonly StringName show_texture_normal_name = "show_texture_normal";
	private static readonly StringName show_texture_rough_name = "show_texture_rough";
	private static readonly StringName show_vertex_grid_name = "show_vertex_grid";
	private static readonly StringName texture_filtering_name = "texture_filtering";
	private static readonly StringName world_background_name = "world_background";
	private static readonly StringName get_material_rid_name = "get_material_rid";
	private static readonly StringName get_region_blend_map_name = "get_region_blend_map";
	private static readonly StringName get_shader_param_name = "get_shader_param";
	private static readonly StringName get_shader_rid_name = "get_shader_rid";
	private static readonly StringName save_name = "save";
	private static readonly StringName set_shader_param_name = "set_shader_param";

	public Terrain3DMaterial(GodotObject instance) : base(instance)
	{
	}

	public Terrain3DMaterial() : base(ClassDB.Instantiate(nameof(Terrain3DMaterial)).AsGodotObject())
	{
	}

	public Dictionary ShaderParameters
	{
		get => Instance.Get(shader_parameters).As<Dictionary>();
		set => Instance.Set(shader_parameters, value);
	}

	public bool AutoShader
	{
		get => Instance.Get(auto_shader_name).AsBool();
		set => Instance.Set(auto_shader_name, value);
	}

	public bool DualScaling
	{
		get => Instance.Get(dual_scaling_name).AsBool();
		set => Instance.Set(dual_scaling_name, value);
	}

	public Shader ShaderOverride
	{
		get => Instance.Get(shader_override_name).As<Shader>();
		set => Instance.Set(shader_override_name, value);
	}

	public bool ShaderOverrideEnabled
	{
		get => Instance.Get(shader_override_enabled_name).AsBool();
		set => Instance.Set(shader_override_enabled_name, value);
	}

	public bool ShowAutoshader
	{
		get => Instance.Get(show_autoshader_name).AsBool();
		set => Instance.Set(show_autoshader_name, value);
	}

	public bool ShowCheckered
	{
		get => Instance.Get(show_checkered_name).AsBool();
		set => Instance.Set(show_checkered_name, value);
	}

	public bool ShowColormap
	{
		get => Instance.Get(show_colormap_name).AsBool();
		set => Instance.Set(show_colormap_name, value);
	}

	public bool ShowControlBlend
	{
		get => Instance.Get(show_control_blend_name).AsBool();
		set => Instance.Set(show_control_blend_name, value);
	}

	public bool ShowControlTexture
	{
		get => Instance.Get(show_control_texture_name).AsBool();
		set => Instance.Set(show_control_texture_name, value);
	}

	public bool ShowGrey
	{
		get => Instance.Get(show_grey_name).AsBool();
		set => Instance.Set(show_grey_name, value);
	}

	public bool ShowHeightmap
	{
		get => Instance.Get(show_heightmap_name).AsBool();
		set => Instance.Set(show_heightmap_name, value);
	}

	public bool ShowNavigation
	{
		get => Instance.Get(show_navigation_name).AsBool();
		set => Instance.Set(show_navigation_name, value);
	}

	public bool ShowRoughmap
	{
		get => Instance.Get(show_roughmap_name).AsBool();
		set => Instance.Set(show_roughmap_name, value);
	}

	public bool ShowTextureHeight
	{
		get => Instance.Get(show_texture_height_name).AsBool();
		set => Instance.Set(show_texture_height_name, value);
	}

	public bool ShowTextureNormal
	{
		get => Instance.Get(show_texture_normal_name).AsBool();
		set => Instance.Set(show_texture_normal_name, value);
	}

	public bool ShowTextureRough
	{
		get => Instance.Get(show_texture_rough_name).AsBool();
		set => Instance.Set(show_texture_rough_name, value);
	}

	public bool ShowVertexGrid
	{
		get => Instance.Get(show_vertex_grid_name).AsBool();
		set => Instance.Set(show_vertex_grid_name, value);
	}

	public TextureFiltering TextureFiltering
	{
		get => (TextureFiltering)Instance.Get(texture_filtering_name).AsInt32();
		set => Instance.Set(texture_filtering_name, (int)value);
	}

	public WorldBackground WorldBackground
	{
		get => (WorldBackground)Instance.Get(world_background_name).AsInt32();
		set => Instance.Set(world_background_name, (int)value);
	}

	public Rid GetMaterialRid()
	{
		return Instance.Call(get_material_rid_name).AsRid();
	}

	public Rid GetRegionBlendMap()
	{
		return Instance.Call(get_region_blend_map_name).AsRid();
	}

	public Variant GetShaderParam(StringName name)
	{
		return Instance.Call(get_shader_param_name);
	}

	public Rid GetShaderRid()
	{
		return Instance.Call(get_shader_rid_name).AsRid();
	}

	public void Save()
	{
		Instance.Call(save_name);
	}

	public void SetShaderParam(StringName name, Variant value)
	{
		Instance.Call(set_shader_param_name, name, value);
	}
}

public class Terrain3DStorage : _Terrain3DInstanceWrapper_
{
	private static readonly StringName regionsize_name = "region_size";
	private static readonly StringName heightrange_name = "height_range";
	private static readonly StringName colormaps_name = "color_maps";
	private static readonly StringName regionoffsets_name = "region_offsets";
	private static readonly StringName controlmaps_name = "control_maps";
	private static readonly StringName heightmaps_name = "height_maps";
	private static readonly StringName set_height_name = "set_height";
	private static readonly StringName getcontrol_name = "get_control";
	private static readonly StringName set_control_name = "set_control";
	private static readonly StringName force_update_maps_name = "force_update_maps";
	private static readonly StringName save_bit_name = "save_16_bit";
	private static readonly StringName version_name = "version";
	private static readonly StringName add_region_name = "add_region";
	private static readonly StringName export_image_name = "export_image";
	private static readonly StringName get_color_name = "get_color";
	private static readonly StringName get_height_name = "get_height";
	private static readonly StringName get_map_region_name = "get_map_region";
	private static readonly StringName get_maps_name = "get_maps";
	private static readonly StringName get_maps_copy_name = "get_maps_copy";
	private static readonly StringName get_mesh_vertex_name = "get_mesh_vertex";
	private static readonly StringName get_normal_name = "get_normal";
	private static readonly StringName get_pixel_name = "get_pixel";
	private static readonly StringName get_region_count_name = "get_region_count";
	private static readonly StringName get_region_index_name = "get_region_index";
	private static readonly StringName get_region_offset_name = "get_region_offset";
	private static readonly StringName get_roughness_name = "get_roughness";
	private static readonly StringName get_texture_id_name = "get_texture_id";
	private static readonly StringName has_region_name = "has_region";
	private static readonly StringName import_images_name = "import_images";
	private static readonly StringName layered_to_image_name = "layered_to_image";
	private static readonly StringName remove_region_name = "remove_region";
	private static readonly StringName save_name = "save";
	private static readonly StringName set_color_name = "set_color";
	private static readonly StringName set_map_region_name = "set_map_region";
	private static readonly StringName set_maps_name = "set_maps";
	private static readonly StringName set_pixel_name = "set_pixel";
	private static readonly StringName set_roughness_name = "set_roughness";
	private static readonly StringName update_height_range_name = "update_height_range";

	public Terrain3DStorage(GodotObject instance) : base(instance)
	{
	}

	private Resource AsResource => Instance as Resource;

	public Array<Image> ColorMaps
	{
		get => AsResource.Get(colormaps_name).AsGodotArray<Image>();
		set => AsResource.Set(colormaps_name, value);
	}

	public Array<Image> ControlMaps
	{
		get => AsResource.Get(controlmaps_name).AsGodotArray<Image>();
		set => AsResource.Set(controlmaps_name, value);
	}

	public Array<Image> HeightMaps
	{
		get => AsResource.Get(heightmaps_name).AsGodotArray<Image>();
		set => AsResource.Set(heightmaps_name, value);
	}

	public Vector2 HeightRange
	{
		get => AsResource.Get(heightrange_name).AsVector2();
		set => AsResource.Set(heightrange_name, value);
	}

	public Array<Vector2I> RegionOffsets
	{
		get => AsResource.Get(regionoffsets_name).AsGodotArray<Vector2I>();
		set => AsResource.Set(regionoffsets_name, value);
	}

	public RegionSize RegionSize
	{
		get => (RegionSize)AsResource.Get(regionsize_name).AsInt32();
		set => AsResource.Set(regionsize_name, (int)value);
	}

	public bool Save16Bit
	{
		get => AsResource.Get(save_bit_name).AsBool();
		set => AsResource.Set(save_bit_name, value);
	}

	public float Version
	{
		get => AsResource.Get(version_name).AsSingle();
		set => AsResource.Set(version_name, value);
	}

	public Error AddRegion(Vector3 globalPosition, Image[]? images = default, bool update = true)
	{
		images ??= System.Array.Empty<Image>();
		var imageArray = new Array();
		imageArray.AddRange(images);
		return Instance.Call(add_region_name, Variant.From(globalPosition), imageArray, update).As<Error>();
	}

	public Error ExportImage(String fileName, MapType mapType)
	{
		return AsResource.Call(export_image_name, fileName, (int)mapType).As<Error>();
	}

	public void ForceUpdateMaps(MapType mapType = MapType.TYPE_MAX)
	{
		AsResource.Call(force_update_maps_name, (int)mapType);
	}

	public Color GetColor(Vector3 globalPosition)
	{
		return AsResource.Call(get_color_name, globalPosition).As<Color>();
	}

	public int GetControl(Vector3 globalPosition)
	{
		return AsResource.Call(getcontrol_name, globalPosition).AsInt32();
	}

	public float GetHeight(Vector3 globalPosition)
	{
		return AsResource.Call(get_height_name, globalPosition).AsSingle();
	}

	public Image GetMapRegion(MapType mapType, int regionIndex)
	{
		return AsResource.Call(get_map_region_name, (int)mapType, regionIndex).As<Image>();
	}

	public Image[] GetMaps(MapType mapType)
	{
		return AsResource.Call(get_maps_name, (int)mapType).As<Image[]>();
	}

	public Image[] GetMapsCopy(MapType mapType)
	{
		return AsResource.Call(get_maps_copy_name, (int)mapType).As<Image[]>();
	}

	public Vector3 GetMeshVertex(int lod, HeightFilter filter, Vector3 globalPosition)
	{
		return AsResource.Call(get_mesh_vertex_name, lod, (int)filter, globalPosition).AsVector3();
	}

	public Vector3 GetNormal(Vector3 globalPosition)
	{
		return AsResource.Call(get_normal_name, globalPosition).AsVector3();
	}

	public Color GetPixel(MapType mapType, Vector3 globalPosition)
	{
		return AsResource.Call(get_pixel_name, (int)mapType, globalPosition).As<Color>();
	}

	public int GetRegionCount()
	{
		return AsResource.Call(get_region_count_name).AsInt32();
	}

	public int GetRegionIndex(Vector3 globalPosition)
	{
		return AsResource.Call(get_region_index_name, globalPosition).AsInt32();
	}

	public Vector2I GetRegionOffset(Vector3 globalPosition)
	{
		return AsResource.Call(get_region_offset_name, globalPosition).AsVector2I();
	}

	public float GetRoughness(Vector3 globalPosition)
	{
		return AsResource.Call(get_roughness_name, globalPosition).AsSingle();
	}

	public Vector3 GetTextureId(Vector3 globalPosition)
	{
		return AsResource.Call(get_texture_id_name, globalPosition).AsVector3();
	}

	public bool HasRegion(Vector3 globalPosition)
	{
		return AsResource.Call(has_region_name, globalPosition).AsBool();
	}

	public void ImportImages(Array<Image> images, Vector3? globalPosition = default, float offset = 0.0f, float scale = 1.0f)
	{
		globalPosition ??= Vector3.Zero;
		AsResource.Call(import_images_name, images, (Vector3)globalPosition, offset, scale);
	}

	public Image LayeredToImage(MapType mapType)
	{
		return AsResource.Call(layered_to_image_name, (int)mapType).As<Image>();
	}

	public static Image LoadImage(String fileName, int cacheMode = 0, Vector2? r16HeightRange = default, Vector2I? r16Size = default)
	{
		r16HeightRange ??= new Vector2(0, 255);
		r16Size ??= new Vector2I(0, 0);
		throw new NotImplementedException();
		// return AsResource.Call("load_image", fileName, cacheMode, (Vector2)r16HeightRange, (Vector2I)r16Size).As<Image>();
	}

	public void RemoveRegion(Vector3 globalPosition, bool update = true)
	{
		AsResource.Call(remove_region_name, globalPosition, update);
	}

	public void Save()
	{
		AsResource.Call(save_name);
	}

	public void SetColor(Vector3 globalPosition, Color color)
	{
		AsResource.Call(set_color_name, globalPosition, color);
	}

	public void SetControl(Vector3 globalPosition, uint control)
	{
		AsResource.Call(set_control_name, globalPosition, control);
	}

	public void SetHeight(Vector3 globalPosition, float height)
	{
		AsResource.Call(set_height_name, globalPosition, height);
	}

	public void SetMapRegion(MapType mapType, int regionIndex, Image image)
	{
		AsResource.Call(set_map_region_name, (int)mapType, regionIndex, image);
	}

	public void SetMaps(MapType mapType, Image[] maps)
	{
		AsResource.Call(set_maps_name, (int)mapType, maps);
	}

	public void SetPixel(MapType mapType, Vector3 globalPosition, Color pixel)
	{
		AsResource.Call(set_pixel_name, (int)mapType, globalPosition, pixel);
	}

	public void SetRoughness(Vector3 globalPosition, float roughness)
	{
		AsResource.Call(set_roughness_name, globalPosition, roughness);
	}

	public void UpdateHeightRange()
	{
		AsResource.Call(update_height_range_name);
	}
}

public class Terrain3DTexture : _Terrain3DInstanceWrapper_
{
	private static readonly StringName albedo_texture_name = "albedo_texture";
	private static readonly StringName albedo_color_name = "albedo_color";
	private static readonly StringName name_name = "name";
	private static readonly StringName normal_texture_name = "normal_texture";
	private static readonly StringName texture_id_name = "texture_id";
	private static readonly StringName uv_rotation_name = "uv_rotation";
	private static readonly StringName uv_scale_name = "uv_scale";
	private static readonly StringName clear_name = "clear";

	public Terrain3DTexture() : base(ClassDB.Instantiate(nameof(Terrain3DTexture)).AsGodotObject())
	{
	}

	public Terrain3DTexture(GodotObject instance) : base(instance)
	{
	}

	private Resource AsResource => Instance as Resource;

	private Color AlbedoColor
	{
		get => AsResource.Get(albedo_color_name).AsColor();
		set => AsResource.Set(albedo_color_name, value);
	}

	public Texture2D AlbedoTexture
	{
		get => AsResource.Get(albedo_texture_name).As<Texture2D>();
		set => AsResource.Set(albedo_texture_name, value);
	}

	public string Name
	{
		get => AsResource.Get(name_name).AsString();
		set => AsResource.Set(name_name, value);
	}

	public Texture2D NormalTexture
	{
		get => AsResource.Get(normal_texture_name).As<Texture2D>();
		set => AsResource.Set(normal_texture_name, value);
	}

	public int TextureId
	{
		get => AsResource.Get(texture_id_name).AsInt32();
		set => AsResource.Set(texture_id_name, value);
	}

	public float UvRotation
	{
		get => AsResource.Get(uv_rotation_name).AsSingle();
		set => AsResource.Set(uv_rotation_name, value);
	}

	public float UvScale
	{
		get => AsResource.Get(uv_scale_name).AsSingle();
		set => AsResource.Set(uv_scale_name, value);
	}

	public void Clear()
	{
		AsResource.Call(clear_name);
	}
}

public class Terrain3DTextureList : _Terrain3DInstanceWrapper_
{
	private static readonly StringName property_name = "textures";
	private static readonly StringName get_texture_name = "get_texture";
	private static readonly StringName get_texture_count_name = "get_texture_count";
	private static readonly StringName save_name = "save";
	private static readonly StringName set_texture_name = "set_texture";

	private Terrain3DTexture[]? textures;

	public Terrain3DTextureList(GodotObject _instance) : base(_instance)
	{
	}

	public Terrain3DTextureList() : base(ClassDB.Instantiate(nameof(Terrain3DTextureList)).AsGodotObject())
	{
	}

	private Resource AsResource => Instance as Resource;

	public Terrain3DTexture[] Textures
	{
		get
		{
			textures ??= Instance.Get(property_name).As<GodotObject[]>().Select(o => new Terrain3DTexture(o)).ToArray();
			return textures;
		}
		set => Instance.Set(property_name, value.Select(texture => texture.Instance).ToArray());
	}

	public Terrain3DTexture GetTexture(int index)
	{
		return new Terrain3DTexture(Instance.Call(get_texture_name, index).AsGodotObject());
	}

	public int GetTextureCount()
	{
		return Instance.Call(get_texture_count_name).AsInt32();
	}

	public void Save()
	{
		Instance.Call(save_name);
	}

	public void SetTexture(int index, Terrain3DTexture texture)
	{
		Instance.Call(set_texture_name, index, texture.Instance);
	}
}

public enum MapType
{
	TYPE_HEIGHT = 0,
	TYPE_CONTROL = 1,
	TYPE_COLOR = 2,
	TYPE_MAX = 3,
}

public enum HeightFilter
{
	HEIGHT_FILTER_NEAREST = 0,
	HEIGHT_FILTER_MINIMUM = 1
}

public enum RegionSize
{
	SIZE_1024 = 1024
}

public enum TextureFiltering
{
	LINEAR = 0,
	NEAREST = 1
}

public enum WorldBackground
{
	NONE = 0,
	FLAT = 1,
	NOISE = 2
}
