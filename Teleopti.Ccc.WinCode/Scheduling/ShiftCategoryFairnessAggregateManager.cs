using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface IShiftCategoryFairnessAggregateManager
	{
		IShiftCategoryFairnessCompareResult GetPerPersonAndGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly);
		IShiftCategoryFairnessCompareResult GetPerGroupAndOtherGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly);
		IList<IShiftCategoryFairnessCompareResult> GetForGroups(IEnumerable<IPerson> persons, IGroupPageLight groupPage,
		                                                       DateOnly dateOnly, IList<DateOnly> selectedDates);

		IList<IShiftCategoryFairnessCompareResult> GetPerPersonsAndGroup(IEnumerable<IPerson> persons, IGroupPageLight groupPage,
		                                                                 DateOnly dateOnly);
	}

	public class ShiftCategoryFairnessAggregateManager : IShiftCategoryFairnessAggregateManager
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IShiftCategoryFairnessComparer _shiftCategoryFairnessComparer;
		private readonly IShiftCategoryFairnessAggregator _shiftCategoryFairnessAggregator;
		private readonly IShiftCategoryFairnessGroupPersonHolder _shiftCategoryFairnessGroupPersonHolder;
		private readonly IScheduleDictionary _dic;
		private DateOnlyPeriod _period;
		private readonly IList<IPerson> _personsWithShiftCategoryFairness = new List<IPerson>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public ShiftCategoryFairnessAggregateManager(ISchedulingResultStateHolder resultStateHolder, IShiftCategoryFairnessComparer shiftCategoryFairnessComparer,
			IShiftCategoryFairnessAggregator shiftCategoryFairnessAggregator, IShiftCategoryFairnessGroupPersonHolder shiftCategoryFairnessGroupPersonHolder)
		{
			_resultStateHolder = resultStateHolder;
			_dic = _resultStateHolder.Schedules;
			_shiftCategoryFairnessComparer = shiftCategoryFairnessComparer;
			_shiftCategoryFairnessAggregator = shiftCategoryFairnessAggregator;
			_shiftCategoryFairnessGroupPersonHolder = shiftCategoryFairnessGroupPersonHolder;
			foreach (var person in _dic.Keys.Where(person => person.WorkflowControlSet != null && person.WorkflowControlSet.UseShiftCategoryFairness))
			{
				_personsWithShiftCategoryFairness.Add(person);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IShiftCategoryFairnessCompareResult GetPerPersonAndGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly)
		{
			if (_period.Equals(new DateOnlyPeriod()))
			{
				var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
				_period = _dic.Period.VisiblePeriodMinusFourWeeksPeriod().ToDateOnlyPeriod(timeZoneInfo);
			}
			
			var ret = new ShiftCategoryFairnessCompareResult();
			var groups = _shiftCategoryFairnessGroupPersonHolder.GroupPersons(_period.DayCollection(), groupPage, dateOnly, _personsWithShiftCategoryFairness);

			foreach (var groupPerson in groups)
			{
				if (groupPerson.GroupMembers.Contains(person))
				{
					var orig = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson> { person });
					var membersWithoutPerson = new List<IPerson>(groupPerson.GroupMembers);
					membersWithoutPerson.Remove(person);
					var compare = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, membersWithoutPerson);

					ret =  _shiftCategoryFairnessComparer.Compare(orig, compare, _resultStateHolder.ShiftCategories);
					ret.OriginalMembers = new List<IPerson>{person};
				}
			}

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IShiftCategoryFairnessCompareResult GetPerGroupAndOtherGroup(IPerson person, IGroupPageLight groupPage, DateOnly dateOnly)
		{
			if (_period.Equals(new DateOnlyPeriod()))
			{
				var timeZoneInfo = person.PermissionInformation.DefaultTimeZone();
				_period = _dic.Period.VisiblePeriodMinusFourWeeksPeriod().ToDateOnlyPeriod(timeZoneInfo);
			}

			var groups = _shiftCategoryFairnessGroupPersonHolder.GroupPersons(_period.DayCollection(), groupPage, dateOnly, _personsWithShiftCategoryFairness);
			IShiftCategoryFairnessHolder orig = new ShiftCategoryFairnessHolder();
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

		public IList<IShiftCategoryFairnessCompareResult> GetForGroups(IEnumerable<IPerson> persons, IGroupPageLight groupPage, DateOnly dateOnly, IList<DateOnly> selectedDates )
		{
			var ret = new List<IShiftCategoryFairnessCompareResult>();
			var groups = _shiftCategoryFairnessGroupPersonHolder.GroupPersons(selectedDates, groupPage, dateOnly, persons);

			foreach (var groupPerson in groups)
			{
				var otherPersons = new List<IPerson>();
				var orig = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, groupPerson.GroupMembers);
				foreach (var groupPerson2 in groups)
				{
					if(!groupPerson2.Equals(groupPerson))
						otherPersons.AddRange(groupPerson2.GroupMembers);
				}
						
				var compare = _shiftCategoryFairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, otherPersons);
				var result = _shiftCategoryFairnessComparer.Compare(orig, compare, _resultStateHolder.ShiftCategories);
				result.OriginalMembers = groupPerson.GroupMembers.ToList();
				ret.Add(result);
			}

			return ret;
		}

		public IList<IShiftCategoryFairnessCompareResult> GetPerPersonsAndGroup(IEnumerable<IPerson> persons, IGroupPageLight groupPage, DateOnly dateOnly)
		{
			return persons.Select(person => GetPerPersonAndGroup(person, groupPage, dateOnly)).ToList();
		}
	}
}