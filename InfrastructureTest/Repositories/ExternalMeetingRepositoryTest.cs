using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
	public class ExternalMeetingRepositoryTest : RepositoryTest<ExternalMeeting>
	{
		private ExternalMeeting _externalMeeting;

		protected override ExternalMeeting CreateAggregateWithCorrectBusinessUnit()
		{
			_externalMeeting = new ExternalMeeting
			{
				Title = "title",
				Agenda = "agenda"
			}.WithId();
			return _externalMeeting;
		}

		protected override void VerifyAggregateGraphProperties(ExternalMeeting loadedAggregateFromDatabase)
		{
			Assert.AreEqual(_externalMeeting.Id, loadedAggregateFromDatabase.Id);
			Assert.AreEqual(_externalMeeting.Title, loadedAggregateFromDatabase.Title);
			Assert.AreEqual(_externalMeeting.Agenda, loadedAggregateFromDatabase.Agenda);
		}

		protected override Repository<ExternalMeeting> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new ExternalMeetingRepository(currentUnitOfWork);
		}
	}
}