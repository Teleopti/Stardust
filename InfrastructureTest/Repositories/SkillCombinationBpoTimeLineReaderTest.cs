using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Intraday.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.SkillGroupManagement;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillCombinationBpoTimeLineReaderTest: IIsolateSystem
	{
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public ISkillCombinationBpoTimeLineReader Target;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public CurrentBusinessUnit CurrentBusinessUnit;
		public IPersonRepository PersonRepository;
		public ISkillGroupRepository SkillGroupRepository;
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<SkillCombinationBpoTimeLineReader>();
		}
		
		[Test]
		public void ShouldGetAllDataForBpoTimeline()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoTimelineData =
				Target.GetAllDataForBpoTimeline().ToList();
			
			bpoTimelineData.Count.Should().Be.EqualTo(1);
			bpoTimelineData.First().Resources.Should().Be.EqualTo(10.6);
			bpoTimelineData.First().Firstname.Should().Be.EqualTo("Magnus");
			bpoTimelineData.First().Lastname.Should().Be.EqualTo("Wedmark");
			bpoTimelineData.First().ImportedDateTime.Should().Be.EqualTo(Now.UtcDateTime());
		}

		[Test]
		public void ShouldGroupBpoTimelineDataByDate()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 03, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 03, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoTimelineData =
				Target.GetAllDataForBpoTimeline().ToList();

			bpoTimelineData.Count.Should().Be.EqualTo(2);
			bpoTimelineData[0].Resources.Should().Be.EqualTo(10.6);
			bpoTimelineData[1].Resources.Should().Be.EqualTo(5.1);
		}

		[Test]
		public void ShouldGroupBpoTimelineDataBySource()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPITALY",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoTimelineData =
				Target.GetAllDataForBpoTimeline().ToList();

			bpoTimelineData.Count.Should().Be.EqualTo(2);
			bpoTimelineData[0].Resources.Should().Be.EqualTo(5.1);
			bpoTimelineData[0].Source.Should().Be.EqualTo("TPITALY");
			bpoTimelineData[1].Resources.Should().Be.EqualTo(10.6);
			bpoTimelineData[1].Source.Should().Be.EqualTo("TPSWEDEN");
		}

		[Test]
		public void ShouldGroupBpoTimelineDataByImportFilename()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170602_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170602_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 1, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 1, 15, 0),
					Resources = 5.2,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170602_TPSWEDEN_NEW",
					PersonId = person.Id.GetValueOrDefault()
				}
			};

			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);
			var bpoTimelineData = Target.GetAllDataForBpoTimeline().ToList();

			bpoTimelineData.Count.Should().Be.EqualTo(2);
			bpoTimelineData.Count(b => b.ImportFilename == "20170602_TPSWEDEN").Should().Be(1);
			bpoTimelineData.Count(b => b.ImportFilename == "20170602_TPSWEDEN_NEW").Should().Be(1);
		}

		[Test]
		public void ShouldGetDataForBpoTimelineForSkill()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoTimelineData = Target.GetBpoTimelineDataForSkill(skillId2).ToList();

			bpoTimelineData.Count.Should().Be.EqualTo(1);
			bpoTimelineData.First().Resources.Should().Be.EqualTo(5.1);
			bpoTimelineData.First().Firstname.Should().Be.EqualTo("Magnus");
			bpoTimelineData.First().Lastname.Should().Be.EqualTo("Wedmark");
		}

		[Test]
		public void ShouldGetDataForBpoTimelineForSkillGroup()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			var skillId3 = persistSkill();

			var intradaySkill1 = new SkillInIntraday { Id = skillId1 };
			var intradaySkill3 = new SkillInIntraday { Id = skillId3 };

			var skillGroup = new SkillGroup {Name = "SkillGroup"};
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1 , intradaySkill3};
			SkillGroupRepository.Add(skillGroup);
			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 2.2,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId3},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoTimelineData = Target.GetBpoTimelineDataForSkillGroup(skillGroup.Id.GetValueOrDefault()).ToList();

			bpoTimelineData.Count.Should().Be.EqualTo(1);
			bpoTimelineData.First().Resources.Should().Be.EqualTo(7.7);
			bpoTimelineData.First().Firstname.Should().Be.EqualTo("Magnus");
			bpoTimelineData.First().Lastname.Should().Be.EqualTo("Wedmark");
		}

		[Test]
		public void ShouldGetImportInfoForSkill()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSPAKISTAN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSPAKISTAN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);
			var fromDate = new DateTime(2017,06,02);
			var bpoImportInfoModels = Target.GetBpoImportInfoForSkill(skillId1, fromDate, fromDate.AddDays(1)).ToList();

			bpoImportInfoModels.Count.Should().Be.EqualTo(2);
			bpoImportInfoModels.First().Firstname.Should().Be.EqualTo("Magnus");
			bpoImportInfoModels.First().Lastname.Should().Be.EqualTo("Wedmark");
		}

		[Test]
		public void ShouldNotGetImportInfoForSkillForMidnightInterval()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-29 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 29, 22, 0, 0),
					EndDateTime = new DateTime(2017, 06, 29, 22, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);
			var fromDate = new DateTime(2017, 06, 28,22,0,0,DateTimeKind.Utc);
			var bpoImportInfoModels = Target.GetBpoImportInfoForSkill(skillId1, fromDate, fromDate.AddDays(1)).ToList();

			bpoImportInfoModels.Count.Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldGetImportInfoForSkillForMidnightInterval()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-29 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 29, 22, 0, 0),
					EndDateTime = new DateTime(2017, 06, 29, 22, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);
			var fromDate = new DateTime(2017, 06, 29, 22, 0, 0, DateTimeKind.Utc);
			var bpoImportInfoModels = Target.GetBpoImportInfoForSkill(skillId1, fromDate, fromDate.AddDays(1)).ToList();

			bpoImportInfoModels.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetImportInfoForSkillGroup()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			var skillId3 = persistSkill();

			var intradaySkill1 = new SkillInIntraday { Id = skillId1 };
			var intradaySkill3 = new SkillInIntraday { Id = skillId3 };

			var skillGroup = new SkillGroup { Name = "SkillGroup" };
			skillGroup.Skills = new List<SkillInIntraday> { intradaySkill1, intradaySkill3 };
			SkillGroupRepository.Add(skillGroup);
			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 08:00");
			var person = PersonFactory.CreatePerson("Magnus", "Wedmark");
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId2},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 2.2,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId3},
					ImportFileName = "20170601_TPSWEDEN",
					PersonId = person.Id.GetValueOrDefault()
				}
			};
			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResourcesBpo);
			var fromDate = new DateTime(2017, 06, 02);
			var bpoImportInfoModels = Target.GetBpoImportInfoForSkillGroup(skillGroup.Id.GetValueOrDefault(), fromDate, fromDate.AddDays(1)).ToList();

			bpoImportInfoModels.Count.Should().Be.EqualTo(1);
			bpoImportInfoModels.First().Firstname.Should().Be.EqualTo("Magnus");
			bpoImportInfoModels.First().Lastname.Should().Be.EqualTo("Wedmark");
		}


		private Guid persistSkill(IBusinessUnit businessUnit = null, string skillname = "skill")
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillTypePhone();
			var skill = new Skill(skillname, skillname, Color.Blue, 15, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			};

			if (businessUnit != null)
			{
				activity.SetBusinessUnit(businessUnit);
				skill.SetBusinessUnit(businessUnit);
			}
			SkillTypeRepository.Add(skillType);
			ActivityRepository.Add(activity);
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();
			return skill.Id.GetValueOrDefault();
		}
	}
}