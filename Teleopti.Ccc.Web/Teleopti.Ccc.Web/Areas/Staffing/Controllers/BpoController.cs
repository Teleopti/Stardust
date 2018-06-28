using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Staffing.Controllers
{
	public class BpoController : ApiController
	{
		private readonly ImportBpoFile _bpoFile;
		private readonly IExportBpoFile _exportBpoFile;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillGroupRepository _skillGroupRepository;
		private readonly ExportStaffingPeriodValidationProvider _periodValidationProvider;
		private readonly BpoProvider _bpoProvider;

		public BpoController(ImportBpoFile bpoFile, BpoProvider bpoProvider, IExportBpoFile exportBpoFile, ExportStaffingPeriodValidationProvider periodValidationProvider, ISkillRepository skillRepository, ISkillGroupRepository skillGroupRepository)
		{
			_bpoFile = bpoFile;
			_exportBpoFile = exportBpoFile;
			_skillRepository = skillRepository;
			_skillGroupRepository = skillGroupRepository;
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
		public virtual IHttpActionResult ImportBpo([FromBody]importObj fileContents)
		{
			var result = _bpoFile.ImportFile(fileContents.FileContent, CultureInfo.InvariantCulture, fileContents.FileName);
			return Ok(result);
		}

		[UnitOfWork, HttpGet, Route("api/staffing/exportStaffingDemand")]
		public virtual IHttpActionResult ExportBpo(Guid skillId, DateTime exportStartDateTime, DateTime exportEndDateTime)
		{
			var returnVal = new ExportStaffingReturnObject();

			var dateOnlyStartDate = new DateOnly(exportStartDateTime);
			var dateOnlyEndDate = new DateOnly(exportEndDateTime);
			var validationObject = _periodValidationProvider.ValidateExportBpoPeriod(dateOnlyStartDate, dateOnlyEndDate);

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
		public virtual IHttpActionResult ExportBpoForSkillArea(Guid skillAreaId, DateTime exportStartDateTime, DateTime exportEndDateTime)
		{
			var returnVal = new ExportStaffingReturnObject();

			var dateOnlyStartDate = new DateOnly(exportStartDateTime);
			var dateOnlyEndDate = new DateOnly(exportEndDateTime);
			var validationObject = _periodValidationProvider.ValidateExportBpoPeriod(dateOnlyStartDate, dateOnlyEndDate);

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

		[UnitOfWork,HttpGet, Route("api/staffing/activebpos")]
		public virtual IHttpActionResult LoadAllActiveBpos()
		{
			return Ok(_bpoProvider.LoadAllActiveBpos());
		}

		[UnitOfWork, HttpGet, Route("api/staffing/clearbpoperiod")]
		public virtual IHttpActionResult ClearBpoForPeriod(Guid bpoGuid, DateTime startDate, DateTime endDate)
		{
			return Ok(_bpoProvider.ClearBpoResources(bpoGuid, startDate, endDate.AddDays(1).AddMinutes(-1)));
		}

		[UnitOfWork, HttpGet, Route("api/staffing/getrangemessage")]
		public virtual IHttpActionResult GetRangeMessage(Guid bpoGuid)
		{
			return Ok(_bpoProvider.GetRangeMessage(bpoGuid));
		}

		public class importObj
		{
			public string FileContent { get; set; }
			public string FileName { get; set; }
		}
	}

}