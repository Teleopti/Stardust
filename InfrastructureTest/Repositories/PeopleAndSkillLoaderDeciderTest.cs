using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("BucketB")]
    public class PeopleAndSkillLoaderDeciderTest
    {
	    [Test]
	    public void VerifyExecute()
	    {
		    var personRep = MockRepository.GenerateMock<IPersonRepository>();
		    var matrixService = MockRepository.GenerateMock<IPairMatrixService<Guid>>();
		    var target = new PeopleAndSkillLoaderDecider(personRep, matrixService);

		    DateTimePeriod period = new DateTimePeriod(2000, 1, 1, 2001, 1, 1);
		    IPerson person = new Person();
		    person.SetId(Guid.NewGuid());

		    IScenario scenario = new Scenario("f");
		    IEnumerable<IPerson> peopleToSearchWith = new List<IPerson> {person};

		    var peopleSkillMatrix = new List<Tuple<Guid, Guid>>();

		    IEnumerable<Guid> skillDependencies = new List<Guid>();
		    IEnumerable<Guid> peopleDependencies = new List<Guid>();
		    IEnumerable<Guid> siteDependencies = new List<Guid>();

		    personRep.Stub(x => x.PeopleSkillMatrix(scenario, period)).Return(peopleSkillMatrix);
		    matrixService.Stub(x => x.CreateDependencies(peopleSkillMatrix, new List<Guid>{person.Id.GetValueOrDefault()}))
			    .Return(new DependenciesPair<Guid>(peopleDependencies, skillDependencies));
		    personRep.Stub(x => x.PeopleSiteMatrix(period)).Return(siteDependencies);

		    var result = target.Execute(scenario, period, peopleToSearchWith);

		    Assert.AreEqual(peopleDependencies, result.PeopleGuidDependencies);
			Assert.AreEqual(skillDependencies, result.SkillGuidDependencies);
			Assert.AreEqual(siteDependencies, result.SiteGuidDependencies);
	    }

	    [Test]
        public void VerifyFilterPeople()
        {
			var valid = Guid.NewGuid();
			
	        ISkill skill = SkillFactory.CreateSkill("skill");
	        ISite site = SiteFactory.CreateSiteWithOneTeam();
	        site.MaxSeats = 1;
	        ITeam team = site.TeamCollection[0];
	        IPerson validPerson = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill}).WithId(valid);
	        validPerson.PersonPeriodCollection[0].Team = team;
	        validPerson.PersonPeriodCollection[0].StartDate = DateOnly.MinValue;
            IPerson nonValidPerson = new Person().WithId();
			Guid guidToAdd = Guid.NewGuid();
            IPerson personToAdd = PersonFactory.CreatePersonWithPersonPeriod(DateOnly.MinValue, new List<ISkill> {skill}).WithId(guidToAdd);
			personToAdd.PersonPeriodCollection[0].Team = team;
			personToAdd.PersonPeriodCollection[0].StartDate = DateOnly.MinValue;
            
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new[] { valid }, new Guid[0], new[] { guidToAdd });
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
            var valid = Guid.NewGuid();
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new[] { valid }, new Guid[0]);

            ISkill validSkill = new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time)).WithId(valid);
            ISkill nonValidSkill = new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time)).WithId();
            IList<ISkill> listToFilter = new List<ISkill> 
                                        {   
                                            validSkill, 
                                            nonValidSkill,  
                                            new Skill("sdf", "sdf", Color.Empty, 23, new SkillTypeEmail(new Description("sdf"), ForecastSource.Time))
                                        };

            Assert.AreEqual(3, listToFilter.Count);
			int removed = target.FilterSkills(listToFilter.ToArray(), s => listToFilter.Remove(s), listToFilter.Add);
            Assert.AreEqual(1, listToFilter.Count);
            Assert.AreSame(validSkill, listToFilter[0]);
            Assert.AreEqual(2, removed);
        }

        [Test]
        public void VerifyParentReturnedIfMatchForChildSkill()
        {
            Guid valid = Guid.NewGuid();
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new[] { valid }, new Guid[0]);

			var parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
            var validSkill = new ChildSkill("sdf", "sdf", Color.Empty, parent).WithId(valid);
            
            IList<ISkill> list2Filter = new List<ISkill> { validSkill, parent };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
            Assert.AreEqual(0, removed);
            Assert.AreEqual(2, list2Filter.Count);
        }

        [Test]
        public void VerifyParentReturnedIfMatchForChildSkillOnly()
        {
            Guid valid = Guid.NewGuid();
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new[] { valid }, new Guid[0]);
			
	        var parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
			var validSkill = new ChildSkill("sdf", "sdf", Color.Empty, parent).WithId(valid);
            
            IList<ISkill> list2Filter = new List<ISkill> { validSkill };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
            Assert.AreEqual(-1, removed);
            Assert.AreEqual(2, list2Filter.Count);
        }

        [Test]
        public void ShouldNotReturnParentForIrrelevantChildSkill()
        {
            Guid valid = Guid.NewGuid();
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new Guid[0], new Guid[0]);
			
	        var parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
			var validSkill = new ChildSkill("sdf", "sdf", Color.Empty, parent).WithId(valid);
            
            IList<ISkill> list2Filter = new List<ISkill> { validSkill };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
            Assert.AreEqual(1, removed);
            Assert.AreEqual(0, list2Filter.Count);
        }

		[Test]
		public void VerifyOnlyOneParentReturnedIfMatchForTwoChildSkillsOnly()
		{
			Guid valid1 = Guid.NewGuid();
			Guid valid2 = Guid.NewGuid();

			var parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));

			var validSkill1 = new ChildSkill("sdf", "sdf", Color.Empty, parent).WithId(valid1);
			var validSkill2 = new ChildSkill("sdf", "sdf", Color.Empty, parent).WithId(valid2);
			
			IList<ISkill> list2Filter = new List<ISkill> { validSkill1, validSkill2 };
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new[] { valid1, valid2 }, new Guid[0]);
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
			Assert.AreEqual(-1, removed);
			Assert.AreEqual(3, list2Filter.Count);
		}

        [Test]
        public void VerifyParentNotReturnedIfNoMatchForChildSkill()
        {
			var target = new LoaderDeciderResult(new DateTimePeriod(1950, 1, 12, 1950, 1, 13), new Guid[0], new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() }, new Guid[0]);

            IMultisiteSkill parent = new MultisiteSkill("multi", "multi", Color.DimGray, 13, new SkillTypeEmail(new Description("d"), ForecastSource.Time));
            parent.SetId(Guid.NewGuid());

            var list2Filter = new List<ISkill> { parent };
			int removed = target.FilterSkills(list2Filter.ToArray(), s => list2Filter.Remove(s), list2Filter.Add);
            Assert.AreEqual(1, removed);
            Assert.AreEqual(0, list2Filter.Count);
        }
    }
}
