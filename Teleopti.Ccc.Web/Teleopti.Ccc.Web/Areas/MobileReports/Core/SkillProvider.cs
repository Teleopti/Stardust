using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public class SkillProvider : ISkillProvider
	{
		private readonly IWebReportRepository _webReportRepository;
		private readonly IWebReportUserInfoProvider _webReportUserInfoProvider;


		public SkillProvider(IWebReportUserInfoProvider webReportUserInfoProvider, IWebReportRepository webReportRepository)
		{
			_webReportUserInfoProvider = webReportUserInfoProvider;
			_webReportRepository = webReportRepository;
		}

		public IEnumerable<ReportControlSkillGet> GetAvailableSkills()
		{
			var webReportUserInformation = _webReportUserInfoProvider.GetUserInformation();
			var reportid = new Guid("8D8544E4-6B24-4C1C-8083-CBE7522DD0E0");
			var orderedSkills =
				_webReportRepository.ReportControlSkillGet(reportid, webReportUserInformation.PersonCode,
				                                           webReportUserInformation.LanguageId,
				                                           webReportUserInformation.BusinessUnitCode).OrderBy(s => s.Name);


			return orderedSkills;
		}
	}
}