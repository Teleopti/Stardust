using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.ShiftCreator
{
    [TestFixture]
    public class RuleSetBagTest
    {
        private RuleSetBag _target;
        private MockRepository _mocks;

        [SetUp]
        public void Setup()
        {
            _target = new RuleSetBag();
            _mocks = new MockRepository();
        }

        [Test]
        public void VerifyDefaultProperties()
        {
            Assert.AreEqual(0, _target.RuleSetCollection.Count);
            Assert.AreEqual(new Description(), _target.Description);
            Assert.IsTrue(_target.IsChoosable);
        }

        [Test]
        public void CanSetProperties()
        {
            var newDesc = new Description("sdf");
            _target.Description = newDesc;
            Assert.AreEqual(newDesc, _target.Description);

            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
        }

        [Test]
        public void VerifyICloneableEntity()
        {
            ((IEntity)_target).SetId(Guid.NewGuid());
            foreach (IWorkShiftRuleSet ruleSet in _target.RuleSetCollection)
            {
                _target.RemoveRuleSet(ruleSet);
            }
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            _target.AddRuleSet(ruleSet1);
            _target.AddRuleSet(ruleSet2);
            
            IRuleSetBag clonedTarget = _target.NoneEntityClone();
            Assert.IsNull(clonedTarget.Id);

            // Change description.
            clonedTarget.Description = new Description("Cloned");
            Assert.AreNotSame(clonedTarget.Description, _target.Description);

            // Check for rule set count.
            Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 2);
            clonedTarget.RemoveRuleSet(ruleSet1);
            Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 1);
            clonedTarget.RemoveRuleSet(ruleSet2);
            Assert.AreEqual(clonedTarget.RuleSetCollection.Count, 0);

            // Check for original.
            Assert.AreEqual(_target.RuleSetCollection.Count, 2);

            clonedTarget = (IRuleSetBag)_target.Clone();
            Assert.IsNotNull(clonedTarget.Id);
        }

        [Test]
        public void CanAddRuleSet()
        {
            var ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            _target.AddRuleSet(ruleSet);
            Assert.AreEqual(1, _target.RuleSetCollection.Count);
            Assert.AreSame(ruleSet, _target.RuleSetCollection[0]);
        }

        [Test]
        public void CanRemoveRuleSet()
        {
            var ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            _target.AddRuleSet(ruleSet);
            Assert.AreEqual(1, _target.RuleSetCollection.Count);
            _target.RemoveRuleSet(ruleSet);
            Assert.AreEqual(0, _target.RuleSetCollection.Count);
        }

        [Test]
        public void CanClearRuleSet()
        {
            var ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            _target.AddRuleSet(ruleSet);
            _target.AddRuleSet(ruleSet2);
            _target.ClearRuleSetCollection();
            Assert.AreEqual(0, _target.RuleSetCollection.Count);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CannotAddNullAsRuleSet()
        {
            _target.AddRuleSet(null);
        }

        [Test]
        public void CanGetWorkTimeMinMaxFromRuleSet()
        {
            var projSvc = new RuleSetProjectionService(new ShiftCreatorService());
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            _target.AddRuleSet(ruleSet1);
            _target.AddRuleSet(ruleSet2);
            IWorkTimeMinMax minMax1 = new WorkTimeMinMax
                                          {
                                              StartTimeLimitation =
                                                  new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(10)),
                                              EndTimeLimitation =
                                                  new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(19)),
                                              WorkTimeLimitation =
                                                  new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(11))
                                          };

            IWorkTimeMinMax minMax2 = new WorkTimeMinMax
                                          {
                                              StartTimeLimitation =
                                                  new StartTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(9)),
                                              EndTimeLimitation =
                                                  new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(20)),
                                              WorkTimeLimitation =
                                                  new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(8))
                                          };

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.DayOffTemplate).Return(null);
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null).Repeat.AtLeastOnce();
                Expect.Call(ruleSet1.MinMaxWorkTime(projSvc, effectiveRestriction)).Return(minMax1);
                Expect.Call(ruleSet2.MinMaxWorkTime(projSvc, effectiveRestriction)).Return(minMax2);

                Expect.Call(ruleSet2.IsValidDate(new DateOnly(2008, 1, 1))).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(ruleSet1.IsValidDate(new DateOnly(2008, 1, 1))).IgnoreArguments().Return(true).Repeat.AtLeastOnce();
                Expect.Call(ruleSet1.OnlyForRestrictions).Return(false);
                Expect.Call(ruleSet2.OnlyForRestrictions).Return(false);
            }

            using (_mocks.Playback())
            {
                var minMax = _target.MinMaxWorkTime(projSvc, new DateOnly(2008, 1, 1), effectiveRestriction);
                Assert.IsNotNull(minMax);
                Assert.AreEqual(TimeSpan.FromHours(6),minMax.StartTimeLimitation.StartTime);
                Assert.AreEqual(TimeSpan.FromHours(10), minMax.StartTimeLimitation.EndTime.Value);
                Assert.AreEqual(TimeSpan.FromHours(17), minMax.EndTimeLimitation.StartTime.Value);
                Assert.AreEqual(TimeSpan.FromHours(20), minMax.EndTimeLimitation.EndTime.Value);
                Assert.AreEqual(TimeSpan.FromHours(4), minMax.WorkTimeLimitation.StartTime.Value);
                Assert.AreEqual(TimeSpan.FromHours(11), minMax.WorkTimeLimitation.EndTime.Value);
            }
        }
        [Test]
        public void VerifyMinMaxFromRuleSetIsNullWhenRestrictionIsNull()
        {
            var projSvc = new RuleSetProjectionService(new ShiftCreatorService());
            var ruleSet1 = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            IEffectiveRestriction effectiveRestriction = null;
            _target.AddRuleSet(ruleSet1);
            _target.AddRuleSet(ruleSet2);
            
            var minMax = _target.MinMaxWorkTime(projSvc, new DateOnly(2008, 1, 1), effectiveRestriction);
            Assert.IsNull(minMax);
        }



        [Test]
        public void CanGetShiftCategoriesFromRuleSet()
        {
            IWorkShiftRuleSet ruleSet1 = WorkShiftRuleSetFactory.Create();
            IWorkShiftRuleSet ruleSet2 = WorkShiftRuleSetFactory.Create();
            IWorkShiftRuleSet ruleSet3 = WorkShiftRuleSetFactory.Create();
            IShiftCategory shiftCategoryNight = ShiftCategoryFactory.CreateShiftCategory("Natt");
            IShiftCategory shiftCategoryDay = ShiftCategoryFactory.CreateShiftCategory("Dag");
            ruleSet1.TemplateGenerator.Category = shiftCategoryDay;
            ruleSet2.TemplateGenerator.Category = shiftCategoryNight;
            ruleSet3.TemplateGenerator.Category = shiftCategoryNight;
            _target.AddRuleSet(ruleSet1);
            _target.AddRuleSet(ruleSet2);
            _target.AddRuleSet(ruleSet3);
            IList<IShiftCategory> returnCategories = _target.ShiftCategoriesInBag();

            Assert.AreEqual(2, returnCategories.Count);
        }

        [Test]
        public void DayOffInRestrictionReturnsNullInMinMaxWorkTime()
        {
            var projSvc = new RuleSetProjectionService(new ShiftCreatorService());
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.DayOffTemplate).Return(new DayOffTemplate(new Description("af")));
            }

            using (_mocks.Playback())
            {
                Assert.IsNull(_target.MinMaxWorkTime(projSvc, new DateOnly(), effectiveRestriction));
            }
            
        }

        [Test]
        public void NoValidWorkShiftRuleSetsOnDayReturnsNull()
        {
            var projSvc = new RuleSetProjectionService(new ShiftCreatorService());
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            _target.AddRuleSet(ruleSet);

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.DayOffTemplate).Return(null);
                Expect.Call(ruleSet.IsValidDate(new DateOnly())).IgnoreArguments().Return(false);
                Expect.Call(effectiveRestriction.IsRestriction).Return(false);
            }

            using (_mocks.Playback())
            {
                Assert.IsNull(_target.MinMaxWorkTime(projSvc, new DateOnly(), effectiveRestriction));
            }
        }

        [Test]
        public void ShouldCheckRuleSetsWithOnlyForRestrictionIfIsRestriction()
        {
            var projSvc = new RuleSetProjectionService(new ShiftCreatorService());
            var effectiveRestriction = _mocks.StrictMock<IEffectiveRestriction>();
            var ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            var ruleSet2 = _mocks.StrictMock<IWorkShiftRuleSet>();
            
            _target.AddRuleSet(ruleSet);
            _target.AddRuleSet(ruleSet2);

            IWorkTimeMinMax minMax1 = new WorkTimeMinMax
            {
                StartTimeLimitation =
                    new StartTimeLimitation(TimeSpan.FromHours(7), TimeSpan.FromHours(10)),
                EndTimeLimitation =
                    new EndTimeLimitation(TimeSpan.FromHours(18), TimeSpan.FromHours(19)),
                WorkTimeLimitation =
                    new WorkTimeLimitation(TimeSpan.FromHours(6), TimeSpan.FromHours(11))
            };

            using (_mocks.Record())
            {
                Expect.Call(effectiveRestriction.DayOffTemplate).Return(null);
                Expect.Call(effectiveRestriction.ShiftCategory).Return(null).Repeat.AtLeastOnce();
                Expect.Call(ruleSet.IsValidDate(new DateOnly())).IgnoreArguments().Return(true);
                Expect.Call(ruleSet2.IsValidDate(new DateOnly())).IgnoreArguments().Return(true);
                Expect.Call(ruleSet.OnlyForRestrictions).Return(false).Repeat.Twice();
                Expect.Call(effectiveRestriction.IsRestriction).Return(true);
                Expect.Call(ruleSet2.OnlyForRestrictions).Return(true).Repeat.Twice();
                Expect.Call(ruleSet.MinMaxWorkTime(projSvc, effectiveRestriction)).Return(null);
                Expect.Call(ruleSet2.MinMaxWorkTime(projSvc, effectiveRestriction)).Return(minMax1);
                
            }

            using (_mocks.Playback())
            {
                Assert.That(_target.MinMaxWorkTime(projSvc, new DateOnly(), effectiveRestriction).StartTimeLimitation.StartTime,Is.EqualTo(minMax1.StartTimeLimitation.StartTime));
            }
        }
    }
}
