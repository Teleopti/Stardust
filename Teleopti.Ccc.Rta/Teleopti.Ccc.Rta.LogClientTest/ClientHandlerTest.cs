using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.Remoting;
using System.Security;
using log4net;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.LogClient;

namespace Teleopti.Ccc.Rta.LogClientTest
{
    [TestFixture]
    [Category("LongRunning")]
    public class ClientHandlerTest
    {
        private ClientHandlerForTest target;
        private MockRepository mocks;
        private ILog loggingSvc;
        private IRtaDataHandlerClient rtaDataHandler;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
            loggingSvc = mocks.DynamicMock<ILog>();
            rtaDataHandler = mocks.StrictMock<IRtaDataHandlerClient>();
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsNotNull(new ClientHandler(new Hashtable()));
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Teleopti.Ccc.Rta.LogClientTest.ClientHandlerTest+ClientHandlerForTest"), Test]
		public void ShouldThrowExceptionGivenClientHandlerIsNull()
		{
			Assert.Throws<ArgumentNullException>(()=>new ClientHandlerForTest(loggingSvc, null, new Hashtable()));
		}

		[Test]
		public void VerifyConstructor()
		{
			using (mocks.Record())
			{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				Expect.Call(()=>loggingSvc.Warn("")).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
				Assert.IsNotNull(target);
			}
		}

        [Test]
        public void VerifyStartLogClient()
        {
        	using (mocks.Record())
        	{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				rtaDataHandler.Url = "http://localhost/TeleoptiRtaService.svc";
				rtaDataHandler.Timeout = 15;

				Expect.Call(()=>loggingSvc.Warn("")).IgnoreArguments();
				Expect.Call(rtaDataHandler.IsAlive).Return(true);
				Expect.Call(rtaDataHandler.IsAlive).Return(false);
        	}
        	using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
				target.StartLogClient();
				Assert.IsTrue(target.IsStarted);
				target.StopLogClient();
				Assert.IsFalse(target.IsStarted);
        	}
        }

        [Test]
        public void VerifyStartLogClientHandlesSecurityException()
        {
        	using (mocks.Record())
        	{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				rtaDataHandler.Url = "http://localhost/TeleoptiRtaService.svc";
				LastCall.Throw(new SecurityException());
				Expect.Call(()=>loggingSvc.Error("", null)).IgnoreArguments();
				Expect.Call(rtaDataHandler.IsAlive).Return(false);
			}
        	using (mocks.Playback())
        	{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
				Assert.Throws<SecurityException>(target.StartLogClient);
				Assert.IsFalse(target.IsStarted);
        	}
        }

        [Test]
        public void VerifyStartLogClientHandlesRemotingException()
        {
        	using (mocks.Record())
			{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				rtaDataHandler.Url = "http://localhost/TeleoptiRtaService.svc";
				LastCall.Throw(new RemotingException());
				Expect.Call(()=>loggingSvc.Error("", null)).IgnoreArguments();
				Expect.Call(rtaDataHandler.IsAlive).Return(false);
        	}
        	using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
				Assert.Throws<RemotingException>(target.StartLogClient);
				Assert.IsFalse(target.IsStarted);
        	}
        }

        [Test]
        public void VerifySendRtaDataToServerWithException()
        {
        	using (mocks.Record())
			{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				Expect.Call(()=> rtaDataHandler.ProcessRtaData(string.Empty, string.Empty, TimeSpan.Zero, DateTime.UtcNow, Guid.Empty, string.Empty, DateTime.UtcNow, false)).IgnoreArguments().Throw(new InvalidOperationException());
				Expect.Call(()=>loggingSvc.Error("",null)).IgnoreArguments();
        	}
			using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, null);
				Assert.Throws<InvalidOperationException>(
					() => target.SendRtaDataToServer("001", "AUX1", TimeSpan.FromSeconds(55), DateTime.UtcNow, Guid.NewGuid(), 1,
					                                 DateTime.UtcNow, true));
			}
        }

        [Test]
        public void VerifySendRtaDataToServer()
        {
			Guid platformId = Guid.NewGuid();
			DateTime timestamp = DateTime.UtcNow;

        	using (mocks.Record())
        	{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				Expect.Call(()=>loggingSvc.Warn("")).IgnoreArguments();
				Expect.Call(rtaDataHandler.ProcessRtaData("001", "AUX1", TimeSpan.FromSeconds(55), timestamp, platformId, "1", SqlDateTime.MinValue.Value, false)).Return(1);
				LastCall.Repeat.Once();
        	}
        	using (mocks.Playback())
        	{
        		target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
        		target.SendRtaDataToServer("001", "AUX1", TimeSpan.FromSeconds(55), timestamp, platformId, 1,
        		                           SqlDateTime.MinValue.Value, false);
        	}
        }

		[Test]
		public void VerifySendBatchRtaDataToServer()
		{
			var platformId = Guid.NewGuid();
			var timestamp = DateTime.UtcNow;
			var rtaStates = new List<ITeleoptiRtaState>
			                	{
			                		new TeleoptiRtaState
			                			{
			                				BatchId = SqlDateTime.MinValue.Value,
			                				IsSnapshot = false,
			                				LogOn = "001",
			                				StateCode = "AUX1",
			                				TimeInState = TimeSpan.FromSeconds(55),
			                				Timestamp = timestamp
			                			}
			                	};

			using (mocks.Record())
			{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				Expect.Call(() => loggingSvc.Warn("")).IgnoreArguments();
				rtaDataHandler.ProcessRtaData(platformId, "1", rtaStates);
				LastCall.Repeat.Once();
			}
			using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, new Hashtable());
				target.SendRtaDataToServer(platformId,1,rtaStates);
			}
		}

		[Test]
		public void VerifySendBatchRtaDataToServerWithException()
		{
			var platformId = Guid.NewGuid();
			var timestamp = DateTime.UtcNow;
			var rtaStates = new List<ITeleoptiRtaState>
			                	{
			                		new TeleoptiRtaState
			                			{
			                				BatchId = SqlDateTime.MinValue.Value,
			                				IsSnapshot = false,
			                				LogOn = "001",
			                				StateCode = "AUX1",
			                				TimeInState = TimeSpan.FromSeconds(55),
			                				Timestamp = timestamp
			                			}
			                	};

			using (mocks.Record())
			{
				Expect.Call(rtaDataHandler.Timeout).Return(15).Repeat.AtLeastOnce();
				Expect.Call(rtaDataHandler.ProcessRtaData(platformId, "1", rtaStates)).IgnoreArguments().Throw(new InvalidOperationException());
				Expect.Call(() => loggingSvc.Error("", null)).IgnoreArguments();
			}
			using (mocks.Playback())
			{
				target = new ClientHandlerForTest(loggingSvc, rtaDataHandler, null);
				Assert.Throws<InvalidOperationException>(
					() => target.SendRtaDataToServer(platformId, 1, rtaStates));
			}
		}

        private class ClientHandlerForTest : ClientHandler
        {
            public ClientHandlerForTest(ILog loggingSvc, IRtaDataHandlerClient rtaDataHandler, IDictionary clientSettings)
                : base(loggingSvc, rtaDataHandler, clientSettings)
            {
            }
        }
    }
}
