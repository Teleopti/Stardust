using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class SkillCombinationResourceRepositoryTest : ISetup
	{
		public ISkillCombinationResourceRepository Target;
		public MutableNow Now;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public IBusinessUnitRepository BusinessUnitRepository;
		public CurrentBusinessUnit CurrentBusinessUnit;
		
		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<CurrentBusinessUnit>().For<ICurrentBusinessUnit>();
		}
		private Guid persistSkill(IBusinessUnit businessUnit=null)
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
			var skill = new Skill("skill", "skill", Color.Blue, 15, skillType)
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
		public void ShouldOnlyAddResourcesForTheMatchingPeriod()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
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
		public void ShouldNotAddBpoResourceFromAnotherBu()
		{
			Now.Is("2017-06-01 08:00");
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

		[Test, Ignore("WIP")]
		public void ShouldHandleDecimalRoudingWithBpoResources()
		{
			Now.Is("2017-06-01 08:00");
			//for first bu
			var bu1 = CurrentBusinessUnit.Current();
			var skillId1 = persistSkill();
			var combinationResourcesOld = new List<SkillCombinationResource>
			{
				new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 0, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					Resource = 1.1,
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
					Resources = 5.2,
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
					Resource = 2.3,
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
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(6.3);
		}

		[Test]
		public void ShouldReturnActualAndBpoResourcesTESTTT()
		{
			var skillId = persistSkill();
			Now.Is("2017-06-01 08:00");
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
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0,30, 0),
					Resource = 2,
					SkillCombination = new[] {skillId}
				},new SkillCombinationResource
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 45, 0),
					Resource = 3,
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
					Resources = 6.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				},
				new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 15, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					Resources = 7.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				},new ImportSkillCombinationResourceBpo()
				{
					StartDateTime = new DateTime(2017, 06, 01, 0, 30, 0),
					EndDateTime = new DateTime(2017, 06, 01, 0, 45, 0),
					Resources = 8.1,
					Source = "TPBRAZIL",
					SkillIds = new List<Guid>(){skillId}
				}

			};
			Target.PersistSkillCombinationResourceBpo(combinationResourcesBpo);

			var loadedCombinationResources =
				Target.LoadSkillCombinationResources(new DateTimePeriod(new DateTime(2017, 06, 01, 0, 15, 0,DateTimeKind.Utc), new DateTime(2017, 06, 01, 0, 30, 0,DateTimeKind.Utc)));
			loadedCombinationResources.Count().Should().Be.EqualTo(1);
			loadedCombinationResources.First().Resource.Should().Be.EqualTo(9.1);
		}
	}
}
