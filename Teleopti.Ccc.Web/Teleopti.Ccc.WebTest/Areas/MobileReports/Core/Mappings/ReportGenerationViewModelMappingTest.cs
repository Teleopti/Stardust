using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WebReport;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MobileReports.Core;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Mappings;
using Teleopti.Ccc.Web.Areas.MobileReports.Models;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Mappings
{
	[TestFixture]
	public class ReportGenerationViewModelMappingTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_skillProvider = new TestSkillProvider();
			_userTextTranslator = new TestUserTextTranslator();

			Mapper.Reset();
			Mapper.Initialize(
				c =>
				c.AddProfile(new ReportGenerationViewModelMappingProfile(() => Mapper.Engine, () => _userTextTranslator,
				                                                         () => _skillProvider, () => null)));
		}

		#endregion

		private IUserTextTranslator _userTextTranslator;
		private ISkillProvider _skillProvider;

		protected class TestUserTextTranslator : IUserTextTranslator
		{
			#region IUserTextTranslator Members

			public string TranslateText(string textToTranslate)
			{
				return textToTranslate + "Loc";
			}

			#endregion
		}

		protected class TestSkillProvider : ISkillProvider
		{
			#region ISkillProvider Members

			public IEnumerable<ReportControlSkillGet> GetAvailableSkills()
			{
				return new List<ReportControlSkillGet>
				       	{
				       		new ReportControlSkillGet {Id = 1, Name = "Telephone"},
				       		new ReportControlSkillGet {Id = 2, Name = "BO1"},
				       		new ReportControlSkillGet {Id = 3, Name = "BO2"}
				       	};
			}

			#endregion
		}

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
		public void ShouldCreateReportInformationWithFormattedDateAndSkillNames()
		{
			var expectedResult = new ReportInfo
			                     	{
			                     		PeriodLegend = Resources.Day,
			                     		ReportDate = "2012-01-23 - 2012-01-30",
			                     		SkillNames = "Telephone, BO1, BO2",
			                     		Y1Legend = "resY1LegendLoc",
			                     		Y2Legend = "resY2LegendLoc",
			                     		ReportName = "resReportNameLoc"
			                     	};

			var source = new ReportGenerationResult
			             	{
			             		Report =
			             			new DefinedReportInformation
			             				{ReportNameResourceKey = "resReportName", LegendResourceKeys = new[] {"resY1Legend", "resY2Legend"}},
			             		ReportInput = new ReportDataParam
			             		              	{
			             		              		IntervalType = 7,
			             		              		Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30),
			             		              		SkillSet = "1,2,3"
			             		              	}
			             	};

			var result = Mapper.Map<ReportGenerationResult, ReportInfo>(source);

			result.PeriodLegend.Should().Be.EqualTo(expectedResult.PeriodLegend);
			result.ReportDate.Should().Be.EqualTo(expectedResult.ReportDate);
			result.SkillNames.Should().Be.EqualTo(expectedResult.SkillNames);
			result.Y1Legend.Should().Be.EqualTo(expectedResult.Y1Legend);
			result.Y2Legend.Should().Be.EqualTo(expectedResult.Y2Legend);
			result.ReportName.Should().Be.EqualTo(expectedResult.ReportName);
		}

		[Test]
		public void ShouldCreateReportInformationAndDisplayDateOnlyWhenDatesEquals()
		{
			var expectedResult = new ReportInfo
			{
				ReportDate = "2012-01-23",
			};

			var source = new ReportGenerationResult
			{
				ReportInput = new ReportDataParam
				{
					Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 23),
				}
			};

			var result = Mapper.Map<ReportGenerationResult, ReportInfo>(source);

			result.ReportDate.Should().Be.EqualTo(expectedResult.ReportDate);
		}

		[Test]
		public void ShouldCreateReportResponseFromReportGenerationResult()
		{
			var source = new ReportGenerationResult
			             	{
			             		Report = null,
			             		ReportInput = new ReportDataParam
			             		              	{
			             		              		IntervalType = 7,
			             		              		Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30),
			             		              		SkillSet = "1,2,3"
			             		              	},
			             		ReportData = new[] {new ReportDataPeriodEntry(), new ReportDataPeriodEntry()}
			             	};
			var result = Mapper.Map<ReportGenerationResult, ReportTableRowViewModel[]>(source);

			result.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldMapReportDataPeriodEntryToReportRowViewModel()
		{
			var source = new ReportDataPeriodEntry {Period = "00:00", Y1 = 1.5M, Y2 = 200.55M};

			var result = Mapper.Map<ReportDataPeriodEntry, ReportTableRowViewModel>(source);

			result.Period.Should().Be.EqualTo("00:00");
			result.DataColumn1.Should().Be.EqualTo("1,5");
			result.DataColumn2.Should().Be.EqualTo("200,6");
		}


		[Test]
		public void ShouldMapReportDataPeriodEntryToReportRowViewModelAndTranslatePeriodIfDefined()
		{
			var source = new ReportDataPeriodEntry {Period = "ResDayOfWeekMonday", Y1 = 1.5M, Y2 = 200.4M};

			var result = Mapper.Map<ReportDataPeriodEntry, ReportTableRowViewModel>(source);

			result.Period.Should().Be.EqualTo("ResDayOfWeekMonday" + "Loc");
			result.DataColumn1.Should().Be.EqualTo("1,5");
			result.DataColumn2.Should().Be.EqualTo("200,4");
		}


/*
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
			var source = new ReportControlSkillGet { Id = 1, Name = "name" };

			SkillSelectionViewModel skillSelectionViewModel =
				Mapper.Map<ReportControlSkillGet, SkillSelectionViewModel>(source);

			skillSelectionViewModel.SkillId.Should().Be.EqualTo(source.Id.ToString());
			skillSelectionViewModel.SkillName.Should().Be.EqualTo(source.Name);
			skillSelectionViewModel.AllSkills.Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldMapMartDomainSkillsToSkillViewModelWithAllFlag()
		{
			var source = new ReportControlSkillGet { Id = -2, Name = "name" };

			SkillSelectionViewModel skillSelectionViewModel =
				Mapper.Map<ReportControlSkillGet, SkillSelectionViewModel>(source);

			skillSelectionViewModel.SkillId.Should().Be.EqualTo(source.Id.ToString());
			skillSelectionViewModel.SkillName.Should().Be.EqualTo(source.Name);
			skillSelectionViewModel.AllSkills.Should().Be.EqualTo(true);
		}
 * */
	}
}