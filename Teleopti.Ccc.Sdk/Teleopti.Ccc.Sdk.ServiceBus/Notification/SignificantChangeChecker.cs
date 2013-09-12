using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface ISignificantChangeChecker
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		INotificationMessage SignificantChangeNotificationMessage(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel);
	}

	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IScheduleDayReadModelComparer _scheduleDayReadModelComparer;

		public SignificantChangeChecker(IScheduleDayReadModelRepository scheduleDayReadModelRepository, IScheduleDayReadModelComparer scheduleDayReadModelComparer)
		{
			_scheduleDayReadModelRepository = scheduleDayReadModelRepository;
			_scheduleDayReadModelComparer = scheduleDayReadModelComparer;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public INotificationMessage SignificantChangeNotificationMessage(DateOnly date, IPerson person, ScheduleDayReadModel newReadModel)
		{
			var ret = new NotificationMessage();
			var lang = person.PermissionInformation.UICulture();
			var endDate = DateOnly.Today.AddDays(14);

			var wfc = person.WorkflowControlSet;
            if (wfc != null && !wfc.SchedulePublishedToDate.HasValue)
                return ret;

			if (wfc != null && wfc.SchedulePublishedToDate.HasValue && wfc.SchedulePublishedToDate.Value < DateOnly.Today)
				return ret;

			if (wfc != null && wfc.SchedulePublishedToDate.HasValue && wfc.SchedulePublishedToDate.Value < endDate)
				endDate = new DateOnly(wfc.SchedulePublishedToDate.Value.Date);

			var period = new DateOnlyPeriod(DateOnly.Today, endDate);
			if (!period.Contains(date)) return ret;


			var oldReadModels = _scheduleDayReadModelRepository.ReadModelsOnPerson(date, date, person.Id.GetValueOrDefault());

			var existingReadModelDay = oldReadModels.FirstOrDefault();

			string message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModelDay, lang, date);
			if (!string.IsNullOrEmpty(message))
				ret.Messages.Add(message);

			if (ret.Messages.Count == 0)
				return ret;

			var subject = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged", lang);
			if (string.IsNullOrEmpty(subject))
				subject = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged");

			ret.Subject = subject;
			return ret;
		}
	}
}