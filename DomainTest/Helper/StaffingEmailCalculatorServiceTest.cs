using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Class for verifying StaffingEmailCalculatorService
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-01-14
    /// </remarks>
    [TestFixture]
    public class StaffingEmailCalculatorServiceTest
    {
        private const double _sla = 1; //SLA is not used! Always calculated from 100%
        private const int _serviceTime = 7200; //2 hours
        private const double _tasks = 100;
        private const double _averageHandlingTime = 600; //10 min
        private readonly TimeSpan _periodLength = TimeSpan.FromMinutes(60);
        private const double _utilization = 2.2;
        private const double _minOcc = 0.6;
        private const double _maxOcc = 0.8;
        private const double _servers = 3.2;
        private const double _intensity = 2;
        private const double _agents = 6.4;
        private const int _orderedSla = 80;
        private readonly IStaffingCalculatorService _calculatorService = new StaffingEmailCalculatorService();

        [Test]
        public void VerifyAgents()
        {
			Assert.AreEqual(16.667, Math.Round(_calculatorService.TeleoptiAgents(_sla, _serviceTime, _tasks, _averageHandlingTime, _periodLength), 3));
			Assert.AreEqual(0, Math.Round(_calculatorService.TeleoptiAgents(_sla, 0, _tasks, _averageHandlingTime, _periodLength), 3));
        }
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyAgentsReturnsErrorWhenSlaGreaterThanOne()
        {
			_calculatorService.TeleoptiAgents(2, _serviceTime, _tasks, _averageHandlingTime, _periodLength);
        }
        [Test]
        public void VerifyAgentsFromUtilization()
        {
            Assert.AreEqual(0,
                            Math.Round(_calculatorService.AgentsFromUtilization(_utilization, _tasks, _averageHandlingTime,
                                                                            _periodLength), 3));
        }

        [Test]
        public void VerifyAgentsFromUtilizationCanHandleZeroCalls()
        {
            Assert.AreEqual(0,
                            Math.Round(_calculatorService.AgentsFromUtilization(_utilization, 0, _averageHandlingTime,
                                                                            _periodLength), 3));
        }

        [Test]
        public void VerifyAgentsFromUtilizationCanHandleZeroHandlingTime()
        {
            Assert.AreEqual(0,
                            Math.Round(_calculatorService.AgentsFromUtilization(_utilization, _tasks, 0,
                                                                            _periodLength), 3));
        }

        [Test]
        public void VerifyAgentsUseOccupancy()
        {
            Assert.AreEqual(16.667,
                            Math.Round(
                                _calculatorService.AgentsUseOccupancy(_sla, _serviceTime, _tasks,
                                                                             _averageHandlingTime, _periodLength,
                                                                             _minOcc, _maxOcc,1), 3));
        }

        [Test]
        public void VerifyTeleoptiErgBExtended()
        {
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(-1, _intensity), 3));
        }
        [Test]
        public void VerifyTeleoptiErgCExtended()
        {
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(-1, _intensity), 3));
        }
        [Test]
        public void VerifyServiceLevelAchieved()
        {
            Assert.AreEqual(1,
                         Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, _tasks, _averageHandlingTime, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, _tasks, 0, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(0, _serviceTime, _tasks, _averageHandlingTime, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, 1, _averageHandlingTime, _periodLength, _orderedSla), 3));
        }
        [Test]
        public void VerifyUtilization()
        {
            Assert.AreEqual(1,
                         Math.Round(_calculatorService.Utilization(_agents, _tasks, _averageHandlingTime, _periodLength), 3));
        }
    }
}
