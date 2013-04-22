using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Messages;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class SaveToDenormalizationQueueTest
	{
		private ISaveToDenormalizationQueue target;
		private MockRepository mocks;
		private IRunSql runSql;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			runSql = mocks.DynamicMock<IRunSql>();
			target = new SaveToDenormalizationQueue(runSql);
		}

		[Test]
		public void ShouldSaveMessageToDenormalizationQueue()
		{
			var message = new TestMessage();
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
				target.Execute(message);

				message.Timestamp.Should().Be.GreaterThan(DateTime.UtcNow.AddMinutes(-1));
				message.Datasource.Should().Be.EqualTo(UnitOfWorkFactory.Current.Name);
				message.BusinessUnitId.Should().Be.EqualTo(((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit.Id.GetValueOrDefault());
			}
		}
	}

	public class TestMessage : RaptorDomainMessage
	{
		private readonly Guid _identity;

		public TestMessage()
		{
			_identity = Guid.NewGuid();
		}

		public override Guid Identity { get { return _identity; } }
	}
}