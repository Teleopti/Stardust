using System;

namespace Node.Interfaces
{
    public class JobToDo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Serialized { get; set; }
        public string Type { get; set; }
    }
}