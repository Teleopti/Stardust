﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            //var date = DateTime.Now.Date;
            //if (dateOnlyPeriod.StartDate > date.AddDays(14))
            //    return ret;
            //if (dateOnlyPeriod.EndDate < date)
            //    return ret;

            var period = new DateOnlyPeriod(new DateOnly(DateTime.Now), new DateOnly(DateTime.Now.AddDays(14)));
		    var overlappingPeriod = dateOnlyPeriod.Intersection(period);
            

            if (overlappingPeriod==null)
                return ret;

            IList<ScheduleDayReadModel> newReadModelWithinIntersectionPeriod = new List<ScheduleDayReadModel>();
            
            newReadModelWithinIntersectionPeriod =
                    newReadModels.Where(y => y.Date >= overlappingPeriod.Value.StartDate && y.Date <= overlappingPeriod.Value.EndDate).ToList();


            var oldReadModels = _scheduleDayReadModelRepository.ReadModelsOnPerson(overlappingPeriod.Value.StartDate,
                                                                                   overlappingPeriod.Value.EndDate, person.Id.Value);
		    
            var lang = person.PermissionInformation.UICulture();
			
            foreach (var dateOnly in dateOnlyPeriod.DayCollection())
			{
			    string message = null;
                var newReadModelDay = newReadModelWithinIntersectionPeriod.FirstOrDefault(x => x.Date == dateOnly);
			    var existingReadModelDay = oldReadModels.FirstOrDefault(y => y.Date == dateOnly);

                message = _scheduleDayReadModelComparer.FindSignificantChanges(newReadModelDay, existingReadModelDay, lang, dateOnly);
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