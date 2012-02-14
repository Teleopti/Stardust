using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.NonBlendSkill;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.NonBlendSkill
{
    [TestFixture]
    public class NonBlendSkillFromGroupingCreatorTest

    {
        private MockRepository _mocks;
        private ISchedulingResultStateHolder _schedulingResultStateHolder;
        private INonBlendPersonSkillFromGroupingCreator _nonBlendPersonSkillFromGroupingCreator;
        private NonBlendSkillFromGroupingCreator _nonBlendSkillFromGroupingCreator;
        private IGroupPage _groupPage;
        private IActivity _activity;
        private IScheduleDictionary _schedDic;
        private Person _person;
        private IList<IPerson> _persons;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingResultStateHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _groupPage = _mocks.StrictMock<IGroupPage>();
            _nonBlendPersonSkillFromGroupingCreator = _mocks.StrictMock<INonBlendPersonSkillFromGroupingCreator>();
            _schedDic = _mocks.StrictMock<IScheduleDictionary>();
            _activity = new Activity("lastaAv");

            _person = new Person();
            _persons = new List<IPerson> {_person};

            Expect.Call(_schedulingResultStateHolder.Schedules).Return(_schedDic);
            Expect.Call(_schedDic.Keys).Return(_persons);
            
        }

        [Test]
        public void ShouldRemoveAllOldNonBlendSkillsFirst()
        {
            
            var skill = new Skill("nonBlend", "", Color.Firebrick, 15,
                                  new SkillTypePhone(new Description(), ForecastSource.NonBlendSkill));
            var skills = new List<ISkill> {skill};

            Expect.Call(_schedulingResultStateHolder.Skills).Return(skills);
            Expect.Call(_groupPage.RootGroupCollection).Return(new ReadOnlyCollection<IRootPersonGroup>(new List<IRootPersonGroup>()));
            _mocks.ReplayAll();
            _nonBlendSkillFromGroupingCreator = new NonBlendSkillFromGroupingCreator(_schedulingResultStateHolder, _nonBlendPersonSkillFromGroupingCreator, _activity);
            _nonBlendSkillFromGroupingCreator.ProcessDate(new DateOnly(), _groupPage);
            Assert.That(skills.IsEmpty());
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateSkillsFromRootGroups()
        {
            var skills = new List<ISkill>();
            var rootGroup1 = new RootPersonGroup("group1");
            rootGroup1.AddPerson(_person);
            var rootGroup2 = new RootPersonGroup("group2");
            rootGroup2.AddPerson(_person);
            var coll = new List<IRootPersonGroup> {rootGroup1, rootGroup2};
            Expect.Call(_schedulingResultStateHolder.Skills).Return(skills);
            Expect.Call(_groupPage.RootGroupCollection).Return(new ReadOnlyCollection<IRootPersonGroup>(coll));
            Expect.Call(() => _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(null, null, new DateOnly())).IgnoreArguments().Repeat.Twice();
            _mocks.ReplayAll();
            _nonBlendSkillFromGroupingCreator = new NonBlendSkillFromGroupingCreator(_schedulingResultStateHolder, _nonBlendPersonSkillFromGroupingCreator, _activity);
            _nonBlendSkillFromGroupingCreator.ProcessDate(new DateOnly(), _groupPage);
            Assert.That(skills.Count,Is.EqualTo(2));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateSkillsFromChildGroups()
        {
            var skills = new List<ISkill>();
            var rootGroup1 = new RootPersonGroup("group1");
            rootGroup1.AddPerson(_person);
            var childGroup1 = new ChildPersonGroup("child1");
            childGroup1.AddPerson(_person);
            var childGroup2 = new ChildPersonGroup("child2");
            childGroup2.AddPerson(_person);
            rootGroup1.AddChildGroup(childGroup1);
            rootGroup1.AddChildGroup(childGroup2);
            var coll = new List<IRootPersonGroup> { rootGroup1 };
            Expect.Call(_schedulingResultStateHolder.Skills).Return(skills);
            Expect.Call(_groupPage.RootGroupCollection).Return(new ReadOnlyCollection<IRootPersonGroup>(coll));
            Expect.Call(() => _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(null, null, new DateOnly())).IgnoreArguments().Repeat.Times(3);
            _mocks.ReplayAll();
            _nonBlendSkillFromGroupingCreator = new NonBlendSkillFromGroupingCreator(_schedulingResultStateHolder, _nonBlendPersonSkillFromGroupingCreator, _activity);
            _nonBlendSkillFromGroupingCreator.ProcessDate(new DateOnly(), _groupPage);
            Assert.That(skills.Count, Is.EqualTo(3));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateSkillsFromChildChildGroups()
        {
            var skills = new List<ISkill>();
            var rootGroup1 = new RootPersonGroup("group1");
            rootGroup1.AddPerson(_person);
            var childGroup1 = new ChildPersonGroup("child1");
            childGroup1.AddPerson(_person);
            var childGroup2 = new ChildPersonGroup("child2");
            childGroup2.AddPerson(_person);
            rootGroup1.AddChildGroup(childGroup1);
            childGroup1.AddChildGroup(childGroup2);
            var coll = new List<IRootPersonGroup> { rootGroup1 };
            Expect.Call(_schedulingResultStateHolder.Skills).Return(skills);
            Expect.Call(_groupPage.RootGroupCollection).Return(new ReadOnlyCollection<IRootPersonGroup>(coll));
            Expect.Call(() => _nonBlendPersonSkillFromGroupingCreator.ProcessPersons(null, null, new DateOnly())).IgnoreArguments().Repeat.Times(3);
            _mocks.ReplayAll();
            _nonBlendSkillFromGroupingCreator = new NonBlendSkillFromGroupingCreator(_schedulingResultStateHolder, _nonBlendPersonSkillFromGroupingCreator, _activity);
            _nonBlendSkillFromGroupingCreator.ProcessDate(new DateOnly(), _groupPage);
            Assert.That(skills.Count, Is.EqualTo(3));
            _mocks.VerifyAll();
        }
    }

    

}