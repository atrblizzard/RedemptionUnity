using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NODEngine
{
    public class NODMesh
    {
        private Header header;

        public readonly List<Bone> Bones = new List<Bone>();
        private readonly List<Vertex> Vertices = new List<Vertex>();
        public readonly List<MeshDefinitions> MeshDefinitions = new List<MeshDefinitions>();
        private readonly List<Face> Faces = new List<Face>();
        private readonly List<MeshGroup> MeshGroups = new List<MeshGroup>();

        public List<string> MeshNames = new List<string>();

        public NODMesh(string filename)
        {
            using (BinaryReader reader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                header = new Header(reader);

                BondiGeek.Logging.LogWriter.Instance.WriteToLog(header.PrintInfo());

                reader.BaseStream.Seek(header.curOffset, SeekOrigin.Begin);

                // Read through the mesh bones
                for (var i = 0; i < header.NumBones; ++i)
                {
                    Bones.Add(new Bone(reader, header));
                    BondiGeek.Logging.LogWriter.Instance.WriteToLog(
                        string.Format("Bone ID: {0}, transform: {1}, {2}, {3}, sibling: {4}, child: {5}, parent: {6}",
                            (i + 1), Bones[i].RestTranslate[0], Bones[i].RestTranslate[1], Bones[i].RestTranslate[2],
                            Bones[i].SiblingID, Bones[i].ChildID, Bones[i].ParentID));
                }

                // Read through the mesh names
                for (var i = 0; i < header.NumMeshes; ++i)
                {
                    string MeshName = System.Text.Encoding.Default.GetString(new MeshDefinitions(reader).MeshName);

                    BondiGeek.Logging.LogWriter.Instance.WriteToLog("Mesh Name: " + MeshName);
                }

                // Read through the vertices
                for (var i = 0; i < header.NumVertices; ++i)
                {
                    Vertices.Add(new Vertex(reader));
                    BondiGeek.Logging.LogWriter.Instance.WriteToLog(
                        string.Format("Vertex ID: {0}, transform: {1}, {2}, {3}, normal: {4}, {5}, {6}, UV: {7}, {8}, Weight: {9}, Bone Number: {10}",
                            i + 1, Vertices[i].Pos[0], Vertices[i].Pos[1], Vertices[i].Pos[2], Vertices[i].Norm[0], Vertices[i].Norm[1], Vertices[i].Norm[2],
                            Vertices[i].UV[0], Vertices[i].UV[1], Vertices[i].Weight, Vertices[i].BoneNum));
                }

                // Read through the LODs
                if (header.ModelFlags == (int)Header.NODModelFlags.HasLOD)
                {
                    for (int i = 0; i < header.NumVertices; ++i)
                    {
                        short HasLOD = reader.ReadInt16();
                    }
                }

                for (int i = 0; i < header.NumFaces; i++)
                {
                    Faces.Add(new Face(reader));

                    BondiGeek.Logging.LogWriter.Instance.WriteToLog(
                        string.Format("Face ID: {0}, Indices {1}, {2}, {3}",
                        i + 1, Faces[i].indices[0], Faces[i].indices[1], Faces[i].indices[2]));
                }

                for (int i = 0; i < header.NumGroups; i++)
                {
                    MeshGroups.Add(new MeshGroup(reader));

                    BondiGeek.Logging.LogWriter.Instance.WriteToLog(
                        string.Format("Mesh group: {0}, Material ID: {1}, Num faces: {2}, Num vertices: {3}, Min vertices: {4}, Group flags: {5}, Bone num: {6}, Mesh Num: {7}",
                        (i + 1), MeshGroups[i].MaterialID, MeshGroups[i].NumFaces, MeshGroups[i].NumVertices, MeshGroups[i].MinVertices,
                        MeshGroups[i].GroupFlags, MeshGroups[i].BoneNum, MeshGroups[i].MeshNum));
                }
                    reader.Close();
            }
        }

        public void TestMesh(string filepath)
        {
            GameObject testMesh = new GameObject();
            testMesh.gameObject.AddComponent<MeshFilter>();
            testMesh.gameObject.AddComponent<MeshRenderer>();
            Mesh mesh = testMesh.GetComponent<MeshFilter>().mesh;
            mesh.Clear();

            // Vertices, work
            var vertices = new Vector3[Vertices.Count];
            for (var i = 0; i < Vertices.Count; ++i)
            {
                vertices[i] = Vertices[i].Pos;
            }

            mesh.vertices = vertices.ToArray();

            // Triangles, don't work in current implementatoin
            var triangles = new Vector3[Vertices.Count];
            for (var i = 0; i < Vertices.Count; ++i)
            {
                triangles[i] = Faces[i].IndicesVector3;
            }

            ArrayList arrayList = new ArrayList();

            for (var i = 0; i < Vertices.Count; ++i)
            {
                arrayList.Add(new[] {
                     Faces[i].IndicesVector3[0],
                    (int) Faces[i].IndicesVector3[1],
                    (int) Faces[i].IndicesVector3[2] }
                    );
            }

            // Nothing works right now as intended because of missing data
            mesh.triangles = new int[arrayList.Count].ToArray();

            var uvs = new List<Vector2>();

            for (int i = 0; i < Vertices.Count; i++)
            {
                uvs.Add(Vertices[i].UV);
            }

            mesh.uv = uvs.ToArray();
        }
    }
}