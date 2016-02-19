using System;
using NUnit.Framework;
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
        private const double _minOcc = 0.6;
        private const double _maxOcc = 0.8;
        private const double _agents = 6.4;
		private readonly IStaffingCalculatorServiceFacade _calculatorService = new StaffingEmailCalculatorService();

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
        public void VerifyUtilization()
        {
            Assert.AreEqual(1,
                         Math.Round(_calculatorService.Utilization(_agents, _tasks, _averageHandlingTime, _periodLength, 1), 3));
        }
    }
}
