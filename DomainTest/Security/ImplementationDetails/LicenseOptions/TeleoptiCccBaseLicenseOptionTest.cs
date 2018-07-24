﻿using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.LicenseOptions;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.LicenseOptions
{
	[TestFixture]
	public class TeleoptiCccBaseLicenseOptionTest
	{
		private TeleoptiCccBaseLicenseOption _target;

		[SetUp]
		public void Setup()
		{
			_target = new TeleoptiCccBaseLicenseOption();
		}

		[Test]
		public void VerifyEnable()
		{
			var inputList = new List<IApplicationFunction>();
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			Assert.That(enabledFunctions, Is.Not.Null);
			Assert.That(enabledFunctions.Any());
		}

		[Test]
		public void ShouldIncludeRemoveShiftInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			Assert.That(enabledFunctions, Is.Not.Null);
			Assert.That(enabledFunctions.Any(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RemoveShift));
		}

		[Test]
		public void ShouldIncludeAddDayOffInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.AddDayOff).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIncludeRemoveDayOffInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.RemoveDayOff).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIncludePeopleAccessInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.PeopleAccess).Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIncludePeopleManageUserInBaseCollection()
		{
			var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
			_target.EnableApplicationFunctions(inputList);
			var enabledFunctions = _target.EnabledApplicationFunctions;
			enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.PeopleManageUsers).Should().Not.Be.Null();
		}

		[Test]
        public void ShouldIncludeWebForecastInBaseCollection()
        {
            var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
            _target.EnableApplicationFunctions(inputList);
            var enabledFunctions = _target.EnabledApplicationFunctions;
            enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.WebForecasts).Should().Not.Be.Null();
        }
		[Test]
        public void ShouldIncludeGamificationInBaseCollection()
        {
            var inputList = new DefinedRaptorApplicationFunctionFactory().ApplicationFunctions;
            _target.EnableApplicationFunctions(inputList);
            var enabledFunctions = _target.EnabledApplicationFunctions;
            enabledFunctions.Single(f => f.FunctionPath == DefinedRaptorApplicationFunctionPaths.Gamification).Should().Not.Be.Null();
        }
	}
}