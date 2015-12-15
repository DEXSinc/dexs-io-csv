using System.Reflection;
using DEXS.IO.CSV.Attributes;

namespace DEXS.IO.CSV
{
    public class CsvProperty
    {
        public PropertyInfo Info { get; set; }
        public CsvColumnAttribute Attributes { get; set; }

        public CsvProperty(PropertyInfo info, CsvColumnAttribute attributes)
        {
            Info = info;
            Attributes = attributes;
        }
    }
}