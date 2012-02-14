using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	public class GroupingReadOnlyRepositoryTest : DatabaseTest
	{
		private IGroupingReadOnlyRepository target;

		protected override void SetupForRepositoryTest()
		{
			target = new GroupingReadOnlyRepository(UnitOfWorkFactory.Current);
		}

		[Test]
		public void ShouldGroupPagesFromReadModel()
		{
			var items = target.AvailableGroupPages();
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsFromReadModel()
		{
			var items = target.AvailableGroups(new ReadOnlyGroupPage{PageId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF"),PageName = "xxMain"},DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModel()
		{
			var items = target.DetailsForGroup(Guid.Empty, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}
	}
}
