using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BondiGeek.Logging;
using NODEngine;
using UnityEditor;

public class NODEditor : EditorWindow
{
    private string FilePath = "";

    private readonly string LogFile = DateTime.Now.ToString("yyyy-MM-dd") + "_Log.txt";

    private BinaryReader reader;
    private readonly LogWriter _writer = LogWriter.Instance;

    private List<string> _materialNames;
    private List<string> _meshNames;

    private List<Vector2> _boundsList = new List<Vector2>();

    public int pos { get; set; }

    #region Public ints
    public uint Version
    {
        get;
        private set;
    }

    public uint NumMaterials
    {
        get;
        private set;
    }

    public byte[] MaterialName
    {
        get;
        private set;
    }

    public short NumBones // short
    {
        get;
        private set;
    }

    public short NumMeshes
    {
        get;
        private set;
    }

    public uint NumVertices
    {
        get;
        private set;
    }

    public uint NumFaces
    {
        get;
        private set;
    }

    public short NumGroups
    {
        get;
        private set;
    }

    public int ModelFlags
    {
        get;
        private set;
    }

    public Vector3[] Bounds
    {
        get;
        private set;
    }

    public int curOffset
    {
        get;
        set;
    }
    #endregion

    // Bones
    public Vector3 RestTranslate { get; set; }
    public float[,] RestMatrixInverse { get; set; }
    public short SiblingID { get; set; }
    public short ChildID { get; set; }
    public short ParentID { get; set; }

    [System.Flags]
    public enum NODModelFlags
    {
        HasLOD = 0x1,
        Inline = 0x2,
        Static = 0x4,
        Res1 = 0x8,
        Res2 = 0x10
    }


    [System.Flags]
    public enum NODGroupFlags
    {
        HasLOD = 0x1,
        NoWeights = 0x2,
        NoSkinning = 0x4,
        MultiTexture = 0x8
    }


    #region EditorGUI
    [MenuItem("Redemption Tools/NOD Editor %#q")]
    public static void Init()
    {
        NODEditor window = GetWindow<NODEditor>();
        window.minSize = new Vector2(100, 100);
        window.title = "NOD Model";
        window.Show();
    }

    private void OnGUI()
    {
        DisplayMainArea();
    }

    private void DisplayMainArea()
    {
        FilePath = EditorGUILayout.TextField("File Path:", FilePath, GUILayout.ExpandWidth(true));
        GUILayout.BeginVertical();
        if (GUILayout.Button("Browse NOD model"))
        {
            FilePath = EditorUtility.OpenFilePanel("Load NOD model", "Assets/Resources/3d/models", "nod");
            if (FilePath.Length != 0)
            {
                FilePath = FilePath.Replace(Application.dataPath, "Assets");
            }
        }
        GUILayout.EndVertical();
        GUILayout.Space(10);
        if (GUILayout.Button("Load NOD model"))
        {
            new NODMesh(FilePath);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Open Log"))
        {
            System.Diagnostics.Process.Start("notepad.exe", UnityEngine.Application.dataPath + "/../Logs/" + LogFile);
        }

        if (GUILayout.Button("Clear Log"))
        {
            FileUtil.DeleteFileOrDirectory(UnityEngine.Application.dataPath + "/../Logs/" + LogFile);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("NOD QUICK ACCESS"))
        {
            FileUtil.DeleteFileOrDirectory(UnityEngine.Application.dataPath + "/../Logs/" + LogFile);
            NODMesh _nodMesh = new NODMesh(FilePath);
            System.Diagnostics.Process.Start("notepad.exe", UnityEngine.Application.dataPath + "/../Logs/" + LogFile);
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Create Mesh"))
        {
            NODMesh nodMesh = new NODMesh(FilePath);
            nodMesh.TestMesh(FilePath);
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
    #endregion

    private void LoadNOD(string filename)
    {
        _writer.WriteToLog(string.Format("Model {0} is loaded.", filename));

        reader = new BinaryReader(File.Open(filename, FileMode.Open, FileAccess.Read));

        ReadHeaderVersion();
        ReadMaterialCount();
        ReadMaterialNames();
        ReadBonesCount();
        ReadMeshCount();
        ReadMeshVertices();
        ReadMeshFaces();
        ReadMeshGroups();
        ReadMeshFlag();
        ReadBounds();
        ReadBones();
        ReadMeshNames();
        ReadVertices();
        ReadLOD();
        ReadFaceDefinitions();
        ReadMeshGroupDefinition();

        //reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        Debug.Log(PrintInfo());

        // Closing reader
        reader.Close();
        _writer.WriteToLog(string.Format("Finished closing {0}.", filename));
    }

    // All classes are below
    private void ReadHeaderVersion()
    {
        // Read header
        reader.BaseStream.Seek(0, SeekOrigin.Begin);

        // Read version
        Version = reader.ReadUInt32();
        if (Version != 7)
        {
            _writer.WriteToLog("Invalid model version/file. Loading stopped.");
            return;
        }

        _writer.WriteToLog("Model Version: " + Version);
    }

    private void ReadMaterialCount()
    {
        // Read material number
        reader.BaseStream.Seek(4, SeekOrigin.Begin);
        NumMaterials = reader.ReadUInt32();
        _writer.WriteToLog("Materials count: " + NumMaterials);
    }

    private void ReadMaterialNames()
    {
        // Read material names
        reader.BaseStream.Seek(8, SeekOrigin.Begin);
        for (var i = 0; i < NumMaterials; ++i)
        {
            MaterialName = reader.ReadBytes(32);
            _writer.WriteToLog("Material name: " + Encoding.Default.GetString(MaterialName));
        }

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadBonesCount()
    {
        // Reading number bones
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        NumBones = reader.ReadInt16();
        _writer.WriteToLog("Bone numbers: " + NumBones);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshCount()
    {
        // Reading mesh count
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        NumMeshes = reader.ReadInt16();
        _writer.WriteToLog("Mesh numbers: " + NumMeshes);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshVertices()
    {
        // Reading mesh vertices count
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        NumVertices = reader.ReadUInt32();
        _writer.WriteToLog("Vertices: " + NumVertices);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshFaces()
    {
        // Reading mesh faces
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        NumFaces = reader.ReadUInt32();
        _writer.WriteToLog("Mesh Faces: " + NumFaces);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshGroups()
    {
        // Reading mesh groups
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        NumGroups = reader.ReadInt16();
        _writer.WriteToLog("Number groups: " + NumGroups);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshFlag()
    {
        // Reading mesh flags
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        ModelFlags = reader.ReadInt32();
        _writer.WriteToLog("Model flag: " + ModelFlags);

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadBounds()
    {
        // Reading mesh bounds
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
        Bounds = new[]
        {
            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
            new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle())
        };

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadMeshNames()
    {
        // Reading through mesh names
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        for (var i = 0; i < NumMeshes; ++i)
        {
            byte[] MeshName = reader.ReadBytes(32);
            string result = Encoding.Default.GetString(MeshName);
            _meshNames.Add(Encoding.Default.GetString(MeshName));

            _writer.WriteToLog("Mesh Name: " + result);
        }

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadBones()
    {
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        for (var i = 0; i < NumBones; ++i)
        {
            // Vector3
            RestTranslate = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            
            RestMatrixInverse = new float[3,4];
            for (int j = 0; j < 4; j++) // column 
            {
                for (int k = 0; k < 3; k++) // row
                {
                    RestMatrixInverse[k, j] = reader.ReadSingle();
                }
            }

            SiblingID = reader.ReadInt16();
            ChildID = reader.ReadInt16();
            ParentID = reader.ReadInt16();

            _writer.WriteToLog(string.Format("Bone ID: {0}, transform: {1}, {2}, {3}, sibling: {4}, child: {5}, parent: {6}", i, RestTranslate[0], RestTranslate[1], RestTranslate[2], SiblingID, ChildID, ParentID));
        }

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadLOD()
    {
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        // Reading LOD
        if (ModelFlags == (int)NODModelFlags.HasLOD)
        {
            for (int i = 0; i < NumVertices; ++i)
            {
                short HasLOD = reader.ReadInt16();
                _writer.WriteToLog(string.Format("Vertex ID: {0}, Collapsed LOD data: {1}", i + 1, HasLOD));
            }
        }

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadVertices()
    {
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        // Reading through the vertices
        for (var i = 0; i < NumVertices; ++i)
        {
            Vector3 Pos = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector3 Norm = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            Vector2 UV = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            float Weight = reader.ReadSingle();

            int BoneNum = reader.ReadInt32();

            _writer.WriteToLog(string.Format("Vertex ID: {0}, transform: {1}, {2}, {3}, normal: {4}, {5}, {6}, UV: {7}, {8}, Weight: {9}, Bone Number: {10}", i + 1, Pos[0], Pos[1], Pos[2], Norm[0], Norm[1], Norm[2], UV[0], UV[1], Weight, BoneNum));
        }

        pos = (int)reader.BaseStream.Position;
    }

    private void ReadFaceDefinitions()
    {
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);
         
        // Read Face Definitions
        for (var i = 0; i < NumFaces; ++i)
        {
            Vector3 indices = new Vector3(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
            _writer.WriteToLog(string.Format("Face ID: {0}, Indices {1}, {2}, {3}", i + 1, indices[0], indices[1], indices[2]));
        }

        pos = (int)reader.BaseStream.Position;
    }

    // Read in the groups. The groups define how the raw materials above are renderered.
    private void ReadMeshGroupDefinition()
    {
        reader.BaseStream.Seek(pos, SeekOrigin.Begin);

        for (var i = 0; i < NumGroups; ++i)
        {
            int MaterialID = reader.ReadInt32();
            reader.ReadBytes(12); // padding
            short NumFaces = reader.ReadInt16();
            short NumVertices = reader.ReadInt16();
            short MinVertices = reader.ReadInt16();
            //short Dummy = reader.ReadInt16(); -> Was present in the Milkshape plugin SDK
            ushort GroupFlags = reader.ReadUInt16();
            short BoneNum = reader.ReadInt16(); // Byte
            short MeshNum = reader.ReadInt16(); // Ditto

            _writer.WriteToLog(string.Format("Mesh group: {0}, Material ID: {1}, Num faces: {2}, Num vertices: {3}, Min vertices: {4}, Group flags: {5}, Bone num: {6}, Mesh Num: {7}", (i + 1), MaterialID, NumFaces, NumVertices, MinVertices, GroupFlags, BoneNum, MeshNum));
        }

        pos = (int)reader.BaseStream.Position;
    }

    public string PrintInfo()
    {
        string blob = "===== NOD Header =====\r\n";
        blob += ("NOD version: " + Version + "\r\n");
        blob += ("Material count: " + NumMaterials + "\r\n");
        for (var i = 0; i < NumMaterials; ++i)
            blob += ("Material name: " + _materialNames[i] + "\r\n");
        blob += ("Bones count: " + NumBones + "\r\n");
        blob += ("Meshes count: " + NumMeshes + "\r\n");
        blob += ("Vertices count: " + NumVertices + "\r\n");
        blob += ("Faces count: " + NumFaces + "\r\n");
        blob += ("Mesh group count: " + NumGroups + "\r\n");
        blob += ("Model flags: " + ModelFlags + "\r\n");
        blob += ("Model bounds: " + Bounds[0] + " " + Bounds[1] + "\r\n");
        for (var i = 0; i < NumMeshes; ++i)
            blob += ("Mesh names: " + _meshNames[i] + "\r\n");

        return blob;
    }
}
