using System;

namespace DEXS.IO.CSV.Attributes
{
    public class CsvColumnAttribute : Attribute
    {
        public string Name { get; set; } = null;
        public Type Type { get; set; } = typeof (string);
        public int Order { get; set; } = -1;
        public string Format { get; set; }
    }
}