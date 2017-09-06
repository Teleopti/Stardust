using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayExportController : ApiController
	{
		private readonly PerformanceViewModelCreator _performanceViewModelCreator;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly IIntradaySkillProvider _intradaySkillProvider;

		public IntradayExportController(
			PerformanceViewModelCreator performanceViewModelCreator,
			IStaffingViewModelCreator staffingViewModelCreator,
			IncomingTrafficViewModelCreator incomingTrafficViewModelCreator,
			IIntradaySkillProvider intradaySkillProvider)
		{
			_performanceViewModelCreator = performanceViewModelCreator;
			_staffingViewModelCreator = staffingViewModelCreator;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_intradaySkillProvider = intradaySkillProvider;
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskillareadatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkillArea(IndradayExportInput input)
		{
			var skillArea = _intradaySkillProvider.GetSkillAreaById(input.id);
			var skillIdList = skillArea?.Skills.Select(skill => skill.Id).ToArray() ?? new Guid[0];
			var intradayExportDataToExcel = new IntradayExportCreator();

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport()
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = skillArea?.Name ?? string.Empty,
					Skills = skillArea?.Skills.Select(skill => skill.Name).ToArray() ?? new string[0],
					PerformanceViewModel = _performanceViewModelCreator.Load(skillIdList, input.dayOffset),
					StaffingViewModel = _staffingViewModelCreator.Load(skillIdList, input.dayOffset),
					IncomingViewModel = _incomingTrafficViewModelCreator.Load(skillIdList, input.dayOffset)
				}
			);

			return CreateResponse(data, "IntradayExportData.xlsx");
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskilldatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkill(IndradayExportInput input)
		{
			var skill = _intradaySkillProvider.GetSkillById(input.id);
			var intradayExportDataToExcel = new IntradayExportCreator();

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = string.Empty,
					Skills = new[] { skill?.Name ?? string.Empty },
					PerformanceViewModel = _performanceViewModelCreator.Load(new[] { input.id }, input.dayOffset),
					StaffingViewModel = _staffingViewModelCreator.Load(new[] { input.id }, input.dayOffset),
					IncomingViewModel = _incomingTrafficViewModelCreator.Load(new[] { input.id }, input.dayOffset)
				}
			);

			return CreateResponse(data, "IntradayExportData.xlsx");
		}

		private HttpResponseMessage CreateResponse(byte[] data, string fileName)
		{
			var response = new HttpResponseMessage();
			response.Content = new ByteArrayContent(data);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.Content.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");
			return response;
		}
	}

	public class IndradayExportInput
	{
		public Guid id { get; set; }
		public int dayOffset { get; set; }
	}
}