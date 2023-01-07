using M3USync.Data.Abstracts;
using System.Diagnostics.CodeAnalysis;

namespace M3USync.Downloaders.Contents.Readers
{
    public class ContentComparer<T> : IEqualityComparer<T> where T : Content
    {
        public bool Equals(T? x, T? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            return x.Url == y.Url;
        }

        public int GetHashCode(T obj)
        {
            if (obj == null || obj.Url == null) return 0;
            return obj.Url.GetHashCode();
        }
    }
}