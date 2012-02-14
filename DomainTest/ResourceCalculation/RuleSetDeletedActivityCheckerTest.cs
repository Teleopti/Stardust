using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
    [TestFixture]
    public class RuleSetDeletedActivityCheckerTest
    {
        private MockRepository _mocks;
        private IActivity _deletedActivity;
        private IActivity _activeActivity;
        private IWorkShiftRuleSet _ruleSet;
        private IWorkShiftTemplateGenerator _templateGenerator;
        private RuleSetDeletedActivityChecker _target;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _deletedActivity = _mocks.StrictMock<IActivity>();
            _activeActivity = _mocks.StrictMock<IActivity>();
            _ruleSet = _mocks.StrictMock<IWorkShiftRuleSet>();
            _templateGenerator = _mocks.StrictMock<IWorkShiftTemplateGenerator>();
            _target = new RuleSetDeletedActivityChecker();
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfRuleSetIsNull()
        {
            _ruleSet = null;
            _target.ContainsDeletedActivity(_ruleSet);
        }

        [Test]
        public void ShouldReturnFalseIfActivityIsNotDeleted()
        {
            Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
            Expect.Call(_templateGenerator.BaseActivity).Return(_activeActivity);
            Expect.Call(_activeActivity.IsDeleted).Return(false);
            Expect.Call(_ruleSet.ExtenderCollection).Return(
                new ReadOnlyCollection<IWorkShiftExtender>(new List<IWorkShiftExtender>()));
            Expect.Call(_ruleSet.LimiterCollection).Return(
                new ReadOnlyCollection<IWorkShiftLimiter>(new List<IWorkShiftLimiter>()));
            
            _mocks.ReplayAll();

            bool result = _target.ContainsDeletedActivity(_ruleSet);
            Assert.That(result, Is.False);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfBaseActivityDeleted()
        {
            Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
            Expect.Call(_templateGenerator.BaseActivity).Return(_deletedActivity);
            Expect.Call(_deletedActivity.IsDeleted).Return(true);
            _mocks.ReplayAll();
            
            bool result = _target.ContainsDeletedActivity(_ruleSet);
            Assert.That(result,Is.True);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfAnActivityExtenderIsDeleted()
        {
            var extender = _mocks.StrictMock<IWorkShiftExtender>();
            Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
            Expect.Call(_templateGenerator.BaseActivity).Return(_activeActivity);
            Expect.Call(_activeActivity.IsDeleted).Return(false);
            Expect.Call(_ruleSet.ExtenderCollection).Return(
                new ReadOnlyCollection<IWorkShiftExtender>(new List<IWorkShiftExtender>{extender}));
            Expect.Call(extender.ExtendWithActivity).Return(_deletedActivity);
            Expect.Call(_deletedActivity.IsDeleted).Return(true);
            _mocks.ReplayAll();

            bool result = _target.ContainsDeletedActivity(_ruleSet);
            Assert.That(result, Is.True);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnTrueIfAnActivityTimeLimiterActivityIsDeleted()
        {
            var activityLimiter = new ActivityTimeLimiter(_deletedActivity, TimeSpan.FromMinutes(15),
                                                          OperatorLimiter.Equals);

            var otherLimiter = _mocks.StrictMock<IWorkShiftLimiter>();

            Expect.Call(_ruleSet.TemplateGenerator).Return(_templateGenerator);
            Expect.Call(_templateGenerator.BaseActivity).Return(_activeActivity);
            Expect.Call(_activeActivity.IsDeleted).Return(false);
            Expect.Call(_ruleSet.ExtenderCollection).Return(
                new ReadOnlyCollection<IWorkShiftExtender>(new List<IWorkShiftExtender>()));
            Expect.Call(_ruleSet.LimiterCollection).Return(
                new ReadOnlyCollection<IWorkShiftLimiter>(new List<IWorkShiftLimiter> { activityLimiter, otherLimiter }));
            
            Expect.Call(_deletedActivity.IsDeleted).Return(true);
            _mocks.ReplayAll();

            bool result = _target.ContainsDeletedActivity(_ruleSet);
            Assert.That(result, Is.True);

            _mocks.VerifyAll();
        }
    }

    
}