using System.IO;

namespace NODEngine
{
    public struct MeshGroup
    {
        public readonly int MaterialID;
        public readonly short NumFaces;
        public readonly short NumVertices;
        public readonly short MinVertices;
        public readonly ushort GroupFlags;
        public readonly short BoneNum;
        public readonly short MeshNum;

        public MeshGroup(BinaryReader reader)
        {
            MaterialID = reader.ReadInt32();
            reader.ReadBytes(12); //byte[] reserved for padding
            NumFaces = reader.ReadInt16();
            NumVertices = reader.ReadInt16();
            MinVertices = reader.ReadInt16();
            GroupFlags = reader.ReadUInt16();
            BoneNum = reader.ReadInt16(); // Byte
            MeshNum = reader.ReadInt16(); // Ditto
        }
    }
}
