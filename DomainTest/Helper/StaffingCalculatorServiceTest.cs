using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Class for verifying StaffingCalculatorService
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-01-14
    /// </remarks>
    [TestFixture]
    public class StaffingCalculatorServiceTest
    {
        private const double _sla = 0.8;
        private const int _serviceTime = 20;
        private const double _calls = 134;
        private const double _averageHandlingTime = 33.0;
        private readonly TimeSpan _periodLength = TimeSpan.FromMinutes(15);
        private const double _utilization = 2.2;
        private const double _minOcc = 0.6;
        private const double _maxOcc = 0.8;
        private const double _servers = 3.2;
        private const double _intensity = 2;
        private const double _agents = 6.4;
        private const int _orderedSla = 80;
        private readonly IStaffingCalculatorService _calculatorService = new StaffingCalculatorService();

        [Test]
        public void VerifyAgents()
        {
            Assert.AreEqual(6.413, Math.Round(_calculatorService.Agents(_sla, _serviceTime, _calls, _averageHandlingTime, _periodLength), 3));
            Assert.AreEqual(32.07d, Math.Round(_calculatorService.Agents(0.7, 60, 456, 120, TimeSpan.FromMinutes(30)), 3));
            Assert.AreEqual(6333.833d, Math.Round(_calculatorService.Agents(1, 20, 380, 30000, TimeSpan.FromMinutes(30)), 3));
            Assert.AreEqual(4004.5d, Math.Round(_calculatorService.Agents(_sla, 20, 1001, 3600, _periodLength), 3));
            Assert.AreEqual(4001.611d, Math.Round(_calculatorService.Agents(_sla, 20, 1000, 3601, _periodLength), 3));
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyAgentsReturnsErrorWhenSlaGreaterThanOne()
        {
            _calculatorService.Agents(2, _serviceTime, _calls, _averageHandlingTime, _periodLength);
        }
        [Test]
        public void VerifyAgentsFromUtilization()
        {
            Assert.AreEqual(2.233,
                            Math.Round(_calculatorService.AgentsFromUtilization(_utilization, _calls, _averageHandlingTime,
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
                            Math.Round(_calculatorService.AgentsFromUtilization(_utilization, _calls, 0,
                                                                            _periodLength), 3));
        }

        [Test]
        public void VerifyAgentsUseOccupancy()
        {
            Assert.AreEqual(6.413,
                            Math.Round(
                                _calculatorService.AgentsUseOccupancy(_sla, _serviceTime, _calls,
                                                                             _averageHandlingTime, _periodLength,
                                                                             _minOcc, _maxOcc), 3));
        }

        [Test]
        public void VerifyAgentsUseOccupancySmallDemandWorks()
        {
            //Bug 8500 Incorrect ErlangCalc when more than 2 calls ond total handlingtime < intervallength/2
            //AgentsUseOccupancy(double sla, int serviceTime, double calls, double averageHandlingTime,
            //                      TimeSpan periodLength, double minOccupancy, double maxOccupancy);

            TimeSpan periodLength = TimeSpan.FromMinutes(15);
            double result1 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds/6) + 1, periodLength, 0, 1);

            double result2 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds / 6), periodLength, 0, 1);

            double result3 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds / 6) - 1, periodLength, 0, 1);
            
            Assert.AreEqual(result1, result2, 0.01);
            Assert.AreEqual(result2, result3, 0.01); //Failed at first difference was almost 1 agent
        }
            

        [Test]
        public void VerifyTeleoptiErgBExtended()
        {
            Assert.AreEqual(0.211,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(-1, _intensity), 3));
        }
        [Test]
        public void VerifyTeleoptiErgCExtended()
        {
            Assert.AreEqual(0.416,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(-1, _intensity), 3));
        }
        [Test]
        public void VerifyServiceLevelAchieved()
        {
            Assert.AreEqual(0.799,
                         Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, _calls, _averageHandlingTime, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, _calls, 0, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, 0, _averageHandlingTime, _periodLength, _orderedSla), 3));
            Assert.AreEqual(0,
                        Math.Round(_calculatorService.ServiceLevelAchieved(0, _serviceTime, _calls, _averageHandlingTime, _periodLength, _orderedSla), 3));
            Assert.AreEqual(1,
                        Math.Round(_calculatorService.ServiceLevelAchieved(_agents, _serviceTime, 1, _averageHandlingTime, _periodLength, _orderedSla), 3));
        }
        [Test]
        public void VerifyUtilization()
        {
            Assert.AreEqual(0.768,
                         Math.Round(_calculatorService.Utilization(_agents, _calls, _averageHandlingTime, _periodLength), 3));
        }
       
    }
}
