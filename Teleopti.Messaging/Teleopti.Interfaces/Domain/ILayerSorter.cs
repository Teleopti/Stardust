using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Interface for all sorters for layers
    /// </summary>
    /// <remarks>
    /// Interfaces should have generic contraints!
    /// Fix later!
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public interface ILayerSorter<T> : IComparer<ILayer<T>>
    {
    }
}