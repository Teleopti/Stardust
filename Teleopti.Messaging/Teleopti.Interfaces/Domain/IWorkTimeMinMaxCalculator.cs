using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// result from work time min max calculator
	/// </summary>
	public class WorkTimeMinMaxCalculationResult
	{
		/// <summary>
		/// the times
		/// </summary>
		public IWorkTimeMinMax WorkTimeMinMax { get; set; }
		/// <summary>
		/// true if the restriction never had the posibility to match anything
		/// </summary>
		public bool RestrictionNeverHadThePosibilityToMatchWithShifts { get; set; }
	}

	/// <summary>
	/// Interface for work time min/max calculator
	/// </summary>
	public interface IWorkTimeMinMaxCalculator
	{
		/// <summary>
		/// Calculate the min/max work times for a day using an alread loaded schedule day
		/// </summary>
		/// <param name="date"></param>
		/// <param name="person"></param>
		/// <param name="scheduleDay"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		WorkTimeMinMaxCalculationResult WorkTimeMinMax(DateOnly date, IPerson person, IScheduleDay scheduleDay);
	}

}