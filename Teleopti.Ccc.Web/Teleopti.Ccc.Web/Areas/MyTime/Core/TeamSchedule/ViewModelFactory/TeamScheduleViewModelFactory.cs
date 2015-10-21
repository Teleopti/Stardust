using System;
using AutoMapper;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global.Core;
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

		public TeamScheduleViewModelFactory(IMappingEngine mapper, IPermissionProvider permissionProvider)
		{
			_mapper = mapper;
			_permissionProvider = permissionProvider;
		}

		public TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id)
		{
			var domainData = _mapper.Map<Tuple<DateOnly, Guid>, TeamScheduleDomainData>(new Tuple<DateOnly, Guid>(date, id));
			var viewmodel = _mapper.Map<TeamScheduleDomainData, TeamScheduleViewModel>(domainData);
			viewmodel.ShiftTradePermisssion =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			viewmodel.ShiftTradeBulletinBoardPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			return viewmodel;
		}

		public TeamScheduleViewModel CreateViewModel()
		{
			return new TeamScheduleViewModel()
			{
				ShiftTradePermisssion =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				ShiftTradeBulletinBoardPermission =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard)
			};
		}

	}
}