using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace OpenGL_Viewer.Models
{
    public class Model3D
    {
        public List<Vector3> Vertices { get; set; } = new List<Vector3>();
        public List<Vector3> Normals { get; set; } = new List<Vector3>();
        public List<Face> Faces { get; set; } = new List<Face>();

        public Model3D() { }
    }

    public class Face
    {
        public List<int> Vertices { get; set; } = new List<int>();
        public Face() { }
    }
}
