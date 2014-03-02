using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class SeniorityExtractorTest
	{
		private MockRepository _mock;
		private SeniorityExtractor _target;
		private IPerson _person1;
		private IPerson _person2;
	    private IRankedPersonBasedOnStartDate _rankedPersonBasedOnStartDate;
	    private ITeamBlockInfo _teamBlockInfo1;
	    private ITeamBlockInfo _teamBlockInfo2;
	    private ITeamInfo _teamInfo1;
	    private ITeamInfo _teamInfo2;
	    private IPerson _person3;
	    private IPerson _person4;
		private Dictionary<IPerson, int> _seniorityDic;

	    [SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_person1 = PersonFactory.CreatePerson("A", "A");
			_person2 = PersonFactory.CreatePerson("B", "B");
			_person3 = PersonFactory.CreatePerson("C", "C");
			_person4 = PersonFactory.CreatePerson("D", "D");
            _rankedPersonBasedOnStartDate = _mock.StrictMock<IRankedPersonBasedOnStartDate>();
			
	        _teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
	        _teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
	        _teamInfo1 = _mock.StrictMock<ITeamInfo>();
	        _teamInfo2 = _mock.StrictMock<ITeamInfo>();
            _target = new SeniorityExtractor(_rankedPersonBasedOnStartDate);
		    _seniorityDic = new Dictionary<IPerson, int>();
			_seniorityDic.Add(_person1, 1);
			_seniorityDic.Add(_person2, 2);
			_seniorityDic.Add(_person3, 3);
			_seniorityDic.Add(_person4, 4);
		}
        
        [Test]
        public void ShouldRunWithSinglePersonInList()
        {
            IList<ITeamBlockInfo> teamBlockInfos = new List<ITeamBlockInfo>();
            teamBlockInfos.Add(_teamBlockInfo1);
            teamBlockInfos.Add(_teamBlockInfo2);
            IEnumerable<IPerson> person1List = new List<IPerson>(){_person1 };
            IEnumerable<IPerson> person2List = new List<IPerson>(){_person2 };
            IEnumerable<IPerson> allPersonList = new List<IPerson>() { _person1, _person2 };
            var teamBlockPoint1 =  new TeamBlockPoints(_teamBlockInfo1 ,1);
            var teamBlockPoint2 = new TeamBlockPoints(_teamBlockInfo2, 2);
            using (_mock.Record())
            {
                commonMocks(person1List, person2List,allPersonList);
            }
            using (_mock.Playback())
            {
                var result = _target.ExtractSeniority(teamBlockInfos);
                Assert.AreEqual(teamBlockPoint1.Points, result[0].Points);
                Assert.AreEqual(teamBlockPoint2.Points, result[1].Points);
            }
        }

	    private void commonMocks(IEnumerable<IPerson> person1List, IEnumerable<IPerson> person2List, IEnumerable<IPerson> allPersonList)
	    {
	        Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1).Repeat.AtLeastOnce();
	        Expect.Call(_teamInfo1.GroupMembers).Return(person1List).Repeat.AtLeastOnce();
	        Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2).Repeat.AtLeastOnce();
	        Expect.Call(_teamInfo2.GroupMembers).Return(person2List).Repeat.AtLeastOnce();
			Expect.Call(_rankedPersonBasedOnStartDate.GetRankedPersonDictionary(allPersonList)).IgnoreArguments().Return(_seniorityDic);
	    }

	    [Test]
        public void ShouldRunWithMultiplePersonInList()
        {
            IList<ITeamBlockInfo> teamBlockInfos = new List<ITeamBlockInfo>();
            teamBlockInfos.Add(_teamBlockInfo1);
            teamBlockInfos.Add(_teamBlockInfo2);
            IEnumerable<IPerson> person1List = new List<IPerson>() { _person1,_person3 };
            IEnumerable<IPerson> person2List = new List<IPerson>() { _person2,_person4 };
            IEnumerable<IPerson> allPersonList = new List<IPerson>() { _person1, _person2,_person3,_person4  };
            var teamBlockPoint1 = new TeamBlockPoints(_teamBlockInfo1, 2);
            var teamBlockPoint2 = new TeamBlockPoints(_teamBlockInfo2, 3);
            using (_mock.Record())
            {
                commonMocks(person1List, person2List, allPersonList);
            }
            using (_mock.Playback())
            {
                var result = _target.ExtractSeniority(teamBlockInfos);
                Assert.AreEqual(teamBlockPoint1.Points, result[0].Points);
                Assert.AreEqual(teamBlockPoint2.Points, result[1].Points);
            }
        }

        
		
	}
}
