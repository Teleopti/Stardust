using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports
{
	[TestFixture]
	public class SkillProviderTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_webReportRepository = _mock.DynamicMock<IWebReportRepository>();
			_webReportUserInfoProvider = _mock.DynamicMock<IWebReportUserInfoProvider>();
		}

		#endregion

		private MockRepository _mock;
		private IWebReportRepository _webReportRepository;
		private IWebReportUserInfoProvider _webReportUserInfoProvider;

		[Test]
		public void ShouldReturnSkills()
		{
			ISkillProvider target = new SkillProvider(_webReportUserInfoProvider, _webReportRepository);

			var skills = new List<ReportControlSkillGet>
			             	{
			             		new ReportControlSkillGet {Id = -2, Name = "All"},
			             		new ReportControlSkillGet {Id = 1, Name = "SkillDesc1"},
			             		new ReportControlSkillGet {Id = 2, Name = "SkillDesc2"}
			             	};
			var reportUserSetting = new WebReportUserInformation {BusinessUnitCode = Guid.NewGuid(), LanguageId = 0, PersonCode = Guid.NewGuid()};

			using (_mock.Record())
			{
				Expect.Call(_webReportUserInfoProvider.GetUserInformation()).Return(reportUserSetting);
				Expect.Call(_webReportRepository.ReportControlSkillGet(10, reportUserSetting.PersonCode,
				                                                       reportUserSetting.LanguageId, reportUserSetting.BusinessUnitCode)).
					Return(skills);
			}
			using (_mock.Playback())
			{
				IEnumerable<ReportControlSkillGet> availableSkills = target.GetAvailableSkills();

				availableSkills.Count().Should().Be.EqualTo(3);
			}
		}
	}
}