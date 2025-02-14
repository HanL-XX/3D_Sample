using System;

class Point3D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public Point3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    // Phép cộng hai điểm
    public static Point3D operator +(Point3D p1, Point3D p2)
    {
        return new Point3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
    }

    // Phép trừ hai điểm
    public static Point3D operator -(Point3D p1, Point3D p2)
    {
        return new Point3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
    }

    // Nhân với một số
    public static Point3D operator *(Point3D p, double scalar)
    {
        return new Point3D(p.X * scalar, p.Y * scalar, p.Z * scalar);
    }

    // Chia cho một số
    public static Point3D operator /(Point3D p, double scalar)
    {
        if (scalar == 0)
            throw new DivideByZeroException("Không thể chia cho 0");
        return new Point3D(p.X / scalar, p.Y / scalar, p.Z / scalar);
    }

    // Tính khoảng cách giữa hai điểm
    public double DistanceTo(Point3D other)
    {
        return Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2) + Math.Pow(other.Z - Z, 2));
    }

    // Hiển thị điểm dưới dạng chuỗi
    public override string ToString()
    {
        return $"({X}, {Y}, {Z})";
    }
}