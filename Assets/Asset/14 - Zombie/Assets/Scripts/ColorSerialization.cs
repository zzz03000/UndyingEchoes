using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExitGames.Client.Photon;

public class ColorSerialization
{
    public static byte[] SerializeColor(object targetObject)
    {
        Color color = (Color)targetObject;

        Quaternion colorToQuaterinon = new Quaternion(color.r, color.g, color.b, color.a);
        byte[] bytes = Protocol.Serialize(colorToQuaterinon);

        return bytes;
    }

    public static object DeserializeColor(byte[] bytes)
    {
        Quaternion quaternion = (Quaternion)Protocol.Deserialize(bytes);

        Color color = new Color(quaternion.x,quaternion.y,quaternion.z,quaternion.w);

        return color;
    }
}
