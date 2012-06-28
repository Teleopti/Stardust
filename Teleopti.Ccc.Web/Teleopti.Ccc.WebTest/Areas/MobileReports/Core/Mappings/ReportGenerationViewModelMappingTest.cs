namespace Teleopti.Ccc.WebTest.Areas.MobileReports.Core.Mappings
{
	using System.Collections.Generic;
	using System.Linq;

	using AutoMapper;

	using NUnit.Framework;

	using SharpTestsEx;

	using Teleopti.Ccc.Domain.Common;
	using Teleopti.Ccc.Domain.WebReport;
	using Teleopti.Ccc.UserTexts;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core;
	using Teleopti.Ccc.Web.Areas.MobileReports.Core.Mappings;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;
	using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;
	using Teleopti.Ccc.WebTest.Areas.MobileReports.TestData;
	using Teleopti.Interfaces.Domain;

	[TestFixture]
	public class ReportGenerationViewModelMappingTest
	{
		#region Setup/Teardown

		[SetUp]
		public void Setup()
		{
			_skillProvider = new TestSkillProvider();
			_userTextTranslator = new TestUserTextTranslator();
			_userCulture = new CurrentThreadUserCulture();

			Mapper.Reset();
			Mapper.Initialize(
				c =>
				c.AddProfile(
					new ReportGenerationViewModelMappingProfile(
					() => Mapper.Engine, () => _userTextTranslator, () => _skillProvider, () => _userCulture)));
		}

		#endregion

		private IUserTextTranslator _userTextTranslator;

		private ISkillProvider _skillProvider;

		private CurrentThreadUserCulture _userCulture;

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
						new ReportControlSkillGet { Id = 1, Name = "Telephone" },
						new ReportControlSkillGet { Id = 2, Name = "BO1" },
						new ReportControlSkillGet { Id = 3, Name = "BO2" }
					};
			}

			#endregion
		}

		[Test]
		public void ShouldConfigureCorrectly()
		{
			Mapper.AssertConfigurationIsValid();
		}

		[Test]
		public void ShouldCreateReportInformationAndDisplayDateOnlyWhenDatesEquals()
		{
			var expectedResult = new ReportInfo { ReportDate = "2012-01-23", };

			var source = new ReportGenerationResult
				{ ReportInput = new ReportDataParam { Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 23), } };

			var result = Mapper.Map<ReportGenerationResult, ReportInfo>(source);

			result.ReportDate.Should().Be.EqualTo(expectedResult.ReportDate);
		}

		[Test]
		public void ShouldCreateReportInformationForInterval()
		{
			var expectedResult = new ReportInfo { PeriodLegend = Resources.Time, ChartTypeHint = "line" };

			var source = new ReportGenerationResult
				{
					Report =
						new DefinedReportInformation
							{
								ReportNameResourceKey = "resReportName",
								ReportInfo =
									new ReportMetaInfo
										{
											SeriesResourceKeys = new[] { "resY1Legend", "resY2Legend" },
											ChartTypeHint = new[] { "line", "stackedbar" }
										}
							},
					ReportInput =
						new ReportDataParam
							{ IntervalType = ReportIntervalType.Day, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), SkillSet = "1,2,3" }
				};

			var result = Mapper.Map<ReportGenerationResult, ReportInfo>(source);

			result.PeriodLegend.Should().Be.EqualTo(expectedResult.PeriodLegend);
			result.ChartTypeHint.Should().Be.EqualTo(expectedResult.ChartTypeHint);
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
					ReportName = "resReportNameLoc",
					ChartTypeHint = "stackedbar",
					Y1DecimalsHint = 2,
					Y2DecimalsHint = 1
				};

			var source = new ReportGenerationResult
				{
					Report =
						new DefinedReportInformation
							{
								ReportNameResourceKey = "resReportName",
								ReportInfo =
									new ReportMetaInfo
										{
											SeriesResourceKeys = new[] { "resY1Legend", "resY2Legend" },
											ChartTypeHint = new[] { "line", "stackedbar" },
											SeriesFixedDecimalHint = new[] { 2, 1 }
										}
							},
					ReportInput =
						new ReportDataParam
							{ IntervalType = ReportIntervalType.Week, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), SkillSet = "1,2,3" }
				};

			var result = Mapper.Map<ReportGenerationResult, ReportInfo>(source);

			result.PeriodLegend.Should().Be.EqualTo(expectedResult.PeriodLegend);
			result.ReportDate.Should().Be.EqualTo(expectedResult.ReportDate);
			result.SkillNames.Should().Be.EqualTo(expectedResult.SkillNames);
			result.Y1Legend.Should().Be.EqualTo(expectedResult.Y1Legend);
			result.Y2Legend.Should().Be.EqualTo(expectedResult.Y2Legend);
			result.ChartTypeHint.Should().Be.EqualTo(expectedResult.ChartTypeHint);
			result.ReportName.Should().Be.EqualTo(expectedResult.ReportName);
			result.Y1DecimalsHint.Should().Be.EqualTo(expectedResult.Y1DecimalsHint);
			result.Y2DecimalsHint.Should().Be.EqualTo(expectedResult.Y2DecimalsHint);
		}

		[Test]
		public void ShouldCreateReportResponseFromReportGenerationResult()
		{
			var source = new ReportGenerationResult
				{
					Report = null,
					ReportInput =
						new ReportDataParam
							{ IntervalType = ReportIntervalType.Week, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), SkillSet = "1,2,3" },
					ReportData = new[] { new ReportDataPeriodEntry(), new ReportDataPeriodEntry() }
				};
			var result = Mapper.Map<ReportGenerationResult, ReportTableRowViewModel[]>(source);

			result.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldMapReportDataPeriodEntryToReportRowViewModelAndTranslatePeriodIfDefined()
		{
			var source = new ReportDataPeriodEntry { Period = "ResDayOfWeekMonday", Y1 = 1.5M, Y2 = 200.4M };

			var result = Mapper.Map<ReportDataPeriodEntry, ReportTableRowViewModel>(source);

			result.Period.Should().Be.EqualTo("ResDayOfWeekMonday" + "Loc");
			result.DataColumn1.Should().Be.EqualTo("1.50");
			result.DataColumn2.Should().Be.EqualTo("200.40");
		}

		[Test]
		[SetCulture("sv-SE")]
		[SetUICulture("sv-SE")]
		public void ShouldMapReportDataPeriodEntryToReportRowViewModelWithPunctiationForSwedishCulture()
		{
			var source = new ReportDataPeriodEntry { Period = "00:00", Y1 = 1.5M, Y2 = 200.556M };

			var result = Mapper.Map<ReportDataPeriodEntry, ReportTableRowViewModel>(source);

			result.Period.Should().Be.EqualTo("00:00");
			result.DataColumn1.Should().Be.EqualTo("1.50");
			result.DataColumn2.Should().Be.EqualTo("200.56");
		}

		[Test]
		[SetCulture("en-US")]
		[SetUICulture("en-US")]
		public void ShouldMapReportDataPeriodEntryToReportRowViewModelWithPunctiationForUsCulture()
		{
			var source = new ReportDataPeriodEntry { Period = "00:00", Y1 = 210000000.5M, Y2 = 200.55M };

			var result = Mapper.Map<ReportDataPeriodEntry, ReportTableRowViewModel>(source);

			result.Period.Should().Be.EqualTo("00:00");
			result.DataColumn1.Should().Be.EqualTo("210000000.50");
			result.DataColumn2.Should().Be.EqualTo("200.55");
		}

		[Test]
		[SetCulture("sv-SE")]
		[SetUICulture("sv-SE")]
		public void ShouldReArrangeEntriesToMatchFirstdayOfWeekForSvCulture()
		{
			var source = new ReportGenerationResult
				{
					Report = null,
					ReportInput = new ReportDataParam { IntervalType = ReportIntervalType.Week, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), },
					ReportData =
						new[]
							{
								new ReportDataPeriodEntry { Period = "Monday", PeriodNumber = 1 },
								new ReportDataPeriodEntry { Period = "Tuesday", PeriodNumber = 2 },
								new ReportDataPeriodEntry { Period = "Friday", PeriodNumber = 5 },
								new ReportDataPeriodEntry { Period = "Saturday", PeriodNumber = 6 },
								new ReportDataPeriodEntry { Period = "Sunday", PeriodNumber = 7 }
							}
				};
			var result = Mapper.Map<ReportGenerationResult, ReportTableRowViewModel[]>(source);

			result.Should().Have.Count.EqualTo(5);
			result.First().Period.Should().Be.EqualTo("Monday");
		}

		[Test]
		[SetCulture("en-US")]
		[SetUICulture("en-US")]
		public void ShouldReArrangeEntriesToMatchFirstdayOfWeekForUsCulture()
		{
			var source = new ReportGenerationResult
				{
					Report = null,
					ReportInput =
						new ReportDataParam
							{ IntervalType = ReportIntervalType.Week, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), SkillSet = "1,2,3" },
					ReportData =
						new[]
							{
								new ReportDataPeriodEntry { Period = "Monday", PeriodNumber = 1 },
								new ReportDataPeriodEntry { Period = "Tuesday", PeriodNumber = 2 },
								new ReportDataPeriodEntry { Period = "Saturday", PeriodNumber = 6 },
								new ReportDataPeriodEntry { Period = "Sunday", PeriodNumber = 7 }
							}
				};
			var result = Mapper.Map<ReportGenerationResult, ReportTableRowViewModel[]>(source);

			result.Should().Have.Count.EqualTo(4);
			result.First().Period.Should().Be.EqualTo("Sunday");
		}

		[Test]
		[SetCulture("en-US")]
		[SetUICulture("en-US")]
		public void ShouldReArrangeEntriesToMatchFirstdayOfWeekForUsCultureEvenWhenSundayEntryIsMissing()
		{
			var source = new ReportGenerationResult
				{
					Report = null,
					ReportInput =
						new ReportDataParam
							{ IntervalType = ReportIntervalType.Week, Period = new DateOnlyPeriod(2012, 01, 23, 2012, 01, 30), SkillSet = "1,2,3" },
					ReportData =
						new[]
							{
								new ReportDataPeriodEntry { Period = "Monday", PeriodNumber = 1 },
								new ReportDataPeriodEntry { Period = "Tuesday", PeriodNumber = 2 },
								new ReportDataPeriodEntry { Period = "Friday", PeriodNumber = 5 },
								new ReportDataPeriodEntry { Period = "Saturday", PeriodNumber = 6 }
							}
				};
			var result = Mapper.Map<ReportGenerationResult, ReportTableRowViewModel[]>(source);

			result.Should().Have.Count.EqualTo(4);
			result.First().Period.Should().Be.EqualTo("Monday");
		}
	}
}