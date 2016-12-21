using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewShiftCategoryLimitationRule : INewBusinessRule
	{
		private readonly IShiftCategoryLimitationChecker _limitationChecker;
		private readonly IVirtualSchedulePeriodExtractor _virtualSchedulePeriodExtractor;

		public NewShiftCategoryLimitationRule(IShiftCategoryLimitationChecker limitationChecker,
			IVirtualSchedulePeriodExtractor virtualSchedulePeriodExtractor)
		{
			_limitationChecker = limitationChecker;
			_virtualSchedulePeriodExtractor = virtualSchedulePeriodExtractor;
		}

		public bool IsMandatory => false;

		public bool HaltModify { get; set; } = true;

		public bool Configurable => true;

		public bool ForDelete { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Domain.Scheduling.Rules.NewShiftCategoryLimitationRule.createResponse(Teleopti.Interfaces.Domain.IPerson,Teleopti.Interfaces.Domain.DateOnly,System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var currentUiCulture = Thread.CurrentThread.CurrentCulture;
			var responseList = new HashSet<IBusinessRuleResponse>();

			var virtualSchedulePeriods =
				_virtualSchedulePeriodExtractor.CreateVirtualSchedulePeriodsFromScheduleDays(scheduleDays);

			foreach (var schedulePeriod in virtualSchedulePeriods)
			{
				if (!schedulePeriod.IsValid) continue;

				var period = schedulePeriod.DateOnlyPeriod;
				var person = schedulePeriod.Person;
				var currentSchedules = rangeClones[person];
				var oldResponses = currentSchedules.BusinessRuleResponseInternalCollection;
				var oldResponseCount = oldResponses.Count;
				foreach (var day in period.DayCollection())
				{
					oldResponses.Remove(createResponse(person, day, "remove"));
				}
				foreach (var shiftCategoryLimitation in schedulePeriod.ShiftCategoryLimitationCollection())
				{
					if (shiftCategoryLimitation.Weekly) continue;

					IList<DateOnly> datesWithCategory;
					if (!_limitationChecker.IsShiftCategoryOverPeriodLimit(shiftCategoryLimitation, period, currentSchedules,
						out datesWithCategory)) continue;

					var errorMessage = Resources.BusinessRuleShiftCategoryLimitationErrorMessage2;
					var message = string.Format(currentUiCulture, errorMessage, shiftCategoryLimitation.ShiftCategory.Description.Name);
					foreach (var dateOnly in datesWithCategory)
					{
						if (!ForDelete)
							responseList.Add(createResponse(schedulePeriod.Person, dateOnly, message));
						oldResponses.Add(createResponse(schedulePeriod.Person, dateOnly, message));
					}
				}
				var newResponseCount = responseList.Count;
				if (newResponseCount <= oldResponseCount)
					responseList = new HashSet<IBusinessRuleResponse>();
			}

			return responseList;
		}

		public string Description => Resources.DescriptionOfNewShiftCategoryLimitationRule;

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message)
		{
			var friendlyName = Resources.BusinessRuleShiftCategoryLimitationFriendlyName;
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), message, HaltModify, IsMandatory,
					period, person, dop, friendlyName)
				{Overridden = !HaltModify};
			return response;
		}
	}
}