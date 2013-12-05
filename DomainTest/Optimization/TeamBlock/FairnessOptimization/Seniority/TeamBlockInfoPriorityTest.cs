using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class TeamBlockInfoPriorityTest
	{
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo;
		private TeamBlockInfoPriority _target;
		private double _seniority;
		private int _shiftCategoryPoint;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
			_seniority = 3.14d;
			_shiftCategoryPoint = 7;
			_target = new TeamBlockInfoPriority(_teamBlockInfo, _seniority, _shiftCategoryPoint);
		}

		[Test]
		public void ShouldGetProperties()
		{
			Assert.AreEqual(_teamBlockInfo, _target.TeamBlockInfo);
			Assert.AreEqual(_seniority, _target.Seniority);
			Assert.AreEqual(_shiftCategoryPoint, _target.ShiftCategoryPriority);
		}
	}
}
