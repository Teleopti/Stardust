using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class SaveToDenormalizationQueueTest
	{
		private ISaveToDenormalizationQueue target;
		private MockRepository mocks;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			target = new SaveToDenormalizationQueue();
		}

		[Test]
		public void ShouldSaveMessageToDenormalizationQueue()
		{
			var message = new DenormalizeScheduleProjection();
			var runSql = mocks.DynamicMock<IRunSql>();
			var sqlQuery = mocks.DynamicMock<ISqlQuery>();
			using (mocks.Record())
			{
				Expect.Call(runSql.Create(string.Empty)).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.SetDateTime(string.Empty, DateTime.Now)).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.SetGuid(string.Empty, Guid.Empty)).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.SetString(string.Empty, string.Empty)).IgnoreArguments().Return(sqlQuery);
				Expect.Call(sqlQuery.Execute);
			}
			using (mocks.Playback())
			{
				target.Execute(message, runSql);

				message.Timestamp.Should().Be.GreaterThan(DateTime.UtcNow.AddMinutes(-1));
				message.Datasource.Should().Be.EqualTo(UnitOfWorkFactory.Current.Name);
				message.BusinessUnitId.Should().Be.EqualTo(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault());
			}
		}
	}
}