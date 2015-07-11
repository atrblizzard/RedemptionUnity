using System.IO;

namespace NODEngine
{
    public struct MeshDefinitions
    {
        public readonly byte[] MeshName;

        public MeshDefinitions(BinaryReader reader)
        {
            MeshName = reader.ReadBytes(32);
        }
    }
}
