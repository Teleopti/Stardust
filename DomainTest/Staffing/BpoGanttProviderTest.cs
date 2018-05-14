using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class BpoGanttProviderTest : IIsolateSystem
	{
		public MutableNow Now;
		public FakeStaffingSettingsReader StaffingSettingsReader;
		public BpoGanttProvider Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStaffingSettingsReader>().For<FakeStaffingSettingsReader, IStaffingSettingsReader>();
		}

		//[Test]
		//public void ShouldInitializeBpoDataWithReadmodelRange()
		//{
		//	Now.Is("2018-05-02 12:00");
		//	StaffingSettingsReader.StaffingSettings[KeyNames.StaffingReadModelNumberOfDays] = 7 * 4;
		//	StaffingSettingsReader.StaffingSettings[KeyNames.StaffingReadModelHistoricalHours] = 8 * 24;

		//	var bpoGanttData = Target.InitializeReadmodelRange();
		//	bpoGanttData.PeriodStartDate.Should().Be(new DateTime(2018, 04, 25));
		//	bpoGanttData.PeriodEndDate.Should().Be(new DateTime(2018, 05, 30));
		//}

		[Test]
		public void ShouldCombineConsecutiveDates()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 01),
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(1);
			ganttData.First().Tasks.Count.Should().Be(1);
			ganttData.First().Tasks.First().From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks.First().To.Should().Be(new DateTime(2018, 05, 2));
		}

		[Test]
		public void ShouldNotCombineDatesWithGap()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 02),
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(1);
			ganttData.First().Tasks.Count.Should().Be(2);
			ganttData.First().Tasks[0].From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks[0].To.Should().Be(new DateTime(2018, 05, 01));
			ganttData.First().Tasks[1].From.Should().Be(new DateTime(2018, 05, 02));
			ganttData.First().Tasks[1].To.Should().Be(new DateTime(2018, 05, 03));
		}

		[Test]
		public void ShouldCreateSeparateTaskPerImportfile()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 01),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "newdata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 02),
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(1);
			ganttData.First().Tasks.Count.Should().Be(2);
			ganttData.First().Tasks[0].Name.Should().Be("bpodata.csv");
			ganttData.First().Tasks[0].From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks[0].To.Should().Be(new DateTime(2018, 05, 02));
			ganttData.First().Tasks[1].Name.Should().Be("newdata.csv");
			ganttData.First().Tasks[1].From.Should().Be(new DateTime(2018, 05, 02));
			ganttData.First().Tasks[1].To.Should().Be(new DateTime(2018, 05, 03));
		}

		[Test]
		public void ShouldCreateSeparateTasksPerBpo()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "OlaBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 10.5,
					OnDate = new DateTime(2018, 05, 01),
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(2);
			ganttData[0].Tasks.Count.Should().Be(1);
			ganttData[0].Name.Should().Be("MagnusBpo");
			ganttData[1].Tasks.Count.Should().Be(1);
			ganttData[1].Name.Should().Be("OlaBpo");
		}

		[Test]
		public void ShouldCreateSeparateTasksPerBpoAndPerImportFilename()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 01),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "OlaBpo",
					ImportFilename = "newdata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 10.5,
					OnDate = new DateTime(2018, 05, 02),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "OlaBpo",
					ImportFilename = "newdata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 10.5,
					OnDate = new DateTime(2018, 05, 03),
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "OlaBpo",
					ImportFilename = "evennewerdata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 10.5,
					OnDate = new DateTime(2018, 05, 04),
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(2);
			ganttData[0].Tasks.Count.Should().Be(1);
			ganttData[0].Name.Should().Be("MagnusBpo");
			ganttData[1].Tasks.Count.Should().Be(2);
			ganttData[1].Name.Should().Be("OlaBpo");
		}

		[Test]
		public void ShouldCreateSeparatesTasksForEachImportDate()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25, 12, 30, 0),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30)
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25, 12, 40, 0),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 05, 01)
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(1);
			ganttData.First().Tasks.Count.Should().Be(2);
			ganttData.First().Tasks.First().From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks.First().To.Should().Be(new DateTime(2018, 05, 01));
			ganttData.First().Tasks.Second().From.Should().Be(new DateTime(2018, 05, 01));
			ganttData.First().Tasks.Second().To.Should().Be(new DateTime(2018, 05, 02));
		}

		[Test]
		public void ShouldHandleSeveralModelsOnTheSameDate()
		{
			var model = new List<SkillCombinationResourceBpoTimelineModel>
			{
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25, 12, 30, 0),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30)
				},
				new SkillCombinationResourceBpoTimelineModel
				{
					Source = "MagnusBpo",
					ImportFilename = "bpodata.csv",
					ImportedDateTime = new DateTime(2018, 04, 25, 12, 40, 0),
					Firstname = "Magnus",
					Lastname = "Wedmark",
					Resources = 5.5,
					OnDate = new DateTime(2018, 04, 30)
				}
			};

			var ganttData = Target.TransformTimelineModelToGanttData(model);
			ganttData.Count.Should().Be(1);
			ganttData.First().Tasks.Count.Should().Be(2);
			ganttData.First().Tasks.First().From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks.First().To.Should().Be(new DateTime(2018, 05, 01));
			ganttData.First().Tasks.Second().From.Should().Be(new DateTime(2018, 04, 30));
			ganttData.First().Tasks.Second().To.Should().Be(new DateTime(2018, 05, 01));
		}

	}
}
