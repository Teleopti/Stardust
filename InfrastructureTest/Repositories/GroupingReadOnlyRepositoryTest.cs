using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
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
		private IGroupingReadOnlyRepository _target;

		protected override void SetupForRepositoryTest()
		{
			_target = new GroupingReadOnlyRepository(UnitOfWorkFactory.CurrentUnitOfWork());
		}

		[Test]
		public void ShouldGroupPagesFromReadModel()
		{
			var items = _target.AvailableGroupPages();
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsWithPageIdFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage{PageId = new Guid("6CE00B41-0722-4B36-91DD-0A3B63C545CF"),PageName = "xxMain"},DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadAvailableGroupsFromReadModel()
		{
			var items = _target.AvailableGroups(new ReadOnlyGroupPage { PageName = "xxMain" }, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

		[Test]
		public void ShouldLoadDetailsForGroupFromReadModel()
		{
			var items = _target.DetailsForGroup(Guid.Empty, DateOnly.Today);
			items.Count().Should().Be.EqualTo(0);
		}

        [Test]
        public void ShouldCallUpdateReadModelWithoutCrash()
        {
            _target.UpdateGroupingReadModel(new Guid[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelGroupPageWithoutCrash()
        {
            _target.UpdateGroupingReadModelGroupPage(new Guid[] { Guid.NewGuid() });
        }

        [Test]
        public void ShouldCallUpdateGroupingReadModelDataWithoutCrash()
        {
            _target.UpdateGroupingReadModelData(new Guid[] { Guid.NewGuid() });
        }
	}
}
