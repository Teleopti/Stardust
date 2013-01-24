using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Threading;
using Teleopti.Interfaces.Domain;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;
using Teleopti.Interfaces.MessageBroker.Client;
using Teleopti.Messaging.Exceptions;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaDataHandlerTest
    {
        private MockRepository _mocks;
        private ILog _loggingSvc;
        private IMessageSender _messageSender;
        private IDatabaseConnectionFactory _databaseConnectionFactory;
        private const string ConnectionString = "connection";
        private RtaDataHandlerForTest _target;
        private IDataSourceResolver _dataSourceResolver;
        private IPersonResolver _personResolver;
    	private IStateResolver _stateResolver;
    	private IRtaConsumer _rtaConsumer;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _loggingSvc = _mocks.DynamicMock<ILog>();
            _messageSender = _mocks.StrictMock<IMessageSender>();
            _databaseConnectionFactory = _mocks.StrictMock<IDatabaseConnectionFactory>();
            _dataSourceResolver = _mocks.StrictMock<IDataSourceResolver>();
        	_personResolver = _mocks.DynamicMock<IPersonResolver>();
    		_stateResolver = _mocks.DynamicMock<IStateResolver>();
        	_rtaConsumer = _mocks.DynamicMock<IRtaConsumer>();
        }

        [Test, ExpectedException(typeof(BrokerNotInstantiatedException))]
        public void VerifyCreateInstanceUsingEmptyConstructorFailsBecauseNoConfigurationAvailable()
        {
			new RtaDataHandler(_rtaConsumer);
        }

        [Test]
        public void VerifyErrorWhenMessageBrokerNotInstantiated()
        {
            _loggingSvc.Error("", new BrokerNotInstantiatedException());
            LastCall.IgnoreArguments();
            _messageSender.InstantiateBrokerService();
            LastCall.Throw(new BrokerNotInstantiatedException());
            _mocks.ReplayAll();
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory, _dataSourceResolver, _personResolver, _stateResolver, _rtaConsumer);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyProperties()
        {
            _messageSender.InstantiateBrokerService();
            Expect.Call(_messageSender.IsAlive).Return(true);
            _mocks.ReplayAll();
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, ConnectionString, _databaseConnectionFactory, _dataSourceResolver, _personResolver, _stateResolver, _rtaConsumer);
            Assert.IsTrue(_target.IsAlive);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyWarningWithNoConnectionString()
        {
            DateTime timestamp = DateTime.UtcNow;
			var autoreset = new AutoResetEvent(true);
            int dataSourceAfterResolve;
            Expect.Call(_dataSourceResolver.TryResolveId("1",
                                                        out dataSourceAfterResolve)).Return(true).OutRef(1);
            IEnumerable<PersonWithBusinessUnit> personListAfterResolve;
            Expect.Call(_personResolver.TryResolveId(1, "002", out personListAfterResolve)).Return(true).
				OutRef(new List<PersonWithBusinessUnit> { new PersonWithBusinessUnit() });
        	_stateResolver.Expect(s => s.HaveStateCodeChanged(Guid.Empty, "002")).IgnoreArguments().Return(true);
        	_rtaConsumer.Expect(
        		r =>
        		r.Consume(Guid.Empty, Guid.Empty, Guid.Empty, "002", DateTime.Now, TimeSpan.FromMinutes(1),
						  autoreset)).IgnoreArguments().Return(new ActualAgentState());

            _messageSender.InstantiateBrokerService();
            _messageSender.SendRtaData(Guid.Empty,Guid.Empty, null);
            LastCall.IgnoreArguments();
            Expect.Call(_messageSender.IsAlive).Return(true);
            _loggingSvc.Warn("No connection information available in configuration file.");

            _mocks.ReplayAll();
			_target = new RtaDataHandlerForTest(_loggingSvc, _messageSender, string.Empty, _databaseConnectionFactory, _dataSourceResolver, _personResolver, _stateResolver, _rtaConsumer);
            _target.ProcessRtaData("002", "AUX2", TimeSpan.FromSeconds(45), timestamp, Guid.NewGuid(), "1",
                                  SqlDateTime.MinValue.Value, false);
            _mocks.VerifyAll();
			autoreset.Dispose();
        }
		
        private class RtaDataHandlerForTest : RtaDataHandler
        {
			public RtaDataHandlerForTest(ILog loggingSvc, IMessageSender messageSender, string connectionStringDataStore, IDatabaseConnectionFactory databaseConnectionFactory, IDataSourceResolver dataSourceResolver, IPersonResolver personResolver, IStateResolver stateResolver, IRtaConsumer rtaConsumer)
				: base(loggingSvc, messageSender, connectionStringDataStore, databaseConnectionFactory, dataSourceResolver, personResolver, stateResolver, rtaConsumer)
            {
            }
        }
    }
}
