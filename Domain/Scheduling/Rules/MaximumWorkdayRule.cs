using System;
using System.Collections.Generic;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class MaximumWorkdayRule: INewBusinessRule
	{
		public bool IsMandatory => false;
		public bool HaltModify { get; set; } = true;
		public bool Configurable => true;
		public bool ForDelete { get; set; }
		public string Description => Resources.DescriptionOfMaximumWorkdayRule;

		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var currentUiCulture = Thread.CurrentThread.CurrentUICulture;
			var responseList = new HashSet<IBusinessRuleResponse>();
			
			foreach (var scheduleDay in scheduleDays)
			{
				var person = scheduleDay.Person;
				if(person.WorkflowControlSet==null) break;
				var maxConsecutiveWorkday = person.WorkflowControlSet.MaximumConsecutiveWorkingDays;
				var scheduleRange = rangeClones[person];
				var startDate = scheduleDay.DateOnlyAsPeriod.DateOnly;
				var scheduleDaysAround = scheduleRange.ScheduledDayCollection(new DateOnlyPeriod(startDate.AddDays(-maxConsecutiveWorkday), startDate.AddDays(maxConsecutiveWorkday)));

				var workdayCount = 0;
				foreach (var scheduleDayAround in scheduleDaysAround)
				{
					if (!scheduleDayAround.HasDayOff()) workdayCount++;
					else workdayCount = 0;

					if (workdayCount > maxConsecutiveWorkday)
					{
						var message = string.Format(currentUiCulture, Resources.BusinessRuleMaximumWorkdayErrorMessage, person.Name, workdayCount, maxConsecutiveWorkday);
						responseList.Add(createResponse(person, startDate, message, typeof(MaximumWorkdayRule)));
						break;
					}
				}
			}

			return responseList;
		}

		private IBusinessRuleResponse createResponse(IPerson person, DateOnly dateOnly, string message, Type type)
		{
			var dop = dateOnly.ToDateOnlyPeriod();
			var period = dop.ToDateTimePeriod(person.PermissionInformation.DefaultTimeZone());
			var response = new BusinessRuleResponse(type, message, HaltModify, IsMandatory, period, person, dop,
					Resources.BusinessRuleMaximumWorkdayFriendlyName)
				{ Overridden = !HaltModify };
			return response;
		}
	}
}
