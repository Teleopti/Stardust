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

		public TeamScheduleProvider(IPersonScheduleDayReadModelFinder personScheduleDayReadModelRepository, IPermissionProvider permissionProvider, ISchedulePersonProvider schedulePersonProvider)
		{
			_personScheduleDayReadModelRepository = personScheduleDayReadModelRepository;
			_permissionProvider = permissionProvider;
			_schedulePersonProvider = schedulePersonProvider;
		}

		public IEnumerable<Shift> TeamSchedule(Guid teamId, DateTime date)
		{
			var result = new List<Shift>();
			var shifts = getTeamScheduleReadModels(teamId, date);

			var permittedPersonsToViewConfidental =
				_schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId, DefinedRaptorApplicationFunctionPaths.ViewConfidential);
			
			foreach (PersonScheduleDayReadModel shift in shifts)
			{
				var canSeeConfidental = permittedPersonsToViewConfidental.Any(x => x.Id == shift.PersonId);

				var deserializedShift = JsonConvert.DeserializeObject<Shift>(shift.Shift);
				result.Add(deserializedShift);
				if (canSeeConfidental) continue;

				foreach (var layer in deserializedShift.Projection)
				{
					if (layer.IsAbsenceConfidential)
					{
						layer.Color = ColorTranslator.ToHtml(ConfidentialPayloadValues.DisplayColor);
						layer.Title = ConfidentialPayloadValues.Description.Name;
					}

				}
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




