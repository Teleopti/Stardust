//using NUnit.Framework;
//using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
//using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
//using Rhino.Mocks;

//namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
//{
//    [TestFixture]
//    public class ShiftCategoryPointInfoTest
//    {
//        private MockRepository _mock;
//        private ShiftCategoryPointInfo _target;
//        private ITeamBlockInfo _teamBlockInfo;
//        private int _point;

//        [SetUp]
//        public void SetUp()
//        {
//            _mock = new MockRepository();
//            _teamBlockInfo = _mock.StrictMock<ITeamBlockInfo>();
//            _point = 42;
//            _target = new ShiftCategoryPointInfo(_teamBlockInfo, _point);
//        }

//        [Test]
//        public void ShouldGetProperties()
//        {
//            Assert.AreEqual(_teamBlockInfo, _target.TeamBlockInfo);
//            Assert.AreEqual(_point, _target.Point);
//        }
//    }
//}
