using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[CreateAssetMenu]
public class CourseData : ScriptableObject, INetworkSerializable
{
    public string courseName;
    public Color backgroundColour;
    public int skyboxIndex;
    public Vector3 sunRotation;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref courseName);
        serializer.SerializeValue(ref backgroundColour);
        serializer.SerializeValue(ref skyboxIndex);
        serializer.SerializeValue(ref sunRotation);
    }

}
