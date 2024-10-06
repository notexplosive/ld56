using System;
using ExplogineMonoGame.Data;
using Newtonsoft.Json;

namespace SandboxFM.Save;

[Serializable]
public class SerializableRectangle
{
    public SerializableRectangle(RectangleF rectangle)
    {
        TopLeft = rectangle.TopLeft;
        Size = rectangle.Size;
    }

    public SerializableRectangle()
    {
    }

    [JsonIgnore]
    public SerializableVector2 TopLeft { get; set; } = new();
    
    [JsonIgnore]
    public SerializableVector2 Size { get; set; } = new();
    
    [JsonProperty("x")]
    public float X { get => TopLeft.X; set => TopLeft.X = value; }
    
    [JsonProperty("y")]
    public float Y { get => TopLeft.Y; set => TopLeft.Y = value; }
    
    [JsonProperty("width")]
    public float Width { get => Size.X; set => Size.X = value; }
    [JsonProperty("height")]
    public float Height { get => Size.Y; set => Size.Y = value; }

    public static implicit operator SerializableRectangle(RectangleF rectangle)
    {
        return new SerializableRectangle(rectangle);
    }

    public RectangleF ToRectangleF()
    {
        return new RectangleF(TopLeft.ToVector2(), Size.ToVector2());
    }
}
