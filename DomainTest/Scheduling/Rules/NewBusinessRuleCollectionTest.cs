using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable"), TestFixture]
    public class NewBusinessRuleCollectionTest
    {
        private INewBusinessRuleCollection _target;
        private const int totalNumberOfRules = 9;
    	private ISchedulingResultStateHolder _state;

        [SetUp]
        public void Setup()
        {
            _target = NewBusinessRuleCollection.All(new SchedulingResultStateHolder());
        }

        [Test]
        public void VerifyAll()
        {
            //rk: kolla istället vilka IBusiness rules-typer som finns i domain... orkar inte just nu
            INewBusinessRule rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
            Assert.AreEqual(totalNumberOfRules, _target.Count);
            Assert.IsTrue(rule.HaltModify);
        }

        [Test, SetUICulture("en-GB")]
        public void ShouldSetCulture()
        {
            _target.SetUICulture(CultureInfo.GetCultureInfo(1053));
            Assert.AreEqual("sv-SE",_target.UICulture.Name);
        }

        [Test]
        public void VerifyMinimum()
        {
            INewBusinessRuleCollection targetSmall = NewBusinessRuleCollection.Minimum();
            foreach (var rule in _target)
            {
                Assert.AreEqual(rule.IsMandatory, collectionContainsType(targetSmall, rule.GetType()));
            }
        }
        
        [Test]
        public void VerifyRemoveBusinessRuleResponse()
        {
            INewBusinessRule rule = _target.Item(typeof(NewShiftCategoryLimitationRule));
            var dateOnlyPeriod = new DateOnlyPeriod();
            _target.Remove(new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), "d", false, false, new DateTimePeriod(2000, 1, 1, 2000, 1, 2), new Person(), dateOnlyPeriod));
            Assert.AreEqual(totalNumberOfRules, _target.Count);
            Assert.IsFalse(rule.HaltModify);
        }

        [Test]
        public void RemovingMandatoryRuleShouldResultDoingNothing()
        {
            _target.Add(new dummyRule(true));
            _target.Remove(typeof(dummyRule));
            Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
        }

        [Test]
        public void VerifyClearKeepsMandatory()
        {
            _target.Add(new dummyRule(true));
            _target.Clear();
            Assert.AreEqual(totalNumberOfRules + 1, _target.Count);
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
		public void NewOverlappingAssignmentRuleHasSetPropertyForDeleteInAllRules()
		{
			var rulesForDelete = NewBusinessRuleCollection.AllForDelete(new SchedulingResultStateHolder());
			foreach (INewBusinessRule rule in rulesForDelete)
			{
				if (rule is DataPartOfAgentDay)
					Assert.IsTrue(rule.ForDelete);
				else
					Assert.IsFalse(rule.ForDelete);
			}
		}

		[Test]
		public void MinimumAndPersonAccountShouldContainPersonAccountRule()
		{
			_state = new SchedulingResultStateHolder();
			var miniAndPa = NewBusinessRuleCollection.MinimumAndPersonAccount(_state);
			Assert.That(miniAndPa.Count, Is.EqualTo(NewBusinessRuleCollection.Minimum().Count + 1));
			Assert.That(collectionContainsType(miniAndPa, typeof(NewPersonAccountRule)));
		}

        [Test]
        public void AllForSchedulingShouldConsiderUseValidation()
        {
            _state = new SchedulingResultStateHolder();
            _state.UseValidation = true;
            var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
            Assert.AreEqual(totalNumberOfRules, allForScheduling.Count);
            _state.UseValidation = false;
            allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
            Assert.AreEqual(NewBusinessRuleCollection.Minimum().Count + 1, allForScheduling.Count);
			Assert.IsFalse((collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule))));
        }

		[Test]
		public void ShouldConsiderUseMinWorktimePerWeek()
		{
			_state = new SchedulingResultStateHolder();
			_state.UseMinWeekWorkTime = true;
			_state.UseValidation = true;
			var allForScheduling = NewBusinessRuleCollection.AllForScheduling(_state);
			Assert.AreEqual(totalNumberOfRules + 1, allForScheduling.Count);
			Assert.IsTrue(collectionContainsType(allForScheduling, typeof(MinWeekWorkTimeRule)));
		}

        private static bool collectionContainsType(IEnumerable<INewBusinessRule> businessRuleCollection, Type type)
        {
            foreach (var rule in businessRuleCollection)
            {
                if (rule.GetType().Equals(type))
                    return true;
            }
            return false;
        }
    
        private class dummyRule : INewBusinessRule
        {
            private readonly bool _mandatory;

        	public dummyRule(bool mandatory)
            {
                _mandatory = mandatory;
            }

            public string ErrorMessage
            {
                get { return string.Empty; }
            }

            public bool IsMandatory
            {
                get { return _mandatory; }
            }

        	public bool HaltModify { get; set; }

        	public bool ForDelete
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
            {
                throw new NotImplementedException();
            }
        }
    }
}