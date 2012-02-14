//using System;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Teleopti.Ccc.Domain.ResourceCalculation;
//using Teleopti.Ccc.Domain.Scheduling.Rules;
//using Teleopti.Interfaces.Domain;

//namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
//{
//    [TestFixture]
//    public class PersonAssignmentBusinessRulesTest
//    {
//        private IBusinessRuleCollection target;
//        private MockRepository mocks = new MockRepository();

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), SetUp]
//        public void Setup()
//        {

//        }

//        /// <summary>
//        /// Verifies can create instance.
//        /// </summary>
//        /// <remarks>
//        /// Created by: robink
//        /// Created date: 2007-11-08
//        /// </remarks>
//        [Test]
//        public void VerifyCanCreate()
//        {
//            CreatePersonAssignmentRules();
//            Assert.IsNotNull(target);
//        }

//        [Test]
//        public void VerifyMaxOneDayOffIsIncluded()
//        {
//            CreatePersonAssignmentRules();
//            foreach (var br in target)
//            {
//                if(br is MaxOneDayOffRule)
//                    return;
//            }
//            Assert.Fail();
//        }

//        [Test]
//        public void VerifyCheckRulesCalled()
//        {
//            CreatePersonAssignmentRules();
//            DateTime start = new DateTime(2000,1,1,0,0,0,DateTimeKind.Utc);
            
//            using (mocks.Playback())
//            {
//                var resp = target.CheckRules(_schedulePart, start);
//                foreach (IBusinessRuleResponse response in resp)
//                {
//                    response.Overridden = true;
//                }
//                resp = target.CheckRules(_schedulePart, start);
//                Assert.IsNotNull(resp); 
//            }
//        }


//        private void CreatePersonAssignmentRules()
//        {
//            target = BusinessRuleCollection.All(new SchedulingResultStateHolder(), new PersonAssignmentBusinessRulesOptions()); 
//        }

//    }
//}