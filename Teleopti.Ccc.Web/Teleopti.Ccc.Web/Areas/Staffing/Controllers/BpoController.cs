using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Exceptions;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;



namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class BpoController : ApiController
	{
		private readonly IExportBpoFile _exportBpoFile;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ExportStaffingPeriodValidationProvider _periodValidationProvider;
		private readonly BpoProvider _bpoProvider;
		private readonly IAuditableBpoOperations _auditableBpoOperations;

		public BpoController(BpoProvider bpoProvider, IExportBpoFile exportBpoFile, ExportStaffingPeriodValidationProvider periodValidationProvider, ISkillRepository skillRepository, ISkillGroupRepository skillGroupRepository, IAuditableBpoOperations auditableBpoOperations)
		{
			_exportBpoFile = exportBpoFile;
			_skillRepository = skillRepository;
			_skillGroupRepository = skillGroupRepository;
			_auditableBpoOperations = auditableBpoOperations;
			_periodValidationProvider = periodValidationProvider;
			_bpoProvider = bpoProvider;
		}

		[UnitOfWork, HttpGet, Route("api/staffing/getallganttdataforbpotimeline")]
		public virtual IHttpActionResult GetAllGanttDataForBpoTimeline()
		{
			return Ok(_bpoProvider.GetAllGanttDataForBpoTimeline());
		}

		[UnitOfWork, HttpGet, Route("api/staffing/getganttdataforbpotimelineonskill")]
		public virtual IHttpActionResult GetGanttDataForBpoTimelineOnSkill(Guid skillId)
		{
			return Ok(_bpoProvider.GetGanttDataForBpoTimelineOnSkill(skillId));
		}

		[UnitOfWork, HttpGet, Route("api/staffing/getganttdataforbpotimelineonskillgroup")]
		public virtual IHttpActionResult GetGanttDataForBpoTimelineOnSkillGroup(Guid skillGroupId)
		{
			return Ok(_bpoProvider.GetGanttDataForBpoTimelineOnSkillGroup(skillGroupId));
		}

		[UnitOfWork, HttpPost, Route("api/staffing/importBpo")]
		public virtual IHttpActionResult ImportBpo([FromBody]ImportBpoActionObj fileContents)
		{
			ImportBpoFileResult result;
			try
			{
				result = _auditableBpoOperations.ImportBpo(fileContents);
			}
			catch (BusinessRuleBrokenException e)
			{
				result = (ImportBpoFileResult) e.ReturnObject;
			}
			return Ok(result);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/exportStaffingDemand")]
		public virtual IHttpActionResult ExportBpo(Guid skillId, string exportStartDateTime, string exportEndDateTime)
		{
			var returnVal = new ExportStaffingReturnObject();
			
			var validationObject = getDateOnly(exportStartDateTime, out var dateOnlyStartDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			validationObject = getDateOnly(exportEndDateTime, out var dateOnlyEndDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			validationObject = _periodValidationProvider.ValidateExportBpoPeriod(dateOnlyStartDate, dateOnlyEndDate);
			
			if (validationObject.Result == false)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			var skill = _skillRepository.Get(skillId);
			if (skill == null)
			{
				returnVal.ErrorMessage = $"Cannot find skill with id: {skillId}";
				return Ok(returnVal);
			}
			var exportedContent = _exportBpoFile.ExportDemand(skill,
				new DateOnlyPeriod(dateOnlyStartDate, dateOnlyEndDate), CultureInfo.InvariantCulture);

			returnVal.Content = exportedContent;
			returnVal.ErrorMessage = "";
			return Ok(returnVal);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/exportStaffingDemandskillArea")]
		public virtual IHttpActionResult ExportBpoForSkillArea(Guid skillAreaId, string exportStartDateTime, string exportEndDateTime)
		{
			var returnVal = new ExportStaffingReturnObject();

			var validationObject = getDateOnly(exportStartDateTime, out var dateOnlyStartDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			validationObject = getDateOnly(exportEndDateTime, out var dateOnlyEndDate);
			if (!validationObject.Result)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			validationObject = _periodValidationProvider.ValidateExportBpoPeriod(dateOnlyStartDate, dateOnlyEndDate);

			if (validationObject.Result == false)
			{
				returnVal.ErrorMessage = validationObject.ErrorMessage;
				return Ok(returnVal);
			}

			var skill = _skillGroupRepository.Get(skillAreaId);
			if (skill == null)
			{
				returnVal.ErrorMessage = $"Cannot find skill area with id: {skillAreaId}";
				return Ok(returnVal);
			}
			var exportedContent = _exportBpoFile.ExportDemand(skillAreaId,
				new DateOnlyPeriod(dateOnlyStartDate, dateOnlyEndDate), CultureInfo.InvariantCulture);

			returnVal.Content = exportedContent;
			returnVal.ErrorMessage = "";
			return Ok(returnVal);
		}


		private ExportStaffingValidationObject getDateOnly(string dateTimeString, out DateOnly dateOnly)
		{
			var validationObject = new ExportStaffingValidationObject { Result = true };
			var dateFormatter = "yyyy-MM-dd";
			dateOnly = DateOnly.MinValue;
			if (DateTime.TryParseExact(dateTimeString,dateFormatter, CultureInfo.InvariantCulture, DateTimeStyles.None,out var dateTime))
			{
				dateOnly = new DateOnly(dateTime);
				return validationObject;
			}
			validationObject.ErrorMessage = $"The date formart is invalid {dateTimeString}";
			validationObject.Result = false;
			return validationObject;
		}
		
		[UnitOfWork,HttpGet, Route("api/staffing/activebpos")]
		public virtual IHttpActionResult LoadAllActiveBpos()
		{
			return Ok(_bpoProvider.LoadAllActiveBpos());
		}

		[UnitOfWork, HttpGet, Route("api/staffing/clearbpoperiod")]
		public virtual IHttpActionResult ClearBpoForPeriod(Guid bpoGuid, DateTime startDate, DateTime endDate)
		{
			ClearBpoReturnObject result;
			try
			{
				result = _auditableBpoOperations.ClearBpoForPeriod(new ClearBpoActionObj()
					{BpoGuid = bpoGuid, EndDate = endDate, StartDate = startDate});
			}
			catch (BusinessRuleBrokenException e)
			{
				result = (ClearBpoReturnObject)e.ReturnObject;
			}
			return Ok(result);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/getrangemessage")]
		public virtual IHttpActionResult GetRangeMessage(Guid bpoGuid)
		{
			return Ok(_bpoProvider.GetRangeMessage(bpoGuid));
		}

		
	}
	
}