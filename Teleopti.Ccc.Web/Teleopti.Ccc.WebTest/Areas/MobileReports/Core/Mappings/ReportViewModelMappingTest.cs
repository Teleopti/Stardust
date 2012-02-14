using System.Globalization;
using AutoMapper;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Mappings;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Core.RequestContext;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Mappings
{
	[TestFixture]
	public class ReportViewModelMappingTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			ISkillProvider skillProvider = null;
			IDefinedReportProvider definedReportProvider = null;
			var userTextTranslator = new FakeUserTextTranslator();
			var cultureProvider = MockRepository.GenerateStub<ICultureProvider>();
			cultureProvider.Stub(s => s.GetCulture()).Return(CultureInfo.GetCultureInfo(1053));
			var dateBoxGlobalizationViewModelFactory = new DateBoxGlobalizationViewModelFactory(cultureProvider);

			Mapper.Reset();
			Mapper.Initialize(
				c =>
				c.AddProfile(new ReportViewModelMappingProfile(() => Mapper.Engine, () => userTextTranslator,
				                                               () => definedReportProvider, () => skillProvider,
				                                               () => dateBoxGlobalizationViewModelFactory)));
		}

		#endregion

		[Test]
		public void ShouldCallTextTranslationWhenMappingDefinedReportsToReportsSelectionViewModel()
		{
			/*
			var reportSelectionViewModel = Mapper.Map<Def, ReportSelectionViewModel>(source);

			reportSelectionViewModel.ReportId.Should().Be.EqualTo(source.ReportId);
			reportSelectionViewModel.ReportName.Should().Be.EqualTo(nameLookup);
			 * */
		}


		[Test]
		public void ShouldConfigureCorrectly()
		{
			Mapper.AssertConfigurationIsValid();
		}

		[Test]
		public void ShouldMapDefinedReportInformationToReportSelectionViewModel()
		{
			var source = new DefinedReportInformation
			             	{
			             		FunctionCode = "aRptFunction",
			             		ReportId = "id",
			             		ReportName = "name",
			             		ReportNameResourceKey = "resourceKey"
			             	};

			ReportSelectionViewModel reportSelectionViewModel =
				Mapper.Map<DefinedReportInformation, ReportSelectionViewModel>(source);

			reportSelectionViewModel.ReportId.Should().Be.EqualTo(source.ReportId);
			reportSelectionViewModel.ReportName.Should().Be.EqualTo(source.ReportNameResourceKey);
		}

		[Test]
		public void ShouldMapMartDomainSkillsToSkillViewModel()
		{
			var source = new ReportControlSkillGet {Id = 1, Name = "name"};

			SkillSelectionViewModel skillSelectionViewModel =
				Mapper.Map<ReportControlSkillGet, SkillSelectionViewModel>(source);

			skillSelectionViewModel.SkillId.Should().Be.EqualTo(source.Id.ToString());
			skillSelectionViewModel.SkillName.Should().Be.EqualTo(source.Name);
			skillSelectionViewModel.AllSkills.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapMartDomainSkillsToSkillViewModelWithAllFlag()
		{
			var source = new ReportControlSkillGet {Id = -2, Name = "name"};

			SkillSelectionViewModel skillSelectionViewModel =
				Mapper.Map<ReportControlSkillGet, SkillSelectionViewModel>(source);

			skillSelectionViewModel.SkillId.Should().Be.EqualTo(source.Id.ToString());
			skillSelectionViewModel.SkillName.Should().Be.EqualTo(source.Name);
			skillSelectionViewModel.AllSkills.Should().Be.EqualTo(true);
		}
	}
}