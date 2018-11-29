using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.StudentAvailability;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.StudentAvailability.Mapping
{
	public class StudentAvailabilityDayFeedbackViewModelMapper
	{
		private readonly IStudentAvailabilityFeedbackProvider _studentAvailabilityFeedbackProvider;
		
		public StudentAvailabilityDayFeedbackViewModelMapper(IStudentAvailabilityFeedbackProvider studentAvailabilityFeedbackProvider)
		{
			_studentAvailabilityFeedbackProvider = studentAvailabilityFeedbackProvider;
		}

		public StudentAvailabilityDayFeedbackViewModel Map(DateOnly s)
		{
			var model = new StudentAvailabilityDayFeedbackViewModel {Date = s.ToFixedClientDateOnlyFormat()};

			var result = _studentAvailabilityFeedbackProvider.WorkTimeMinMaxForDate(s) ?? new WorkTimeMinMaxCalculationResult();
			if (result.WorkTimeMinMax == null)
			{
				if (result.RestrictionNeverHadThePossibilityToMatchWithShifts)
				{
					model.FeedbackError = "";
					return model;
				}

				model.FeedbackError = Resources.NoAvailableShifts;
				return model;
			}
			
			model.PossibleStartTimes = result.WorkTimeMinMax.StartTimeLimitation.StartTimeString.ToLower() + "-" +
									   result.WorkTimeMinMax.StartTimeLimitation.EndTimeString.ToLower();
			model.PossibleEndTimes = result.WorkTimeMinMax.EndTimeLimitation.StartTimeString.ToLower() + "-" +
									 result.WorkTimeMinMax.EndTimeLimitation.EndTimeString.ToLower();
			model.PossibleContractTimeMinutesLower = result.WorkTimeMinMax.WorkTimeLimitation.StartTime?.TotalMinutes.ToString();
			model.PossibleContractTimeMinutesUpper = result.WorkTimeMinMax.WorkTimeLimitation.EndTime?.TotalMinutes.ToString();
			return model;
		}
	}
}