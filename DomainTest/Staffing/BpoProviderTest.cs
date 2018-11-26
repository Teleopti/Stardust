using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using DateTime = System.DateTime;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	public class BpoProviderTest : IIsolateSystem
	{
		public MutableNow Now;
		public BpoProvider Target;
		public FakeSkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeUserTimeZone UserTimeZone;
		public FakeUserCulture UserCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeStaffingSettingsReader>().For<FakeStaffingSettingsReader, IStaffingSettingsReader>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
		}

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

		[Test]
		public void ShouldClearBpoResources()
		{
			DateTime startDate  = new DateTime(2018,6,14,0,0,0);
			DateTime endDate = new DateTime(2018, 6, 15, 0, 0, 0);
			UserTimeZone.IsSweden();

			var combinationResources = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 13, 22, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 13, 23, 0, 0, DateTimeKind.Utc)
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 13, 23, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 14, 0, 0, 0, DateTimeKind.Utc)
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 14, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 14, 1, 0, 0, DateTimeKind.Utc),
					
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResources);

			var ret = Target.ClearBpoResources(Guid.NewGuid(), startDate, endDate);

			ret.SuccessMessage.Should().Not.Be.Empty();

			SkillCombinationResourceRepository.LoadSkillCombinationResourcesBpo().Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldReturnRangeMessage()
		{
			UserCulture.IsSwedish();
			UserTimeZone.IsSweden();

			var combinationResources = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 13, 22, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 13, 23, 0, 0, DateTimeKind.Utc),
					Source = "OUTSOURCER"
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 13, 23, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 14, 0, 0, 0, DateTimeKind.Utc),
					Source = "OUTSOURCER"
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2018, 6, 14, 0, 0, 0, DateTimeKind.Utc),
					EndDateTime = new DateTime(2018, 6, 14, 1, 0, 0, DateTimeKind.Utc),
					Source = "OUTSOURCER"
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResources);
			var id = Guid.NewGuid();
			SkillCombinationResourceRepository.BpoResourceRange = new BpoResourceRangeRaw{StartDate = new DateTime(2018,06,14,22,00,00), EndDate = new DateTime(2018,06,25,22,00,00)};
			SkillCombinationResourceRepository.ActiveBpos.Add(new ActiveBpoModel {Id = id, Source = "OUTSOURCER"});
			var mess = Target.GetRangeMessage(id).Message;
			mess.Should().Contain("OUTSOURCER");
			mess.Should().Contain("2018-06-15");
			mess.Should().Contain("2018-06-26");
		}

	}
}
