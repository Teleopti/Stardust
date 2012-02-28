namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Providers
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using NUnit.Framework;

	using Rhino.Mocks;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Repositories;
	using Teleopti.Ccc.Domain.WebReport;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

	[TestFixture]
	public class SkillProviderTest
	{
		private MockRepository _mock;

		private IWebReportRepository _webReportRepository;

		private IWebReportUserInfoProvider _webReportUserInfoProvider;

		[SetUp]
		public void Setup()
		{
			this._mock = new MockRepository();
			this._webReportRepository = this._mock.DynamicMock<IWebReportRepository>();
			this._webReportUserInfoProvider = this._mock.DynamicMock<IWebReportUserInfoProvider>();
		}

		[Test]
		public void ShouldReturnSkills()
		{
			ISkillProvider target = new SkillProvider(this._webReportUserInfoProvider, this._webReportRepository);

			var skills = new List<ReportControlSkillGet>
				{
					new ReportControlSkillGet { Id = -2, Name = "All" },
					new ReportControlSkillGet { Id = 1, Name = "SkillDesc1" },
					new ReportControlSkillGet { Id = 2, Name = "SkillDesc2" }
				};
			var reportUserSetting = new WebReportUserInformation
				{ BusinessUnitCode = Guid.NewGuid(), LanguageId = 0, PersonCode = Guid.NewGuid() };

			using (this._mock.Record())
			{
				Expect.Call(this._webReportUserInfoProvider.GetUserInformation()).Return(reportUserSetting);
				Expect.Call(_webReportRepository.ReportControlSkillGet(new Guid("8D8544E4-6B24-4C1C-8083-CBE7522DD0E0"), reportUserSetting.PersonCode,
				                                                       reportUserSetting.LanguageId, reportUserSetting.BusinessUnitCode)).
					Return(skills);
			}
			using (this._mock.Playback())
			{
				IEnumerable<ReportControlSkillGet> availableSkills = target.GetAvailableSkills();

				availableSkills.Count().Should().Be.EqualTo(3);
			}
		}
	}
}