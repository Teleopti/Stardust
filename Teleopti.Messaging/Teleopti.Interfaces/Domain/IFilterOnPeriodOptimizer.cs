using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Implemented by optimizers for faster 
    /// IVisualLayerCollection.FilterLayers(period)
    /// search.
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2009-02-02
    /// </remarks>
    public interface IFilterOnPeriodOptimizer
    {
        /// <summary>
        /// Finds the start index.
        /// Must not be the correct one.
        /// However - it must be the exact one
        /// or _prior_ the actual one.
        /// If you have no idea, 
        /// return 0 and everything will be searched.
        /// 
        /// Will only be called if collection size > 1
        /// </summary>
        /// <param name="unmergedCollection">The unmerged collection.</param>
        /// <param name="start">The start.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-02
        /// </remarks>
        int FindStartIndex(IEnumerable<IVisualLayer> unmergedCollection, DateTime start);

        /// <summary>
        /// Called when a period search is ready.
        /// Tells the latest layer's index position.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-02-02
        /// </remarks>
        void FoundEndIndex(int index);
    }
}