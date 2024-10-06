using System;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace SandboxFM.Save;

[Serializable]
public class SerializableVector2
{
    [JsonProperty("x")]
    public float X { get; set; }

    [JsonProperty("y")]
    public float Y { get; set; }

    public SerializableVector2(Vector2 vector2)
    {
        X = vector2.X;
        Y = vector2.Y;
    }

    public SerializableVector2()
    {
    }

    public SerializableVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public static implicit operator SerializableVector2(Vector2 vector2)
    {
        return new SerializableVector2(vector2);
    }

    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }
}
