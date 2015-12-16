# dexs-io-csv
DEXS.IO.CSV Lightweight .Net CSV Serializer

Example use:

###Serialize/Deserialize To/From CSV File:
```

using System.Collections.Generic;
using System.IO;
using DEXS.IO.CSV;

namespace CSVTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = CanLoadCSV(@"C:\dev\temp\input.csv");
            CanSaveCSV(x, @"C:\dev\temp\test.csv");
        }

        public static bool CanSaveCSV(IEnumerable<MyClass> items, string fileName)
        {
            var fileStream = new StreamWriter(fileName, false);
            var serializer = new CsvSerializer<MyClass>();
            serializer.Serialize(fileStream.BaseStream, items);
            return true;
        }

        public static IEnumerable<MyClass> CanLoadCSV(string fileName)
        {
            var fileStream = new StreamReader(fileName);
            var serializer = new CsvSerializer<MyClass>();
            var x = serializer.Deserialize(fileStream);
            return x;
        }
    }
}


```

###The Class can have annotations

```

using System;
using DEXS.IO.CSV.Attributes;

namespace CSVTestProject
{
    [CsvFormatOptions(QuoteChar = '\'', Separator = ',', QuoteEscape = '\'')]
    public class MyClass
    {
        [CsvColumn(Name = "JOBTYPE")]
        public string JobType { get; set; }
        [CsvColumn(Name = "DATEMINUTE", Format = "dd-MMM-yyyy hh:mm:ss.ffffff")]
        public DateTime DateMinute { get; set; }
        [CsvColumn(Name = "QUEUED")]
        public long Queued { get; set; }
        [CsvColumn(Name = "PROCESSING")]
        public long Processing { get; set; }
        [CsvColumn(Name = "COMPLETED")]
        public long Completed { get; set; }
        [CsvColumn(Name = "FAILED")]
        public long Failed { get; set; }
        [CsvColumn(Name = "VACUUMED")]
        public long Vacuumed { get; set; }
        [CsvColumn(Name = "TOTALJOBS")]
        public long TotalJobs { get; set; }
        [CsvColumn(Name = "AVERAGECOSTINMILLISECONDS")]
        public long AverageCostInMilliseconds { get; set; }
        [CsvColumn(Name = "FIRSTMESSAGE", Format = "dd-MMM-yyyy hh:mm:ss.ffffff")]
        public DateTime FirstMessage { get; set; }
        [CsvColumn(Name = "LASTMESSAGE", Format = "dd-MMM-yyyy hh:mm:ss.ffffff")]
        public DateTime LastMessage { get; set; }
    }
}


```