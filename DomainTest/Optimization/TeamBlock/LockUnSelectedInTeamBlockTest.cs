using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class LockUnSelectedInTeamBlockTest
	{
		private ILockUnSelectedInTeamBlock _target;
		private ITeamBlockInfo _teamBlockInfo;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPerson _person4;

		[SetUp]
		public void Setup()
		{
			_person1 = PersonFactory.CreatePerson("1");
			_person2 = PersonFactory.CreatePerson("2");
			_person3 = PersonFactory.CreatePerson("3");
			_person4 = PersonFactory.CreatePerson("4");
			IList<IPerson> personList = new List<IPerson>{_person1,_person2,_person3,_person4 };
			ITeamInfo teamInfo = new TeamInfo(new Group(personList,"test"),new List<IList<IScheduleMatrixPro>>() );
			IBlockInfo blockInfo = new BlockInfo(new DateOnlyPeriod(new DateOnly(2014,05,1),new DateOnly(2014,05,15) ));
			_teamBlockInfo = new TeamBlockInfo(teamInfo ,blockInfo );
			_target = new LockUnSelectedInTeamBlock();
		}

		[Test]
		public void TestNothingIsLocked()
		{
			_target.Lock(_teamBlockInfo,new List<IPerson>(),new DateOnlyPeriod() );
			Assert.AreEqual(0,_teamBlockInfo.TeamInfo.UnLockedMembers().Count( ) );
			Assert.AreEqual(0,_teamBlockInfo.BlockInfo.UnLockedDates().Count( ) );
		}

		[Test]
		public void TestOnlyPersonAreLocked()
		{
			_target.Lock(_teamBlockInfo, new List<IPerson>{_person2,_person3 }, new DateOnlyPeriod());
			Assert.AreEqual(2, _teamBlockInfo.TeamInfo.UnLockedMembers().Count());
			Assert.IsTrue(_teamBlockInfo.TeamInfo.UnLockedMembers().Contains(_person2 ));
			Assert.IsTrue(_teamBlockInfo.TeamInfo.UnLockedMembers().Contains(_person3));
			Assert.AreEqual(0, _teamBlockInfo.BlockInfo.UnLockedDates().Count());
		}

		[Test]
		public void TestOnlyDatesAreLocked()
		{
			_target.Lock(_teamBlockInfo, new List<IPerson>(), new DateOnlyPeriod(new DateOnly(2014, 05, 8), new DateOnly(2014, 05, 10)));
			Assert.AreEqual(0, _teamBlockInfo.TeamInfo.UnLockedMembers().Count());
			Assert.AreEqual(3, _teamBlockInfo.BlockInfo.UnLockedDates().Count());
			Assert.IsTrue(_teamBlockInfo.BlockInfo.UnLockedDates().Contains(new DateOnly(2014, 05, 8)));
			Assert.IsTrue(_teamBlockInfo.BlockInfo.UnLockedDates().Contains(new DateOnly(2014, 05, 9)));
			Assert.IsTrue(_teamBlockInfo.BlockInfo.UnLockedDates().Contains(new DateOnly(2014, 05, 10)));
		}
	}

	
}
