using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class StaffingController : ApiController
	{
		private readonly AddOverTime _addOverTime;
		private readonly ScheduledStaffingToDataSeries _scheduledStaffingToDataSeries;
		private readonly ForecastedStaffingToDataSeries _forecastedStaffingToDataSeries;
		private readonly IUserTimeZone _timeZone;
		private readonly IMultiplicatorDefinitionSetRepository _multiplicatorDefinitionSetRepository;
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ScheduledStaffingViewModelCreator _staffingViewModelCreator;
		private readonly ImportBpoFile _bpoFile;
		private readonly IExportBpoFile _exportBpoFile;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly ISkillRepository _skillRepository;
		private readonly IAuthorization _authorization;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly INow _now;
		private readonly IUserUiCulture _userUiCulture;

		public StaffingController(AddOverTime addOverTime, ScheduledStaffingToDataSeries scheduledStaffingToDataSeries,
								  ForecastedStaffingToDataSeries forecastedStaffingToDataSeries, IUserTimeZone timeZone,
								  IMultiplicatorDefinitionSetRepository multiplicatorDefinitionSetRepository, ISkillGroupRepository skillGroupRepository,
								  ScheduledStaffingViewModelCreator staffingViewModelCreator, ImportBpoFile bpoFile, ICurrentDataSource currentDataSource, 
								  IExportBpoFile exportBpoFile, ISkillRepository skillRepository, IAuthorization authorization,
								  IStaffingSettingsReader staffingSettingsReader, INow now, IUserUiCulture userUiCulture)
		{
			_addOverTime = addOverTime;
			_scheduledStaffingToDataSeries = scheduledStaffingToDataSeries;
			_forecastedStaffingToDataSeries = forecastedStaffingToDataSeries;
			_timeZone = timeZone;
			_multiplicatorDefinitionSetRepository = multiplicatorDefinitionSetRepository;
			_skillGroupRepository = skillGroupRepository;
			_staffingViewModelCreator = staffingViewModelCreator;
			_bpoFile = bpoFile;
			_currentDataSource = currentDataSource;
			_exportBpoFile = exportBpoFile;
			_skillRepository = skillRepository;
			_authorization = authorization;
			_staffingSettingsReader = staffingSettingsReader;
			_now = now;
			_userUiCulture = userUiCulture;
		}

		[UnitOfWork, HttpGet, Route("api/staffing/monitorskillareastaffing")]
		public virtual IHttpActionResult MonitorSkillAreaStaffingByDate(Guid SkillAreaId, DateTime DateTime, bool UseShrinkage)
		{
			var skillArea = _skillGroupRepository.Get(SkillAreaId);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			return Ok(_staffingViewModelCreator.Load(skillIdList, new DateOnly(DateTime), UseShrinkage));
		}

		[UnitOfWork, HttpGet, Route("api/staffing/monitorskillstaffing")]
		public virtual IHttpActionResult MonitorSkillStaffingByDate(Guid SkillId, DateTime DateTime, bool UseShrinkage)
		{
			return Ok(_staffingViewModelCreator.Load(new[] { SkillId }, new DateOnly(DateTime), UseShrinkage));
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

		[UnitOfWork, HttpPost, Route("api/staffing/importBpo")]
		public virtual IHttpActionResult ImportBpo([FromBody]string fileContents)
		{
			var result = _bpoFile.ImportFile(fileContents, CultureInfo.InvariantCulture);
			return Ok(result);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/exportStaffingDemand")]
		public virtual IHttpActionResult ExportBpo(Guid skillId, DateTime exportStartDateTime, DateTime exportEndDateTime)
		{
			var returnVal = new exportReturnObject();
			var exportStartDate = new DateOnly(exportStartDateTime);
			var exportEndDate = new DateOnly(exportEndDateTime);
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14);

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);
			if (exportStartDate > exportEndDate)
			{
				returnVal.ErrorMessage = Resources.BpoExportPeriodStartDateBeforeEndDate;
				return Ok(returnVal);
			}
			if (exportStartDate < utcNowDate || exportEndDate > exportPeriodMaxDate)
			{
				var validExportPeriodText = getExportPeriodMessageString();
				returnVal.ErrorMessage = validExportPeriodText;
				return Ok(returnVal);
			}

			var skill = _skillRepository.Get(skillId);
			if (skill == null)
			{
				returnVal.ErrorMessage = $"Cannot find skill with id: {skillId}";
				return Ok(returnVal);
			}
			var exportedContent = _exportBpoFile.ExportDemand(skill,
				new DateOnlyPeriod(exportStartDate, exportEndDate), CultureInfo.InvariantCulture);
		
			returnVal.Content = exportedContent;
			returnVal.ErrorMessage = "";
			return Ok(returnVal);
		}

		private string getExportPeriodMessageString()
		{
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting("StaffingReadModelNumberOfDays", 14);

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);

			var validExportPeriodText =
				$"{utcNowDate.ToShortDateString(_userUiCulture.GetUiCulture())} - {exportPeriodMaxDate.ToShortDateString(_userUiCulture.GetUiCulture())}";
			var exportPeriodMessage = string.Format(Resources.BpoOnlyExportPeriodBetweenDates, validExportPeriodText);
			return exportPeriodMessage;
		}

		[HttpGet, Route("api/staffing/exportPeriodMessage")]
		public virtual IHttpActionResult GetExportPeriodMessage()
		{
			return Ok(new ExportPeriodInfo {ExportPeriodMessage = getExportPeriodMessageString()});
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

		class ExportPeriodInfo
		{
			public string ExportPeriodMessage;
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

		class returnObj
		{
			public bool isLicenseAvailable { get; set; }
			public bool HasPermissionForBpoExchange { get; set; }
		}

		internal class exportReturnObject
		{
			public string Content { get; set; }
			public string ErrorMessage { get; set; }
		}

	}
}
