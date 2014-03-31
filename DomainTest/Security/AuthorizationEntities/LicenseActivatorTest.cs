using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Licensing;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
    [TestFixture]
    public class LicenseActivatorTest
    {
        private ILicenseActivator _target;
        private const string _customerName = "Kunden";
        private const int _maxActiveAgents = 10;
        private readonly Percent _maxActiveAgentsGrace = new Percent(0.1);
        private readonly DateTime _expirationDate = DateTime.Now.AddDays(2);

        [SetUp]
        public void Setup()
        {
            _target = new LicenseActivator(_customerName, _expirationDate, _maxActiveAgents, 100, LicenseType.Agent, _maxActiveAgentsGrace,
								XmlLicenseService.IsThisAlmostTooManyActiveAgents, LicenseActivator.IsThisTooManyActiveAgents);
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

            string expectedSchema = "schema1";
            string getName = _target.EnabledLicenseSchemaName;
            Assert.AreEqual(expectedSchema, getName);

            string customerName = _target.CustomerName;
            Assert.IsFalse(string.IsNullOrEmpty(customerName));
            DateTime expirationDate = _target.ExpirationDate;
            Assert.AreEqual(_expirationDate, expirationDate);
            int maxActiveAgents = _target.MaxActiveAgents;
            Assert.Less(0, maxActiveAgents);
            Percent maxActiveAgentsGrace = _target.MaxActiveAgentsGrace;
            Assert.Less(0.0, maxActiveAgentsGrace.Value);

            Assert.IsFalse(_target.IsThisTooManyActiveAgents(0));
            Assert.IsFalse(_target.IsThisTooManyActiveAgents(maxActiveAgents));
            Assert.IsTrue(_target.IsThisTooManyActiveAgents((int)Math.Ceiling(maxActiveAgents*(1.0 + maxActiveAgentsGrace.Value)) + 1));

            Assert.IsFalse(_target.IsThisAlmostTooManyActiveAgents(0));
            Assert.IsFalse(_target.IsThisAlmostTooManyActiveAgents(maxActiveAgents - 1));
            Assert.IsTrue(_target.IsThisAlmostTooManyActiveAgents(maxActiveAgents + 1));

        }

    }
}
