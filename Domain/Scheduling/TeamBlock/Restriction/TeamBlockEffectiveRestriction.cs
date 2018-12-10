using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.Restriction
{
	public class TeamBlockEffectiveRestriction : IScheduleRestrictionStrategy
	{
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly IEnumerable<IPerson> _persons;
		private readonly IScheduleDictionary _scheduleDictionary;
		private readonly SchedulingOptions _schedulingOptions;

		public TeamBlockEffectiveRestriction(IEffectiveRestrictionCreator effectiveRestrictionCreator,
		                                     IPerson person, SchedulingOptions schedulingOptions,
		                                     IScheduleDictionary scheduleDictionary)
			: this(effectiveRestrictionCreator, new List<IPerson> {person}, schedulingOptions, scheduleDictionary)
		{

		}

		public TeamBlockEffectiveRestriction(IEffectiveRestrictionCreator effectiveRestrictionCreator,
											 IEnumerable<IPerson> persons, SchedulingOptions schedulingOptions,
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
			var emptyRestriction = new EffectiveRestriction(new StartTimeLimitation(),
													 new EndTimeLimitation(),
													 new WorkTimeLimitation(), null, null, null,
													 new List<IActivityRestriction>());
			foreach (DateOnly dateOnly in dateOnlyList)
			{
				IEffectiveRestriction restriction =
					_effectiveRestrictionCreator.GetEffectiveRestriction(_persons,
																		 dateOnly, _schedulingOptions,
																		 _scheduleDictionary);
				if (restriction == null)
					return emptyRestriction;
				if (effectiveRestriction != null)
					effectiveRestriction = effectiveRestriction.Combine(restriction);
				else
					effectiveRestriction = restriction;
				if (effectiveRestriction == null)
					return emptyRestriction;
			}
			return effectiveRestriction;
		}
	}
}