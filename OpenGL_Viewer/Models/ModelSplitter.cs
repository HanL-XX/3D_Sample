using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                    int region = GetFaceRegion(splitFace, model, xm, ym);
  
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
            // Tính trọng tâm của face
            Vector3 centroid = GetCentroid(face, model);

            // Xác định vùng của trọng tâm
            return GetRegion(centroid, xm, ym);
        }

        // Hàm tính trọng tâm của Face
        static Vector3 GetCentroid(Face face, Model3D model)
        {
            float sumX = 0, sumY = 0, sumZ = 0;
            int count = face.Vertices.Count;

            foreach (var index in face.Vertices)
            {
                Vector3 vertex = model.Vertices[index];
                sumX += vertex.X;
                sumY += vertex.Y;
                sumZ += vertex.Z;
            }

            return new Vector3(sumX / count, sumY / count, sumZ / count);
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
            return SplitFaceIntoSubfaces(model, face, intersectionPoints.Distinct().ToList());
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
                    // Đoạn thẳng nằm trên mặt phẳng
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

            // Nếu có đúng 4 điểm giao
            if (intersectionPoints.Count == 4)
            {
                // Chia thành hai nhóm: Hai điểm có cùng X và hai điểm có cùng Y
                List<Vector3> xAligned = intersectionPoints.Where(p => Math.Abs(p.X - xm) < 1e-6).ToList();
                List<Vector3> yAligned = intersectionPoints.Where(p => Math.Abs(p.Y - ym) < 1e-6).ToList();

                if (xAligned.Count > 2)
                {
                    xAligned = intersectionPoints.Where(p => Math.Abs(p.X - xm) < 1e-6 && Math.Abs(p.Y - ym) >= 1e-6).ToList();
                }
                if (yAligned.Count > 2)
                {
                    yAligned = intersectionPoints.Where(p => Math.Abs(p.Y - ym) < 1e-6 && Math.Abs(p.X - xm) >= 1e-6).ToList();
                }

                if (xAligned.Count == 2 && yAligned.Count == 2)
                { 
                    // Tìm giao điểm của hai đường thẳng
                    Vector3? intersection = GetSegmentIntersection(xAligned[0], xAligned[1], yAligned[0], yAligned[1]);

                    if (intersection.HasValue)
                    {
                        allVertices.Add((Vector3)intersection);
                    }
                }
            }

            allVertices = allVertices.Distinct().ToList();

            List<Vector3> region1 = new List<Vector3>();
            List<Vector3> region2 = new List<Vector3>();
            List<Vector3> region3 = new List<Vector3>();
            List<Vector3> region4 = new List<Vector3>();

            foreach (var point in allVertices)
            {
                List<int> region = GetRegionSplit(point, xm, ym);

                if (region.Contains(1)) region1.Add(point);
                if (region.Contains(2)) region2.Add(point);
                if (region.Contains(3)) region3.Add(point);
                if (region.Contains(4)) region4.Add(point);
            }

            if (region1.Count > 2) newFaces.AddRange(CreateFacesFromRegion(region1, model));
            if (region2.Count > 2) newFaces.AddRange(CreateFacesFromRegion(region2, model));
            if (region3.Count > 2) newFaces.AddRange(CreateFacesFromRegion(region3, model));
            if (region4.Count > 2) newFaces.AddRange(CreateFacesFromRegion(region4, model));

            return newFaces;
        }

        public static Vector3? GetSegmentIntersection(Vector3 p1, Vector3 p2, Vector3 q1, Vector3 q2)
        {
            float A1 = p2.Y - p1.Y;
            float B1 = p1.X - p2.X;
            float C1 = A1 * p1.X + B1 * p1.Y;

            float A2 = q2.Y - q1.Y;
            float B2 = q1.X - q2.X;
            float C2 = A2 * q1.X + B2 * q1.Y;

            float det = A1 * B2 - A2 * B1;

            if (Math.Abs(det) < 1e-6)
            {
                return null; // Hai đoạn thẳng song song hoặc trùng nhau
            }

            float x = (B2 * C1 - B1 * C2) / det;
            float y = (A1 * C2 - A2 * C1) / det;

            float minXP = Math.Min(p1.X, p2.X), maxXP = Math.Max(p1.X, p2.X);
            float minYP = Math.Min(p1.Y, p2.Y), maxYP = Math.Max(p1.Y, p2.Y);
            float minXQ = Math.Min(q1.X, q2.X), maxXQ = Math.Max(q1.X, q2.X);
            float minYQ = Math.Min(q1.Y, q2.Y), maxYQ = Math.Max(q1.Y, q2.Y);

            bool withinSegmentP = (Math.Abs(p2.X - p1.X) > 1e-6) ? (x >= minXP && x <= maxXP) : (y >= minYP && y <= maxYP);
            bool withinSegmentQ = (Math.Abs(q2.X - q1.X) > 1e-6) ? (x >= minXQ && x <= maxXQ) : (y >= minYQ && y <= maxYQ);

            if (!withinSegmentP || !withinSegmentQ)
            {
                return null; // Giao điểm nằm ngoài đoạn thẳng
            }

            float t = (Math.Abs(p2.X - p1.X) > Math.Abs(p2.Y - p1.Y)) ? (x - p1.X) / (p2.X - p1.X) : (y - p1.Y) / (p2.Y - p1.Y);
            float s = (Math.Abs(q2.X - q1.X) > Math.Abs(q2.Y - q1.Y)) ? (x - q1.X) / (q2.X - q1.X) : (y - q1.Y) / (q2.Y - q1.Y);

            float z1 = p1.Z + t * (p2.Z - p1.Z);
            float z2 = q1.Z + s * (q2.Z - q1.Z);
            float z = (z1 + z2) / 2;

            return new Vector3(x, y, z);
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
            if (points.Count < 3) return new List<Vector3>(); // Trả về rỗng nếu ít hơn 3 điểm

            points = points.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList(); // Loại bỏ điểm trùng

            List<Vector3> lower = new List<Vector3>(), upper = new List<Vector3>();

            foreach (var p in points)
            {
                while (lower.Count >= 2 && Cross(lower[^2], lower[^1], p) <= 0)
                    lower.RemoveAt(lower.Count - 1);
                lower.Add(p);
            }

            for (int i = points.Count - 1; i >= 0; i--)
            {
                Vector3 p = points[i];
                while (upper.Count >= 2 && Cross(upper[^2], upper[^1], p) <= 0)
                    upper.RemoveAt(upper.Count - 1);
                upper.Add(p);
            }

            if (lower.Count > 1) lower.RemoveAt(lower.Count - 1);
            if (upper.Count > 1) upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            return lower;
        }

        public static List<Face> TriangulatePolygon(List<Vector3> polygon, Model3D model)
        {
            List<Face> faces = new List<Face>();

            if (polygon.Count < 3) return faces;

            // Nếu chỉ có 3 điểm, thêm trực tiếp một tam giác
            if (polygon.Count == 3)
            {
                faces.Add(new Face()
                {
                    Vertices = new List<int>
            {
                AddVertexToModel(model, polygon[0]),
                AddVertexToModel(model, polygon[1]),
                AddVertexToModel(model, polygon[2])
            }
                });
                return faces;
            }

            // Kiểm tra nếu đa giác bị lõm
            if (!IsConvexPolygon(polygon))
            {
                throw new InvalidOperationException("Không thể chia đa giác lõm bằng thuật toán này.");
            }

            for (int i = 1; i < polygon.Count - 1; i++)
            {
                if (polygon[i] != polygon[i + 1]) // Bỏ qua điểm trùng
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
            }

            return faces;
        }

        public static bool IsConvexPolygon(List<Vector3> polygon)
        {
            if (polygon.Count < 3) return false; // Không đủ điểm để tạo thành một đa giác

            bool hasPositive = false;
            bool hasNegative = false;

            int n = polygon.Count;
            for (int i = 0; i < n; i++)
            {
                Vector3 p0 = polygon[i];
                Vector3 p1 = polygon[(i + 1) % n];
                Vector3 p2 = polygon[(i + 2) % n];

                float crossProduct = Cross(p0, p1, p2);

                if (crossProduct > 0) hasPositive = true;
                if (crossProduct < 0) hasNegative = true;

                // Nếu có cả dấu dương và âm → Đa giác bị lõm
                if (hasPositive && hasNegative) return false;
            }

            return true;
        }


        private static float Cross(Vector3 O, Vector3 A, Vector3 B)
        {
            return (A.X - O.X) * (B.Y - O.Y) - (A.Y - O.Y) * (B.X - O.X);
        }


        // Hàm để thêm vertex vào model và trả về chỉ số
        public static int AddVertexToModel(Model3D model, Vector3 vertex)
        {
            model.Vertices.Add(vertex);
            return model.Vertices.Count - 1;
        }
    }
}