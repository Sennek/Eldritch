#if UNITY_EDITOR
using NoxLibrary;
using System.Linq;
using MoreLinq;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpriteSheetInfo : IEquatable<SpriteSheetInfo>
{
#region Properties

    public string AssetPath { get; set; }
    public string ObjectName { get; set; }
    public string CategoryName { get; set; }
    public string ActionName { get; set; }
    public AbsoluteDirection Direction { get; set; }
    public Vector2 SpriteSize { get; set; }
    public Vector2 TextureSize { get; set; }
    public Vector2 Pivot { get; set; }
    public int FPS { get; set; }
    public bool IsDained { get; set; }
    public bool IsTexture { get; set; } //If False, then it's a Sprite(sheet?)
    public bool IsNineSliced { get; set; }

#endregion

#region Constructors

    public SpriteSheetInfo() { }
    public SpriteSheetInfo(string assetPath)
    {
        AssetPath = assetPath; //Saves the actual asset path for future reference

        TextureSize = GetBitmapSize(assetPath);

        IsDained = assetPath.Contains(".dain");

        //Gets a cleaned file name and splits into the main segments
        do { assetPath = Path.GetFileNameWithoutExtension(assetPath); } while (assetPath.Contains('.'));
        string[] mainParts = assetPath.Replace("_iso", "").Replace("_#", "").Replace("#", "").Split('(', '[');

        //If there aren't at least 2 parts then we won't have enough information to continue
        if (mainParts.Length < 2) return;

#region Core Animation Name
        //Gets the core part of the asset name, cleans it up a bit further, and splits into the individual pieces
        string[] parts = mainParts[0]//.ToPascalCase(' ')
            .Replace('-', '_').Replace("_(", "(").Replace("__", "_")
            .Replace("_spritesheet", "").Replace("_sheet", "").Replace("_ss", "").Replace("_SS", "")
            .Split('_').Where(s => !string.IsNullOrEmpty(s)).ToArray();

        //Gets how many parts don't represent the directional component
        int nonDirectionPartsCount = parts.Length;
        if (nonDirectionPartsCount > 1 && parts.Last().Length.Within(1, 2) && (Direction = new AbsoluteDirection(parts.Last())) != default)
            nonDirectionPartsCount--;

        //Gets the information from the name based on actual part count and naming convention
        switch (nonDirectionPartsCount)
        {
            case 1: (ObjectName, CategoryName, ActionName) = (parts[0], null, parts[0]); break;
            case 2: (ObjectName, CategoryName, ActionName) = (parts[0], null, parts[1]); break;
            case 3: (ObjectName, CategoryName, ActionName) = (parts[0], parts[1], parts[2]); break;
            default: return;
        }
#endregion

#region Detailed Info
        //Tries to get the individual sprite size
        parts = mainParts[1].Remove(mainParts[1].IndexOfAny(')', ']')).Split(',', 'x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            SpriteSize = new Vector2(width, height);

        //Tries to get the pivot information
        if (mainParts.Length > 2)
        {
            parts = mainParts[2].Remove(mainParts[2].IndexOfAny(')', ']')).Split(',', 'x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                Pivot = new Vector2(x, y);
        }

        //Gets and interprets any extra information (such as FPS)
        foreach (string part in mainParts.Last().Split('_').Skip(1))
        {
            int.TryParse(new string(part.TakeWhile(c => char.IsDigit(c)).ToArray()), out int value);
            string key = new string(part.SkipWhile(c => char.IsDigit(c)).ToArray());
            switch (key)
            {
                case "F":
                case "FPS":
                    FPS = value;
                    break;
                case "TX":
                    IsTexture = true;
                    break;
                case "NS":
                    IsNineSliced = true;
                    break;
            }
        }
#endregion
    }

    private static Vector2 GetBitmapSize(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        using (MemoryStream ms = new MemoryStream(bytes))
        {
            using (System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(ms))
            {
                return new Vector2(bitmap.Width, bitmap.Height);
            }
        }
    }

    /// <summary>
    /// Gets a copy of this object
    /// </summary>
    /// <returns>A perfect deep copy of this object</returns>
    public SpriteSheetInfo Copy() => new SpriteSheetInfo()
    {
        AssetPath = AssetPath,
        ObjectName = ObjectName,
        CategoryName = CategoryName,
        ActionName = ActionName,
        Direction = Direction,
        SpriteSize = SpriteSize,
        Pivot = Pivot,
        FPS = FPS,
    };

#endregion

#region Methods

    /// <summary>
    /// Attempts to create a SpriteSheetInfo from a given asset path
    /// </summary>
    /// <param name="assetPath">The complete asset path</param>
    /// <param name="ssi">A new instance of SpriteSheetInfo</param>
    /// <returns>True if the new SSI is valid, false otherwise</returns>
    public static bool TryLoadFrom(string assetPath, out SpriteSheetInfo ssi) => (ssi = new SpriteSheetInfo(assetPath)).IsValid();

    /// <summary>
    /// Indicates whether this SSI has at least the minimum core information
    /// </summary>
    /// <returns>True if this SSI can be used at all, false otherwise</returns>
    public virtual bool IsValid() =>
        !string.IsNullOrEmpty(ObjectName) &&
        SpriteSize != default;

    /// <summary>
    /// Gets the base information for naming individual frames
    /// </summary>
    /// <returns>A StringBuilder populated with the base frame information</returns>
    protected virtual StringBuilder GetBaseFrameNameSB()
        => new StringBuilder().Append(ObjectName)
        .Append('_').Append(CategoryName ?? "Default")
        .Append('_').Append(ActionName ?? "Default")
        .Append('_').Append(Direction == default ? AbsoluteDirection.South : Direction);

    /// <summary>
    /// Gets the base information for naming individual frames
    /// </summary>
    /// <returns>A string with the base frame information</returns>
    public virtual string GetBaseFrameName() => GetBaseFrameNameSB().ToString();

    /// <summary>
    /// Gets the full name for a single frame
    /// </summary>
    /// <param name="pos">The position (or index) of a given frame. Zero-based is recommended</param>
    /// <returns></returns>
    public virtual string GetFrameName(int pos) => GetBaseFrameNameSB().Append('_').Append(pos).ToString();

    /// <summary>
    /// Gets a fully qualified new Asset Name (not path) based on information available (EXCEPT extension such as .png)
    /// <br>Use <seealso cref="GetFileName"/></br> for a file name with included .png
    /// </summary>
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append(ObjectName);

        if (!string.IsNullOrEmpty(CategoryName))
            sb.Append('_').Append(CategoryName);

        if (!string.IsNullOrEmpty(ActionName) && ActionName != ObjectName)
            sb.Append('_').Append(ActionName);

        if (Direction != default)
            sb.Append('_').Append(Direction);

        sb.Append('(').Append(SpriteSize.x).Append(',').Append(SpriteSize.y).Append(')');

        if (Pivot != default)
            sb.Append('(').Append(Pivot.x).Append(',').Append(Pivot.y).Append(')');

        if (FPS != default)
            sb.Append('_').Append(FPS).Append('F');

        if (IsTexture)
            sb.Append("_TX");

        if (IsNineSliced)
            sb.Append("_NS");

        if (IsDained)
            sb.Append(".dain");

        return sb.ToString();
    }

    /// <summary>
    /// Gets a fully qualified new Asset Name (not path) based on information available (INCLUDING extension .png)
    /// <br>Use <seealso cref="ToString"/></br> for a file name excluding the .png
    /// </summary>
    public string GetFileName() => ToString() + ".png";

    /// <summary>
    /// Determines whether this SSI is equivalent to another (same information)
    /// </summary>
    /// <param name="obj">Another object to compare to</param>
    /// <returns>True if the object is an SSI and the information matches exactly, false otherwise</returns>
    public override bool Equals(object obj) => Equals(obj as SpriteSheetInfo);

    /// <summary>
    /// Determines whether this SSI is equivalent to another (same information)
    /// </summary>
    /// <param name="other">Another SSI to compare to</param>
    /// <returns>True if the information matches exactly, false otherwise</returns>
    public bool Equals(SpriteSheetInfo other)
        => other != null &&
        ObjectName == other.ObjectName &&
        CategoryName == other.CategoryName &&
        ActionName == other.ActionName &&
        Direction == other.Direction &&
        SpriteSize == other.SpriteSize &&
        Pivot == other.Pivot &&
        FPS == other.FPS &&
        IsTexture == other.IsTexture &&
        IsNineSliced == other.IsNineSliced;

    /// <summary>
    /// Gets a HashCode that represents this instance of an SSI
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        int hashCode = 809115837;
        hashCode = (hashCode * -1521134295) + ObjectName.GetHashCode();
        hashCode = (hashCode * -1521134295) + CategoryName.GetHashCode();
        hashCode = (hashCode * -1521134295) + ActionName.GetHashCode();
        hashCode = (hashCode * -1521134295) + Direction.GetHashCode();
        hashCode = (hashCode * -1521134295) + SpriteSize.GetHashCode();
        hashCode = (hashCode * -1521134295) + Pivot.GetHashCode();
        hashCode = (hashCode * -1521134295) + FPS.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsTexture.GetHashCode();
        hashCode = (hashCode * -1521134295) + IsNineSliced.GetHashCode();
        return hashCode;
    }

    /// <summary>
    /// Determines whether this SSI is equivalent to another (same information)
    /// </summary>
    /// <param name="left">The first SSI of the comparison</param>
    /// <param name="right">Another SSI to compare to</param>
    /// <returns>True if the information matches exactly, false otherwise</returns>
    public static bool operator ==(SpriteSheetInfo left, SpriteSheetInfo right) => EqualityComparer<SpriteSheetInfo>.Default.Equals(left, right);

    /// <summary>
    /// Determines whether this SSI is NOT equivalent to another (same information)
    /// </summary>
    /// <param name="left">The first SSI of the comparison</param>
    /// <param name="right">Another SSI to compare to</param>
    /// <returns>True if the information DOES NOT match exactly, false otherwise</returns>
    public static bool operator !=(SpriteSheetInfo left, SpriteSheetInfo right) => !(left == right);

#endregion
}
#endif