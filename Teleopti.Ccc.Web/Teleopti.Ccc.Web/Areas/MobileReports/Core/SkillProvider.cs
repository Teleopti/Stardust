using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;

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

		#region ISkillProvider Members

		public IEnumerable<ReportControlSkillGet> GetAvailableSkills()
		{
			WebReportUserInformation webReportUserInformation = _webReportUserInfoProvider.GetUserInformation();

			IOrderedEnumerable<ReportControlSkillGet> orderedSkills =
				_webReportRepository.ReportControlSkillGet(10, webReportUserInformation.PersonCode,
				                                           webReportUserInformation.LanguageId,
				                                           webReportUserInformation.BusinessUnitCode).OrderBy(s => s.Name);


			return orderedSkills;


			//return new List<ReportControlSkillGet> { new ReportControlSkillGet() { Id =-1, Name="Dummy"}} ;
		}

		#endregion
	}
}