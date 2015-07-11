using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace NODEngine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Header
    {
        #region Public variables
        public uint Version { get; private set; }
        public uint NumMaterials { get; private set; }
        public byte[] MaterialName { get; private set; }
        public short NumBones { get; private set; }
        public short NumMeshes { get; private set; }
        public uint NumVertices { get; private set; }
        public uint NumFaces { get; private set; }
        public short NumGroups { get; private set; }
        public int ModelFlags { get; private set; }
        public Vector3[] Bounds { get; private set; }
        public int curOffset { get; private set; }

        // Bones
        public Vector3 RestTranslate { get; set; }
        public float[,] RestMatrixInverse { get; set; }
        public short SiblingID { get; set; }
        public short ChildID { get; set; }
        public short ParentID { get; set; }
        #endregion

        public List<string> MaterialNames { get; set; }

        public Header(BinaryReader reader) : this()
        {
            MaterialNames = new List<string>();

            Version = reader.ReadUInt32();
            if (Version != 7)
            {
                BondiGeek.Logging.LogWriter.Instance.WriteToLog("Invalid model version/file. Loading stopped.");
                return;
            }

            NumMaterials = reader.ReadUInt32();
            for (var i = 0; i < NumMaterials; ++i)
            {
                MaterialName = reader.ReadBytes(32);
                MaterialNames.Add(Encoding.Default.GetString(MaterialName));
            }
            NumBones = reader.ReadInt16();
            NumMeshes = reader.ReadInt16();
            NumVertices = reader.ReadUInt32();
            NumFaces = reader.ReadUInt32();
            NumGroups = reader.ReadInt16();
            ModelFlags = reader.ReadInt32();
            Bounds = new[] { new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                             new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()) };

            curOffset = (int) reader.BaseStream.Position;
        }

        public string PrintInfo()
        {
            string blob = "===== NOD Header =====\r\n";
            blob += ("NOD version: " + Version + "\r\n");
            blob += ("Material count: " + NumMaterials + "\r\n");
            for (var i = 0; i < NumMaterials; ++i)
                blob += ("Material name: " + MaterialNames[i] + "\r\n");
            blob += ("Bones count: " + NumBones + "\r\n");
            blob += ("Meshes count: " + NumMeshes + "\r\n");
            blob += ("Vertices count: " + NumVertices + "\r\n");
            blob += ("Faces count: " + NumFaces + "\r\n");
            blob += ("Mesh group count: " + NumGroups + "\r\n");
            blob += ("Model flags: " + ModelFlags + "\r\n");
            blob += ("Model bounds: " + Bounds[0] + " " + Bounds[1] + "\r\n");

            return blob;
        }

        [System.Flags]
        public enum NODModelFlags
        {
            HasLOD = 0x1,
            Inline = 0x2,
            Static = 0x4,
            Res1 = 0x8,
            Res2 = 0x16
        }
    }
}
