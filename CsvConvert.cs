using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using DEXS.IO.CSV.Attributes;

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
}