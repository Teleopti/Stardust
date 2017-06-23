using System;
using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class TeamScheduleDataController : ApiController
	{
		private readonly IActivityProvider _teamScheduleDataProvider;
		private readonly IScheduleValidationProvider _validationProvider;
		private readonly IShiftCategoryProvider _shiftCategoryProvider;
		private readonly IToggleManager _toggleManager;
		private readonly IMultiplicatorDefinitionSetProvider _multiplicatorDefinitionSetProvider;
		private readonly ITeamsProvider _teamProvider;

		public TeamScheduleDataController(IActivityProvider teamScheduleDataProvider, IScheduleValidationProvider validationProvider, IShiftCategoryProvider shiftCategoryProvider, IToggleManager toggleManager, IMultiplicatorDefinitionSetProvider multiplicatorDefinitionSetProvider, ITeamsProvider teamProvider)
		{
			_teamScheduleDataProvider = teamScheduleDataProvider;
			_validationProvider = validationProvider;
			_shiftCategoryProvider = shiftCategoryProvider;
			_toggleManager = toggleManager;
			_multiplicatorDefinitionSetProvider = multiplicatorDefinitionSetProvider;
			_teamProvider = teamProvider;
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchActivities")]
		public virtual IList<ActivityViewModel> FetchActivities()
		{
			return _teamScheduleDataProvider.GetAll();
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/FetchRuleValidationResult")]
		public virtual IList<BusinessRuleValidationResult> FetchRuleValidationResult([FromBody]FetchRuleValidationResultFormData input)
		{
			var ruleFlags = BusinessRuleFlags.None;
			ruleFlags |= BusinessRuleFlags.NewNightlyRestRule;
			ruleFlags |= BusinessRuleFlags.MinWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.NewMaxWeekWorkTimeRule;
			if (_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800))
			{
				ruleFlags |= BusinessRuleFlags.MinWeeklyRestRule;
			}
			if (_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowDayOffWarning_39801))
			{
				ruleFlags |= BusinessRuleFlags.NewDayOffRule;
			}
			if (_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109))
			{
				ruleFlags |= BusinessRuleFlags.NotOverwriteLayerRule;
			}

			return _validationProvider.GetBusinessRuleValidationResults(input, ruleFlags);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchAllValidationRules")]
		public virtual IList<string> FetchAllValidationRules()
		{
			var ruleFlags = BusinessRuleFlags.None;
			ruleFlags |= BusinessRuleFlags.NewNightlyRestRule;
			ruleFlags |= BusinessRuleFlags.MinWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.NewMaxWeekWorkTimeRule;
			if(_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowWeeklyRestTimeWarning_39800))
			{
				ruleFlags |= BusinessRuleFlags.MinWeeklyRestRule;
			}
			if(_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowDayOffWarning_39801))
			{
				ruleFlags |= BusinessRuleFlags.NewDayOffRule;
			}
			if(_toggleManager.IsEnabled(Toggles.WfmTeamSchedule_ShowOverwrittenLayerWarning_40109))
			{
				ruleFlags |= BusinessRuleFlags.NotOverwriteLayerRule;
			}

			return _validationProvider.GetAllValidationRuleTypes(ruleFlags);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchMultiplicationDefinitionSets")]
		public virtual IList<MultiplicatorDefinitionSetViewModel> FetchMultiplicationDefinitionSets()
		{
			return _multiplicatorDefinitionSetProvider.GetAll();
		}


		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchShiftCategories")]
		public virtual IList<ShiftCategoryViewModel> FetchShiftCategories()
		{
			return _shiftCategoryProvider.GetAll();
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/CheckOverlapppingCertainActivities")]
		public virtual IList<ActivityLayerOverlapCheckingResult> CheckOverlapppingCertainActivities(CheckActivityLayerOverlapFormData input)
		{
			return _validationProvider.GetActivityLayerOverlapCheckingResult(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/CheckPersonAccounts")]
		public virtual IList<CheckingResult> CheckPersonAccounts(CheckPersonAccountFormData input)
		{
			return _validationProvider.CheckPersonAccounts(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/CheckMoveActivityOverlapppingCertainActivities")]
		public virtual IList<ActivityLayerOverlapCheckingResult> CheckMoveActivityOverlapppingCertainActivities(CheckMoveActivityLayerOverlapFormData input)
		{
			return _validationProvider.GetMoveActivityLayerOverlapCheckingResult(input);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchPermittedTeamHierachy")]
		public virtual BusinessUnitWithSitesViewModel FetchPermittedTeamHierachy(DateTime date)
		{
			return  _teamProvider.GetPermittedTeamHierachy(new DateOnly(date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/GetOrganizationWithPeriod")]
		public virtual BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateTime date)
		{
			return _teamProvider.GetOrganizationWithPeriod(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)),
				DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);

		}
	}

	public class CheckPersonAccountFormData
	{
		public IEnumerable<Guid> PersonIds { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public bool IsFullDay { get; set; }
	}
}