namespace GridGain.CodeGen
{
    public class Method
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Desc { get; set; }
        public string Annotation { get; set; }
        public string Arguments { get; set; }
        public string Returns { get; set; }

        public override string ToString()
        {
            return string.Format("Method: {0}", Name);
        }
    }
}