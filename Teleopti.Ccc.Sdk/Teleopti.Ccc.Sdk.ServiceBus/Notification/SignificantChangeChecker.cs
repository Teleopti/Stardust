﻿using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel;
using Teleopti.Ccc.Sdk.Common.Contracts;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
	public class SignificantChangeChecker : ISignificantChangeChecker
	{
		private readonly IScheduleDayReadModelRepository _scheduleDayReadModelRepository;
		private readonly IScheduleDayReadModelComparer _scheduleDayReadModelComparer;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelHandler));

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
			if (wfc == null)
			{
				Logger.Info("No notification will be sent, no WorkflowControlSet on person " + person.Name);
				return ret;
			}

			if (!wfc.SchedulePublishedToDate.HasValue)
			{
				Logger.Info("No notification will be sent, the schedule has no Published To on the WorkflowControlSet");
				return ret;
			}

			DateTime? publishedToDate = wfc.SchedulePublishedToDate;

			if (publishedToDate.Value < DateOnly.Today)
			{
				Logger.Info("No notification will be sent, the schedule is only Published to " + publishedToDate.Value);
				return ret;
			}

			if (publishedToDate.Value < endDate)
			endDate = new DateOnly(publishedToDate.Value);

			var period = new DateOnlyPeriod(DateOnly.Today, endDate);
			if (!period.Contains(date))
			{
				Logger.Info("No notification will be sent, the schedule is changed on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " and it is not in the period " + period.DateString);
				return ret;
			}

			var oldReadModels = _scheduleDayReadModelRepository.ReadModelsOnPerson(date, date, person.Id.GetValueOrDefault());

			var existingReadModelDay = oldReadModels.FirstOrDefault();

			string message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModel, existingReadModelDay, lang, date);
			if (!string.IsNullOrEmpty(message))
				ret.Messages.Add(message);

			if (ret.Messages.Count == 0)
			{
				Logger.Info("No notification will be sent,  did not find a significant change on " + date.ToShortDateString(CultureInfo.InvariantCulture) + " for " + person.Name);
				return ret;
			}

			ret.Subject = UserTexts.Resources.ResourceManager.GetString("YourWorkingHoursHaveChanged", lang);
			return ret;
		}
	}
}