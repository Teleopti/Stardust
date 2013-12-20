using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Class for verifying StaffingCalculatorService
    /// </summary>
    [TestFixture]
    public class StaffingCalculatorServiceTest
    {
        private double _sla;
        private int _serviceTime;
        private double _calls;
        private double _averageHandlingTime;
        private TimeSpan _periodLength;
        private double _utilization = 2.2;
        private double _minOcc = 0.6;
        private double _maxOcc = 0.8;
        private double _servers = 3.2;
        private double _intensity = 2;
        private double _agents = 6.4;
        private int _orderedSla = 80;
        private IStaffingCalculatorService _calculatorService;

		[SetUp]
		public void Setup()
		{
			_sla = 0.8;
			_serviceTime = 20;
			_calls = 134;
			_averageHandlingTime = 33.0;
			_periodLength = TimeSpan.FromMinutes(15);
			_utilization = 2.2;
			_minOcc = 0.6;
			_maxOcc = 0.8;
			_servers = 3.2;
			_agents = 6.4;
			_orderedSla = 80;
			_intensity = 2;
			_calculatorService = new StaffingCalculatorService();
		}

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyAgentsReturnsErrorWhenSlaGreaterThanOne()
        {
			_calculatorService.TeleoptiAgents(2, _serviceTime, _calls, _averageHandlingTime, _periodLength);
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
            Assert.AreEqual(6.4359, _calculatorService.AgentsUseOccupancy(_sla, _serviceTime, _calls,
                                                                             _averageHandlingTime, _periodLength,
                                                                             _minOcc, _maxOcc,1), 0.001);
        }
		
        [Test]
        public void VerifyAgentsUseOccupancySmallDemandWorks()
        {
            //Bug 8500 Incorrect ErlangCalc when more than 2 calls ond total handlingtime < intervallength/2
            //AgentsUseOccupancy(double sla, int serviceTime, double calls, double averageHandlingTime,
            //                      TimeSpan periodLength, double minOccupancy, double maxOccupancy);

            TimeSpan periodLength = TimeSpan.FromMinutes(15);
            double result1 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds/6) + 1, periodLength, 0, 1,1);

            double result2 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds / 6), periodLength, 0, 1,1);

            double result3 = _calculatorService.AgentsUseOccupancy(0.8, 20, 3,
                                                                  (periodLength.TotalSeconds / 6) - 1, periodLength, 0, 1,1);
            
            Assert.AreEqual(result1, result2, 0.01);
            Assert.AreEqual(result2, result3, 0.01); //Failed at first difference was almost 1 agent
        }
        
        [Test]
        public void VerifyTeleoptiErgBExtended()
        {
            Assert.AreEqual(0.187d,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgBExtended(-1, _intensity), 3));
        }

        [Test]
        public void VerifyTeleoptiErgCExtended()
        {
            Assert.AreEqual(0.381d,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(_servers, _intensity), 3));
            Assert.AreEqual(0,
                           Math.Round(_calculatorService.TeleoptiErgCExtended(-1, _intensity), 3));
        }

        [Test]
        public void VerifyServiceLevelAchieved()
        {
            Assert.AreEqual(0.793,
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


		[Test]
		public void ShouldDivideAgentsWithParallelTasks()
		{
			var expected =  Math.Round(6.435 / 3, 3);

			var result = Math.Round(_calculatorService.AgentsUseOccupancy(_sla, _serviceTime, _calls,
																		_averageHandlingTime, _periodLength,
																		_minOcc, _maxOcc, 3), 3);

			Assert.That(result,Is.EqualTo(expected));
		}
    }
}
