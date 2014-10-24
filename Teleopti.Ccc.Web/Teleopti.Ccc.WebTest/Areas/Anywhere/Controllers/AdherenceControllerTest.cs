using System;
using System.Web.Mvc;
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
	}
}
