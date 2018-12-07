using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.ApplicationLayer;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayExportController : ApiController
	{
		private readonly IntradayPerformanceApplicationService _performanceApplicationService;
		private readonly IIntradaySkillProvider _intradaySkillProvider;
		private readonly IntradayStaffingApplicationService _intradayStaffingApplicationService;
		private readonly IntradayIncomingTrafficApplicationService _intradayIncomingTrafficApplicationService;

		public IntradayExportController(
			IntradayPerformanceApplicationService performanceApplicationService,
			IIntradaySkillProvider intradaySkillProvider,
			IntradayStaffingApplicationService intradayStaffingApplicationService,
			IntradayIncomingTrafficApplicationService intradayIncomingTrafficApplicationService)
		{
			_intradaySkillProvider = intradaySkillProvider;
			_intradayStaffingApplicationService = intradayStaffingApplicationService ?? throw new ArgumentNullException(nameof(intradayStaffingApplicationService));
			_performanceApplicationService = performanceApplicationService ?? throw new ArgumentNullException(nameof(performanceApplicationService));
			_intradayIncomingTrafficApplicationService = intradayIncomingTrafficApplicationService ?? throw new ArgumentNullException(nameof(intradayIncomingTrafficApplicationService));
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskillareadatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkillArea(IndradayExportInput input)
		{
			var skillArea = _intradaySkillProvider.GetSkillGroupById(input.id);
			var skillIdList = skillArea?.Skills.Select(skill => skill.Id).ToArray() ?? new Guid[0];
			var intradayExportDataToExcel = new IntradayExportCreator();

			var staffingViewModel =  _intradayStaffingApplicationService.GenerateStaffingViewModel(skillIdList, input.dayOffset);

			var incomingTrafficViewModel = _intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(skillIdList, input.dayOffset);

			var performanceViewModel = _performanceApplicationService.GeneratePerformanceViewModel(skillIdList, input.dayOffset);

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = skillArea?.Name ?? string.Empty,
					Skills = skillArea?.Skills.Select(skill => skill.Name).ToArray() ?? new string[0],
					PerformanceViewModel = performanceViewModel,
					StaffingViewModel = staffingViewModel,
					IncomingViewModel = incomingTrafficViewModel
				});

			return CreateResponse(data, "IntradayExportData.xlsx");
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskilldatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkill(IndradayExportInput input)
		{
			var skill = _intradaySkillProvider.GetSkillById(input.id);
			var intradayExportDataToExcel = new IntradayExportCreator();

			var staffingViewModel = _intradayStaffingApplicationService.GenerateStaffingViewModel(new[] { input.id }, input.dayOffset);

			var incomingTrafficViewModel = _intradayIncomingTrafficApplicationService.GenerateIncomingTrafficViewModel(new[] { input.id }, input.dayOffset);

			var performanceViewModel = _performanceApplicationService.GeneratePerformanceViewModel(new[] { input.id }, input.dayOffset);

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = string.Empty,
					Skills = new[] { skill?.Name ?? string.Empty },
					PerformanceViewModel = performanceViewModel,
					StaffingViewModel = staffingViewModel,
					IncomingViewModel = incomingTrafficViewModel
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