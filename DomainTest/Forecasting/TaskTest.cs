using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Forecasting
{
    /// <summary>
    /// Tests for ClassName
    /// </summary>
    [TestFixture]
    public class TaskTest
    {
        private Task target;
        private Task target2;
        private double tasks = 9.11;
        private TimeSpan averageTaskTime = new TimeSpan(0,0,112);
        private TimeSpan averageAfterTaskTime = new TimeSpan(0, 0, 118);

        /// <summary>
        /// Runs once per test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            target = new Task(tasks,averageTaskTime,averageAfterTaskTime);
            //Exactly the same
            target2 = new Task(tasks, averageTaskTime, averageAfterTaskTime);
        }

        /// <summary>
        /// Determines whether this instance [can set and get properties].
        /// </summary>
        [Test]
        public void CanSetAndGetProperties()
        {
            Assert.AreEqual(target.Tasks, tasks);
            Assert.AreEqual(target.AverageTaskTime, averageTaskTime);
            Assert.AreEqual(target.AverageAfterTaskTime, averageAfterTaskTime);
        }

        [Test]
        public void VerifyEqualsWork()
        {
            Task task = new Task(1, averageTaskTime, averageAfterTaskTime);

            Assert.IsTrue(target.Equals(target2));
            Assert.IsFalse(new ServiceAgreement().Equals(null));
            Assert.AreEqual(target, target);
            Assert.IsFalse(new ServiceAgreement().Equals(3));
            Assert.IsFalse(target.Equals(task));
        }

        [Test]
        public void VerifyEqualsReturnsFalseIfParameterIsNullAndParameterIsTask()
        {
            Task testObject = null;
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
            IDictionary<Task, int> dic = new Dictionary<Task, int>();
            dic[target] = 5;

            Assert.AreEqual(5, dic[target]);
        }

        [Test]
        public void VerifyOverloadedOperatorsWork()
        {
            Assert.IsTrue(target == target2);
            Task task = new Task(1, averageTaskTime, averageAfterTaskTime);
            Assert.IsTrue(task != target2);
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
            Assert.IsFalse(null == target2);
            Assert.IsFalse(target == null);
            Assert.IsTrue(null != target2);
            Assert.IsTrue(target != null);
        }

        [Test]
        public void PropertiesInitializesCorrect()
        {
            target = new Task();
            Assert.AreEqual(0,target.Tasks);
            Assert.AreEqual(TimeSpan.Zero, target.AverageAfterTaskTime);
            Assert.AreEqual(TimeSpan.Zero, target.AverageTaskTime);
        }

    }
}