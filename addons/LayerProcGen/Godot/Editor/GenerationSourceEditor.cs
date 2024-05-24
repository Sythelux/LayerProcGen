using System.Linq;
using Godot;
using Godot.Collections;
using Runevision.Common;
using AppDomain = System.AppDomain;
using Type = System.Type;

namespace Runevision.LayerProcGen;

[Tool]
public partial class GenerationSource
{
    static string[] layerTypeStrings;

    public override Array<Dictionary> _GetPropertyList()
    {
        var properties = new Array<Dictionary>
        {
            new()
            {
                { "name", "Layer" },
                { "type", (int)Variant.Type.StringName },
                { "usage", (int)PropertyUsageFlags.Default }, // See above assignment.
                { "hint", (int)PropertyHint.Enum },
                { "hint_string", FillLayerHintString() }
            },
            new()
            {
                { "name", "Size" },
                { "type", (int)Variant.Type.Vector2 },
            }
        };

        return properties;
    }

    public override Variant _Get(StringName property)
    {
        return (string)property switch
        {
            "Layer" => layer?.className ?? string.Empty,
            "Size" => (Vector2)size,
            _ => base._Get(property)
        };
    }

    public override bool _Set(StringName property, Variant value)
    {
        switch (property)
        {
            case "Layer":
                layer ??= new LayerNamedReference();
                layer.className = value.AsString();
                return true;
            case "Size":
                size = (Point)value.AsVector2();
                return true;
            default:
                return base._Set(property, value);
        }
    }

    private string FillLayerHintString()
    {
        if (layerTypeStrings == null)
        {
            var layerBaseType = typeof(AbstractChunkBasedDataLayer);
            layerTypeStrings = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => t != layerBaseType && layerBaseType.IsAssignableFrom(t) && !t.IsGenericType)
                .Select(t => t.FullName)
                .ToArray();
        }

        return string.Join(',', layerTypeStrings.Select(s => s[(s.LastIndexOf('.') + 1)..]));
    }
}