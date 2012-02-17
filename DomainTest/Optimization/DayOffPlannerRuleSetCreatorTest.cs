using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.DayOffPlanning;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class DayOffPlannerRuleSetCreatorTest
    {
        private DayOffPlannerRuleSetCreator _target;
        private IDaysOffPreferences _daysOffPreferences;

        [SetUp]
        public void Setup()
        {
            _daysOffPreferences = new DaysOffPreferences();
            _target = new DayOffPlannerRuleSetCreator();
        }

        [Test]
        public void ShouldCreateNewDayOffPlannerRuleSet()
        {
           IDayOffPlannerSessionRuleSet result = _target.CreateDayOffPlannerSessionRuleSet(_daysOffPreferences);
           Assert.IsNotNull(result);
        }
    }
}
