using System;
using System.Globalization;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    /// <summary>
    /// Tests the BusinessRules class.
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-08
    /// </remarks>
    [TestFixture]
    public class BusinessRulesTest
    {
        }


        /// <summary>
        /// Verifies the can create mandatory rule.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        [Test]
        public void VerifyCanCreateMandatoryRule()
        {
            mocks.ReplayAll();

            target = new MandatoryBusinessRule(null);
            Assert.IsNotNull(target);
        }

        [Test]
        public void VerifyAllMethodsCalledOnMandatory()
        {
            mocks.ReplayAll();
            MandatoryBusinessRule rule = new MandatoryBusinessRule(null);

            bool result = rule.Validate(new DateTime(), CultureInfo.CurrentUICulture);
            Assert.IsTrue(result);
            DateTimePeriod per = rule.LongestDateTimePeriodForAssignment(new DateTime());
            Assert.IsNotNull(per);
            result = rule.IsMandatory;
            Assert.IsTrue(result);

        }

        [Test]
        public void VerifyLongestDateTimePeriodForAssignmentReturnsCorrect()
        {
            mocks.ReplayAll(); 
            BusinessRule baseRule = new BaseBusinessRule(null);
            DateTimePeriod result = baseRule.LongestDateTimePeriodForAssignment(DateTime.Now);
            Assert.AreEqual(DateTime.MinValue, result.StartDateTime);
            Assert.AreEqual(DateTime.MaxValue, result.EndDateTime);
        }

        /// <summary>
        /// Runs after each test.
        /// </summary>
        [TearDown]
        public void BaseTeardown()
        {
            mocks.VerifyAll();
        }

        /// <summary>
        /// An internal dummy mandatory class
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        internal class MandatoryBusinessRule : BusinessRule
        {
            public MandatoryBusinessRule(ISchedulePart schedulePart) : base(schedulePart)
            {
            }

            public override bool IsMandatory
            {
                get { return true; }
            }

            public override bool Validate(DateTime DateToStartCheckOn, CultureInfo cultureInfo)
            {
                return true;
            }

            public override DateTimePeriod LongestDateTimePeriodForAssignment(DateTime approximateTime)
            {
                var start = new DateTime(2008,1,1,8,0,0,DateTimeKind.Utc);
                var end = new DateTime(2008, 1, 2, 8, 0, 0, DateTimeKind.Utc);
                return new DateTimePeriod(start,end);
            }
            
        }

        //for testing virtual method(s)
        internal class BaseBusinessRule : BusinessRule
        {
            public BaseBusinessRule(ISchedulePart schedulePart) : base(schedulePart)
            {
            }

            public override bool IsMandatory
            {
                get { throw new System.NotImplementedException(); }
            }

            public override bool Validate(DateTime dateToStartCheckOn, CultureInfo cultureInfo)
            {
                throw new System.NotImplementedException();
            }
        }
       
    }
}