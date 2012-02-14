using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.OldTests
{
    [TestFixture]
    public class QueueDtoTest
    {
        private QueueDto _target;
        private string _queueName;
        private int _answeredContracts;
        private TimeSpan _averageTalkTime;
        private TimeSpan _afterContactWork;
        private TimeSpan _totalHandlingTime;
        private TimeSpan _availableTime;

        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/23/2008
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _target = new QueueDto();

            _queueName = "Risath´s Queue";
            _answeredContracts = 50;
            _averageTalkTime = new TimeSpan(0, 8, 0, 0);
            _availableTime = new TimeSpan(0, 8, 0, 0);
            _totalHandlingTime = new TimeSpan(0, 8, 0, 0);
            _afterContactWork = new TimeSpan(0, 8, 0, 0);

            _target.Name = _queueName;
            _target.AnsweredContracts = _answeredContracts;
            _target.AverageTalkTime = _averageTalkTime.Ticks;
            _target.TotalHandlingTime = _totalHandlingTime.Ticks;
            _target.AvailableTime = _availableTime.Ticks;
            _target.AfterContactWork = _afterContactWork.Ticks;

        }

        /// <summary>
        /// Verifies the can set and get properties.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/23/2008
        /// </remarks>
        [Test]
        public void VerifyCanSetAndGetProperties()
        {
            Assert.AreEqual(_target.Name, _queueName);
            Assert.AreEqual(_target.AnsweredContracts, _answeredContracts);
            Assert.AreEqual(_target.AverageTalkTime, _averageTalkTime.Ticks);
            Assert.AreEqual(_target.AfterContactWork, _afterContactWork.Ticks);
            Assert.AreEqual(_target.AvailableTime, _availableTime.Ticks);
            Assert.AreEqual(_target.TotalHandlingTime, _totalHandlingTime.Ticks);
        }



        /// <summary>
        /// Verifies the instance created.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 10/23/2008
        /// </remarks>
        [Test]
        public void VerifyInstanceCreated()
        {
            Assert.IsNotNull(_target);
        }



    }
}