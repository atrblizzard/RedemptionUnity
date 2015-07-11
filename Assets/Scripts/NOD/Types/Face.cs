using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NODEngine
{
    public struct Face
    {
        public readonly Vector3 indices;

        public int[] Indices
        {
            get { return new[] {(int) indices[0], (int) indices[1], (int) indices[2]}; }
        }

        public Vector3 IndicesVector3
        {
            get { return new Vector3((int) indices[0], (int) indices[1], (int) indices[2]); }
        }

        public Face(BinaryReader reader)
        {
            indices = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public IEnumerable<int> GetIndices()
        {
            yield return (int) indices[0];
            yield return (int) indices[1];
            yield return (int) indices[2];
        }
    }
}
