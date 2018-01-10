﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Skill;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Ccc.Web.Areas.SkillGroup;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Data;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class TeamScheduleDataController : ApiController
	{
		private readonly IActivityProvider _activityProvider;
		private readonly IScheduleValidationProvider _validationProvider;
		private readonly IShiftCategoryProvider _shiftCategoryProvider;
		private readonly ITeamsProvider _teamProvider;
		private readonly IToggleManager _toggleManager;
		private readonly SkillViewModelBuilder _skillBuilder;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IOptionalColumnRepository _optionalColumnRepository;
		private readonly SkillGroupViewModelBuilder _skillGroupViewModelBuilder;
		private readonly IDayOffTemplateRepository _dayOffTemplateRepository;

		public TeamScheduleDataController(IActivityProvider activityProvider,
			IScheduleValidationProvider validationProvider,
			IShiftCategoryProvider shiftCategoryProvider,
			ITeamsProvider teamProvider,
			IToggleManager toggleManager,
			SkillViewModelBuilder skillBuilder,
			IScenarioRepository scenarioRepository,
			IOptionalColumnRepository optionalColumnRepository,
			SkillGroupViewModelBuilder skillGroupViewModelBuilder,
			IDayOffTemplateRepository dayOffTemplateRepository)
		{
			_activityProvider = activityProvider;
			_validationProvider = validationProvider;
			_shiftCategoryProvider = shiftCategoryProvider;
			_teamProvider = teamProvider;
			_toggleManager = toggleManager;
			_skillBuilder = skillBuilder;
			_scenarioRepository = scenarioRepository;
			_optionalColumnRepository = optionalColumnRepository;
			_skillGroupViewModelBuilder = skillGroupViewModelBuilder;
			_dayOffTemplateRepository = dayOffTemplateRepository;
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchActivities")]
		public virtual IList<ActivityViewModel> FetchActivities()
		{
			return _activityProvider.GetAll();
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleData/FetchRuleValidationResult")]
		public virtual IList<BusinessRuleValidationResult> FetchRuleValidationResult([FromBody]FetchRuleValidationResultFormData input)
		{
			var ruleFlags = BusinessRuleFlags.None;
			ruleFlags |= BusinessRuleFlags.NewNightlyRestRule;
			ruleFlags |= BusinessRuleFlags.MinWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.NewMaxWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.MinWeeklyRestRule;
			ruleFlags |= BusinessRuleFlags.NewDayOffRule;
			ruleFlags |= BusinessRuleFlags.NotOverwriteLayerRule;

			return _validationProvider.GetBusinessRuleValidationResults(input, ruleFlags);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/FetchAllValidationRules")]
		public virtual IList<string> FetchAllValidationRules()
		{
			var ruleFlags = BusinessRuleFlags.None;
			ruleFlags |= BusinessRuleFlags.NewNightlyRestRule;
			ruleFlags |= BusinessRuleFlags.MinWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.NewMaxWeekWorkTimeRule;
			ruleFlags |= BusinessRuleFlags.MinWeeklyRestRule;
			ruleFlags |= BusinessRuleFlags.NewDayOffRule;
			ruleFlags |= BusinessRuleFlags.NotOverwriteLayerRule;

			return _validationProvider.GetAllValidationRuleTypes(ruleFlags);
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
			return _teamProvider.GetPermittedTeamHierachy(new DateOnly(date), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/GetOrganizationWithPeriod")]
		public virtual BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateTime date)
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_FetchBusinessHierarchyFromReadModel_45275)
				? _teamProvider.GetOrganizationWithPeriod(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)),
					DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
				: _teamProvider.GetOrganizationBasedOnRawData(new DateOnlyPeriod(new DateOnly(date), new DateOnly(date)),
					DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/GetOrganizationWithPeriod")]
		public virtual BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateTime startDate, DateTime endDate)
		{
			return _toggleManager.IsEnabled(Toggles.Wfm_FetchBusinessHierarchyFromReadModel_45275)
				? _teamProvider.GetOrganizationWithPeriod(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					DefinedRaptorApplicationFunctionPaths.MyTeamSchedules)
				: _teamProvider.GetOrganizationBasedOnRawData(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
					DefinedRaptorApplicationFunctionPaths.MyTeamSchedules);
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_skillBuilder.BuildSkillsConnectedWithQueue());
		}
		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/skillgroups")]
		public virtual IHttpActionResult GetSkillGroups()
		{
			return Ok(new SkillGroupInfo
			{
				SkillAreas = _skillGroupViewModelBuilder.GetAll()
			});
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/Scenarios")]
		public virtual IHttpActionResult GetAllScenarios()
		{
			return Ok(_scenarioRepository.FindAllSorted().Select(sc => new { sc.Id, sc.Description.Name }));
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/OptionalColumns")]
		public virtual IHttpActionResult GetAllOptionalColumns()
		{
			return Ok(_optionalColumnRepository.GetOptionalColumns<Person>().Select(oc => new { oc.Id, oc.Name }));
		}

		[UnitOfWork, HttpGet, Route("api/TeamScheduleData/AllDayOffTemplates")]
		public virtual IHttpActionResult GetAllDayOffTemplates()
		{
			return Ok(_dayOffTemplateRepository.FindActivedDayOffsSortByDescription()
				.Select(t => new { t.Id, t.Description.Name }));
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