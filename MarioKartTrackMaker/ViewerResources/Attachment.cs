﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace MarioKartTrackMaker.ViewerResources
{
    public class Attachment
    {
        public string name = "<unknown>";
        public bool isFirst;
        public bool isFemale;
        public Matrix4 transform;
        public Matrix4 get_world_transform(Matrix4 parentMtx)
        {
            return transform * parentMtx;
        }

        public void draw(float size)
        {
            GL.PushMatrix();
            GL.MultMatrix(ref transform);
            GL.LineWidth(4F);
            if (isFemale)
                GL.Color3(0.875F, 0, 1F);
            else
                GL.Color3(0, 0.875F, 1F);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            if (isFemale)
                GL.Vertex3(0, -size, 0);
            else
                GL.Vertex3(0, size, 0);
            GL.End();
            GL.LineWidth(1F);
            GL.PopMatrix();
        }
        public override string ToString()
        {
            return name;
        }
    }
}