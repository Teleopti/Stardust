using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.TeamSchedule.Core
{
	[TestFixture, TeamScheduleTest]
	public class TeamScheduleCommandHandlingProviderTest
	{
		public ITeamScheduleCommandHandlingProvider Target;
		public FakeActivityCommandHandler AddActivityCommandHandler;

		[Test]
		public void ShouldInvokeAddActivityCommandHandleWithCorrectCommandData()
		{
			var input = new AddActivityFormData
			{
				ActivityId = Guid.NewGuid(),
				BelongsToDate = DateOnly.Today,
				StartTime = new TimeSpan(8, 0, 0),
				EndTime = new TimeSpan(17, 0, 0),
				PersonIds = new [] { Guid.NewGuid(),Guid.NewGuid() },
				TrackedCommandInfo = new TrackedCommandInfo()
			};

			Target.AddActivity(input);

			AddActivityCommandHandler.CalledCount.Should().Be.EqualTo(2);
		}
	}

	public class FakeActivityCommandHandler : IHandleCommand<AddActivityCommand>
	{
		private int calledCount;
		public void Handle(AddActivityCommand command)
		{
			calledCount++;
		}

		public int CalledCount
		{
			get { return calledCount; }
		}

		public void ResetCalledCount()
		{
			calledCount = 0;
		}
	}
}
