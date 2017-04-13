using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	[Toggle(Toggles.ETL_EventbasedDate_39562)]
	[TestFixture]
	[DomainTest]
	public class PersonPeriodMapTests : ISetup
	{
		public FakeAnalyticsSkillRepository AnalyticsSkillRepository;
		public PersonPeriodTransformer Target;
		public FakeAnalyticsDateRepository AnalyticsDateRepository;

		private readonly AnalyticsSkill fakeSkill1 = new AnalyticsSkill
		{
			SkillId = 1,
			SkillCode = Guid.NewGuid()
		};
		private readonly AnalyticsSkill fakeSkill2 = new AnalyticsSkill
		{
			SkillId = 2,
			SkillCode = Guid.NewGuid()
		};
		private readonly AnalyticsSkill fakeSkill3 = new AnalyticsSkill
		{
			SkillId = 3,
			SkillCode = Guid.NewGuid()
		};

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeGlobalSettingDataRepository>().For<IGlobalSettingDataRepository>();
			system.AddService<PersonPeriodTransformer>();
		}

		private void setupTests()
		{

			var fakeSkills = new List<AnalyticsSkill>
			{
				fakeSkill1,
				fakeSkill2,
				fakeSkill3
			};
			AnalyticsSkillRepository.SetSkills(fakeSkills);

			var skills1 = fakeSkills; // 1,2,3
			var skills2 = fakeSkills.Take(2).ToList(); // 1,2
			var skills3 = fakeSkills.Take(1).ToList(); // 1

			var skillSet1 = PersonPeriodTransformer.NewSkillSetFromSkills(skills1);
			var skillSet2 = PersonPeriodTransformer.NewSkillSetFromSkills(skills2);
			var skillSet3 = PersonPeriodTransformer.NewSkillSetFromSkills(skills3);

			skillSet1.SkillsetId = 1;
			skillSet2.SkillsetId = 2;
			skillSet3.SkillsetId = 3;

			var fakeSkillSets = new List<AnalyticsSkillSet>
			{
				skillSet1,
				skillSet2,
				skillSet3
			};

			foreach (var newBridges in PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills1, skillSet1.SkillsetId))
			{
				AnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			foreach (var newBridges in PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills2, skillSet2.SkillsetId))
			{
				AnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			foreach (var newBridges in PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills3, skillSet3.SkillsetId))
			{
				AnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			AnalyticsSkillRepository.SetSkillSets(fakeSkillSets);

			AnalyticsDateRepository.HasDatesBetween(new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));
		}

		[Test]
		public void NoSkills_MapSkillSet_NotDefined()
		{
			setupTests();

			var skillsList = new List<Guid>();
			var skillSet = Target.MapSkillsetId(skillsList, 0, new ReturnNotDefined());
			Assert.AreEqual(-1, skillSet);
		}

		[Test]
		public void OneSkill_MapSkillSet_GotCorrectSkillSet()
		{
			setupTests();

			var skills = new List<Guid> { fakeSkill1.SkillCode };
			var skillSet = Target.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(3, skillSet);
		}

		[Test]
		public void TwoSkills_MapSkillSet_GotCorrectSkillSet()
		{
			setupTests();

			var skills = new List<Guid>
			{
				fakeSkill1.SkillCode,
				fakeSkill2.SkillCode
			};
			var skillSet = Target.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(2, skillSet);
		}

		[Test]
		public void OneSkill_MapSkillSet_NewSkillSet()
		{
			setupTests();

			var skills = new List<Guid> { fakeSkill2.SkillCode };
			var newSkillSetId = Target.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(4, newSkillSetId);
		}

		[Test]
		public void OneNewSkillNotYetInAnalytics_MapSkillSet_NotDefinedSkillset()
		{
			setupTests();

			var skills = new List<Guid> { Guid.NewGuid() };
			var newSkillSetId = Target.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(-1, newSkillSetId);
		}

		[Test]
		public void OneSkill_MapNewSkillSet_NewBridgeRowsForSkillSet()
		{
			setupTests();

			var nrOfBridgeRows = AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var skills = new List<Guid> { fakeSkill2.SkillCode };
			Target.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows + 1, AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
		}

		[Test]
		public void OneSkill_MapSkillSet_NoNewBridgeOrSkillSet()
		{
			setupTests();

			var nrOfBridgeRows = AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var nrOfSkillSets = AnalyticsSkillRepository.SkillSets().Count;

			var skills = new List<Guid> { fakeSkill1.SkillCode };
			Target.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows, AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
			Assert.AreEqual(nrOfSkillSets, AnalyticsSkillRepository.SkillSets().Count);
		}

		[Test]
		public void TwoSkill_MapNewSkillSet_TwoNewBridgeRowsAndSkillSet()
		{
			setupTests();

			var nrOfBridgeRows = AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var nrOfSkillSets = AnalyticsSkillRepository.SkillSets().Count;

			var skills = new List<Guid>
			{
				fakeSkill2.SkillCode,
				fakeSkill3.SkillCode
			};

			Target.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows + 2, AnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
			Assert.AreEqual(nrOfSkillSets + 1, AnalyticsSkillRepository.SkillSets().Count);
		}

		[Test]
		public void OneSkill_TransformToNewSkillSet_CorrectNewSkillSet()
		{
			setupTests();

			var skills = new List<AnalyticsSkill>
			{
				new AnalyticsSkill
				{
					SkillId = 1,
					SkillName = "Skill1",
					SkillCode = Guid.NewGuid(),
					BusinessUnitId = 123,
					DatasourceUpdateDate = new DateTime(2011, 1, 1)
				}
			};

			var skillSet = PersonPeriodTransformer.NewSkillSetFromSkills(skills);
			Assert.AreEqual(123, skillSet.BusinessUnitId);
			Assert.AreEqual(new DateTime(2011, 1, 1), skillSet.DatasourceUpdateDate);
			Assert.AreEqual("1", skillSet.SkillsetCode);
			Assert.AreEqual("Skill1", skillSet.SkillsetName);
		}

		[Test]
		public void TwoSkill_TransformToNewSkillSet_CorrectNewSkillSet()
		{
			setupTests();

			var skills = new List<AnalyticsSkill>
			{
				new AnalyticsSkill
				{
					SkillId = 32,
					SkillName = "Skill32",
					SkillCode = Guid.NewGuid(),
					BusinessUnitId = 123,
					DatasourceUpdateDate = new DateTime(2012, 1, 1)
				},
				new AnalyticsSkill
				{
					SkillId = 1,
					SkillName = "Skill1",
					SkillCode = Guid.NewGuid(),
					BusinessUnitId = 123,
					DatasourceUpdateDate = new DateTime(2011, 1, 1)
				}
			};

			var skillSet = PersonPeriodTransformer.NewSkillSetFromSkills(skills);
			Assert.AreEqual(123, skillSet.BusinessUnitId);
			Assert.AreEqual(new DateTime(2012, 1, 1), skillSet.DatasourceUpdateDate);
			Assert.AreEqual("1,32", skillSet.SkillsetCode);
			Assert.AreEqual("Skill1,Skill32", skillSet.SkillsetName);
		}

		[Test]
		public void OneSkill_TransformToBridgeRows_CorrectBridge()
		{
			setupTests();

			var skills = new List<AnalyticsSkill>
			{
				new AnalyticsSkill
				{
					SkillId = 42,
					BusinessUnitId = 123,
					DatasourceId = 321
				}
			};
			var bridges = PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills, 1);
			var analyticsBridgeSkillsetSkills = bridges as IList<AnalyticsBridgeSkillsetSkill> ?? bridges.ToList();
			Assert.AreEqual(1, analyticsBridgeSkillsetSkills.Count);

			var bridge = analyticsBridgeSkillsetSkills.First();
			Assert.AreEqual(1, bridge.SkillsetId);
			Assert.AreEqual(42, bridge.SkillId);
			Assert.AreEqual(123, bridge.BusinessUnitId);
			Assert.AreEqual(321, bridge.DatasourceId);
		}
	}
}