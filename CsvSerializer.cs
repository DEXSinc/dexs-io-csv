using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using DEXS.IO.CSV.Attributes;
using LumenWorks.Framework.IO.Csv;
using DEXS.IO.CSV.Extensions;

namespace DEXS.IO.CSV
{
    public class CsvSerializer<T> where T : class, new()
    {
        private readonly IEnumerable<CsvProperty> _csvProperties;
        public CsvFormatOptions FormatOptions { get; set; }

        public CsvSerializer()
        {
            var type = typeof(T);

            FormatOptions = type.GetCustomAttribute<CsvFormatOptions>() ?? new CsvFormatOptions();
            _csvProperties = CsvConvert.GetCSVProperties(type);
        }

        public void Serialize(Stream stream, IEnumerable<T> data, CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            var sb = new StringBuilder();
            
            sb.Append(GetHeader(csvFormatOptions));
            sb.Append(FormatOptions.LineSeparator);

            foreach (var items in data.Select(item => GetValueItems(item, csvFormatOptions)))
            {
                sb.Append(string.Join(csvFormatOptions.Separator.ToString(), items));
                sb.Append(csvFormatOptions.LineSeparator);
            }

            using (var sw = new StreamWriter(stream))
            {
                sw.Write(sb.ToString().Trim());
            }
        }
        
        public object ConvertTo(object value, Type t, string format = null)
        {
            try
            {
                if (t == typeof (DateTime)) return (value as string).ToDateTime(format);
                if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof (Nullable<>))
                    try
                    {
                        return Convert.ChangeType(value, t);
                    }
                    catch
                    {
                        return Activator.CreateInstance(t);
                    }
                var nc = new NullableConverter(t);
                return nc.ConvertFrom(value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occured trying to convert {value} as {t} using format {format}", ex);
            }
        }

        public IList<T> Deserialize(StreamReader stream, CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            using (var csv = new CachedCsvReader(stream, true, csvFormatOptions.Separator, csvFormatOptions.QuoteChar, csvFormatOptions.EscapeChar, '#', ValueTrimmingOptions.UnquotedOnly))
            {
                var items = new List<T>();
                csv.MoveToStart();
                while (csv.ReadNextRecord())
                {
                    var item = new T();
                    _csvProperties.ToList().ForEach(p =>
                    {
                        // if (!string.IsNullOrEmpty(p.Attributes.Format)) return;
                        var csvValue = csv[p.Attributes.Name ?? p.Info.Name];
                        var val = ConvertTo(csvValue, p.Info.PropertyType);
                        p.Info.SetValue(item, val);
                    });
                    items.Add(item);
                }
                return items;
            }
        }

        public string GetString(object o, string format = null)
        {
            if (o == null) return null;
            var oType = o.GetType();
            if (oType != typeof (DateTime)) return o.ToString();
            // Else convert DateTime
            const string defaultDateTime = "yyyy-MM-ddTHH:mm:ss.ffffffZ";
            return ((DateTime)o).ToString(format ?? defaultDateTime);
        }

        public IEnumerable<string> GetValueItems(T item, CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            var rawItems = _csvProperties.Select(cp => GetString(cp.Info.GetValue(item), cp.Attributes.Format));
            return FormatValues(rawItems, csvFormatOptions);
        }

        public string GetHeader(CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            var formattedHeaderItems = FormatValues(GetHeaderItems(), formatOptions);
            return string.Join(csvFormatOptions.Separator.ToString(), formattedHeaderItems);
        }

        public IEnumerable<string> GetHeaderItems()
        {
            return _csvProperties.Select(cp => cp.Attributes.Name ?? cp.Info.Name);
        }

        public IEnumerable<string> FormatValues(IEnumerable<string> values, CsvFormatOptions options = null)
        {
            var ra = options ?? FormatOptions;
            var formatedValues = values.Select(h =>
            {
                var result = h?.Replace(ra.QuoteChar.ToString(), $"{ra.QuoteEscape}{ra.QuoteChar}") ?? "";
                ra.MustEscape.ToList().ForEach(c =>
                {
                    result = result.Replace(c.ToString(), $"{ra.EscapeChar}{c}");
                });
                var quoteChar = (ra.ForceQuotes ||
                                 result.Contains(ra.QuoteChar) ||
                                 result.Contains(ra.Separator) ||
                                 ra.MustQuote.Any(c => result.Contains(c)))
                    ? ra.QuoteChar.ToString() : string.Empty;
                return $"{quoteChar}{result}{quoteChar}";
            });
            return formatedValues;
        }
    }
}