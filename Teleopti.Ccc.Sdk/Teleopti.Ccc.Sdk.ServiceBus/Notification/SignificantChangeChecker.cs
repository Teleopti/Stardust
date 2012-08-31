using System;
using System.Collections.Generic;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public interface ISignificantChangeChecker
	{
		INotificationMessage SignificantChangeNotificationMessage(DateOnlyPeriod dateOnlyPeriod, IPerson person, IList<ScheduleDayReadModel> newReadModels);
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
		public INotificationMessage SignificantChangeNotificationMessage(DateOnlyPeriod dateOnlyPeriod, IPerson person, IList<ScheduleDayReadModel> newReadModels)
		{
			var ret = new NotificationMessage();
			var date = DateTime.Now.Date;
			if (dateOnlyPeriod.StartDate > date.AddDays(14))
				return ret;
			if (dateOnlyPeriod.EndDate < date)
				return ret;

			var oldReadModels = _scheduleDayReadModelRepository.ReadModelsOnPerson(dateOnlyPeriod.StartDate,
			                                                                       dateOnlyPeriod.EndDate, person.Id.Value);

			var lang = person.PermissionInformation.UICulture();
			foreach (var dateOnly in dateOnlyPeriod.DayCollection())
			{
				//TALHA find the day in both list and add some test to testclass of course
				ScheduleDayReadModel newModel = null;
				ScheduleDayReadModel oldModel = null;

				var message = _scheduleDayReadModelComparer.FindSignificantChanges(newModel, oldModel, lang);
				if(!string.IsNullOrEmpty(message))
					ret.Messages.Add(message);

			}

			if (ret.Messages.Count == 0)
				return ret;

			var subject = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged",lang);
			if (string.IsNullOrEmpty(subject))
				subject = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged");

			ret.Subject = subject;
			return ret;
		}
	}
}