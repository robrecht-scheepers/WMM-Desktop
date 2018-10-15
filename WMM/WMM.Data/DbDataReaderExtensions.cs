using System;
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

        public static DateTime GetDateTimeNullSafe(this DbDataReader reader, int colIndex)
        {
            return reader.IsDBNull(colIndex)
                ? DateTime.MinValue
                : reader.GetDateTime(colIndex);
        }
    }
}
