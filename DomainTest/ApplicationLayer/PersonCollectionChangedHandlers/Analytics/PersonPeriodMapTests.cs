using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers.Analytics.Transformer;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.PersonCollectionChangedHandlers.Analytics
{
	public class PersonPeriodMapTests
	{
		private PersonPeriodTransformer personPeriodTransformer;
		private FakeAnalyticsPersonPeriodRepository fakeAnalyticsPersonPeriodRepository;
		private FakePersonRepository fakePersonRepository;

		[SetUp]
		public void SetupTests()
		{
			fakePersonRepository = new FakePersonRepository();
			fakeAnalyticsPersonPeriodRepository = new FakeAnalyticsPersonPeriodRepository(
				new DateTime(2015, 01, 01),
				new DateTime(2017, 12, 31));


			List<IAnalyticsSkill> fakeSkills = new List<IAnalyticsSkill>
			{
				new AnalyticsSkill
				{
					SkillId = 1,
					SkillCode = Guid.Parse("00000000-0000-0000-0000-000000000001")
				},
				new AnalyticsSkill
				{
					SkillId = 2,
					SkillCode = Guid.Parse("00000000-0000-0000-0000-000000000002")
				},
				new AnalyticsSkill
				{
					SkillId = 3,
					SkillCode = Guid.Parse("00000000-0000-0000-0000-000000000003")
				}
			};
			fakeAnalyticsPersonPeriodRepository.SetSkills(fakeSkills);


			List<KeyValuePair<int, string>> fakeSkillSets = new List<KeyValuePair<int, string>>
			{
				new KeyValuePair<int, string>(1, "1,2,3"),
				new KeyValuePair<int, string>(2, "1,2"),
				new KeyValuePair<int, string>(3, "1")
			};

			fakeAnalyticsPersonPeriodRepository.SetSkillSets(fakeSkillSets);


			personPeriodTransformer = new PersonPeriodTransformer(fakePersonRepository, fakeAnalyticsPersonPeriodRepository);
		}


		[Test]
		public void NormalDate_MapDateToId_Date()
		{
			var date = new DateTime(2015, 01, 01);
			var dateId = personPeriodTransformer.MapDateId(date);
			Assert.AreEqual(0, dateId);
		}

		[Test]
		public void BigBangDate_MapDateToId_NotDefinedDate()
		{
			var date = new DateTime(1900, 01, 01);
			var dateId = personPeriodTransformer.MapDateId(date);
			Assert.AreEqual(-1, dateId);
		}

		[Test]
		public void Eternity_MapDateToId_EternityDate()
		{
			var date = new DateTime(2059, 12, 31);
			var dateId = personPeriodTransformer.MapDateId(date);
			Assert.AreEqual(-2, dateId);
		}

		[Test]
		public void OutSideScopeDate_MapDateToId_NotDefinedDate()
		{
			var date = new DateTime(2025, 12, 31);
			var dateId = personPeriodTransformer.MapDateId(date);
			Assert.AreEqual(-1, dateId);
		}

		[Test]
		public void NoSkills_MapSkillSet_NotDefined()
		{
			var skillsList = new List<Guid>();
			var skillSet = personPeriodTransformer.MapSkillsetId(skillsList, 0);
			Assert.AreEqual(-1, skillSet);
		}

		[Test]
		public void OneSkill_MapSkillSet_GotCorrectSkillSet()
		{
			var skills = new List<Guid> { Guid.Parse("00000000-0000-0000-0000-000000000001") };
			var skillSet = personPeriodTransformer.MapSkillsetId(skills, 0);
			Assert.AreEqual(3, skillSet);
		}

		[Test]
		public void TwoSkills_MapSkillSet_GotCorrectSkillSet()
		{
			var skills = new List<Guid>
			{
				Guid.Parse("00000000-0000-0000-0000-000000000001"),
				Guid.Parse("00000000-0000-0000-0000-000000000002")
			};
			var skillSet = personPeriodTransformer.MapSkillsetId(skills, 0);
			Assert.AreEqual(2, skillSet);
		}

		[Test]
		public void TwoSkills_MapSkillSet_NoSkillSet()
		{
			var skills = new List<Guid> { Guid.Parse("00000000-0000-0000-0000-000000000002") };
			var skillSet = personPeriodTransformer.MapSkillsetId(skills, 0);
			Assert.AreEqual(null, skillSet);
		}
	}
}
