namespace SharedKernel.Extensions;

public static class DoubleExtensions
{
    public static bool IsApproximatelyEqual(this double a, double b, double tolerance = 0.0001)
    {
        return Math.Abs(a - b) <= tolerance;
    }
}
