class Program
{
    static void Main()
    {
        Point3D P1 = new Point3D(1, 2, 3);
        Point3D P2 = new Point3D(4, 5, 6);

        Console.WriteLine($"P1 + P2 = {P1 + P2}");
        Console.WriteLine($"P1 - P2 = {P1 - P2}");
        Console.WriteLine($"P1 * 5 = {P1 * 5}");
        Console.WriteLine($"P1 / 10 = {P1 / 10}");
        Console.WriteLine($"Distance from P1 to P2 = {P1.DistanceTo(P2)}");
    }
}