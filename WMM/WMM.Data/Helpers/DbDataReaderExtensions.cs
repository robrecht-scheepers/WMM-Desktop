using System;
using System.Data.Common;

namespace WMM.Data.Helpers
{
    public static class DbDataReaderExtensions
    {

        public static Guid GetGuidNullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? Guid.Empty
                : reader.GetGuid(colIndex);
        }

        public static string GetStringNullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? string.Empty
                : reader.GetString(colIndex);
        }

        public static int GetInt32NullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? -1
                : reader.GetInt32(colIndex);
        }

        public static DateTime GetDateTimeNullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? DateTime.MinValue
                : reader.GetDateTime(colIndex);
        }
    }
}
