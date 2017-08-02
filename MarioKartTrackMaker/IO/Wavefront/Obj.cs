using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ObjParser.Types;

namespace ObjParser
{
	public class Obj
    {
        public List<Vertex> VertexList;
        public List<Normal> NormalList;
        public List<Face> FaceList;
		public List<TextureVertex> TextureList;

		public Extent Size { get; set; }

		public string UseMtl { get; set; }
		public string Mtl { get; set; }

        /// <summary>
        /// Constructor. Initializes VertexList, FaceList and TextureList.
        /// </summary>
	    public Obj()
        {
            VertexList = new List<Vertex>();
            NormalList = new List<Normal>();
            FaceList = new List<Face>();
            TextureList = new List<TextureVertex>();
        }

        /// <summary>
        /// Load .obj from a filepath.
        /// </summary>
        /// <param name="file"></param>
        public void LoadObj(string path)
        {
            LoadObj(File.ReadAllLines(path));
        }

        /// <summary>
        /// Load .obj from a stream.
        /// </summary>
        /// <param name="file"></param>
	    public void LoadObj(Stream data)
	    {
            using (var reader = new StreamReader(data))
            {
                LoadObj(reader.ReadToEnd().Split(Environment.NewLine.ToCharArray()));
            }
	    }

        /// <summary>
        /// Load .obj from a list of strings.
        /// </summary>
        /// <param name="data"></param>
	    public void LoadObj(IEnumerable<string> data)
	    {
            foreach (var line in data)
            {
                processLine(line);
            }

            updateSize();
        }

		public void WriteObjFile(string path, string[] headerStrings)
		{
			using (var outStream = File.OpenWrite(path))
			using (var writer = new StreamWriter(outStream))
			{
				// Write some header data
			    WriteHeader(writer, headerStrings);

				if (!string.IsNullOrEmpty(Mtl))
				{
					writer.WriteLine("mtllib " + Mtl);
				}

				VertexList.ForEach(v => writer.WriteLine(v));
				TextureList.ForEach(tv => writer.WriteLine(tv));
				string lastUseMtl = "";
				foreach (Face face in FaceList) {
					if (face.UseMtl != null && !face.UseMtl.Equals(lastUseMtl)) {
						writer.WriteLine("usemtl " + face.UseMtl);
						lastUseMtl = face.UseMtl;
					}
					writer.WriteLine(face);
				}
			}
		}

	    private void WriteHeader(StreamWriter writer, string[] headerStrings)
	    {
	        if (headerStrings == null || headerStrings.Length == 0)
	        {
	            writer.WriteLine("# Generated by ObjParser");
	            return;
	        }

	        foreach (var line in headerStrings)
	        {
	            writer.WriteLine("# " + line);
	        }
	    }

	    /// <summary>
		/// Sets our global object size with an extent object
		/// </summary>
		private void updateSize()
		{
            // If there are no vertices then size should be 0.
	        if (VertexList.Count == 0)
	        {
	            Size = new Extent
	            {
                    XMax = 0,
                    XMin = 0,
                    YMax = 0,
                    YMin = 0,
                    ZMax = 0,
                    ZMin = 0
	            };

	            // Avoid an exception below if VertexList was empty.
	            return;
	        }

			Size = new Extent
			{
				XMax = VertexList.Max(v => v.X),
				XMin = VertexList.Min(v => v.X),
				YMax = VertexList.Max(v => v.Y),
				YMin = VertexList.Min(v => v.Y),
				ZMax = VertexList.Max(v => v.Z),
				ZMin = VertexList.Min(v => v.Z)
			};		
		}

		/// <summary>
		/// Parses and loads a line from an OBJ file.
		/// Currently only supports V, VT, F and MTLLIB prefixes
		/// </summary>		
		private void processLine(string line)
		{
			string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			if (parts.Length > 0)
			{
				switch (parts[0])
				{
					case "usemtl":
						UseMtl = parts[1];
						break;
					case "mtllib":
						Mtl = parts[1];
						break;
                    case "v":
                        Vertex v = new Vertex();
                        v.LoadFromStringArray(parts);
                        VertexList.Add(v);
                        v.Index = VertexList.Count();
                        break;
                    case "vn":
                        Normal vn = new Normal();
                        vn.LoadFromStringArray(parts);
                        NormalList.Add(vn);
                        vn.Index = NormalList.Count();
                        break;
                    case "f":
						Face f = new Face();
						f.LoadFromStringArray(parts);
						f.UseMtl = UseMtl;
						FaceList.Add(f);
						break;
					case "vt":
						TextureVertex vt = new TextureVertex();
						vt.LoadFromStringArray(parts);
						TextureList.Add(vt);
						vt.Index = TextureList.Count();
						break;

				}
			}
		}

	}
}
