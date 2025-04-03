
using System;
using System.Text;
using UnityEngine;

public static class ExtensionMethods
{
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }

    /// <summary>
    /// Format floats into string
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string FormatFloat(this float num)
    {
        if (Math.Abs(num - Mathf.Floor(num)) < Constants.TOLERANCE)
        {
            return ((int)num).ToString("N0");
        }
        else
        {
            return num.ToString("N2");
        }
    }

    /// <summary>
    /// Format ints into string
    /// </summary>
    /// <param name="Num"></param>
    /// <returns></returns>
    public static string FormatInt(this int num)
    {
        return num.ToString("N0");
    }

    /// <summary>
    /// Repeats a string a specified number of times.
    /// </summary>
    /// <param name="str">The string to repeat</param>
    /// <param name="count">Number of times to repeat</param>
    /// <returns>A new string containing the input string repeated</returns>
    public static string Repeat(this string str, int count)
    {
        if (string.IsNullOrEmpty(str) || count <= 0)
            return string.Empty;
            
        if (count == 1)
            return str;
            
        StringBuilder sb = new StringBuilder(str.Length * count);
        for (int i = 0; i < count; i++)
        {
            sb.Append(str);
        }
        
        return sb.ToString();
    }
    public static string FormatRewardMoney(this int num)
    {
        int count = num > Constants.MAX_REWARD_COUNT ? Constants.MAX_REWARD_COUNT : num;
        string s = "$";
        return Repeat(s, count);
    }
}
