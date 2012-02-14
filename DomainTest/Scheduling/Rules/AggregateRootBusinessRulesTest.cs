//using System;
//using System.Collections.Generic;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Teleopti.Ccc.Domain.Scheduling;
//using Teleopti.Ccc.Domain.Scheduling.Assignment;
//using Teleopti.Ccc.Domain.Scheduling.Rules;
//using Teleopti.Ccc.DomainTest.FakeData;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
//{
//    /// <summary>
//    /// Tests the BusinessRuleCollection class.
//    /// </summary>
//    /// <remarks>
//    /// Created by: robink
//    /// Created date: 2007-11-08
//    /// </remarks>
//    [TestFixture]
//    public class AggregateRootBusinessRulesTest
//    {
//        private MockRepository mocks;
//        private IPerson _person;
//        private DateTimePeriod _range;
//        private IScenario _scenario;
//        private ISchedulePart _schedulePart;
//        private IScheduleRange _scheduleRange;

//        [SetUp]
//        public void Setup()
//        {
//            mocks = new MockRepository();
//        }


//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
//        public void VerifyError()
//        {
//            _range = new DateTimePeriod(2007, 8, 1, 2007, 8, 5);
//            _scenario = ScenarioFactory.CreateScenarioAggregate();
//            _person = PersonFactory.CreatePerson();

//            var dic = new ScheduleDictionaryForTest(_scenario, new ScheduleDateTimePeriod(_range),
//                                                    new Dictionary<IPerson, IScheduleRange>());
//            _scheduleRange = new ScheduleRange(dic, new ScheduleParameters(_scenario, _person, _range));

//            _schedulePart = _scheduleRange.ScheduledPeriod(_range);

//            var rule = mocks.CreateMock<IBusinessRule>();
//            var rules = BusinessRuleCollection.Minimum();

//            rules.Add(rule);
            
//            DateTime theDate = new DateTime(2008,1,1,0,0,0, DateTimeKind.Utc);
           
//            Assert.IsNotNull(rule);

//            using (mocks.Record())
//            {
//                Expect
//                    .Call(rule.Validate(_schedulePart, theDate, StateHolderReader.Instance.StateReader.SessionScopeData.LoggedOnPerson.PermissionInformation.Culture()))
//                    .Return(false)
//                    .Repeat.Once();

//                Expect.Call(rule.IsMandatory).Return(false).Repeat.AtLeastOnce();

//            }
            
//            using (mocks.Playback())
//            {
//                rules.CheckRules(_schedulePart, theDate);
//            }
//        }

//        /// <summary>
//        /// Runs after each test.
//        /// </summary>
//        [TearDown]
//        public void BaseTeardown()
//        {
//            mocks.VerifyAll();
//        }
//    }
//}