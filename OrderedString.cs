namespace DEXS.IO.CSV
{
    public class OrderedString
    {
        public int Order { get; set; } = -1;
        public string Value { get; set; }
    }
}