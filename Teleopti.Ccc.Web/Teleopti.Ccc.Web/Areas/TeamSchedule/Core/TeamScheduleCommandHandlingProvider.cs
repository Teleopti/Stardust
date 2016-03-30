using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public class TeamScheduleCommandHandlingProvider : ITeamScheduleCommandHandlingProvider
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamScheduleCommandHandlingProvider(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		public void AddActivity(AddActivityFormData formData)
		{
			foreach (var person in formData.PersonIds)
			{
				var command = new AddActivityCommand
				{
					PersonId = person,
					ActivityId = formData.ActivityId,
					Date = formData.BelongsToDate,
					StartTime = formData.StartTime,
					EndTime = formData.EndTime,
					TrackedCommandInfo = formData.TrackedCommandInfo != null ? formData.TrackedCommandInfo : new TrackedCommandInfo { OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value }
				};
				_commandDispatcher.Execute(command);
			}
		}

	}

	public interface ITeamScheduleCommandHandlingProvider
	{
		void AddActivity(AddActivityFormData formData);
	}
}