/// <summary>
/// this API is actually not generated at all. Until this is a thing this file is a placeholder with specific functions implemented as needed.
/// https://github.com/j20001970/GDMP-demo/discussions/6#discussioncomment-7008945
/// </summary>

using Godot;
using Godot.Collections;
using System;

namespace Terrain3DBindings
{
    public class _Terrain3DInstanceWrapper_ : IDisposable
    {
        public GodotObject Instance { get; protected set; }

        public _Terrain3DInstanceWrapper_(GodotObject _instance)
        {
            if (_instance == null) throw new ArgumentNullException(nameof(_instance));
            if (!ClassDB.IsParentClass(_instance.GetClass(), GetType().Name)) throw new ArgumentException("\"_instance\" has the wrong type.");
            Instance = _instance;
        }

        public void Dispose()
        {
            Instance?.Dispose();
            Instance = null;
        }

        public void ClearNativePointer()
        {
            Instance = null;
        }
    }

    public class Terrain3D : _Terrain3DInstanceWrapper_
    {
        private const string STORAGE_PROPERTY_NAME = "storage";
        private const string TERRAINLAYERS_PROPERTY_NAME = "texture_list";
        private const string MATERIAL_PROPERTY_NAME = "material";
        private const string MESHLODS_PROPERTY_NAME = "mesh_lods";

        private Terrain3DStorage? _storage;
        private Terrain3DTextureList? _textureList;
        private Terrain3DMaterial? _material;

        private Node3D _asNode3D => Instance as Node3D;

        public bool Visible
        {
            get => _asNode3D.Visible;
            set => _asNode3D.Visible = value;
        }

        public Vector3 Position
        {
            get => _asNode3D.Position;
            set => _asNode3D.Position = value;
        }

        public Terrain3DStorage Storage
        {
            get
            {
                _storage ??= new Terrain3DStorage(Instance.Get(STORAGE_PROPERTY_NAME).AsGodotObject());
                return _storage;
            }
            set => Instance.Set(STORAGE_PROPERTY_NAME, value.Instance); //TODO: maybe cleanup the old one
        }

        public Terrain3DTextureList TextureList
        {
            get
            {
                _textureList ??= new Terrain3DTextureList(Instance.Get(TERRAINLAYERS_PROPERTY_NAME).AsGodotObject());
                return _textureList;
            }
            set => Instance.Set(TERRAINLAYERS_PROPERTY_NAME, value.Instance); //TODO: maybe cleanup the old one
        }

        public Terrain3DMaterial Material
        {
            get
            {
                _material ??= new Terrain3DMaterial(Instance.Get(MATERIAL_PROPERTY_NAME).AsGodotObject());
                return _material;
            }
            set => Instance.Set(MATERIAL_PROPERTY_NAME, value.Instance); //TODO: maybe cleanup the old one
        }

        public int mesh_lods
        {
            get => Instance.Get(MESHLODS_PROPERTY_NAME).AsInt32();
            set => Instance.Set(MESHLODS_PROPERTY_NAME, value);
        }

        public Terrain3D(GodotObject _instance) : base(_instance)
        {
        }
        
        public Terrain3D() : base(ClassDB.Instantiate(nameof(Terrain3D)).AsGodotObject())
        {
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

    public class Terrain3DStorage : _Terrain3DInstanceWrapper_
    {
        private const string REGIONSIZE_PROPERTY_NAME = "region_size";
        private const string HEIGHTRANGE_PROPERTY_NAME = "height_range";
        private const string COLORMAPS_PROPERTY_NAME = "color_maps";
        private const string REGIONOFFSETS_PROPERTY_NAME = "region_offsets";
        private const string CONTROLMAPS_PROPERTY_NAME = "control_maps";
        private const string HEIGHTMAPS_PROPERTY_NAME = "height_maps";
        private const string SETHEIGHT_FUNCTION_NAME = "set_height";
        private const string FORCE_UPDATE_MAPS_FUNCTION_NAME = "force_update_maps";

        private Resource _asResource => Instance as Resource;

        public RegionSize RegionSize
        {
            get => (RegionSize)_asResource.Get(REGIONSIZE_PROPERTY_NAME).AsInt32();
            set => _asResource.Set(REGIONSIZE_PROPERTY_NAME, (int)value);
        }

        public Vector2 HeightRange
        {
            get => _asResource.Get(HEIGHTRANGE_PROPERTY_NAME).AsVector2();
            set => _asResource.Set(HEIGHTRANGE_PROPERTY_NAME, value);
        }

        public Array<Image> ColorMaps
        {
            get => _asResource.Get(COLORMAPS_PROPERTY_NAME).AsGodotArray<Image>();
            set => _asResource.Set(COLORMAPS_PROPERTY_NAME, value);
        }

        public Array<Image> ControlMaps
        {
            get => _asResource.Get(CONTROLMAPS_PROPERTY_NAME).AsGodotArray<Image>();
            set => _asResource.Set(CONTROLMAPS_PROPERTY_NAME, value);
        }

        public Array<Image> HeightMaps
        {
            get => _asResource.Get(HEIGHTMAPS_PROPERTY_NAME).AsGodotArray<Image>();
            set => _asResource.Set(HEIGHTMAPS_PROPERTY_NAME, value);
        }

        public Array<Vector2I> RegionOffsets
        {
            get => _asResource.Get(REGIONOFFSETS_PROPERTY_NAME).AsGodotArray<Vector2I>();
            set => _asResource.Set(REGIONOFFSETS_PROPERTY_NAME, value);
        }

        public Terrain3DStorage(GodotObject _instance) : base(_instance)
        {
        }

        public void SetHeight(Vector3 globalPosition, float height)
        {
            _asResource.Call(SETHEIGHT_FUNCTION_NAME, globalPosition, height);
        }

        public void ForceUpdateMaps(MapType mapType)
        {
            _asResource.Call(FORCE_UPDATE_MAPS_FUNCTION_NAME, (int)mapType);
        }

    }

    public class Terrain3DTexture : _Terrain3DInstanceWrapper_
    {
        private const string ALBEDOTEXTURE_PROPERTY_NAME = "albedo_texture";
        private Resource _asResource => Instance as Resource;

        public Texture2D AlbedoTexture
        {
            get => _asResource.Get(ALBEDOTEXTURE_PROPERTY_NAME).As<Texture2D>();
            set => _asResource.Set(ALBEDOTEXTURE_PROPERTY_NAME, value);
        }


        public Terrain3DTexture() : base(ClassDB.Instantiate(nameof(Terrain3DTexture)).AsGodotObject())
        {
        }

        public Terrain3DTexture(GodotObject _instance) : base(_instance)
        {
        }
    }

    public class Terrain3DTextureList : _Terrain3DInstanceWrapper_
    {
        public Terrain3DTextureList(GodotObject _instance) : base(_instance)
        {
        }

        public Terrain3DTextureList() : base(ClassDB.Instantiate(nameof(Terrain3DTextureList)).AsGodotObject())
        {
        }

        public Terrain3DTexture GetTexture(int index)
        {
            return new Terrain3DTexture(Instance.Call("get_texture", index).AsGodotObject());
        }

        public void SetTexture(int index, Terrain3DTexture texture)
        {
            Instance.Call("set_texture", index, texture.Instance);
        }

    }

    public class Terrain3DMaterial : _Terrain3DInstanceWrapper_
    {
        public Terrain3DMaterial(GodotObject _instance) : base(_instance)
        {
        }
    }
}
