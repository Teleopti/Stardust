using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class SkillLoaderDeciderTest
    {
        private targetForTest target;
        private MockRepository mocks;
        private IPersonRepository personRep;
        private IPairMatrixService<Guid> matrixService;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            personRep = mocks.StrictMock<IPersonRepository>();
            matrixService = mocks.StrictMock<IPairMatrixService<Guid>>();
            target = new targetForTest(personRep, matrixService);
        }

        [Test]
        public void VerifyDefaultService()
        {
            Assert.IsTrue(typeof(PairMatrixService<Guid>).Equals(new SkillLoaderDecider(personRep).MatrixService.GetType()));
        }

        [Test]
        public void VerifyExecute()
        {
            DateTimePeriod period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
            ISkill skill = SkillFactory.CreateSkill("Direct Sales");
            skill.SetId(Guid.NewGuid());

            IScenario scenario = new Scenario("f");

            IEnumerable<Tuple<Guid, Guid>> peopleSkillMatrix = new List<Tuple<Guid, Guid>>();

            IEnumerable<Guid> skillDependencies = new List<Guid>();
            IEnumerable<Guid> peopleDependencies = new List<Guid>();
            IEnumerable<Guid> siteDependencies = new List<Guid>();

            using (mocks.Record())
            {
                Expect.Call(personRep.PeopleSkillMatrix(scenario, period))
                    .Return(peopleSkillMatrix);
	            Expect.Call(matrixService.CreateDependencies(null, new List<Guid> {skill.Id.GetValueOrDefault()}))
		            .IgnoreArguments()
		            .Return(new DependenciesPair<Guid>(skillDependencies, peopleDependencies));
                Expect.Call(personRep.PeopleSiteMatrix(period)).Return(siteDependencies);
            }
            using (mocks.Playback())
            {
                target.Execute(scenario, period, skill);
                Assert.AreSame(peopleDependencies, target.PeopleGuidDependencies);
                Assert.AreSame(skillDependencies, target.SkillGuidDependencies);
                Assert.AreSame(siteDependencies, target.SiteGuidDependencies);
            }
        }

        [Test]
        public void VerifyFilterPeople()
        {
            Guid valid = Guid.NewGuid();
            target.SetPeopleGuids(new List<Guid> { valid });
	        ISkill skill = SkillFactory.CreateSkill("skill");
	        ISite site = SiteFactory.CreateSiteWithOneTeam();
	        site.MaxSeats = 1;
	        ITeam team = site.TeamCollection[0];
	        IPerson validPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill});
	        validPerson.PersonPeriodCollection[0].Team = team;
	        validPerson.PersonPeriodCollection[0].StartDate = DateOnly.MinValue;
            validPerson.SetId(valid);
            IPerson nonValidPerson = new Person();
            nonValidPerson.SetId(Guid.NewGuid());
            IPerson personToAdd = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill});
			personToAdd.PersonPeriodCollection[0].Team = team;
			personToAdd.PersonPeriodCollection[0].StartDate = DateOnly.MinValue;
            Guid guidToAdd = Guid.NewGuid();
            personToAdd.SetId(guidToAdd);
            target.SetSiteGuids(new List<Guid> { guidToAdd });
            IList<IPerson> listToFilter = new List<IPerson> { nonValidPerson, validPerson, new Person(), personToAdd };

            Assert.AreEqual(4, listToFilter.Count);
            int removed = target.FilterPeople(listToFilter);
            Assert.AreEqual(2, listToFilter.Count);
            Assert.AreSame(validPerson, listToFilter[0]);
            Assert.AreSame(personToAdd, listToFilter[1]);
            Assert.AreEqual(2, removed);
        }

        [Test]
        public void VerifyFilterSkills()
        {
            Guid valid = Guid.NewGuid();
            target.SetSkillGuids(new List<Guid> { valid });

            ISkill validSkill = new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time));
            validSkill.SetId(valid);
            ISkill nonValidSkill = new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time)); ;
            nonValidSkill.SetId(Guid.NewGuid());
            IList<ISkill> listToFilter = new List<ISkill> 
                                        {   
                                            validSkill, 
                                            nonValidSkill,  
                                            new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time))
                                        };

            Assert.AreEqual(3, listToFilter.Count);
            int removed = target.FilterSkills(listToFilter.ToArray(),s => listToFilter.Remove(s),listToFilter.Add);
            Assert.AreEqual(1, listToFilter.Count);
            Assert.AreSame(validSkill, listToFilter[0]);
            Assert.AreEqual(2, removed);
        }

        [Test]
        public void VerifyParentReturnedIfMatchForChildSkill()
        {
            Guid valid = Guid.NewGuid();
            target.SetSkillGuids(new List<Guid> { valid });

            IChildSkill validSkill = new ChildSkill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time));
            validSkill.SetId(valid);

            IMultisiteSkill parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
            parent.AddChildSkill(validSkill);

            IList<ISkill> list2Filter = new List<ISkill> { validSkill, parent };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
            Assert.AreEqual(0, removed);
            Assert.AreEqual(2, list2Filter.Count);
        }

        [Test]
        public void VerifyParentReturnedIfMatchForChildSkillOnly()
        {
            Guid valid = Guid.NewGuid();
            target.SetSkillGuids(new List<Guid> { valid });

            IChildSkill validSkill = new ChildSkill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time));
            validSkill.SetId(valid);

            IMultisiteSkill parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
            parent.AddChildSkill(validSkill);

            IList<ISkill> list2Filter = new List<ISkill> { validSkill };
            int removed = target.FilterSkills(list2Filter.ToArray(),s => list2Filter.Remove(s),list2Filter.Add);
            Assert.AreEqual(-1, removed);
            Assert.AreEqual(2, list2Filter.Count);
        }

		[Test]
		public void VerifyOnlyOneParentReturnedIfMatchForTwoChildSkillsOnly()
		{
			Guid valid1 = Guid.NewGuid();
			Guid valid2 = Guid.NewGuid();
			target.SetSkillGuids(new List<Guid> { valid1, valid2 });

			IChildSkill validSkill1 = new ChildSkill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time));
			validSkill1.SetId(valid1);

			IChildSkill validSkill2 = new ChildSkill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time));
			validSkill2.SetId(valid1);

			IMultisiteSkill parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
			parent.AddChildSkill(validSkill1);
			parent.AddChildSkill(validSkill2);

			IList<ISkill> list2Filter = new List<ISkill> { validSkill1, validSkill2 };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
			Assert.AreEqual(-1, removed);
			Assert.AreEqual(3, list2Filter.Count);
		}

        [Test]
        public void VerifyParentNotReturnedIfNoMatchForChildSkill()
        {
            target.SetSkillGuids(new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() });
            IMultisiteSkill parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
            parent.SetId(Guid.NewGuid());

            IList<ISkill> list2filter = new List<ISkill> { parent };
			int removed = target.FilterSkills(list2filter.ToArray(), s => list2filter.Remove(s), list2filter.Add);
            Assert.AreEqual(1, removed);
            Assert.AreEqual(0, list2filter.Count);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCallFilterSkillBeforeExecute()
        {
            target.FilterSkills(new ISkill[]{},null,null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CannotCallFilterPeopleBeforeExecute()
        {
            target.FilterPeople(new List<IPerson>());
        }

        [Test]
        public void ShouldShowZeroPercentageWhenFilterPeopleIsNotCalled()
        {
            target.SetPeopleGuids(new List<Guid>());
            Assert.AreEqual(0, target.PercentageOfPeopleFiltered);
        }

        [Test]
        public void ShouldFilterOut75PercentOfAllPeople()
        {
            Guid validGuid1 = Guid.NewGuid();
            Guid validGuid2 = Guid.NewGuid();
            Guid validGuid3 = Guid.NewGuid();
            IPerson person1 = new Person();
            person1.SetId(validGuid1);
            IPerson person2 = new Person();
            person2.SetId(validGuid2);
            IPerson person3 = new Person();
            person3.SetId(validGuid3);
            IPerson person4 = new Person();
            person4.SetId(Guid.NewGuid());

            IList<Guid> peopleDependencies = new List<Guid> { validGuid1, validGuid2, validGuid3 };
            IList<IPerson> allPersonsInOrganization = new List<IPerson> { person1, person2, person3, person4 };
            target.SetPeopleGuids(peopleDependencies);
            target.SetSiteGuids(new List<Guid>());
            target.FilterPeople(allPersonsInOrganization);

            Assert.AreEqual(75d, target.PercentageOfPeopleFiltered);
        }


        private class targetForTest : SkillLoaderDecider
        {
            private readonly IPairMatrixService<Guid> _matrixService;

            public targetForTest(IPersonRepository personRepository,
                                    IPairMatrixService<Guid> matrixService)
                : base(personRepository)
            {
                _matrixService = matrixService;
	            Period = new DateTimePeriod(1950, 1, 12, 1950, 1, 13);
            }

            public override IPairMatrixService<Guid> MatrixService
            {
                get { return _matrixService; }
            }

            public void SetPeopleGuids(IEnumerable<Guid> people)
            {
                PeopleGuidDependencies = people;
            }

            public void SetSkillGuids(IEnumerable<Guid> skills)
            {
                SkillGuidDependencies = skills;
            }

            public void SetSiteGuids(IEnumerable<Guid> peopleAtSameSite)
            {
                SiteGuidDependencies = peopleAtSameSite;
            }
        }
    }
}
