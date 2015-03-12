using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Web.Areas.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Controllers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class StateControllerTest
	{
		[Test]
		public void ShouldHandleNullBatchId()
		{
			var target = new StateController(new FakeRta());

			Assert.DoesNotThrow(() =>
				target.Change(new ExternalUserStateWebModelForTest
				{
					UserCode = "usercode",
					StateCode = "statecode",
					BatchId = null
				}));
		}
	}

	public class FakeRta : IRta
	{
		public int SaveState(ExternalUserStateInputModel input)
		{
			return 1;
		}

		public int SaveStateBatch(IEnumerable<ExternalUserStateInputModel> states)
		{
			return 1;
		}

		public int SaveStateSnapshot(IEnumerable<ExternalUserStateInputModel> states)
		{
			return 1;
		}

		public void CheckForActivityChange(CheckForActivityChangeInputModel input)
		{
		}

		public void Initialize()
		{
		}
	}
}