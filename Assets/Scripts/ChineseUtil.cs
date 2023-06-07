using System.Runtime.InteropServices;

/// <summary>
/// 進行 簡體 <==> 繁體 中文轉換
/// </summary>
public class ChineseUtil
{
    private const int LocaleSystem = 0x0800;
    private const int LocaleSimp = 0x02000000;
    private const int LocaleTrad = 0x04000000;

    [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern int LCMapString(int locale, int mapFlag, string srcStr, int srcLen, [Out] string destStr, int destLen);

    public static string ToTrad(string input)
    {
        string output = new string(' ', input.Length);
        LCMapString(LocaleSystem, LocaleTrad, input, input.Length, output, input.Length);
        return output;
    }

    public static string ToSimp(string input)
    {
        string output = new string(' ', input.Length);
        LCMapString(LocaleSystem, LocaleSimp, input, input.Length, output, input.Length);
        return output;
    }
}

public class StringFormatter
{
    public static string RemoveSpaces(string input) => input.Replace(" ", string.Empty);
    public static string SingleSpaceSeparate(string input) => string.Join(" ", RemoveSpaces(input).ToCharArray());
}