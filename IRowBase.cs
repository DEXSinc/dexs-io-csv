using System.Collections.Generic;
using DEXS.IO.CSV.Attributes;

namespace DEXS.IO.CSV
{
    public interface IRowBase
    {
        IEnumerable<string> GetHeaderItems();
        IEnumerable<string> GetStringValues();
        IEnumerable<string> FormatValues(IEnumerable<string> values, CsvFormatOptions options = null);
        string GetRow(CsvFormatOptions options = null);
        string GetHeader(CsvFormatOptions options = null);
    }
}