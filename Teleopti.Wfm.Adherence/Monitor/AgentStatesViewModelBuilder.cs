using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Monitor
{
	public class AgentStatesViewModel
	{
		public DateTime Time { get; set; }
		public IEnumerable<AgentStateViewModel> States { get; set; }
	}

	public class AgentStateViewModel
	{
		public Guid PersonId { get; set; }
		public string Name { get; set; }

		public string TeamId { get; set; }
		public string TeamName { get; set; }
		public string SiteId { get; set; }
		public string SiteName { get; set; }

		public Guid? StateId { get; set; }
		public string State { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public string NextActivityStartTime { get; set; }
		public string Rule { get; set; }
		public string Color { get; set; }
		public int TimeInState { get; set; }
		public int? TimeInRule { get; set; }
		public int? TimeInAlarm { get; set; }
		public IEnumerable<AgentStateActivityViewModel> Shift { get; set; }
		public IEnumerable<AgentOutOfAdherenceViewModel> OutOfAdherences { get; set; }
	}

	public class AgentStateActivityViewModel
	{
		public string Color { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public string Name { get; set; }
	}

	public class AgentOutOfAdherenceViewModel
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}

	public class AgentStateFilter
	{
		public IEnumerable<Guid> SiteIds { get; set; } // include
		public IEnumerable<Guid> TeamIds { get; set; } // include

		public IEnumerable<Guid> SkillIds { get; set; } // filter
		public IEnumerable<Guid?> ExcludedStateIds { get; set; } // filter
		public string TextFilter { get; set; } // filter

		public bool InAlarm { get; set; } // filter and order

		public string OrderBy { get; set; }
		public string Direction { get; set; }
	}

	public class AgentStatesViewModelBuilder
	{
		private readonly INow _now;
		private readonly IUserTimeZone _timeZone;
		private readonly IUserCulture _culture;
		private readonly ProperAlarm _appliedAlarm;
		private readonly IAgentStateReadModelReader _reader;
		private readonly ICommonAgentNameProvider _nameDisplaySetting;
		private readonly ICurrentAuthorization _authorization;
		private readonly IUserNow _userNow;
		private readonly ILoggedOnUser _user;

		public AgentStatesViewModelBuilder(
			INow now,
			IUserTimeZone timeZone,
			IUserCulture culture,
			ProperAlarm appliedAlarm,
			IAgentStateReadModelReader reader,
			ICommonAgentNameProvider nameDisplaySetting,
			ICurrentAuthorization authorization,
			IUserNow userNow,
			ILoggedOnUser user)
		{
			_now = now;
			_timeZone = timeZone;
			_culture = culture;
			_appliedAlarm = appliedAlarm;
			_reader = reader;
			_nameDisplaySetting = nameDisplaySetting;
			_authorization = authorization;
			_userNow = userNow;
			_user = user;
		}

		public AgentStatesViewModel Build(AgentStateFilter filter)
		{
			if (filter.SiteIds.EmptyIfNull().IsEmpty() && filter.TeamIds.EmptyIfNull().IsEmpty())
			{
				var userRoles = _user.CurrentUser().PermissionInformation.ApplicationRoleCollection.EmptyIfNull();
				var team = _user.CurrentUser().Period(_userNow.Date())?.Team;

				if (team != null)
				{
					if (userRoles
						.All(x => x.AvailableData.AvailableDataRange == AvailableDataRangeOption.MySite))
						filter.SiteIds = new[] {team.Site.Id.Value};

					if (userRoles
						.All(x => x.AvailableData.AvailableDataRange == AvailableDataRangeOption.MyTeam))
						filter.TeamIds = new[] {team.Id.Value};
				}
			}

			return new AgentStatesViewModel
			{
				Time = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()),
				States = buildStates(_reader.Read(filter))
			};
		}

		private IEnumerable<AgentStateViewModel> buildStates(IEnumerable<AgentStateReadModel> states)
		{
			var nameDisplayedAs = _nameDisplaySetting.CommonAgentNameSettings;
			var auth = _authorization.Current();
			return from state in states
				where
					auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _userNow.Date(), new SiteAuthorization {BusinessUnitId = state.BusinessUnitId.GetValueOrDefault(), SiteId = state.SiteId.GetValueOrDefault()}) ||
					auth.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview, _userNow.Date(), new TeamAuthorization {BusinessUnitId = state.BusinessUnitId.GetValueOrDefault(), SiteId = state.SiteId.GetValueOrDefault(), TeamId = state.TeamId.GetValueOrDefault()})
				let timeInAlarm = calculateTimeInAlarm(state)
				select new AgentStateViewModel
				{
					PersonId = state.PersonId,
					Name = nameDisplayedAs.BuildFor(state.FirstName, state.LastName, state.EmploymentNumber),
					SiteId = state.SiteId.ToString(),
					SiteName = state.SiteName,
					TeamId = state.TeamId.ToString(),
					TeamName = state.TeamName,
					State = state.StateName,
					StateId = state.StateGroupId,
					Activity = state.Activity,
					NextActivity = state.NextActivity,
					NextActivityStartTime = formatTime(state.NextActivityStartTime),
					Rule = state.RuleName,
					Color = _appliedAlarm.ColorTransition(state, timeInAlarm),
					TimeInState = state.StateStartTime.HasValue ? (int) (_now.UtcDateTime() - state.StateStartTime.Value).TotalSeconds : 0,
					TimeInAlarm = timeInAlarm,
					TimeInRule = state.RuleStartTime.HasValue ? (int?) (_now.UtcDateTime() - state.RuleStartTime.Value).TotalSeconds : null,
					Shift = state.Shift?.Select(y => new AgentStateActivityViewModel
					{
						Color = ColorTranslator.ToHtml(Color.FromArgb(y.Color)),
						StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH\\:mm\\:ss"),
						EndTime = TimeZoneHelper.ConvertFromUtc(y.EndTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH\\:mm\\:ss"),
						Name = y.Name
					}),
					OutOfAdherences = state.OutOfAdherences?.Select(y =>
					{
						string endTime = null;
						if (y.EndTime.HasValue)
							endTime = TimeZoneHelper.ConvertFromUtc(y.EndTime.Value, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH\\:mm\\:ss");
						return new AgentOutOfAdherenceViewModel
						{
							StartTime = TimeZoneHelper.ConvertFromUtc(y.StartTime, _timeZone.TimeZone()).ToString("yyyy-MM-ddTHH\\:mm\\:ss"),
							EndTime = endTime
						};
					})
				};
		}

		private int? calculateTimeInAlarm(AgentStateReadModel x)
		{
			if (x.AlarmStartTime.HasValue)
			{
				return _now.UtcDateTime() >= x.AlarmStartTime.Value ? (int?) (_now.UtcDateTime() - x.AlarmStartTime.Value).TotalSeconds : null;
			}

			return null;
		}

		private string formatTime(DateTime? timestamp)
		{
			if (!timestamp.HasValue) return null;
			var userTime = TimeZoneInfo.ConvertTimeFromUtc(timestamp.Value, _timeZone.TimeZone());
			var today = TimeZoneInfo.ConvertTimeFromUtc(_now.UtcDateTime(), _timeZone.TimeZone()).Date;

			return userTime < today.AddDays(1)
					? userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
					: userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortDatePattern) + " " +
					  userTime.ToString(_culture.GetCulture().DateTimeFormat.ShortTimePattern)
				;
		}
	}
}