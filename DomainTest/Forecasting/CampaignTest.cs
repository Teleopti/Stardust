using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for the Campaign class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-03-04
    /// </remarks>
    [TestFixture]
    public class CampaignTest
    {
        Campaign target;
        Percent _tasksPercent;
        Percent _taskTimePercent;
        Percent _afterTaskTimePercent;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            target = new Campaign();

            _tasksPercent = new Percent(0.10d);
            _taskTimePercent = new Percent(0.15d);
            _afterTaskTimePercent = new Percent(0.20d);
        }

        /// <summary>
        /// Verifies the instance is created.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyInstanceIsCreated()
        {
            Assert.IsNotNull(target);
        }

        /// <summary>
        /// Verifies the default property values are zero.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyDefaultPropertyValuesAreZero()
        {
            Assert.AreEqual(0d, target.CampaignTasksPercent.Value);
            Assert.AreEqual(0d, target.CampaignTaskTimePercent.Value);
            Assert.AreEqual(0d, target.CampaignAfterTaskTimePercent.Value);
        }

        /// <summary>
        /// Verifies the constructor with arguments.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-03-04
        /// </remarks>
        [Test]
        public void VerifyConstructorWithArguments()
        {
            target = new Campaign(_tasksPercent,
                _taskTimePercent,
                _afterTaskTimePercent);

            Assert.IsNotNull(target);
            Assert.AreEqual(_tasksPercent, target.CampaignTasksPercent);
            Assert.AreEqual(_taskTimePercent, target.CampaignTaskTimePercent);
            Assert.AreEqual(_afterTaskTimePercent, target.CampaignAfterTaskTimePercent);
        }

        [Test]
        public void VerifyEqualsWork()
        {
            Campaign campaign = new Campaign(new Percent(), _taskTimePercent, _afterTaskTimePercent);
            Campaign campaign2 = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);
            target = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);

            Assert.IsTrue(target.Equals(campaign2));
            Assert.AreEqual(target, target);
            Assert.IsFalse(target.Equals(campaign));
        }

        [Test]
        public void VerifyEqualsReturnsFalseIfParameterIsNullAndParameterIsTask()
        {
            Campaign testObject = null;
            Assert.IsFalse(target.Equals(testObject));
        }

        [Test]
        public void VerifyEqualsReturnsFalseIfParameterIsNull()
        {
            object testObject = null;
            Assert.IsFalse(target.Equals(testObject));
        }

        [Test]
        public void VerifyGetHashCodeWorks()
        {
            IDictionary<Campaign, int> dic = new Dictionary<Campaign, int>();
            dic[target] = 5;

            Assert.AreEqual(5, dic[target]);
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            Campaign campaign2 = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);
            target = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);

            Assert.IsTrue(target == campaign2);
            Campaign campaign = new Campaign(new Percent(), _taskTimePercent, _afterTaskTimePercent);
            Assert.IsTrue(campaign != campaign2);
        }

        /// <summary>
        /// Verifies the overloaded operators work with null.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-20
        /// </remarks>
        [Test]
        public void VerifyOverloadedOperatorsWorkWithNull()
        {
            Campaign campaign2 = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);
            target = new Campaign(_tasksPercent, _taskTimePercent, _afterTaskTimePercent);

            Assert.IsFalse(null == campaign2);
            Assert.IsFalse(target == null);
            Assert.IsTrue(null != campaign2);
            Assert.IsTrue(target != null);
        }
    }
}
