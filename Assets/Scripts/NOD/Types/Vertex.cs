using UnityEngine;
using System.IO;

namespace NODEngine
{
    public struct Vertex
    {
        public readonly Vector3 Pos;
        public readonly Vector3 Norm;
        public readonly Vector2 UV;

        public readonly float Weight;
        public readonly int BoneNum;

        public Vertex(BinaryReader reader)
        {
            Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Norm = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            UV = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Weight = reader.ReadSingle();
            BoneNum = reader.ReadInt32();
        }
    }
}
