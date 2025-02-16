using System;
using System.Collections.Generic;
using System.IO;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace OpenGL_Viewer.Models
{
    public class ModelSplitter
    {
        public static void SplitModel(Model3D model, string saveDirectory)
        {
            float xm = (model.Vertices.Min(v => v.X) + model.Vertices.Max(v => v.X)) / 2;
            float ym = (model.Vertices.Min(v => v.Y) + model.Vertices.Max(v => v.Y)) / 2;

            // Mô hình con cho mỗi phần
            Model3D part1 = new Model3D();
            Model3D part2 = new Model3D();
            Model3D part3 = new Model3D();
            Model3D part4 = new Model3D();

            // Tạo danh sách chứa các đỉnh mới của từng phần
            List<Vector3> newVertices1 = new List<Vector3>();
            List<Vector3> newVertices2 = new List<Vector3>();
            List<Vector3> newVertices3 = new List<Vector3>();
            List<Vector3> newVertices4 = new List<Vector3>();

            // Duyệt qua các face
            foreach (var face in model.Faces)
            {
                List<int> newFace1 = new List<int>();
                List<int> newFace2 = new List<int>();
                List<int> newFace3 = new List<int>();
                List<int> newFace4 = new List<int>();

                //Kiểm tra xem face có bị cắt không và chia mặt nếu cần thiết
                var splitFaces = SplitFaceIfNeeded(model, face, xm, ym);

                // Xác định face mới thuộc phân vùng nào và lưu vào các phần tương ứng
                foreach (var splitFace in splitFaces)
                {
                    List<int> newFaceIndices = new List<int>();
                    if (part1.Faces.Count == 486) {
                        var a = 1;
                    }

                    foreach (var vertexIndex in splitFace.Vertices)
                    {
                        Vector3 vertex = model.Vertices[vertexIndex];

                        // Cập nhật các phân vùng
                        if (vertex.X < xm && vertex.Y < ym && !part1.Vertices.Contains(vertex))
                        {
                            part1.Vertices.Add(vertex);
                            newFaceIndices.Add(part1.Vertices.Count);
                        }
                        else if(vertex.X < xm && vertex.Y < ym)
                        {
                            newFaceIndices.Add(part1.Vertices.IndexOf(vertex) + 1);
                        }

                        if (vertex.X > xm && vertex.Y < ym && !part2.Vertices.Contains(vertex))
                        {
                            part2.Vertices.Add(vertex);
                            newFaceIndices.Add(part2.Vertices.Count);
                        }
                        else if (vertex.X > xm && vertex.Y < ym)
                        {
                            newFaceIndices.Add(part2.Vertices.IndexOf(vertex) + 1);
                        }

                        if (vertex.X < xm && vertex.Y > ym && !part3.Vertices.Contains(vertex))
                        {
                            part3.Vertices.Add(vertex);
                            newFaceIndices.Add(part3.Vertices.Count);
                        }
                        else if (vertex.X < xm && vertex.Y > ym)
                        {
                            newFaceIndices.Add(part3.Vertices.IndexOf(vertex) + 1);
                        }

                        if (vertex.X > xm && vertex.Y > ym && !part4.Vertices.Contains(vertex))
                        {
                            part4.Vertices.Add(vertex);
                            newFaceIndices.Add(part4.Vertices.Count);
                        }
                        else if (vertex.X > xm && vertex.Y > ym)
                        {
                            newFaceIndices.Add(part4.Vertices.IndexOf(vertex) + 1);
                        }

                        // Lưu face vào các phần con nếu có
                        if (newFaceIndices.Count == 3)
                        {
                            if (vertex.X < xm && vertex.Y < ym)
                                part1.Faces.Add(new Face() { Vertices = newFaceIndices });
                            if (vertex.X > xm && vertex.Y < ym)
                                part2.Faces.Add(new Face() { Vertices = newFaceIndices });
                            if (vertex.X < xm && vertex.Y > ym)
                                part3.Faces.Add(new Face() { Vertices = newFaceIndices });
                            if (vertex.X > xm && vertex.Y > ym)
                                part4.Faces.Add(new Face() { Vertices = newFaceIndices });
                        }
                    }
                }
            }

            // Lưu các mô hình con thành các file .obj mới
            Model3D[] subModels = { part1, part2, part3, part4 };
            for (int i = 0; i < 4; i++)
            {
                string filePath = Path.Combine(saveDirectory, $"split_{i}.obj");
                ObjWriter.WriteObj(subModels[i], filePath);
            }
        }

        public static List<Face> SplitFaceIfNeeded(Model3D model, Face face, float xm, float ym)
        {
            // Kiểm tra xem tam giác có bị cắt không
            bool isCut = false;
            List<Vector3> intersectionPoints = new List<Vector3>();

            // Kiểm tra các cạnh của mặt tam giác
            for (int i = 0; i < face.Vertices.Count; i++)
            {
                Vector3 p0 = model.Vertices[face.Vertices[i]];
                Vector3 p1 = model.Vertices[face.Vertices[(i + 1) % face.Vertices.Count]];

                // Kiểm tra giao cắt với mặt phẳng theo trục X (xm) hoặc trục Y (ym)
                Vector3? intersectionX = GetIntersectionPoint(p0, p1, 1, 0, 0, xm); // Mặt phẳng x = xm
                if (intersectionX.HasValue)
                {
                    intersectionPoints.Add(intersectionX.Value);
                    isCut = true;
                }

                Vector3? intersectionY = GetIntersectionPoint(p0, p1, 0, 1, 0, ym); // Mặt phẳng y = ym
                if (intersectionY.HasValue)
                {
                    intersectionPoints.Add(intersectionY.Value);
                    isCut = true;
                }
            }

            // Nếu không bị cắt, trả về mặt nguyên bản
            if (!isCut)
            {
                return new List<Face> { face };
            }
            // Nếu có cắt, chia tam giác thành các phần nhỏ hơn
            //return SplitFaceIntoSubfaces(model, face, intersectionPoints);
            return new List<Face>();
        }

        public static Vector3? GetIntersectionPoint(Vector3 p0, Vector3 p1, float a, float b, float c, float d)
        {
            // Vector hướng của đoạn thẳng
            Vector3 dir = p1 - p0;

            // Phương trình mặt phẳng: Ax + By + Cz + D = 0
            // Thay vào phương trình: A(x0 + t * (x1 - x0)) + B(y0 + t * (y1 - y0)) + C(z0 + t * (z1 - z0)) + D = 0

            float denominator = a * dir.X + b * dir.Y + c * dir.Z;

            // Nếu mẫu số = 0, đoạn thẳng song song với mặt phẳng, không có giao cắt
            if (Math.Abs(denominator) < 1e-6)
                return null;

            float t = -(a * p0.X + b * p0.Y + c * p0.Z + d) / denominator;

            // Nếu t nằm trong khoảng 0 <= t <= 1, thì đoạn thẳng giao với mặt phẳng trong đoạn
            if (t >= 0 && t <= 1)
            {
                // Tính toán điểm giao
                Vector3 intersectionPoint = p0 + t * dir;
                return intersectionPoint;
            }

            // Nếu không giao trong đoạn thẳng, trả về null
            return null;
        }

        public static List<Face> SplitFaceIntoSubfaces(Model3D model, Face face, List<Vector3> intersectionPoints)
        {
            List<Face> subfaces = new List<Face>();

            // Nếu không có điểm giao cắt, trả lại mặt gốc
            if (intersectionPoints.Count < 2)
            {
                subfaces.Add(face);
                return subfaces;
            }

            // Các điểm giao cắt sẽ tạo thành các mặt phẳng phân chia mới
            List<Vector3> verticesForNewFaces = new List<Vector3>();

            // Thêm các điểm giao cắt vào danh sách các đỉnh mới
            foreach (var point in intersectionPoints)
            {
                verticesForNewFaces.Add(point);
            }

            // Đỉnh của tam giác gốc
            Vector3 p0 = model.Vertices[face.Vertices[0]];
            Vector3 p1 = model.Vertices[face.Vertices[1]];
            Vector3 p2 = model.Vertices[face.Vertices[2]];

            // Tạo các mặt phẳng phân chia từ các điểm giao cắt và các đỉnh gốc
            List<Vector3> allVertices = new List<Vector3> { p0, p1, p2 };
            allVertices.AddRange(verticesForNewFaces);

            // Phân chia mặt phẳng thành các tam giác con
            subfaces.AddRange(CreateFacesFromVertices(allVertices, face, model));

            return subfaces;
        }

        // Hàm tạo các mặt con từ các đỉnh và điểm giao cắt
        public static List<Face> CreateFacesFromVertices(List<Vector3> vertices, Face originalFace, Model3D model)
        {
            List<Face> newFaces = new List<Face>();

            // Tạo các mặt con từ các điểm giao cắt và đỉnh gốc
            for (int i = 0; i < vertices.Count - 2; i++)
            {
                Face subface = new Face();

                subface.Vertices.Add(AddVertexToModel(model, vertices[i]));
                subface.Vertices.Add(AddVertexToModel(model, vertices[i + 1]));
                subface.Vertices.Add(AddVertexToModel(model, vertices[i + 2]));

                newFaces.Add(subface);
            }

            return newFaces;
        }

        // Hàm để thêm vertex vào model và trả về chỉ số
        public static int AddVertexToModel(Model3D model, Vector3 vertex)
        {
            model.Vertices.Add(vertex);
            return model.Vertices.Count - 1;
        }

        private static void AddFaceToSubModel(Face face, Model3D originalModel, Model3D subModel, Dictionary<Vector3, int> vertexMap)
        {
            Face newFace = new Face();
            foreach (var vertexIndex in face.Vertices)
            {
                Vector3 vertex = originalModel.Vertices[vertexIndex];

                if (!vertexMap.ContainsKey(vertex))
                {
                    vertexMap[vertex] = subModel.Vertices.Count;
                    subModel.Vertices.Add(vertex);
                }

                newFace.Vertices.Add(vertexMap[vertex]);
            }
            subModel.Faces.Add(newFace);
        }

        private static List<Face> SplitFace(List<Vector3> faceVertices, Vector3 center, List<Model3D> subModels, List<Dictionary<Vector3, int>> vertexMaps)
        {
            List<Face> newFaces = new List<Face>();
            Dictionary<Vector3, int> newVertexMap = new Dictionary<Vector3, int>();

            for (int i = 0; i < faceVertices.Count; i++)
            {
                Vector3 v1 = faceVertices[i];
                Vector3 v2 = faceVertices[(i + 1) % faceVertices.Count];

                if ((v1.X > center.X) != (v2.X > center.X) || (v1.Y > center.Y) != (v2.Y > center.Y))
                {
                    Vector3 midPoint = (v1 + v2) / 2;
                    if (!newVertexMap.ContainsKey(midPoint))
                    {
                        newVertexMap[midPoint] = subModels[0].Vertices.Count;
                        subModels[0].Vertices.Add(midPoint);
                    }
                }
            }

            foreach (var kvp in newVertexMap)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!vertexMaps[i].ContainsKey(kvp.Key))
                    {
                        vertexMaps[i][kvp.Key] = subModels[i].Vertices.Count;
                        subModels[i].Vertices.Add(kvp.Key);
                    }
                }
            }

            for (int i = 0; i < faceVertices.Count - 2; i++)
            {
                Face newFace = new Face { Vertices = new List<int> { i, i + 1, i + 2 } };
                newFaces.Add(newFace);
            }

            return newFaces;
        }
    }
}