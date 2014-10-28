using System;
using System.Web.Mvc;
using Microsoft.IdentityModel.SecurityTokenService;
using Newtonsoft.Json;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class AdherenceControllerTest
	{

		[Test]
		public void ShouldGetAdherenceForAgentToday()
		{
			var thisIsNow = new DateTime(2014, 12, 24, 15, 0, 0);
			const int expectedMinutesInAdherence = 12;
			const int expectedminutesOutOfAdherence = 36;
			var expectedLastTimeStamp = thisIsNow.AddMinutes(-5);
			var personId = Guid.NewGuid();
			var now = new ThisIsNow(thisIsNow);
			var readModelRepo = MockRepository.GenerateStub<IAdherencePercentageReadModelPersister>();
			
			readModelRepo.Stub(r => r.Get(new DateOnly(now.UtcDateTime()), personId)).Return(new AdherencePercentageReadModel()
																	 {
																		 MinutesInAdherence = expectedMinutesInAdherence,
																		 MinutesOutOfAdherence = expectedminutesOutOfAdherence,
																		 LastTimestamp = expectedLastTimeStamp
																	 });

			var target = new AdherenceController(readModelRepo,now);
			dynamic result = target.ForToday(personId).Data;

			((object) result.MinutesInAdherence).Should().Be.EqualTo(expectedMinutesInAdherence);
			((object) result.MinutesOutOfAdherence).Should().Be.EqualTo(expectedminutesOutOfAdherence);
			((object) result.LastTimestamp).Should().Be.EqualTo(expectedLastTimeStamp);
		}


		//note: more rules will be added for calculating historical adherence
		[Test]
		public void HistoricalAdherence_ShouldBeFiftyPercent_WhenInAdherenceAndOutOfAdherenceAreSame()
		{
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 74,
				MinutesOutOfAdherence = 74
			};
			
			var repo = new FakePersisterForTest(data);

			var target = new AdherenceController(repo, new Now());
			dynamic result = target.ForToday(Guid.Empty).Data;
			((object)result.AdherencePercent).Should().Be.EqualTo(50d);
		}

		[Test]
		public void HistoricalAdherence_ShouldBeHundredPercent_WhenOnlyInAdherence()
		{
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 12,
				MinutesOutOfAdherence = 0
			};

			var repo = new FakePersisterForTest(data);

			var target = new AdherenceController(repo, new Now());
			dynamic result = target.ForToday(Guid.Empty).Data;
			((object)result.AdherencePercent).Should().Be.EqualTo(100d);
		}

		[Test]
		public void HistoricalAdherence_ShouldBeZeroPercent_WhenOnlyOutOfAdherence()
		{
			var data = new AdherencePercentageReadModel()
			{
				MinutesInAdherence = 0,
				MinutesOutOfAdherence = 54
			};

			var repo = new FakePersisterForTest(data);

			var target = new AdherenceController(repo, new Now());
			dynamic result = target.ForToday(Guid.Empty).Data;
			((object)result.AdherencePercent).Should().Be.EqualTo(0d);
		}

		[Test]
		public void HistoricalAdherence_ShouldNotThrowNullException_WhenThereIsNoReadmodel()
		{
			var repo = new FakePersisterForTest(null);
			var target = new AdherenceController(repo, new Now());
			
			Assert.DoesNotThrow(() => target.ForToday(Guid.Empty));
		}

		[Test]
		public void HistoricalAdherence_ShouldNotReturnNull_WhenThereIsNoReadmodel_BecauseItCausesAjaxErrorsInJavascript()
		{
			var repo = new FakePersisterForTest(null);
			var target = new AdherenceController(repo, new Now());

			var result = target.ForToday(Guid.Empty);

			result.Should().Not.Be.Null();
		}


		public class FakePersisterForTest : IAdherencePercentageReadModelPersister
		{
			private readonly AdherencePercentageReadModel _model;

			public FakePersisterForTest(AdherencePercentageReadModel model)
			{
				_model = model;
			}

			public void Persist(AdherencePercentageReadModel model)
			{
				//throw new NotImplementedException();
			}

			public AdherencePercentageReadModel Get(DateOnly date, Guid personId)
			{
				return _model;
			}
		}


	}
}
