using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class TeamBlockEffectiveRestrcition : IScheduleRestrictionStrategy
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IEnumerable<IPerson> _persons;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly ISchedulingOptions _schedulingOptions;

		public TeamBlockEffectiveRestrcition(IEffectiveRestrictionCreator effectiveRestrictionCreator,
											 IEnumerable<IPerson> persons, ISchedulingOptions schedulingOptions,
											 IScheduleDictionary scheduleDictionary)
		{
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_persons = persons;
			_schedulingOptions = schedulingOptions;
			_scheduleDictionary = scheduleDictionary;
		}

		/// <summary>
		/// </summary>
		/// <param name="dateOnlyList"></param>
		/// <param name="matrixList">Pass null as no matrix list is used here</param>
		/// <returns></returns>
		public IEffectiveRestriction ExtractRestriction(IList<DateOnly> dateOnlyList,
														IList<IScheduleMatrixPro> matrixList)
		{
			IEffectiveRestriction effectiveRestriction = null;
			foreach (DateOnly dateOnly in dateOnlyList)
			{
				IEffectiveRestriction restriction =
					_effectiveRestrictionCreator.GetEffectiveRestriction(_persons,
																		 dateOnly, _schedulingOptions,
																		 _scheduleDictionary);
				if (restriction == null)
					return null;
				if (effectiveRestriction != null)
					effectiveRestriction = effectiveRestriction.Combine(restriction);
				else
					effectiveRestriction = restriction;
				if (effectiveRestriction == null)
					return null;
			}
			return effectiveRestriction;
		}
	}
}