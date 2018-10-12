using System.Data.Common;

namespace WMM.Data
{
    public static class DbDataReaderExtensions
    {
        public static string GetStringNullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? string.Empty
                : reader.GetString(colIndex);
        }
    }
}
