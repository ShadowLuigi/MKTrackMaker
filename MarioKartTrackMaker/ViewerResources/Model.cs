﻿using System;
using System.Collections.Generic;
using System.IO;
using MarioKartTrackMaker.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ObjParser;

namespace MarioKartTrackMaker.ViewerResources
{
    /// <summary>
    /// A display model. Contains multiple meshes.
    /// </summary>
    public class Model
    {
        /// <summary>
        /// This is the database that holds all the models in the program's memory.
        /// </summary>
        public static List<Model> database = new List<Model>();

        /// <summary>
        /// This indicates whether this program is running on a Macintosh or not.
        /// </summary>
        private static bool runningOnMac { get {
                int plat = (int)Environment.OSVersion.Platform;
                return ((plat == 4) || (plat == 128));
            } }
        /// <summary>
        /// This is dynamic due to the fact that a Windows computer uses \\ when a Macintosh uses //.
        /// </summary>
        private static string filepathSlash { get {
                if (runningOnMac) return "//";
                return "\\";
            } }
        

        /// <summary>
        /// Checks if the model is already loaded. If so, it'll return the database's index of the model, otherwise -1;
        /// </summary>
        /// <param name="path">The path of the model file to check.</param>
        /// <returns></returns>
        public static int IsLoaded(string path)
        {
            for (int i = 0; i < database.Count; i++)
                if (database[i].path == path)
                    return i;
            return -1;
        }
        /// <summary>
        /// Loads and adds a new model from a file into the database.
        /// </summary>
        /// <param name="path">The path of the model file to load.</param>
        /// <returns></returns>
        public static int AddModel(string path)
        {
            int id = IsLoaded(path);
            if(id == -1)
            {
                Model m = new Model(path);
                database.Add(m);
                id = database.IndexOf(m);
            }
            return id;
        }
        /// <summary>
        /// The path that this model was loaded from.
        /// </summary>
        public string path;
        /// <summary>
        /// The name of this model.
        /// </summary>
        public string name;
        /// <summary>
        /// The bounding box size of this model.
        /// </summary>
        public Bounds size = new Bounds();
        /// <summary>
        /// All the meshes contained inside this model.
        /// </summary>
        public List<Mesh> meshes = new List<Mesh>();
        /// <summary>
        /// All the collision meshes contained inside this model.
        /// </summary>
        public List<Collision_Mesh> KCLs = new List<Collision_Mesh>();
        /// <summary>
        /// All the attachments (connection points) contained inside this model.
        /// </summary>
        public List<Attachment> attachments = new List<Attachment>();
        /// <summary>
        /// Defines whether the color picker will affect this model or not.
        /// </summary>
        public bool useColor
        {
            get
            {
                foreach (Mesh m in meshes)
                    if (m.texture != -1)
                        return false;
                return true;
            }
        }
        /// <summary>
        /// Constructor. Loads the model from file.
        /// </summary>
        /// <param name="filepath">The path of the file.</param>
        public Model(string filepath)
        {
            path = filepath;
            name = Path.GetFileNameWithoutExtension(filepath).Replace('_', ' ');
            Obj tobj = new Obj();
            tobj.LoadObj(filepath);
            Mtl tmtl = new Mtl();
            tmtl.LoadMtl(Path.GetDirectoryName(filepath) + filepathSlash + tobj.Mtl);

            List<Vector3> Vertices = new List<Vector3>();
            foreach (ObjParser.Types.Vertex v in tobj.VertexList)
            {
                Vertices.Add(new Vector3((float)v.X, (float)v.Y, (float)v.Z));
            }
            List<Vector3> Normals = new List<Vector3>();
            foreach (ObjParser.Types.Normal vn in tobj.NormalList)
            {
                Normals.Add(new Vector3((float)vn.X, (float)vn.Y, (float)vn.Z));
            }
            List<Vector2> UVs = new List<Vector2>();
            foreach (ObjParser.Types.TextureVertex vt in tobj.TextureList)
            {
                UVs.Add(new Vector2((float)vt.X, (float)vt.Y));
            }
            foreach (ObjParser.Types.Material mat in tmtl.MaterialList)
            {
                List<int[]> faces = new List<int[]>();
                List<int[]> fnmls = new List<int[]>();
                List<int[]> fuvs = new List<int[]>();
                int texture;
                if (Path.IsPathRooted(mat.DiffuseTexture))
                {
                    texture = ContentPipe.Load_and_AddTexture(mat.DiffuseTexture);
                }
                else {
                    texture = ContentPipe.Load_and_AddTexture(Path.GetDirectoryName(filepath) + filepathSlash + mat.DiffuseTexture);
                }

                foreach (ObjParser.Types.Face f in tobj.FaceList)
                {
                    if (f.UseMtl == mat.Name)
                    {
                        faces.Add(f.VertexIndexList);
                        fnmls.Add(f.NormalIndexList);
                        fuvs.Add(f.TextureVertexIndexList);
                    }
                }
                List<Vector3> cverts = new List<Vector3>();
                List<Vector3> cnorms = new List<Vector3>();
                List<Vector2> cuvs = new List<Vector2>();
                foreach (Vector3 v in Vertices)
                    cverts.Add(v);
                foreach (Vector3 n in Normals)
                    cnorms.Add(n);
                foreach (Vector2 t in UVs)
                    cuvs.Add(t);
                meshes.Add(new Mesh(cverts, cnorms, cuvs, faces, fnmls, fuvs, texture));

            }
            string KCL_filepath = Path.GetDirectoryName(filepath) + filepathSlash + Path.GetFileNameWithoutExtension(filepath) + "_KCL.obj";
            if (File.Exists(KCL_filepath))
            {
                Obj tobjkcl = new Obj();
                tobjkcl.LoadObj(KCL_filepath);

                List<Vector3> CVerts = new List<Vector3>();
                foreach (ObjParser.Types.Vertex v in tobjkcl.VertexList)
                {
                    CVerts.Add(new Vector3((float)v.X, (float)v.Y, (float)v.Z));
                }
                foreach (string name in tobjkcl.objects)
                {
                    Collision_Mesh.CollisionType coll;
                    if (name == "ROAD")
                        coll = Collision_Mesh.CollisionType.road;
                    else if (name == "WALL")
                        coll = Collision_Mesh.CollisionType.wall;
                    else if (name == "OFFROAD")
                        coll = Collision_Mesh.CollisionType.off_road;
                    else if (name == "WAYOFFROAD")
                        coll = Collision_Mesh.CollisionType.way_off_road;
                    else if (name == "OUTOFBOUNDS")
                        coll = Collision_Mesh.CollisionType.out_of_bounds;
                    else if (name == "BOOST")
                        coll = Collision_Mesh.CollisionType.boost;
                    else if (name == "RAMP")
                        coll = Collision_Mesh.CollisionType.ramp;
                    else if (name == "ENGAGEGLIDER")
                        coll = Collision_Mesh.CollisionType.engage_glider;
                    else if (name == "SIDERAMP")
                        coll = Collision_Mesh.CollisionType.side_ramp;
                    else if (name == "CANNON")
                        coll = Collision_Mesh.CollisionType.cannon;
                    else if (name == "WATER")
                        coll = Collision_Mesh.CollisionType.water;
                    else if (name == "LAVA")
                        coll = Collision_Mesh.CollisionType.lava;
                    else if (name == "SPINOUT")
                        coll = Collision_Mesh.CollisionType.spin_out;
                    else if (name == "KNOCKOUT")
                        coll = Collision_Mesh.CollisionType.knock_out;
                    else continue;

                    List<int[]> faces = new List<int[]>();
                    foreach (ObjParser.Types.Face f in tobjkcl.FaceList)
                    {
                        if (f.objectName == name)
                        {
                            faces.Add(f.VertexIndexList);
                        }
                    }
                    List<Vector3> cverts = new List<Vector3>();
                    foreach (Vector3 v in CVerts)
                        cverts.Add(v);
                    KCLs.Add(new Collision_Mesh(cverts, faces, coll));
                }
            }
            CalculateBounds();
            ImportAttachments(Path.GetDirectoryName(filepath) + filepathSlash + Path.GetFileNameWithoutExtension(filepath) + "_Atch.txt");
        }

        /// <summary>
        /// Imports attachments from a text file.
        /// </summary>
        /// <param name="path">The path of the text file.</param>
        private void ImportAttachments(string path)
        {
            if (File.Exists(path))
            {
                Attachment atch = new Attachment();
                foreach (string line in File.ReadAllLines(path))
                {
                    string[] parts = line.Split(new string[] { ": ", ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0].ToUpper() == "NAME")
                    {
                        atch.name = parts[1];
                    }
                    if (parts[0].ToUpper() == "ISFIRST")
                    {
                        atch.isFirst = int.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "ISFEMALE")
                    {
                        atch.isFemale = parts[1] == "1";
                    }
                    if (parts[0].ToUpper() == "MAT00")
                    {
                        atch.transform[0, 0] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT10")
                    {
                        atch.transform[0, 1] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT20")
                    {
                        atch.transform[0, 2] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT30")
                    {
                        atch.transform[0, 3] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT01")
                    {
                        atch.transform[1, 0] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT11")
                    {
                        atch.transform[1, 1] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT21")
                    {
                        atch.transform[1, 2] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT31")
                    {
                        atch.transform[1, 3] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT02")
                    {
                        atch.transform[2, 0] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT12")
                    {
                        atch.transform[2, 1] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT22")
                    {
                        atch.transform[2, 2] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT32")
                    {
                        atch.transform[2, 3] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT03")
                    {
                        atch.transform[3, 0] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT13")
                    {
                        atch.transform[3, 1] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT23")
                    {
                        atch.transform[3, 2] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "MAT33")
                    {
                        atch.transform[3, 3] = float.Parse(parts[1]);
                    }
                    if (parts[0].ToUpper() == "END")
                    {
                        //atch.transform *= Matrix4.CreateTranslation(100,100,100);
                        attachments.Add(atch);
                        atch = new Attachment();
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the boundaries of this model.
        /// </summary>
        private void CalculateBounds()
        {
            size.minX = float.PositiveInfinity;
            size.minY = float.PositiveInfinity;
            size.minZ = float.PositiveInfinity;
            size.maxX = float.NegativeInfinity;
            size.maxY = float.NegativeInfinity;
            size.maxZ = float.NegativeInfinity;

            foreach (Mesh m in meshes)
            {
                size.minX = Math.Min(size.minX, m.size.minX);
                size.maxX = Math.Max(size.maxX, m.size.maxX);
                size.minY = Math.Min(size.minY, m.size.minY);
                size.maxY = Math.Max(size.maxY, m.size.maxY);
                size.minZ = Math.Min(size.minZ, m.size.minZ);
                size.maxZ = Math.Max(size.maxZ, m.size.maxZ);
            }
        }

        /// <summary>
        /// Renders the model.
        /// </summary>
        /// <param name="program">The id of the shader program.</param>
        /// <param name="mtx">The transform matrix. Where do you want the model rendered?</param>
        /// <param name="mode">The collision mode. 1 displays only the model, 2 displays only the model's collisions, and 3 displays both.</param>
        /// <param name="wireframe">Render this as wireframe?</param>
        /// <param name="selected">Make the model look highlighted/selected?</param>
        /// <param name="Color">The diffuse color.</param>
        public void DrawModel(int program, Matrix4 mtx, int mode, bool wireframe, bool selected, Vector3 Color)
        {
            if ((mode & 1) == 1)
            {
                Matrix4 matnoscale = mtx.ClearScale();
                Vector3 matscale = mtx.ExtractScale();
                int sclloc = GL.GetUniformLocation(program, "scale");
                GL.MultMatrix(ref matnoscale);
                GL.ProgramUniform3(program, sclloc, ref matscale);
                foreach (Mesh mesh in meshes)
                {
                    mesh.DrawMesh(program, wireframe, selected, Color);
                }
            }
            if ((mode & 2) == 2)
            {
                if ((mode & 1) == 1)
                {
                    GL.PopMatrix();
                    GL.PushMatrix();
                }
                GL.MultMatrix(ref mtx);
                foreach (Collision_Mesh mesh in KCLs)
                {
                    mesh.DrawMesh(wireframe);
                }
            }
        }
    }
}
