﻿namespace Teleopti.Interfaces.Domain
{
	/// <summary>
    /// The resource optimization helper.
    /// </summary>
    /// <remarks>
    /// Created by: peterwe
    /// Created date: 2009-01-21
    /// </remarks>
    public interface IResourceOptimizationHelper
    {
		void ResourceCalculate(DateOnly localDate, ResourceOptimizationData resourceOptimizationData);
	}
}
