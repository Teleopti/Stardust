using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
		private ITeamInfo _teamInfo;
		private SeniorityExtractor _target;
		private IPerson _person1;
		private IPerson _person2;
		private IList<IPerson> _groupMembers;
		private IList<ITeamInfo> _teamInfos;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfo = _mock.StrictMock<ITeamInfo>();
			_person1 = PersonFactory.CreatePerson("A", "A");
			_person2 = PersonFactory.CreatePerson("B", "B");
			_groupMembers = new List<IPerson> { _person1, _person2 };
			_teamInfos = new List<ITeamInfo>{_teamInfo};
			_target = new SeniorityExtractor();
		}

		
	}
}
