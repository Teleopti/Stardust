using System;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamScheduleViewModelFactory : ITeamScheduleViewModelFactory
	{
		private readonly TeamScheduleDomainDataMapper _mapper;
		private readonly TeamScheduleViewModelMapper _viewModelMapper;
		private readonly IPermissionProvider _permissionProvider;

		public TeamScheduleViewModelFactory(IPermissionProvider permissionProvider, TeamScheduleViewModelMapper viewModelMapper, TeamScheduleDomainDataMapper mapper)
		{
			_permissionProvider = permissionProvider;
			_viewModelMapper = viewModelMapper;
			_mapper = mapper;
		}

		public TeamScheduleViewModel CreateViewModel(DateOnly date, Guid id)
		{
			var domainData = _mapper.Map(date, id);
			var viewmodel = _viewModelMapper.Map(domainData);
			viewmodel.ShiftTradePermisssion =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb);
			viewmodel.ShiftTradeBulletinBoardPermission =
				_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard);
			return viewmodel;
		}

		public TeamScheduleViewModel CreateViewModel()
		{
			return new TeamScheduleViewModel
			{
				ShiftTradePermisssion =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				ShiftTradeBulletinBoardPermission =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard)
			};
		}
	}
}