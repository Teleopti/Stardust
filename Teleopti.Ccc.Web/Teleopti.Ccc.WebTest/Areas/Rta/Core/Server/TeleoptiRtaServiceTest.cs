using System;
using System.Collections.Specialized;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core.Server
{
	[TestFixture]
	public class TeleoptiRtaServiceTest
	{
		[Test]
		public void ShouldProcessValidState()
		{
			var state = new externalUserStateForTest();
			var target = new rtaServiceFactoryForTest().CreateRtaServiceBasedOnAValidState(state);

			state.SaveExternalUserStateTo(target).Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfAuthenticationKeyIsIncorrect()
		{

			var state = new externalUserStateForTest(){AuthenticationKey = "something" };
			var target = new rtaServiceFactoryForTest().CreateRtaServiceBasedOnAValidState(state);

			state.AuthenticationKey += " else";

			Assert.Throws(typeof(FaultException), () => state.SaveExternalUserStateTo(target));
		}


		[Test]
		public void ShouldNotProcessStatesThatsToOld()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);
			
			var state = new externalUserStateForTest() { TimeStamp = christmas.AddHours(1) };
			var target = new rtaServiceFactoryForTest() { Now = now}.CreateRtaServiceBasedOnAValidState(state);

			state.SaveExternalUserStateTo(target).Should().Not.Be.EqualTo(1);	
		}

		[Test]
		public void ShouldNotProcessStatesWithTimeStampNewerThanNow()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);

			var state = new externalUserStateForTest() { TimeStamp = christmas.Subtract(TimeSpan.FromHours(1)) };
			var target = new rtaServiceFactoryForTest() { Now = now }.CreateRtaServiceBasedOnAValidState(state);

			state.SaveExternalUserStateTo(target).Should().Not.Be.EqualTo(1);	
		}

		[Test]
		public void ShouldNotProcessIfNoSourceId()
		{
			var state = new externalUserStateForTest();
			var target = new rtaServiceFactoryForTest().CreateRtaServiceBasedOnAValidState(state);

			state.SourceId = string.Empty;

			state.SaveExternalUserStateTo(target).Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldCallRTaDataHandlerWithSuppliedStateCode()
		{
			var state = new externalUserStateForTest();
			var dataHandler = new fakeDataHandler();
			var target = new rtaServiceFactoryForTest(){RtaDataHandler = dataHandler}.CreateRtaServiceBasedOnAValidState(state);

			state.SaveExternalUserStateTo(target);

			dataHandler.StateCodeParameter.Should().Be.EqualTo(state.StateCode);
		}

		[Test]
		public void ShouldNotProcessStateIfNoPlatformId()
		{
			var state = new externalUserStateForTest();
			var target = new rtaServiceFactoryForTest().CreateRtaServiceBasedOnAValidState(state);

			state.PlatformTypeId = string.Empty;

			state.SaveExternalUserStateTo(target).Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldCutStateCodeIfToLong()
		{
			var state = new externalUserStateForTest();
			var dataHandler = new fakeDataHandler();
			var target = new rtaServiceFactoryForTest(){RtaDataHandler = dataHandler}.CreateRtaServiceBasedOnAValidState(state);

			state.StateCode = "a really really really really looooooooong statecode that should be trimmed somehow for whatever reason";

			state.SaveExternalUserStateTo(target);

			Assert.That(dataHandler.StateCodeParameter.Length,Is.LessThanOrEqualTo(25));

		}

		[Test]
		public void ShouldSetStateCodeToLoggedOutIfNotLoggedIn()
		{
			var state = new externalUserStateForTest();
			var dataHandler = new fakeDataHandler();
			var target = new rtaServiceFactoryForTest() { RtaDataHandler = dataHandler }.CreateRtaServiceBasedOnAValidState(state);

			state.IsLoggedOn = false;

			state.SaveExternalUserStateTo(target);

			dataHandler.StateCodeParameter.Should().Be.EqualTo(TeleoptiRtaService.LogOutStateCode);
		}

		#region helpers
		private class fakeDataHandler : IRtaDataHandler
		{
			public string StateCodeParameter { get; private set; }


			public int ProcessRtaData(string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId,
				string sourceId, DateTime batchId, bool isSnapshot)
			{
				StateCodeParameter = stateCode;

				return 1;
			}

			public void ProcessScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
			{
				throw new NotImplementedException();
			}

			public bool IsAlive { get; set; }
		}

		private class externalUserStateForTest
		{
			public string AuthenticationKey = "secret auth key";
			public string UserCode = "8808";
			public string StateCode = "AUX2";
			public string StateDescription { get; set; }
			public bool IsLoggedOn = true;
			public int SecondsInState { get; set; }
			public DateTime TimeStamp { get; set; }
			public string PlatformTypeId = Guid.Empty.ToString();
			public string SourceId = "sourceId";
			public DateTime BatchId { get; set; }
			public bool IsSnapshot { get; set; }


			public int SaveExternalUserStateTo(TeleoptiRtaService service)
			{
				return service.SaveExternalUserState(AuthenticationKey, UserCode, StateCode, StateDescription,
					IsLoggedOn, SecondsInState, TimeStamp, PlatformTypeId, SourceId, BatchId, IsSnapshot);
			}

			
		
		}

		private class rtaServiceFactoryForTest
		{
			public INow Now { get; set; }
			public IConfigReader ConfigReader { get; set; }
			public IRtaDataHandler RtaDataHandler { get; set; }

			public TeleoptiRtaService CreateRtaServiceBasedOnAValidState(externalUserStateForTest userState)
			{
				if (ConfigReader == null)
				{
					ConfigReader = MockRepository.GenerateStub<IConfigReader>();
					var appSettings = new NameValueCollection();
					appSettings.Add("AuthenticationKey", userState.AuthenticationKey);
					ConfigReader.Expect(c => c.AppSettings).Return(appSettings).Repeat.Any();
				}
				if (Now == null)
				{
					Now = MockRepository.GenerateStub<INow>();
					Now.Expect(n => n.UtcDateTime()).Return(userState.TimeStamp).Repeat.Any();
				}
				if (RtaDataHandler == null)
				{
					RtaDataHandler = new fakeDataHandler();
				}


				return new TeleoptiRtaService(RtaDataHandler, Now, ConfigReader);
			}

		}
		#endregion

	}
}