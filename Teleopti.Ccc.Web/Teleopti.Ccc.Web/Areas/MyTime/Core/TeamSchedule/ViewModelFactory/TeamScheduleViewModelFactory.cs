using System;
using AutoMapper;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly IMappingEngine _mapper;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IBadgeSettingProvider _badgeSettingProvider;

		public TeamScheduleViewModelFactory(IMappingEngine mapper, IPermissionProvider permissionProvider, IToggleManager toggleManager, IBadgeSettingProvider badgeSettingProvider)
		{
			_mapper = mapper;
			_permissionProvider = permissionProvider;
			_toggleManager = toggleManager;
			_badgeSettingProvider = badgeSettingProvider;
		}

		public TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id)
		{
			var domainData = _mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(date, id));
			var viewmodel = _mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(domainData);
			viewmodel.ShiftTradePermisssion =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			viewmodel.AgentBadgeEnabled = _toggleManager.IsEnabled(Toggles.MyTimeWeb_AgentBadge_28913) && _badgeSettingProvider.GetBadgeSettings().EnableBadge;
			return viewmodel;
		}
	}
}