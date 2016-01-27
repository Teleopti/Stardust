using System;

namespace Node.Interfaces
{
    public interface IJobProgress<T>
    {
        Progress<T> JobProgress { get; set; }
    }
}