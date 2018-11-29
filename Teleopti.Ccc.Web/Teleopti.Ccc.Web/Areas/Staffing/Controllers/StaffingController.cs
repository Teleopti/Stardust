using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Staffing;


namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class StaffingController : ApiController
	{
		private readonly AddOverTime _addOverTime;
		private readonly IScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly IForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly IUserTimeZone _timeZone;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ScheduledStaffingViewModelCreator _staffingViewModelCreator;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IAuthorization _authorization;
		private readonly ExportForecastAndStaffingFile _exportForecastAndStaffingFile;
		private readonly ExportStaffingPeriodValidationProvider _periodValidationProvider;
		private readonly BpoProvider _bpoProvider;
		private readonly IAllSkillForSkillGroupProvider _allSkillForSkillGroupProvider;

		public StaffingController(AddOverTime addOverTime, IScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
								  IForecastedStaffingToDataSeries forecastedStaffingToDataSeries, IUserTimeZone timeZone,
								  IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ISkillGroupRepository skillGroupRepository,
								  ScheduledStaffingViewModelCreator staffingViewModelCreator, ICurrentDataSource currentDataSource, 
			IAuthorization authorization,
			ExportForecastAndStaffingFile exportForecastAndStaffingFile, ExportStaffingPeriodValidationProvider periodValidationProvider,
			BpoProvider bpoProvider, IAllSkillForSkillGroupProvider allSkillForSkillGroupProvider)
		{
			_addOverTime = addOverTime;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_timeZone = timeZone;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_skillGroupRepository = skillGroupRepository;
			_staffingViewModelCreator = staffingViewModelCreator;
			_currentDataSource = currentDataSource;
			_authorization = authorization;
			_exportForecastAndStaffingFile = exportForecastAndStaffingFile;
			_periodValidationProvider = periodValidationProvider;
			_bpoProvider = bpoProvider;
			_allSkillForSkillGroupProvider = allSkillForSkillGroupProvider;
		}

		[UnitOfWork, HttpGet, Route("api/staffing/monitorskillareastaffing")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDate(Guid SkillAreaId, DateTime DateTime, bool UseShrinkage)
		{
			var skillArea = _skillGroupRepository.Get(SkillAreaId);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			var model = _staffingViewModelCreator.Load(skillIdList, new DateOnly(DateTime), UseShrinkage);
			model.ImportBpoInfoList = _bpoProvider.ImportInfoOnSkillGroup(SkillAreaId, DateTime); ;
			return Ok(model);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/monitorskillstaffing")]
		public virtual IHttpActionResult MonitorSkillStaffingByDate(Guid SkillId, DateTime DateTime, bool UseShrinkage)
		{
			var model = _staffingViewModelCreator.Load(new[] {SkillId}, new DateOnly(DateTime), UseShrinkage);
			model.ImportBpoInfoList = _bpoProvider.ImportInfoOnSkill(SkillId, DateTime);
			return Ok(model);
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime/suggestion")]
		public virtual IHttpActionResult ShowAddOvertime([FromBody]GetOvertimeSuggestionModel getModel)
		{
			if (getModel == null || getModel.SkillIds.IsEmpty()) return BadRequest();

			var model = new OverTimeSuggestionModel{NumberOfPersonsToTry = getModel.NumberOfPersonsToTry, SkillIds = getModel.SkillIds,TimeSerie = getModel.TimeSerie};

			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();
			model.OvertimePreferences = new OvertimePreferences
			{
				ScheduleTag = new NullScheduleTag(),
				OvertimeType = multiplicationDefinition,
				SelectedTimePeriod = new TimePeriod(TimeSpan.FromMinutes(getModel.OvertimePreferences.MinMinutesToAdd), TimeSpan.FromMinutes(getModel.OvertimePreferences.MaxMinutesToAdd)),
				SelectedSpecificTimePeriod = new TimePeriod(TimeSpan.FromHours(model.TimeSerie.Min().Hour), TimeSpan.FromHours(model.TimeSerie.Max().Hour+1))
			};
			//set in GUI?
			model.NumberOfPersonsToTry = 1000;

			var wraperModel =_addOverTime.GetSuggestion(model);
			var returnModel = extractDataSeries(model, wraperModel);
			if (!wraperModel.Models.Any())
			{
				returnModel.StaffingHasData = false;
			}
			return Ok(returnModel);
		}

		[UnitOfWork, HttpPost, Route("api/staffing/overtime")]
		public virtual IHttpActionResult AddOvertime([FromBody]IList<OverTimeModel> models)
		{
			if (models == null || models.IsEmpty()) return BadRequest();
			var multiplicationDefinition = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions().FirstOrDefault();
			_addOverTime.Apply(models, multiplicationDefinition.Id.GetValueOrDefault());
			return Ok();
		}

		[UnitOfWork, HttpGet, Route("api/staffing/GetCompensations")]
		public virtual IHttpActionResult GetCompensations()
		{
			var multiplicationDefinitions = _multiplicatorDefinitionSetRepository.FindAllOvertimeDefinitions();
			var retList = multiplicationDefinitions.Select(multiplicatorDefinitionSet => new CompensationModel
			{
				Id = multiplicatorDefinitionSet.Id.GetValueOrDefault(),
				Name = multiplicatorDefinitionSet.Name
			}).ToList();

			return Ok(retList);
		}
		
		[UnitOfWork, HttpGet, Route("api/staffing/exportforecastandstaffing")]
		public virtual IHttpActionResult ExportForecastAndStaffing(Guid skillId, string stringExportStartDate, string stringExportEndDate, bool useShrinkage)
		{
			var returnVal = new ExportStaffingReturnObject();

			var validationObject = getDateOnly(stringExportStartDate, out var dateOnlyStartDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			validationObject = getDateOnly(stringExportEndDate, out var dateOnlyEndDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			return Ok(_exportForecastAndStaffingFile.ExportForecastAndStaffing(skillId, dateOnlyStartDate, dateOnlyEndDate, 
				useShrinkage));
		}
		
		[HttpGet, Route("api/staffing/exportStaffingPeriodMessage")]
		public virtual IHttpActionResult GetExportStaffingPeriodMessage()
		{
			return Ok(new {ExportPeriodMessage = _periodValidationProvider.GetExportStaffingPeriodMessageString()});
		}
		
		[HttpGet, Route("api/staffing/exportGapPeriodMessage")]
		public virtual IHttpActionResult GetExportGapPeriodMessage()
		{
			return Ok(new {ExportPeriodMessage = _periodValidationProvider.GetExportGapPeriodMessageString()});
		}

		[UnitOfWork, HttpGet, Route("api/staffing/staffingSettings")]
		public virtual IHttpActionResult StaffingSettingInfo()
		{
			var currentName = _currentDataSource.CurrentName();
			var isLicenseAvailible = DefinedLicenseDataFactory.HasLicense(currentName) &&
									 DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(
										 DefinedLicenseOptionPaths.TeleoptiCccPilotCustomersBpoExchange);
			var returnVal = new returnObj
			{
				isLicenseAvailable = isLicenseAvailible,
				HasPermissionForBpoExchange = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.BpoExchange)
			};
			return Ok(returnVal);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/skills")]
		public virtual IHttpActionResult GetAllSkills()
		{
			return Ok(_allSkillForSkillGroupProvider.AllExceptSubSkills().ToArray());
		}

		private OverTimeSuggestionResultModel extractDataSeries(OverTimeSuggestionModel overTimeSuggestionModel,OvertimeWrapperModel wrapperModels)
		{
			var sourceTimeZone = _timeZone.TimeZone();
			var returnModel = new OverTimeSuggestionResultModel() { StaffingHasData = true, DataSeries = new StaffingDataSeries() };
			returnModel.DataSeries.ScheduledStaffing = _scheduledStaffingToDataSeries.DataSeries(
				wrapperModels.ResourceCalculationPeriods.Select(x => new SkillStaffingIntervalLightModel()
				{
					StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, sourceTimeZone),
					EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, sourceTimeZone),
					StaffingLevel = x.StaffingLevel
				}).ToList(), overTimeSuggestionModel.TimeSerie);

			returnModel.DataSeries.ForecastedStaffing = _forecastedStaffingToDataSeries.DataSeries(
				wrapperModels.ResourceCalculationPeriods.Select(x => new StaffingIntervalModel()
				{
					StartTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, sourceTimeZone),
					SkillId = x.SkillId,
					Agents = x.FStaff
				}).ToList(), overTimeSuggestionModel.TimeSerie);

			returnModel.DataSeries.Time = overTimeSuggestionModel.TimeSerie;
			var modelInUtc = wrapperModels.ResourceCalculationPeriods;
			modelInUtc.ForEach(x =>
			{
				x.StartDateTime = TimeZoneHelper.ConvertFromUtc(x.StartDateTime, sourceTimeZone);
				x.EndDateTime = TimeZoneHelper.ConvertFromUtc(x.EndDateTime, sourceTimeZone);
			});
			calculateAbsoluteDifference(returnModel.DataSeries);
			returnModel.OverTimeModels = wrapperModels.Models;
			return returnModel;
		}

		private void calculateAbsoluteDifference(StaffingDataSeries dataSeries)
		{
			dataSeries.AbsoluteDifference = new double?[dataSeries.ForecastedStaffing.Length];
			for (var index = 0; index < dataSeries.ForecastedStaffing.Length; index++)
			{
				if (dataSeries.ForecastedStaffing[index].HasValue)
				{
					if (dataSeries.ScheduledStaffing.Length == 0)
					{
						dataSeries.AbsoluteDifference[index] = -dataSeries.ForecastedStaffing[index];
						continue;
					}

					if (dataSeries.ScheduledStaffing[index].HasValue) {
						dataSeries.AbsoluteDifference[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1) -
															   Math.Round((double)dataSeries.ForecastedStaffing[index], 1);
						dataSeries.AbsoluteDifference[index] = Math.Round((double)dataSeries.AbsoluteDifference[index], 1);
						dataSeries.ScheduledStaffing[index] = Math.Round((double)dataSeries.ScheduledStaffing[index], 1);
					}
					dataSeries.ForecastedStaffing[index] = Math.Round((double)dataSeries.ForecastedStaffing[index], 1);
				}

			}
		}

		private ExportStaffingValidationObject getDateOnly(string dateTimeString, out DateOnly dateOnly)
		{
			var validationObject = new ExportStaffingValidationObject { Result = true };
			var dateFormatter = "yyyy-MM-dd";
			dateOnly = DateOnly.MinValue;
			if (DateTime.TryParseExact(dateTimeString, dateFormatter, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
			{
				dateOnly = new DateOnly(dateTime);
				return validationObject;
			}
			validationObject.ErrorMessage = $"The date formart is invalid {dateTimeString}";
			validationObject.Result = false;
			return validationObject;
		}

		class returnObj
		{
			public bool isLicenseAvailable { get; set; }
			public bool HasPermissionForBpoExchange { get; set; }
		}
	}
}
