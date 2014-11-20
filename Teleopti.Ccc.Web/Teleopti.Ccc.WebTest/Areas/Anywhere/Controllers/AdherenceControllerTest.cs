﻿using System;
using System.Collections.Generic;
using System.Linq;
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

			var target = new AdherenceController(calculateAdherence,null);

			var result = target.ForToday(personId);

			((AdherencePercentageModel) result.Data).AdherencePercent.Should().Be.EqualTo(70);
		}

		[Test]
		public void ForToday_WhenAdherenceInfoIsNotValid_ShouldHaveFalsyAdherencePercentage()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherence>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForToday(personId)).Return(null);

			var target = new AdherenceController(calculateAdherence,null);

			var result = target.ForToday(personId);

			Assert.That(isFalsy(result.Data));
		}

		[Test]
		public void ForDetails_WhenAdherenceInfoIsValid_ShouldGenerateResultBasedOnAdherenceInfo()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherenceDetails>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForDetails(personId)).Return(new[]
			{
				new AdherenceDetailsPercentageModel {AdherencePercent = 70}
			});

			var target = new AdherenceController(null, calculateAdherence);

			var result = target.ForDetails(personId);

			((IEnumerable<AdherenceDetailsPercentageModel>)result.Data).First().AdherencePercent.Should().Be.EqualTo(70);
		}

		[Test]
		public void ForDetails_WhenAdherenceInfoIsNotValid_ShouldHaveFalsyAdherencePercentage()
		{
			var calculateAdherence = MockRepository.GenerateStub<ICalculateAdherenceDetails>();
			var personId = Guid.NewGuid();
			calculateAdherence.Expect(h => h.ForDetails(personId)).Return(new List<AdherenceDetailsPercentageModel>());

			var target = new AdherenceController(null, calculateAdherence);

			var result = target.ForDetails(personId);

			((IEnumerable<AdherenceDetailsPercentageModel>) result.Data).Count().Should().Be(0);
		}

		private static bool isFalsy(dynamic resultFromController)
		{
			return resultFromController.GetType().GetProperty("AdherencePercent") == null;
		}

	}
}
