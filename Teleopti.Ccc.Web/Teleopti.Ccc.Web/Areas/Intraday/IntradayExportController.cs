using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Intraday
{
	[ApplicationFunctionApi(DefinedRaptorApplicationFunctionPaths.WebIntraday)]
	public class IntradayExportController : ApiController
	{
		private readonly ISkillAreaRepository _skillAreaRepository;
		private readonly PerformanceViewModelCreator _performanceViewModelCreator;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IncomingTrafficViewModelCreator _incomingTrafficViewModelCreator;
		private readonly ISkillRepository _skillRepository;

		public IntradayExportController(
			ISkillAreaRepository skillAreaRepository,
			PerformanceViewModelCreator performanceViewModelCreator,
			IStaffingViewModelCreator staffingViewModelCreator,
			IncomingTrafficViewModelCreator incomingTrafficViewModelCreator,
			ISkillRepository skillRepository)
		{
			_skillAreaRepository = skillAreaRepository;
			_performanceViewModelCreator = performanceViewModelCreator;
			_staffingViewModelCreator = staffingViewModelCreator;
			_incomingTrafficViewModelCreator = incomingTrafficViewModelCreator;
			_skillRepository = skillRepository;
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskillareadatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkillArea(IndradayExportInput input)
		{
			var skillArea = _skillAreaRepository.Get(input.id);
			var skillIdList = skillArea.Skills.Select(skill => skill.Id).ToArray();
			var intradayExportDataToExcel = new IntradayExportCreator();

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport()
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = skillArea.Name,
					Skills = skillArea.Skills.Select(skill => skill.Name).ToArray(),
					PerformanceViewModel = _performanceViewModelCreator.Load(skillIdList, input.dayOffset),
					StaffingViewModel = _staffingViewModelCreator.Load(skillIdList, input.dayOffset),
					IncomingViewModel = _incomingTrafficViewModelCreator.Load(skillIdList, input.dayOffset)
				}
			);

			var response = new HttpResponseMessage();
			response.Content = new ByteArrayContent(data);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.Content.Headers.Add("Content-Disposition", "attachment; filename=IntradayExportData.xlsx");
			return response;
		}

		[UnitOfWork, HttpPost, Route("api/intraday/exportskilldatatoexcel")]
		public virtual HttpResponseMessage GetIntradayDataAsExcelFileFromSkill(IndradayExportInput input)
		{
			var skill = _skillRepository.Get(input.id);
			var intradayExportDataToExcel = new IntradayExportCreator();

			var data = intradayExportDataToExcel.ExportDataToExcel(
				new IntradayExcelExport
				{
					Date = DateTime.Now.AddDays(input.dayOffset),
					SkillAreaName = string.Empty,
					Skills = new[] { skill.Name },
					PerformanceViewModel = _performanceViewModelCreator.Load(new[] { input.id }, input.dayOffset),
					StaffingViewModel = _staffingViewModelCreator.Load(new[] { input.id }, input.dayOffset),
					IncomingViewModel = _incomingTrafficViewModelCreator.Load(new[] { input.id }, input.dayOffset)
				}
			);

			var response = new HttpResponseMessage();
			response.Content = new ByteArrayContent(data);
			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
			response.Content.Headers.Add("Content-Disposition", "attachment; filename=IntradayExportData.xlsx");
			return response;
		}
	}

	public class IndradayExportInput
	{
		public Guid id { get; set; }
		public int dayOffset { get; set; }
	}
}