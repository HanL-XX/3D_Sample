using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;

namespace OpenGL_Viewer.Models
{
    public static class ObjLoader
    {
        public static Model3D LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File không tồn tại!", filePath);

            Model3D model = new Model3D();
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                if (parts[0] == "v")
                {
                    if (float.TryParse(parts[1], out float x) &&
                        float.TryParse(parts[2], out float y) &&
                        float.TryParse(parts[3], out float z))
                    {
                        model.Vertices.Add(new Vector3(x, y, z));
                    }
                }
                else if (parts[0] == "f")
                {
                    Face face = new Face();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string[] indices = parts[i].Split('/');
                        if (int.TryParse(indices[0], out int vertexIndex))
                        {
                            face.Vertices.Add(vertexIndex - 1);
                        }
                    }
                    model.Faces.Add(face);
                }
            }

            return model;
        }
    }
}
