using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[EnabledBy(Toggles.WFM_Intraday_ImproveSkillCombinationDeltaLoad_80128)]
	public class SkillCombinationResourceRepositoryToggleOnTest: BaseSkillCombinationResourceRepositoryTest, IIsolateSystem
	{
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<SkillCombinationResourcesWithoutBpoToggleOn>().For<ISkillCombinationResourcesWithoutBpo>();
		}
	}

	[TestFixture]
	[UnitOfWorkTest]
	[DisabledBy(Toggles.WFM_Intraday_ImproveSkillCombinationDeltaLoad_80128)]
	public class SkillCombinationResourceRepositoryToggleOffTest : BaseSkillCombinationResourceRepositoryTest, IIsolateSystem
	{
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
			isolate.UseTestDouble<SkillCombinationResourcesWithoutBpoToggleOff>().For<ISkillCombinationResourcesWithoutBpo>();
		}
	}

	
	public class BaseSkillCombinationResourceRepositoryTest 
	{
		public ISkillCombinationResourceRepository Target;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public CurrentBusinessUnit CurrentBusinessUnit;
		public UpdateStaffingLevelReadModelStartDate UpdateStaffingLevelReadModelStartDate;
		
		private Guid persistSkill(IBusinessUnit businessUnit=null, string skillname = "skill")
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
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

		[Test]
		public void ShouldPersistSingleSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {persistSkill()}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);
			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {persistSkill()}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);
			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldPersistSkillCombinationResourceButKeep8DaysHistoricalData()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);
			Now.Is("2017-06-09 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldKeep8DaysHistoricalDataForDeltas()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1
				},
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1
				}
			});
			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-09 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skillId},
					StartDateTime = new DateTime(2017, 06, 09, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 09, 0, 15, 0),
					Resource = 1
				}
			});
			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(2);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
			loadedCombinationResources.Second().Resource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldLoadSkillCombinationInHexaDecimalOrder()
		{
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var skillIds = new List<Guid> {persistSkill(), persistSkill(), persistSkill()};
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2016, 12, 20, 0, 0, 0),
					EndDateTime = new DateTime(2016, 12, 20, 0, 15, 0),
					Resource = 1,
					SkillCombination = skillIds
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));

			skillIds.Sort();
			loadedCombinationResources.Single().SkillCombination.SequenceEqual(skillIds).Should().Be.True();
		}

		[Test]
		public void ShouldInsertDelta()
		{
			var skill = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);


			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 2,
					SkillCombination = new[] {skill}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = 1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(3d);
		}

		[Test]
		public void ShouldMergeDeltaWithPositiveAndNegativeValues()
		{
			var skill = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 2,
					SkillCombination = new[] {skill}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = 8
				}
			});
			Thread.Sleep(TimeSpan.FromSeconds(1)); //insertedOn is PK
			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -3
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(7d);
		}

		[Test]
		public void ShouldAddEmptySkillCombinationResourceIfThereIsNoWhenAddingDelta()
		{
			var skill = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource //to have skillCombination that we should always have
				{
					StartDateTime = start.AddHours(1),
					EndDateTime = end.AddHours(1),
					Resource = 1,
					SkillCombination = new[] {skill}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = 1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(1d);
		}

		[Test]
		public void ShouldMergeDeltatWithResourceWithMultipleSkills()
		{
			var skill = persistSkill();
			var skill2 = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill, skill2},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(2d);
		}

		[Test]
		public void ShouldMergeDeltatWithResourceWithMultipleSkillsInOneQuery()
		{
			var skill = persistSkill();
			var skill2 = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				},
				new SkillCombinationResource
				{
					StartDateTime = start.AddMinutes(15),
					EndDateTime = end.AddMinutes(15),
					Resource = 3,
					SkillCombination = new[] {skill, skill2}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill, skill2},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -1
				}
			});
			Thread.Sleep(TimeSpan.FromSeconds(1)); //insertedOn is PK
			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill, skill2},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start)).Resource.Should().Be.EqualTo(1d);
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start.AddMinutes(15))).Resource.Should().Be
				.EqualTo(3d);
		}

		[Test]
		public void ShouldReturnActualAndBpoResources()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(6.1);
		}

		[Test]
		public void ShouldRemoveResourcesOlderThan8Days()
		{
			var skillId = persistSkill();
			Now.Is("2017-05-01 08:00");
			
			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 05, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 05, 02, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			Now.Is("2017-06-01 08:00");
			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					Resources = 6.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(6.1);
		}

		[Test]
		public void ShouldOnlyAddResourcesForTheMatchingPeriod()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 2.1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					Resource = 6,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 3.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					Resources = 2.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);
			loadedCombinationResources.FirstOrDefault(x=>x.StartDateTime== new DateTime(2017, 06, 01, 0, 0, 0)).Resource.Should().Be.EqualTo(5.2);
			loadedCombinationResources.FirstOrDefault(x=>x.StartDateTime== new DateTime(2017, 06, 01, 0, 15, 0)).Resource.Should().Be.EqualTo(8.1);
		}
		
		[Test]
		public void ShouldReturnBpoResourcesEvenIfSkillCombResourceIsMissing()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId1}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId2}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);			
		}

		[Test]
		public void ShouldUpdateExistingBpoIntervals()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-01 08:00");
			
			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){ skillId1 }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 2.3,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){ skillId1 }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2.3);
		}

		[Test]
		public void ShouldNotUpdateExistingIntervalsBpoIntervals()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-01 08:00");

			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){ skillId1 }
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					Resources = 2.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){ skillId1 }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 1.5,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){ skillId1 }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 06, 01, 2017, 06, 02));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);
			loadedCombinationResources.Count(x => x.Resource == 1.5).Should().Be.EqualTo(1);
			loadedCombinationResources.Count(x => x.Resource == 2.1).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotAddBpoResourceFromAnotherBu()
		{
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			//for first bu
			var bu1 = CurrentBusinessUnit.Current();
			var skillId1 = persistSkill();
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId1}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);
			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId1}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			//for secondbu
			var bu2 = BusinessUnitFactory.CreateSimpleBusinessUnit("Test2");
			BusinessUnitRepository.Add(bu2);
			CurrentUnitOfWork.Current().PersistAll();

			CurrentBusinessUnit.OnThisThreadUse(bu2);
			var skillId2 = persistSkill(bu2);
			combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 2,
					SkillCombination = new[] {skillId2}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);
			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 2,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId2}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);


			CurrentBusinessUnit.OnThisThreadUse(bu2);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(4);
		}

		[Test]
		public void ShouldHandleDecimalRoudingWithBpoResources()
		{
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			//for first bu
			var bu1 = CurrentBusinessUnit.Current();
			var skillId1 = persistSkill();
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					//Resource = 1.1, this wont work
					Resource = 1.1,
					SkillCombination = new[] {skillId1}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);
			List<ImportSkillCombinationResourceBpo> combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					//Resources = 5.2, this wont work
					Resources = 8.5,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId1}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			//for secondbu
			var bu2 = BusinessUnitFactory.CreateSimpleBusinessUnit("Test2");
			BusinessUnitRepository.Add(bu2);
			CurrentUnitOfWork.Current().PersistAll();

			CurrentBusinessUnit.OnThisThreadUse(bu2);
			var skillId2 = persistSkill(bu2);
			combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 2.3,
					SkillCombination = new[] {skillId2}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);
			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 2.4,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId2}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);


			CurrentBusinessUnit.OnThisThreadUse(bu1);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(9.6);
		}

		[Test]
		public void ShouldNotMergeSkillCombResourceWhichAreAlmostTheSame()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));

			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 2.1,
					SkillCombination = new[] { skillId1 , skillId2}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 6,
					SkillCombination = new[] { skillId1 }
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);
			
			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12));
			loadedCombinationResources.Count().Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldNotAddSameBpoTwiceInSourceBpo()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);

			Dictionary<Guid, string> bpoList;
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				bpoList = Target.LoadSourceBpo(connection);
			}
			bpoList.Count.Should().Be.EqualTo(1);
		}
		
		[Test]
		public void ShouldRemoveLeadingAndTrailingWhiteSpaceInSourceBpo()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = " TPBrazil "
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);

			Dictionary<Guid, string> bpoList;
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				bpoList = Target.LoadSourceBpo(connection);
			}
			
			bpoList.Single().Value.Should().Be.EqualTo("TPBrazil");
		}
		
		[Test]
		public void ShouldHandleSourceBpoAsCaseInsensitive()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "tpbrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate.AddMinutes(15),
					EndDateTime = endDate.AddMinutes(15),
					Resources = 3.5,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);

			Dictionary<Guid, string> bpoList;
			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				bpoList = Target.LoadSourceBpo(connection);
			}
			bpoList.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldPersistDifferentBposSkillCombinationResource()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var combinationResources = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPBrazil"
				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{persistSkill()},
					Source = "TPParis"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResources);
			CurrentUnitOfWork.Current().PersistAll();

			var loadedBpoCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 2016, 12, 21)).ToList();
			loadedBpoCombinationResources.Count.Should().Be.EqualTo(2);
		}
		
		[Test]
		public void ShouldNotOverwriteSkillCombination()
		{
			Now.Is("2016-12-19 08:00");
			var startDate = new DateTime(2016, 12, 20, 0, 0, 0);
			var endDate = new DateTime(2016, 12, 20, 0, 15, 0);

			var skill = persistSkill(null, "skill");
			var skill2 = persistSkill(null, "skill2");
			
			var combinationResource2 = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{skill, skill2},
					Source = "TPBrazil"
				}
			};
			
			Target.PersistSkillCombinationResourceBpo(combinationResource2);
			CurrentUnitOfWork.Current().PersistAll();
			
			var combinationResource1 = new List<ImportSkillCombinationResourceBpo>
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = startDate,
					EndDateTime = endDate,
					Resources = 1,
					SkillIds = new List<Guid>{skill},
					Source = "TPBrazil"
				}
			};

			Target.PersistSkillCombinationResourceBpo(combinationResource1);
			CurrentUnitOfWork.Current().PersistAll();

			var loadedBpoCombinationResources = Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 2016, 12, 21)).ToList();
			loadedBpoCombinationResources.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldLoadFromDeltaOrResource()
		{
			var skill = persistSkill();
			
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			// so we persist the combination
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start.AddDays(1),
					EndDateTime = end.AddDays(1),
					Resource = 0,
					SkillCombination = new[] {skill}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = 3
				}
			});
			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill},
					StartDateTime = start,
					EndDateTime = end,
					Resource = 1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1),false);
			loadedCombinationResources.Single().Resource.Should().Be.EqualTo(4d);
		}
		
		[Test]
		public void ShouldReturnSumOfResourcesPerSkill()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.5,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>{skillId1, skillId2}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>{skillId1}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.BpoResourcesForSkill(skillId1, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 01)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be(10.6);
		}
		
		[Test]
		public void ShouldReturnSumOfResourcesPerBpo()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1, skillId2}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>{skillId1}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var skill1Resources =
				Target.BpoResourcesForSkill(skillId1, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 01)).ToList();
			skill1Resources.Count.Should().Be.EqualTo(2);
			
			var skill2Resources =
				Target.BpoResourcesForSkill(skillId2, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 01)).ToList();
			skill2Resources.Count.Should().Be.EqualTo(1);
			skill2Resources.First().Resource.Should().Be(5.5);
		}
		
		[Test]
		public void ShouldReturnHeadsPerSkill()
		{
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));

			var combinationResources = new List<SkillCombinationResource>()
			{
				new SkillCombinationResource()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 5.5,
					SkillCombination = new List<Guid>{skillId1, skillId2}
				},
				new SkillCombinationResource()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 5.1,
					SkillCombination = new List<Guid>{skillId1}
				}
			};
			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			var skill1Resources =
				Target.ScheduledHeadsForSkill(skillId1, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 01)).ToList();
			skill1Resources.Count.Should().Be.EqualTo(1);
			skill1Resources.First().Heads.Should().Be(10.6);
			
			var skill2Resources =
				Target.ScheduledHeadsForSkill(skillId2, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 01)).ToList();
			skill2Resources.Count.Should().Be.EqualTo(1);
			skill2Resources.First().Heads.Should().Be(5.5);
		}

		[Test]
		public void ShouldReturnZeroIfLessThanZeroResources()
		{
			var skill = persistSkill();
			var skill2 = persistSkill();
			Now.Is("2016-12-19 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var start = new DateTime(2016, 12, 20, 0, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2016, 12, 20, 0, 15, 0, DateTimeKind.Utc);

			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = start,
					EndDateTime = end,
					Resource = 1,
					SkillCombination = new[] {skill, skill2}
				},
				new SkillCombinationResource
				{
					StartDateTime = start.AddMinutes(15),
					EndDateTime = end.AddMinutes(15),
					Resource = 1,
					SkillCombination = new[] {skill, skill2}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill, skill2},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -1
				}
			});
			Thread.Sleep(TimeSpan.FromSeconds(1)); //insertedOn is PK
			Target.PersistChanges(new[]
			{
				new SkillCombinationResource
				{
					SkillCombination = new[] {skill, skill2},
					StartDateTime = start,
					EndDateTime = end,
					Resource = -1
				}
			});

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(2016, 12, 20, 0, 2016, 12, 20, 1));
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start)).Resource.Should().Be.EqualTo(0d);
			loadedCombinationResources.Single(x => x.StartDateTime.Equals(start.AddMinutes(15))).Resource.Should().Be
				.EqualTo(1d);
		}

		[Test]
		public void ShouldincludeIntervalsThatBelogToEndDay()
		{
			var skillId1 = persistSkill();
			Now.Is("2017-06-01 08:00");

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1}

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 03, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 03, 0, 15, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{skillId1}
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var skillResources =
				Target.BpoResourcesForSkill(skillId1, new DateOnlyPeriod(2017, 06, 01, 2017, 06, 02)).ToList();
			skillResources.Count.Should().Be.EqualTo(2);
			skillResources.Any(x => x.StartDateTime == new DateTime(2017, 06, 03, 0, 0, 0)).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldGetAllActiveBpo()
		{
			Now.Is("2017-06-01 08:00");
			var skillId =  persistSkill() ;
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPBrazil",
					SkillIds = new List<Guid>{ skillId }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var activeBpo =Target.LoadActiveBpos().ToList();

			activeBpo.Count.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldGetAllActiveBpoForCurrentBu()
		{
			Now.Is("2017-06-01 08:00");
			var skillId1 = persistSkill();
			var skillId2 = persistSkill();
			var bu1 = CurrentBusinessUnit.Current();
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bu2 = BusinessUnitFactory.CreateSimpleBusinessUnit("Test2");
			BusinessUnitRepository.Add(bu2);
			CurrentUnitOfWork.Current().PersistAll();

			CurrentBusinessUnit.OnThisThreadUse(bu2);

			combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPBrazil",
					SkillIds = new List<Guid>{ skillId2 }

				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			CurrentBusinessUnit.OnThisThreadUse(bu1);

			var activeBpo = Target.LoadActiveBpos().ToList();

			activeBpo.Count.Should().Be.EqualTo(1);
			activeBpo.First().Source.Should().Be.EqualTo("TPSWEDEN");
		}

		[Test]
		public void ShouldOnlyReturnEachBpoOnce()
		{
			Now.Is("2017-06-01 08:00");
			var skillId1 = persistSkill();
			
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var activeBpo = Target.LoadActiveBpos().ToList();

			activeBpo.Count.Should().Be.EqualTo(1);
			activeBpo.First().Source.Should().Be.EqualTo("TPSWEDEN");
		}

		[Test]
		public void ShouldIgnoreInActiveBpo()
		{
			Now.Is("2017-06-01 08:00");
			var skillId = persistSkill();
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resources = 5.1,
					Source = "TPBrazil",
					SkillIds = new List<Guid>{ skillId }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				using (var insertCommand = new SqlCommand(@"insert into [BusinessProcessOutsourcer] (Id, Source) Values (@id,@source)", connection))
				{
					insertCommand.Parameters.AddWithValue("@id", Guid.NewGuid());
					insertCommand.Parameters.AddWithValue("@source", "TPPakistan");
					insertCommand.ExecuteNonQuery();
				}
			}

			var activeBpo = Target.LoadActiveBpos().ToList();
			activeBpo.Count.Should().Be.EqualTo(2);
			activeBpo.Any(x=>x.Source.Equals("TPPakistan")).Should().Be.False();
		}

		[Test]
		public void ShouldRemoveBpoResourceOnGivenBpo()
		{
			Now.Is("2017-06-01 08:00");
			var skillId = persistSkill();
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 03, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 03, 0, 30, 0),
					Resources = 5.1,
					Source = "TPBrazil",
					SkillIds = new List<Guid>{ skillId }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoGuid = Target.LoadActiveBpos().FirstOrDefault(x => x.Source.Equals("TPBrazil")).Id;
			var dateTimePeriod = new DateTimePeriod(new DateTime(2017, 06, 03, 0, 0, 0,DateTimeKind.Utc), new DateTime(2017, 06, 03, 23, 0, 0, DateTimeKind.Utc));
			Target.ClearBpoResources(bpoGuid, dateTimePeriod);
			CurrentUnitOfWork.Current().PersistAll();

			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				using (var command = new SqlCommand(@"select count(*) from ReadModel.SkillCombinationResourceBpo where sourceId = @bpoGuid", connection))
				{
					command.Parameters.AddWithValue("@bpoGuid", bpoGuid);
					var count = (Int32)command.ExecuteScalar();
					count.Should().Be.EqualTo(0);
				}
			}

		}
		[Test]
		public void ShouldRemoveBpoResourceOnGivenDate()
		{
			Now.Is("2017-06-01 08:00");
			var skillId = persistSkill();
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 03, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 03, 0, 15, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 4, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 4, 0, 15, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoGuid = Target.LoadActiveBpos().FirstOrDefault(x => x.Source.Equals("TPSWEDEN")).Id;
			var dateTimePeriod = new DateTimePeriod(new DateTime(2017, 06, 2, 0, 0, 0,DateTimeKind.Utc), new DateTime(2017, 06, 03, 23, 0, 0,DateTimeKind.Utc));
			Target.ClearBpoResources(bpoGuid, dateTimePeriod);
			CurrentUnitOfWork.Current().PersistAll();

			using (var connection = new SqlConnection(InfraTestConfigReader.ConnectionString))
			{
				connection.Open();
				var returnList = new List<DateTime>();
				using (var command = new SqlCommand(@"select StartDateTime from ReadModel.SkillCombinationResourceBpo where sourceId = @bpoGuid", connection))
				{
					command.Parameters.AddWithValue("@bpoGuid", bpoGuid);
					var reader = command.ExecuteReader();
					while (reader.Read())
					{
						returnList.Add(reader.GetDateTime(0));

					}
				}
				returnList.Any(x=>x==new DateTime(2017, 06, 2, 0, 0, 0)).Should().Be.False();
				returnList.Any(x=>x==new DateTime(2017, 06, 3, 0, 0, 0)).Should().Be.False();
			}

		}

		[Test]
		public void ShouldGetNumberOfDeletedBpoResource()
		{
			Now.Is("2017-06-01 08:00");
			var skillId = persistSkill();
			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }

				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 03, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 03, 0, 15, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 4, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 4, 0, 15, 0),
					Resources = 5.1,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId }
				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpoGuid = Target.LoadActiveBpos().FirstOrDefault(x => x.Source.Equals("TPSWEDEN")).Id;
			var dateTimePeriod = new DateTimePeriod(new DateTime(2017, 06, 2, 0, 0, 0, DateTimeKind.Utc), new DateTime(2017, 06, 03, 23, 0, 0, DateTimeKind.Utc));
			 Target.ClearBpoResources(bpoGuid, dateTimePeriod).Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldReturnRangeWithData()
		{
			Now.Is("2017-06-01 08:00");
			var skillId1 = persistSkill();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 20, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 20, 0, 30, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpo = Target.LoadActiveBpos().FirstOrDefault();
			var range = Target.GetRangeForBpo(bpo.Id);

			range.StartDate.Should().Be.EqualTo(new DateTime(2017, 06, 02, 0, 0, 0));
			range.EndDate.Should().Be.EqualTo(new DateTime(2017, 06, 20, 0, 30, 0));
		}

		[Test]
		public void ShouldHandleNoData()
		{
			Now.Is("2017-06-01 08:00");
			var skillId1 = persistSkill();

			var combinationResourcesBpo = new List<ImportSkillCombinationResourceBpo>()
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				},
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = new DateTime(2017, 06, 20, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 20, 0, 30, 0),
					Resources = 5.5,
					Source = "TPSWEDEN",
					SkillIds = new List<Guid>{ skillId1 }

				}
			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var bpo = Target.LoadActiveBpos().FirstOrDefault();
			var range = Target.GetRangeForBpo(Guid.NewGuid());

			range.StartDate.Should().Be.EqualTo(new DateTime());
			range.EndDate.Should().Be.EqualTo(new DateTime());
		}

		[Test]
		public void ShouldRemoveOldIntervalsBeforeAddingNewOne()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 09:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldRemoveExistingAndFutureIntervalsBeforeAddingNew()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 09:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldRemoveDeletedHeadsWithZeroStaffing()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 09:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 02, 0, 30, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
		}

		[Test]
		public void ShouldRemoveDeletedHeadsHandlingUtc()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 22, 0, 0),
					EndDateTime = new DateTime(2017, 06, 02, 23, 0, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 02, 23, 0, 0),
					EndDateTime = new DateTime(2017, 06, 3, 0, 0, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 3, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 3, 1, 0, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 3, 1, 0, 0),
					EndDateTime = new DateTime(2017, 06, 3, 2, 0, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-01 09:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 3, 1, 0, 0),
					EndDateTime = new DateTime(2017, 06, 3, 2, 0, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
		}


		[Test]
		public void ShouldNotremoveIntervalsInHistoricalData()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-02 07:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 6, 45, 0),
					EndDateTime = new DateTime(2017, 06, 01, 7, 0, 0),
					Resource = 1,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 7, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 7, 15, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				},
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 7, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 7, 30, 0),
					Resource = 3,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResourcesOld);

			CurrentUnitOfWork.Current().PersistAll();

			Now.Is("2017-06-02 08:00");
			UpdateStaffingLevelReadModelStartDate.RememberStartDateTime(Now.UtcDateTime().AddDays(-1).AddHours(-1));
			var combinationResources = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 7, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 7, 30, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				}
			};

			Target.PersistSkillCombinationResource(Now.UtcDateTime(), combinationResources);

			CurrentUnitOfWork.Current().PersistAll();

			var loadedCombinationResources = Target
				.LoadSkillCombinationResources(new DateTimePeriod(2017, 01, 01, 2017, 12, 12)).ToList();
			loadedCombinationResources.Count.Should().Be.EqualTo(2);
			//loadedCombinationResources.First().Resource.Should().Be.EqualTo(2);
		}
	}
}
