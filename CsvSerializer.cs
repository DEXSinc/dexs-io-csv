using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DEXS.IO.CSV.Attributes;
using LumenWorks.Framework.IO.Csv;
using DEXS.IO.CSV.Extensions;

namespace DEXS.IO.CSV
{
    public class CsvConvert
    {
        public static string SerializeObject(object o, CsvFormatOptions options)
        {
            return SerializeObject(o, null, options);
        }

        public static string SerializeObject(object o, Type type, CsvFormatOptions options)
        {
            // var serializer = new CsvSerializer();
            return "";
        }

        public static IEnumerable<T> DeserializeString<T>(string data) where T : class, new()
        {
            var serializer = new CsvSerializer<T>();
            var stream = new StreamReader(GenerateStreamFromString(data));
            var x = serializer.Deserialize(stream);
            return x;
        }

        public static IEnumerable<T> DeserializeFromStream<T>(StreamReader data) where T : class, new()
        {
            var serializer = new CsvSerializer<T>();
            var x = serializer.Deserialize(data);
            return x;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static IEnumerable<CsvProperty> GetCSVProperties(Type type)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance
                                                | BindingFlags.GetProperty | BindingFlags.SetProperty);

            return (from prop in properties
                where prop.GetCustomAttribute<CsvIgnoreAttribute>() == null
                let csvColumnAttribute = (CsvColumnAttribute)Attribute.GetCustomAttribute(prop, typeof(CsvColumnAttribute)) ?? new CsvColumnAttribute()
                orderby csvColumnAttribute.Order, csvColumnAttribute.Name ?? prop.Name
                select new CsvProperty(prop, csvColumnAttribute)).ToList();
        }
    }

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
            if (t == typeof(DateTime)) return (value as string).ToDateTime(format);
            if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof (Nullable<>))
                return Convert.ChangeType(value, t);
            var nc = new NullableConverter(t);
            return nc.ConvertFrom(value);
        }

        public IList<T> Deserialize(StreamReader stream, CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            using (var csv = new CachedCsvReader(stream, true, csvFormatOptions.Separator, csvFormatOptions.QuoteChar, csvFormatOptions.EscapeChar, '#', ValueTrimmingOptions.None))
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

        public IEnumerable<string> GetValueItems(T item, CsvFormatOptions formatOptions = null)
        {
            var csvFormatOptions = formatOptions ?? FormatOptions;
            var rawItems = _csvProperties.Select(cp => cp.Info.GetValue(item)?.ToString());
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