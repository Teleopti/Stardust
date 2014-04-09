using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Calculation;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Helper
{
    /// <summary>
    /// Class for verifying StaffingCalculatorService model working
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 2013-10-21
    /// </remarks>
    [TestFixture]
    public class StaffingCalculatorServiceModelTest
    {
      
        private IStaffingCalculatorService _calculatorService;
	    private double _delta;

		[SetUp]
		public void Setup()
		{
			_calculatorService = new Domain.Calculation.StaffingCalculatorService();
			_delta = 0.0001;

		}
		
		[Test]	
		public void ModelTest1()
		{
			var res = _calculatorService.TeleoptiAgents(0.8, 30, 41.7d, 500, TimeSpan.FromSeconds(900));
			Assert.AreEqual(27.8439d, res, _delta);
		}

		[Test]
		public void ModelTest2()
		{
			var res = _calculatorService.TeleoptiAgents(0.7, 60, 456, 120, TimeSpan.FromMinutes(30));
			Assert.AreEqual(32.0769d, res, _delta);
		}

		[Test]
		public void ModelTest3()
		{
			var res = _calculatorService.TeleoptiAgents(0.99, 20, 380, 3000, TimeSpan.FromMinutes(30));
			Assert.AreEqual(690.6935d, res, _delta);
		}

		[Test]
		public void ModelTest4()
		{
			var res = _calculatorService.TeleoptiAgents(0.8, 20, 1000, 3601, TimeSpan.FromMinutes(30));
			Assert.AreEqual(2042.4907d, res, _delta);
		}

		[Test]
		public void ModelTest5()
		{
			var res = _calculatorService.TeleoptiAgents(0.6, 120, 500, 400, TimeSpan.FromMinutes(15));
			Assert.AreEqual(224.6483d, res, _delta);
		}       
    }
}
