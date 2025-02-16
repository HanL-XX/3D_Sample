using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenTK.Mathematics;

namespace OpenGL_Viewer.Models
{
    public class ObjWriter
    {
        public static void WriteObj(Model3D model, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                // Ghi các vertex vào file .obj
                foreach (var vertex in model.Vertices)
                {
                    writer.WriteLine($"v {vertex.X} {vertex.Y} {vertex.Z}");
                }

                // Ghi các face vào file .obj
                foreach (var face in model.Faces)
                {
                    // Các chỉ số trong .obj bắt đầu từ 1, vì vậy cộng thêm 1 vào chỉ số vertex
                    writer.WriteLine($"f {face.Vertices[0]} {face.Vertices[1]} {face.Vertices[2]}");
                }
            }
        }
    }
}
