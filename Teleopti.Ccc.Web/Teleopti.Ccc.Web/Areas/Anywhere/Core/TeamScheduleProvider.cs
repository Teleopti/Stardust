using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.PersonScheduleDayReadModel;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public class TeamScheduleProvider : ITeamScheduleProvider
	{
		private readonly IPersonScheduleDayReadModelFinder _personScheduleDayReadModelRepository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleProvider(IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider, ILoggedOnUser loggedOnUser)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
			_loggedOnUser = loggedOnUser;
		}

		public IEnumerable<Shift> TeamSchedule(Guid teamId, DateTime date)
		{
			var result = new List<Shift>();
			var shifts = getTeamScheduleReadModels(teamId, date);

			var schedulesToShowConfidental =
				_schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId, DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			
			foreach (PersonScheduleDayReadModel shift in shifts)
			{
				var canSeeConfidental = (schedulesToShowConfidental.SingleOrDefault(x => x.Id == shift.PersonId)) != null;

				var shift0 = JsonConvert.DeserializeObject<Shift>(shift.Shift);

				foreach (var layer in shift0.Projection)
				{
					if (!canSeeConfidental && layer.IsAbsenceConfidential)
					{
						layer.Color = ColorTranslator.ToHtml(ConfidentialPayloadValues.DisplayColor);
						layer.Title = ConfidentialPayloadValues.Description.Name;
					}

				}
				result.Add(shift0);

			}
			return result;
		}

		private IEnumerable<PersonScheduleDayReadModel> getTeamScheduleReadModels(Guid teamId, DateTime date)
 		{
			date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(date, date.AddHours(25));

			var schedules = _personScheduleDayReadModelRepository.ForTeam(dateTimePeriod, teamId);

			if (schedules != null)
			{
				var scheduleList = new List<PersonScheduleDayReadModel>(schedules);

				if (_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ViewUnpublishedSchedules))
					return scheduleList;
				var published = new PublishedScheduleSpecification(_schedulePersonProvider, teamId, date);
				return scheduleList.FindAll(published.IsSatisfiedBy);


			}
			return new List<PersonScheduleDayReadModel>();
		}
	}
}




