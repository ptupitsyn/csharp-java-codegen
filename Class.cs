namespace GridGain.CodeGen
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string GenericArgs { get; set; }
        public string ShortDisplayCode { get; set; }
        public string ToStringCode { get; set; }
        public Method[] Properties { get; set; }
        public string Desc { get; set; }
    }
}