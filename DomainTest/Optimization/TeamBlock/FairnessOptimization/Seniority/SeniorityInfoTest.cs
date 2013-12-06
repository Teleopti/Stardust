using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class SeniorityInfoTest
	{
		private MockRepository _mock;
		private ITeamInfo _teamInfo;
		private double _seniority;
		private SeniorityInfo _target;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_seniority = 3;
			_target = new SeniorityInfo(_teamInfo, _seniority);
		}

		[Test]
		public void ShouldGetInfo()
		{
			Assert.AreEqual(_teamInfo, _target.TeamInfo);
			Assert.AreEqual(_seniority, _target.Seniority);
		}
	}
}
