using UnityEngine;

public static class AnimationConstants
{
    // Durations
    
    public const float HudToggleFadeDuration = .5f;
    public const float PicoDuration = .01f;
    public const float NanoDuration = .02f;
    public const float MicroDuration = .05f;
    public const float TinyDuration = .2f;
    public const float ShortDuration = .4f;
    public const float MediumDuration = 1f;
    public const float LongDuration = 2f;
    
    // Colors 

    public static readonly Color AccentColor = HexToColor("FFE09A");
    public static readonly Color EnergyColor = HexToColor("F6B81F");
    public static readonly Color RedColor = HexToColor("E93826");
    public static readonly Color BgColor = new Color32(4,22,25,209);
    public static readonly Color DisabledColor = new Color32(255, 255, 255, 76);
    
    // Helping Function
    public static Color HexToColor(string hex)
    {
        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte a = hex.Length >= 8 ? byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) : (byte)255;

        return new Color32(r, g, b, a);
    }

    /// <summary>
    /// Takes color as input and returns the hex code
    /// </summary>
    /// <param name="color">Color to convert to hex</param>
    /// <param name="includeHashTag">whether or not hashtag should be included in the string</param>
    /// <returns></returns>
    public static string ColorToHex(Color color, bool includeHashTag)
    {
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        int a = Mathf.RoundToInt(color.a * 255);

        if (includeHashTag)
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        else
            return $"{r:X2}{g:X2}{b:X2}{a:X2}";
    }
    
}