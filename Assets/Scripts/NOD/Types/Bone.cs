using System.IO;
using UnityEngine;

namespace NODEngine
{
    public struct Bone
    {
        public readonly Vector3 RestTranslate;
        public readonly float[,] RestMatrixInverse;
        public readonly short SiblingID;
        public readonly short ChildID;
        public readonly short ParentID;

        public Bone(BinaryReader reader, Header header)
        {
            RestTranslate = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            RestMatrixInverse = new float[3, 4];
            for (int j = 0; j < 4; j++) // column 
            {
                for (int k = 0; k < 3; k++) // row
                    RestMatrixInverse[k, j] = reader.ReadSingle();
            }

            SiblingID = reader.ReadInt16();
            ChildID = reader.ReadInt16();
            ParentID = reader.ReadInt16();
        }
    }
}
