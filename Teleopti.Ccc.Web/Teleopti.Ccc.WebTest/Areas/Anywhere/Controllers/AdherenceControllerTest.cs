using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[TestFixture]
	public class AdherenceControllerTest
	{
		[Test]
		public void ForToday_WhenAdherenceInfoIsValid_ShouldGenerateResultBasedOnAdherenceInfo()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherence>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForToday(personId)).Return(new AdherencePercentageModel()
			                                               {
															   AdherencePercent = 70
			                                               });

			var target = new AdherenceController(calculateAdherence);

			var result = target.ForToday(personId);

			((AdherencePercentageModel) result.Data).AdherencePercent.Should().Be.EqualTo(70);
		}

		[Test]
		public void ForToday_WhenAdherenceInfoIsNotValid_ShouldHaveFalsyAdherencePercentage()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherence>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForToday(personId)).Return(null);

			var target = new AdherenceController(calculateAdherence);

			var result = target.ForToday(personId);

			Assert.That(isFalsy(result.Data));
		}

		private static bool isFalsy(dynamic resultFromController)
		{
			return resultFromController.GetType().GetProperty("AdherencePercent") == null;
		}

	}
}
