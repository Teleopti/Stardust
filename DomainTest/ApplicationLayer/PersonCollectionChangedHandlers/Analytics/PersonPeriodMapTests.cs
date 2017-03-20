using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class PersonPeriodMapTests
	{
		private FakeAnalyticsPersonPeriodRepository fakeAnalyticsPersonPeriodRepository;
		private FakeAnalyticsSkillRepository fakeAnalyticsSkillRepository;
		private FakeAnalyticsBusinessUnitRepository fakeAnalyticsBusinessUnitRepository;
		private FakeAnalyticsTeamRepository fakeAnalyticsTeamRepository;
		private PersonPeriodTransformer personPeriodTransformer;
		private IAnalyticsDateRepository _analyticsDateRepository;
		private IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private FakeGlobalSettingDataRepository _globalSettingDataRepository;
		private AnalyticsPersonPeriodDateFixer _analyticsPersonPeriodDateFixer;

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

		[SetUp]
		public void SetupTests()
		{
			_analyticsIntervalRepository = new FakeAnalyticsIntervalRepository();
			fakeAnalyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository();
			fakeAnalyticsSkillRepository = new FakeAnalyticsSkillRepository();

			var fakeSkills = new List<AnalyticsSkill>
			{
				fakeSkill1,
				fakeSkill2,
				fakeSkill3
			};
			fakeAnalyticsSkillRepository.SetSkills(fakeSkills);

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
				fakeAnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			foreach (var newBridges in PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills2, skillSet2.SkillsetId))
			{
				fakeAnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			foreach (var newBridges in PersonPeriodTransformer.NewBridgeSkillSetSkillsFromSkills(skills3, skillSet3.SkillsetId))
			{
				fakeAnalyticsSkillRepository.AddBridgeSkillsetSkill(newBridges);
			}
			fakeAnalyticsSkillRepository.SetSkillSets(fakeSkillSets);

			fakeAnalyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository();
			fakeAnalyticsTeamRepository = new FakeAnalyticsTeamRepository();
			_analyticsDateRepository = new FakeAnalyticsDateRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));
			_analyticsTimeZoneRepository = new FakeAnalyticsTimeZoneRepository();

			_globalSettingDataRepository = new FakeGlobalSettingDataRepository();

			
			_analyticsPersonPeriodDateFixer = new AnalyticsPersonPeriodDateFixer(_analyticsDateRepository, _analyticsIntervalRepository);
			personPeriodTransformer = new PersonPeriodTransformer(fakeAnalyticsPersonPeriodRepository,
				fakeAnalyticsSkillRepository, 
				fakeAnalyticsBusinessUnitRepository, 
				fakeAnalyticsTeamRepository, 
				new ReturnNotDefined(), 
				_analyticsTimeZoneRepository, 
				_analyticsIntervalRepository,
				_globalSettingDataRepository,
				_analyticsPersonPeriodDateFixer);
		}

		[Test]
		public void NoSkills_MapSkillSet_NotDefined()
		{
			var skillsList = new List<Guid>();
			var skillSet = personPeriodTransformer.MapSkillsetId(skillsList, 0, new ReturnNotDefined());
			Assert.AreEqual(-1, skillSet);
		}

		[Test]
		public void OneSkill_MapSkillSet_GotCorrectSkillSet()
		{
			var skills = new List<Guid> { fakeSkill1.SkillCode };
			var skillSet = personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(3, skillSet);
		}

		[Test]
		public void TwoSkills_MapSkillSet_GotCorrectSkillSet()
		{
			var skills = new List<Guid>
			{
				fakeSkill1.SkillCode,
				fakeSkill2.SkillCode
			};
			var skillSet = personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(2, skillSet);
		}

		[Test]
		public void OneSkill_MapSkillSet_NewSkillSet()
		{
			var skills = new List<Guid> { fakeSkill2.SkillCode };
			var newSkillSetId = personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(4, newSkillSetId);
		}

		[Test]
		public void OneNewSkillNotYetInAnalytics_MapSkillSet_NotDefinedSkillset()
		{
			var skills = new List<Guid> { Guid.NewGuid() };
			var newSkillSetId = personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());
			Assert.AreEqual(-1, newSkillSetId);
		}

		[Test]
		public void OneSkill_MapNewSkillSet_NewBridgeRowsForSkillSet()
		{
			var nrOfBridgeRows = fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var skills = new List<Guid> { fakeSkill2.SkillCode };
			personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows + 1, fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
		}

		[Test]
		public void OneSkill_MapSkillSet_NoNewBridgeOrSkillSet()
		{
			var nrOfBridgeRows = fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var nrOfSkillSets = fakeAnalyticsSkillRepository.SkillSets().Count;

			var skills = new List<Guid> { fakeSkill1.SkillCode };
			personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows, fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
			Assert.AreEqual(nrOfSkillSets, fakeAnalyticsSkillRepository.SkillSets().Count);
		}

		[Test]
		public void TwoSkill_MapNewSkillSet_TwoNewBridgeRowsAndSkillSet()
		{
			var nrOfBridgeRows = fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count;
			var nrOfSkillSets = fakeAnalyticsSkillRepository.SkillSets().Count;

			var skills = new List<Guid>
			{
				fakeSkill2.SkillCode,
				fakeSkill3.SkillCode
			};

			personPeriodTransformer.MapSkillsetId(skills, 0, new ReturnNotDefined());

			Assert.AreEqual(nrOfBridgeRows + 2, fakeAnalyticsSkillRepository.fakeBridgeSkillsetSkills.Count);
			Assert.AreEqual(nrOfSkillSets + 1, fakeAnalyticsSkillRepository.SkillSets().Count);
		}

		[Test]
		public void OneSkill_TransformToNewSkillSet_CorrectNewSkillSet()
		{
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