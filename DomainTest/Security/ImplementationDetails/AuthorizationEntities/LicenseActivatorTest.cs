using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Secrets.Licensing;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.AuthorizationEntities
{
	[TestFixture]
	public class LicenseActivatorTest
	{
		private ILicenseActivator _target;
		private const string _customerName = "Kunden";
		private const int _maxActiveAgents = 10;
		private readonly Percent _maxActiveAgentsGrace = new Percent(0.1);
		private readonly DateTime _expirationDate = DateTime.Now.AddDays(2);
		private readonly bool _showAsPerpetual = true;

		[SetUp]
		public void Setup()
		{
			_target = new LicenseActivator(_customerName, _expirationDate, _showAsPerpetual, _maxActiveAgents, 100, LicenseType.Agent, _maxActiveAgentsGrace,
							XmlLicenseService.IsThisAlmostTooManyActiveAgents, LicenseActivator.IsThisTooManyActiveAgents, "8");
		}

		[Test]
		public void VerifyProperties()
		{
			IList<string> setOptions = new List<string> { "schema1/option1", "schema1/option2" };
			foreach (string setOption in setOptions)
			{
				_target.EnabledLicenseOptionPaths.Add(setOption);
			}

			IList<string> getOptions = _target.EnabledLicenseOptionPaths;

			foreach (string option in getOptions)
			{
				Assert.IsTrue(setOptions.Contains(option));
			}

			Assert.AreEqual("schema1", _target.EnabledLicenseSchemaName);
			Assert.IsFalse(string.IsNullOrEmpty(_target.CustomerName));;
			Assert.AreEqual(_expirationDate, _target.ExpirationDate);
			Assert.AreEqual(_showAsPerpetual, _target.Perpetual);
			int maxActiveAgents = _target.MaxActiveAgents;
			Assert.Less(0, maxActiveAgents);
			Percent maxActiveAgentsGrace = _target.MaxActiveAgentsGrace;
			Assert.Less(0.0, maxActiveAgentsGrace.Value);

			Assert.IsFalse(_target.IsThisTooManyActiveAgents(0));
			Assert.IsFalse(_target.IsThisTooManyActiveAgents(maxActiveAgents));
			Assert.IsTrue(_target.IsThisTooManyActiveAgents((int)Math.Ceiling(maxActiveAgents * (1.0 + maxActiveAgentsGrace.Value)) + 1));

			Assert.IsFalse(_target.IsThisAlmostTooManyActiveAgents(0));
			Assert.IsFalse(_target.IsThisAlmostTooManyActiveAgents(maxActiveAgents - 1));
			Assert.IsTrue(_target.IsThisAlmostTooManyActiveAgents(maxActiveAgents + 1));
			_target.MajorVersion.Should().Be.EqualTo("8");
		}

	}
}
