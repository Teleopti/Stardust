using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Creates cachable and serializable work shift projections
	/// </summary>
	public interface IRuleSetProjectionService
	{
		/// <summary>
		/// Creates the projection data from specified rule set
		/// </summary>
		/// <param name="workShiftRuleSet">The rule set</param>
		/// <returns>Work shift projections</returns>
		IEnumerable<IWorkShiftProjection> ProjectionCollection(IWorkShiftRuleSet workShiftRuleSet);
	}

	/// <summary>
	/// Work shift projection
	/// </summary>
	public interface IWorkShiftProjection
	{
		/// <summary>
		/// The contract time
		/// </summary>
		TimeSpan ContractTime { get; }
		/// <summary>
		/// The time period
		/// </summary>
		TimePeriod TimePeriod { get; }
		/// <summary>
		/// Shift category id
		/// </summary>
		Guid ShiftCategoryId { get; }
		/// <summary>
		/// Layers
		/// </summary>
		IEnumerable<IActivityRestrictableVisualLayer> Layers { get; }
	}

}