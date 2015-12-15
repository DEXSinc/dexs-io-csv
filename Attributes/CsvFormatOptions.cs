using System;
using System.Text;

namespace DEXS.IO.CSV.Attributes
{
    public class CsvFormatOptions : Attribute
    {
        public char Separator { get; set; } = ',';
        public char QuoteChar { get; set; } = '"';
        public char EscapeChar { get; set; } = '\\';
        public char QuoteEscape { get; set; } = '"';

        public char[] MustQuote { get; set; } = { '+', ' ' };
        public char[] MustEscape { get; set; } = { };

        public string LineSeparator { get; set; } = Environment.NewLine;

        public bool ForceQuotes { get; set; } = false;

        public string Encoding
        {
            get { return TextEncoding.EncodingName; }
            set
            {
                try
                {
                    TextEncoding = System.Text.Encoding.GetEncoding(value);
                }
                catch
                {
                    TextEncoding = System.Text.Encoding.UTF8;
                }
            }
        }

        public Encoding TextEncoding { get; set; } = System.Text.Encoding.UTF8;
    }
}