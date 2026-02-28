using System;

/// <summary>
/// A generic reference to an asset inside an Asset Bundle.
/// </summary>
[Serializable]
public struct BunnyReference<T> : IEquatable<BunnyReference<T>> where T : UnityEngine.Object
{
    public string assetBundleName;
    public string assetName;

    public BunnyReference(string bundleName, string assetName)
    {
        this.assetBundleName = bundleName;
        this.assetName = assetName;
    }

    /// <summary>
    /// Represents an empty or unassigned reference.
    /// </summary>
    public static BunnyReference<T> None => new(string.Empty, string.Empty);

    public readonly bool Equals(BunnyReference<T> other)
    {
        bool bundleEqual = (string.IsNullOrEmpty(assetBundleName) && string.IsNullOrEmpty(other.assetBundleName)) ||
            string.Equals(assetBundleName, other.assetBundleName, StringComparison.OrdinalIgnoreCase);

        bool assetEqual = (string.IsNullOrEmpty(assetName) && string.IsNullOrEmpty(other.assetName)) ||
            string.Equals(assetName, other.assetName, StringComparison.OrdinalIgnoreCase);

        return bundleEqual && assetEqual;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is BunnyReference<T> other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        int bundleHash = string.IsNullOrEmpty(assetBundleName) ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(assetBundleName);
        int assetHash = string.IsNullOrEmpty(assetName) ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(assetName);

        return HashCode.Combine(bundleHash, assetHash);
    }

    public static bool operator ==(BunnyReference<T> left, BunnyReference<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(BunnyReference<T> left, BunnyReference<T> right)
    {
        return !left.Equals(right);
    }
}
