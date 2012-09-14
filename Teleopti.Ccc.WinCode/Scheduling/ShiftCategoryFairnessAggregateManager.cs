using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessAggregateManager
	{
		ShiftCategoryFairnessCompareResult GetPerPersonAndGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly);
		ShiftCategoryFairnessCompareResult GetPerGroupAndOtherGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly);
	}

	public class ShiftCategoryFairnessAggregateManager : IShiftCategoryFairnessAggregateManager
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IShiftCategoryFairnessComparer _shiftCategoryFairnessComparer;
		private readonly IShiftCategoryFairnessAggregator _shiftCategoryFairnessAggregator;
		private readonly IShiftCategoryFairnessGroupPersonHolder _shiftCategoryFairnessGroupPersonHolder;
		private readonly IScheduleDictionary _dic;
		private DateOnlyPeriod _period;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessAggregateManager(ISchedulingResultStateHolder resultStateHolder, IShiftCategoryFairnessComparer shiftCategoryFairnessComparer,
			IShiftCategoryFairnessAggregator shiftCategoryFairnessAggregator, IShiftCategoryFairnessGroupPersonHolder shiftCategoryFairnessGroupPersonHolder)
		{
			_resultStateHolder = resultStateHolder;
			_dic = _resultStateHolder.Schedules;
			_shiftCategoryFairnessComparer = shiftCategoryFairnessComparer;
			_shiftCategoryFairnessAggregator = shiftCategoryFairnessAggregator;
			_shiftCategoryFairnessGroupPersonHolder = shiftCategoryFairnessGroupPersonHolder;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessCompareResult GetPerPersonAndGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly)
		{
			if (_period.Equals(new DateOnlyPeriod()))
			{
				var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
				_period = _dic.Period.VisiblePeriodMinusFourWeeksPeriod().ToDateOnlyPeriod(timeZoneInfo);
			}
			var ret = new ShiftCategoryFairnessCompareResult();
			var groups = _shiftCategoryFairnessGroupPersonHolder.GroupPersons(_period.DayCollection(), groupPage, dateOnly, new Collection<IPerson>(new List<IPerson>(_dic.Keys)));

			foreach (var groupPerson in groups)
			{
				if (groupPerson.GroupMembers.Contains(person))
				{
					var orig = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson> { person });
					var compare = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, groupPerson.GroupMembers);

					ret =  _shiftCategoryFairnessComparer.Compare(orig, compare, _resultStateHolder.ShiftCategories);
				}
			}

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessCompareResult GetPerGroupAndOtherGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly)
		{
			if (_period.Equals(new DateOnlyPeriod()))
			{
				var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
				_period = _dic.Period.VisiblePeriodMinusFourWeeksPeriod().ToDateOnlyPeriod(timeZoneInfo);
			}

			var groups = _shiftCategoryFairnessGroupPersonHolder.GroupPersons(_period.DayCollection(), groupPage, dateOnly, new Collection<IPerson>(new List<IPerson>(_dic.Keys)));
			IShiftCategoryFairness orig = new ShiftCategoryFairness();
			var otherPersons = new List<IPerson>();
			foreach (var groupPerson in groups)
			{
				if (groupPerson.GroupMembers.Contains(person))
					orig = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, groupPerson.GroupMembers);
				else
					otherPersons.AddRange(groupPerson.GroupMembers);
			}

			var compare = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, otherPersons);

			return _shiftCategoryFairnessComparer.Compare(orig, compare, _resultStateHolder.ShiftCategories);

		}
	}
}