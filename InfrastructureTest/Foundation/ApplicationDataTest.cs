using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Client.Composite;


namespace Teleopti.Ccc.InfrastructureTest.Foundation
{
	[TestFixture]
    [Category("LongRunning")]
    public class ApplicationDataTest
    {
        private MockRepository mocks;
        private IDictionary<string, string> _receivedSettings;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();

            _receivedSettings = new Dictionary<string, string>
                                    {
                                        {"HelpUrl", "http://wiki/ccc/"},
                                        {"MatrixWebSiteUrl", "http://localhost/Analytics"}
                                    };
        }



		[Test]
        public void VerifyApplicationDataCanBeSet()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            ILoadPasswordPolicyService passwordPolicy = mocks.StrictMock<ILoadPasswordPolicyService>();
			var target = new ApplicationData(_receivedSettings, messBroker, passwordPolicy);
            Assert.AreEqual(_receivedSettings["HelpUrl"], target.AppSettings["HelpUrl"]);
            Assert.AreSame(_receivedSettings, target.AppSettings);
            Assert.AreSame(messBroker, target.Messaging);
            Assert.AreSame(passwordPolicy,target.LoadPasswordPolicyService);
        }

        [Test]
        public void VerifyDispose()
        {
            IMessageBrokerComposite messBroker = mocks.StrictMock<IMessageBrokerComposite>();
            
			using (mocks.Record())
            {
                //dispose mess broker
                messBroker.Dispose();
            }
            using (mocks.Playback())
            {
                ApplicationData target = new ApplicationData(_receivedSettings, messBroker, null);
                target.Dispose();
            }
        }
    }
}