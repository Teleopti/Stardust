﻿using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// A custom collection of generic type Layer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ILayerCollection<T> : IList<ILayer<T>>
    {
        /// <summary>
        /// Get the period with the earliest start and the latest end.
        /// </summary>
        /// <returns></returns>
        DateTimePeriod? Period();
    }
}
