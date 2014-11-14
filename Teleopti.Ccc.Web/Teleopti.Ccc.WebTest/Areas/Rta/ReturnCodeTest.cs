using System;
using System.Collections.ObjectModel;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class ReturnCodeTest
	{
		[Test]
		public void ShouldProcessValidState()
		{
			var state = new ExternalUserStateForTest();
			var target = RtaForTest.MakeBasedOnState(state);

			var result = target.SaveState(state);

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessStatesThatsToOld()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var state = new ExternalUserStateForTest() { Timestamp = christmas.AddHours(1) };
			var target = RtaForTest.MakeBasedOnState(state, new ThisIsNow(christmas));

			var result = target.SaveState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessStatesWithTimeStampNewerThanNow()
		{
			var christmas = new DateTime(2001, 12, 24, 15, 0, 0);
			var now = MockRepository.GenerateStub<INow>();
			now.Expect(n => n.UtcDateTime()).Return(christmas);
			var state = new ExternalUserStateForTest { Timestamp = christmas.Subtract(TimeSpan.FromHours(1)) };
			var target = RtaForTest.MakeBasedOnState(state, now);

			var result = target.SaveState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessIfNoSourceId()
		{
			var state = new ExternalUserStateForTest();
			var target = RtaForTest.MakeBasedOnState(state);
			state.SourceId = string.Empty;

			var result = target.SaveState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldNotProcessStateIfNoPlatformId()
		{
			var state = new ExternalUserStateForTest();
			var target = RtaForTest.MakeBasedOnState(state);
			state.PlatformTypeId = string.Empty;

			var result = target.SaveState(state);

			result.Should().Not.Be.EqualTo(1);
		}

		[Test]
		public void ShouldProcessExternalUserStatesInBatch()
		{
			var state1 = new ExternalUserStateForTest();
			var state2 = new ExternalUserStateForTest();
			var target = RtaForTest.MakeBasedOnState(state1);

			var result = target.SaveStateBatch(new[] { state1, state2 });

			result.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldThrowIfTooManyExternalUserStatesInBatch()
		{
			const int tooManyStates = 200;
			var externalStates = new Collection<ExternalUserStateForTest>();
			for (var i = 0; i < tooManyStates; i++)
				externalStates.Add(new ExternalUserStateForTest());
			var state = new ExternalUserStateForTest();
			var target = RtaForTest.MakeBasedOnState(state);

			Assert.Throws(typeof(FaultException), () => target.SaveStateBatch(externalStates));
		}

	}
}