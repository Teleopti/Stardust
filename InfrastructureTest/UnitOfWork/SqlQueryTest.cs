using System;
using NHibernate;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.UnitOfWork
{
	[TestFixture]
	public class SqlQueryTest
	{
		private SqlQuery target;
		private MockRepository mocks;
		private IQuery query;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			query = mocks.DynamicMock<IQuery>();
			target = new SqlQuery(query);
		}

		[Test]
		public void ShouldSetString()
		{
			using (mocks.Record())
			{
				Expect.Call(query.SetString("name", "pelle")).Return(query);
			}
			using (mocks.Playback())
			{
				target.SetString("name", "pelle").Should().Be.EqualTo(target);
			}
		}

		[Test]
		public void ShouldSetDateTime()
		{
			var dateTime = DateTime.UtcNow;
			using (mocks.Record())
			{
				Expect.Call(query.SetDateTime("timestamp", dateTime)).Return(query);
			}
			using (mocks.Playback())
			{
				target.SetDateTime("timestamp", dateTime).Should().Be.EqualTo(target);
			}
		}

		[Test]
		public void ShouldSetGuid()
		{
			var id = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(query.SetGuid("id", id)).Return(query);
			}
			using (mocks.Playback())
			{
				target.SetGuid("id", id).Should().Be.EqualTo(target);
			}
		}

		[Test]
		public void ShouldExecute()
		{
			using (mocks.Record())
			{
				Expect.Call(query.ExecuteUpdate()).Return(1);
			}
			using (mocks.Playback())
			{
				target.Execute();
			}
		}
	}
}