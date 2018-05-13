
public static class Math
{
    public static uint Pow(int x, int y)
    {
        if (y == 0) return 1;

        return (uint) x * Pow(x, y - 1);
    }
}

