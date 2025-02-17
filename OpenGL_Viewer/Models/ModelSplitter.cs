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
                if (model.Faces.IndexOf(face) == 0)
                {
                    var a = face;
                }
                    
                //Kiểm tra xem face có bị cắt không và chia mặt nếu cần thiết
                var splitFaces = SplitFaceIfNeeded(model, face, xm, ym);

                // Xác định face mới thuộc phân vùng nào và lưu vào các phần tương ứng
                foreach (var splitFace in splitFaces)
                {
                    List<int> newFaceIndices = new List<int>();
                    int region = GetFaceRegion(face, model, xm, ym);

                    foreach (var vertexIndex in splitFace.Vertices)
                    {
                        Vector3 vertex = model.Vertices[vertexIndex];

                        // Chọn part tương ứng dựa vào region
                        var part = region switch
                        {
                            1 => part1,
                            2 => part2,
                            3 => part3,
                            _ => part4
                        };

                        // Thêm vertex nếu chưa có
                        if (!part.Vertices.Contains(vertex))
                        {
                            part.Vertices.Add(vertex);
                            newFaceIndices.Add(part.Vertices.Count);
                        }
                        else
                        {
                            newFaceIndices.Add(part.Vertices.IndexOf(vertex) + 1);
                        }
                    }

                    // Lưu face vào đúng part
                    if (newFaceIndices.Count == 3)
                    {
                        switch (region)
                        {
                            case 1: part1.Faces.Add(new Face() { Vertices = newFaceIndices }); break;
                            case 2: part2.Faces.Add(new Face() { Vertices = newFaceIndices }); break;
                            case 3: part3.Faces.Add(new Face() { Vertices = newFaceIndices }); break;
                            case 4: part4.Faces.Add(new Face() { Vertices = newFaceIndices }); break;
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

        static int GetFaceRegion(Face face, Model3D model, float xm, float ym)
        {
            HashSet<int> regions = new HashSet<int>();

            foreach (var index in face.Vertices)
            {
                Vector3 vertex = model.Vertices[index];
                regions.Add(GetRegion(vertex, xm, ym));
            }

            return regions.Sum() != -3 ? regions.Max(): -1;
        }

        static int GetRegion(Vector3 vertex, float xm, float ym)
        {
            if (vertex.X > xm && vertex.Y > ym) return 1;  // Top-Right
            if (vertex.X < xm && vertex.Y > ym) return 2; // Top-Left
            if (vertex.X > xm && vertex.Y < ym) return 3; // Bottom-Right
            if (vertex.X < xm && vertex.Y < ym) return 4; // Bottom-Left
            return -1;
        }

        static List<int> GetRegionSplit(Vector3 vertex, float xm, float ym)
        {
            List<int> regions = new List<int>();
            if (vertex.X >= xm && vertex.Y >= ym) regions.Add(1);  // Top-Right
            if (vertex.X <= xm && vertex.Y >= ym) regions.Add(2); // Top-Left
            if (vertex.X >= xm && vertex.Y <= ym) regions.Add(3); // Bottom-Right
            if (vertex.X <= xm && vertex.Y <= ym) regions.Add(4); // Bottom-Left
            return regions;
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
                Vector3? intersectionX = GetIntersectionPoint(p0, p1, 1, 0, 0, -xm); // Mặt phẳng x = xm
                if (intersectionX.HasValue)
                {
                    intersectionPoints.Add(intersectionX.Value);
                    isCut = true;
                }

                Vector3? intersectionY = GetIntersectionPoint(p0, p1, 0, 1, 0, -ym); // Mặt phẳng y = ym
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
            return SplitFaceIntoSubfaces(model, face, intersectionPoints);
            //return new List<Face>();
        }

        public static Vector3? GetIntersectionPoint(Vector3 p0, Vector3 p1, float a, float b, float c, float d)
        {
            Vector3 dir = p1 - p0;

            float denominator = a * dir.X + b * dir.Y + c * dir.Z;

            // Nếu đoạn thẳng song song với mặt phẳng, kiểm tra nếu nó nằm hoàn toàn trên mặt phẳng
            if (Math.Abs(denominator) < 1e-6)
            {
                float f_p0 = a * p0.X + b * p0.Y + c * p0.Z + d;
                float f_p1 = a * p1.X + b * p1.Y + c * p1.Z + d;

                if (Math.Abs(f_p0) < 1e-6 && Math.Abs(f_p1) < 1e-6)
                {
                    // Đoạn thẳng nằm trên mặt phẳng (cần xử lý riêng nếu muốn)
                    return null;
                }

                return null; // Song song và không nằm trên mặt phẳng => Không giao
            }

            float t = -(a * p0.X + b * p0.Y + c * p0.Z + d) / denominator;

            // Chỉ lấy điểm nếu nằm trong đoạn [0,1]
            if (t >= 0 && t <= 1)
            {
                return p0 + t * dir;
            }

            return null; // Giao điểm nằm ngoài đoạn thẳng
        }

        public static List<Face> SplitFaceIntoSubfaces(Model3D model, Face face, List<Vector3> intersectionPoints)
        {
            List<Face> subfaces = new List<Face>();

            // Nếu không có điểm giao cắt, giữ nguyên mặt gốc
            if (intersectionPoints.Count < 2)
            {
                subfaces.Add(face);
                return subfaces;
            }

            // Đỉnh của tam giác gốc
            Vector3 p0 = model.Vertices[face.Vertices[0]];
            Vector3 p1 = model.Vertices[face.Vertices[1]];
            Vector3 p2 = model.Vertices[face.Vertices[2]];

            // Gọi hàm tạo tam giác con từ đỉnh gốc và điểm giao cắt
            subfaces.AddRange(CreateFacesFromVertices(new List<Vector3> { p0, p1, p2 }, intersectionPoints, model));

            return subfaces;
        }

        // Hàm tạo các mặt con từ các đỉnh và điểm giao cắt
        public static List<Face> CreateFacesFromVertices(List<Vector3> originalVertices, List<Vector3> intersectionPoints, Model3D model)
        {
            List<Face> newFaces = new List<Face>();
            float xm = (model.Vertices.Min(v => v.X) + model.Vertices.Max(v => v.X)) / 2;
            float ym = (model.Vertices.Min(v => v.Y) + model.Vertices.Max(v => v.Y)) / 2;

            // Lưu danh sách tất cả các đỉnh (gồm đỉnh gốc và điểm giao)
            List<Vector3> allVertices = new List<Vector3>(originalVertices);
            allVertices.AddRange(intersectionPoints);

            //Vector3? intersectionZ = GetIntersectionPoint(p0, p1, 1, 0, 0, -xm); // Mặt phẳng x = xm


            List<Vector3> region1 = new List<Vector3>();  
            List<Vector3> region2 = new List<Vector3>();  
            List<Vector3> region3 = new List<Vector3>(); 
            List<Vector3> region4 = new List<Vector3>();

            foreach (var point in allVertices)
            {
                List<int> region = GetRegionSplit(point, xm, ym);

                if(region.Contains(1))
                {
                    region1.Add(point);
                }
                if (region.Contains(2))
                {
                    region2.Add(point);
                }
                if (region.Contains(3))
                {
                    region3.Add(point);
                }
                if (region.Contains(4))
                {
                    region4.Add(point);
                }
            }

            if(region1.Count > 2)
                newFaces.AddRange(CreateFacesFromRegion(region1, model));
            if (region2.Count > 2)
                newFaces.AddRange(CreateFacesFromRegion(region2, model));
            if (region3.Count > 2)
                newFaces.AddRange(CreateFacesFromRegion(region3, model));
            if (region4.Count > 2)
                newFaces.AddRange(CreateFacesFromRegion(region4, model));

            return newFaces;
        }

        public static List<Face> CreateFacesFromRegion(List<Vector3> regionPoints, Model3D model)
        {
            List<Face> faces = new List<Face>();

            if (regionPoints.Count < 3)
                return faces;

            // Sắp xếp các điểm theo đường bao lồi
            List<Vector3> convexHull = GetConvexHull(regionPoints);

            // Chia đa giác thành tam giác bằng thuật toán Ear Clipping
            List<Face> triangulatedFaces = TriangulatePolygon(convexHull, model);

            faces.AddRange(triangulatedFaces);
            return faces;
        }

        public static List<Vector3> GetConvexHull(List<Vector3> points)
        {
            points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            List<Vector3> lower = new List<Vector3>();
            List<Vector3> upper = new List<Vector3>();

            foreach (var p in points)
            {
                while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector3 p = points[i];
                while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);
            return lower;
        }

        public static List<Face> TriangulatePolygon(List<Vector3> polygon, Model3D model)
        {
            List<Face> faces = new List<Face>();

            if (polygon.Count < 3)
                return faces;

            for (int i = 1; i < polygon.Count - 1; i++)
            {
                faces.Add(new Face()
                {
                    Vertices = new List<int>
            {
                AddVertexToModel(model, polygon[0]),
                AddVertexToModel(model, polygon[i]),
                AddVertexToModel(model, polygon[i + 1])
            }
                });
            }
            return faces;
        }

        private static float Cross(Vector3 O, Vector3 A, Vector3 B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }


        // Hàm để thêm vertex vào model và trả về chỉ số
        public static int AddVertexToModel(Model3D model, Vector3 vertex)
        {
            model.Vertices.Add(vertex);
            return model.Vertices.Count;
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