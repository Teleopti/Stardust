using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamInfoFactoryTest
	{
		private MockRepository _mocks;
		private ITeamInfoFactory _target;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private BaseLineData _baseLineData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_target = new TeamInfoFactory(_groupPersonBuilderForOptimization);
			_baseLineData = new BaseLineData();
		}

		[Test]
		public void ShouldReturnNewTeamInfoFromDate()
		{
			
			IScheduleMatrixPro matrixOnOtherPerson = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnOtherPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnPersonAndPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

			var allMatrixesInScheduler = new List<IScheduleMatrixPro> { matrixOnOtherPerson, matrixOnOtherPeriod, matrixOnPersonAndPeriod };

			IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_baseLineData.Person1, new DateOnly(2013, 2, 26))).Return(groupPerson);
				Expect.Call(groupPerson.GroupMembers)
				      .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> {_baseLineData.Person1})).Repeat.Twice();

				Expect.Call(matrixOnOtherPerson.Person).Return(_baseLineData.Person2);

				Expect.Call(matrixOnOtherPeriod.Person).Return(_baseLineData.Person1);
				Expect.Call(matrixOnOtherPeriod.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod());

				Expect.Call(matrixOnPersonAndPeriod.Person).Return(_baseLineData.Person1);
				Expect.Call(matrixOnPersonAndPeriod.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 26)));
			}

			using (_mocks.Playback())
			{
				ITeamInfo result = _target.CreateTeamInfo(_baseLineData.Person1, new DateOnly(2013, 2, 26), allMatrixesInScheduler);
				Assert.AreSame(matrixOnPersonAndPeriod, result.MatrixesForGroup().FirstOrDefault());
				Assert.AreEqual(1, result.MatrixesForGroup().Count());
			}
		}

		[Test]
		public void ShouldReturnNewTeamInfoFromDateOnlyPeriod()
		{

			IScheduleMatrixPro matrixOnOtherPerson = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnOtherPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnPersonAndPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

			var allMatrixesInScheduler = new List<IScheduleMatrixPro> { matrixOnOtherPerson, matrixOnOtherPeriod, matrixOnPersonAndPeriod };

			IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_baseLineData.Person1, new DateOnly(2013, 2, 26))).Return(groupPerson);
				Expect.Call(groupPerson.GroupMembers)
					  .Return(new ReadOnlyCollection<IPerson>(new List<IPerson> { _baseLineData.Person1 })).Repeat.Twice();

				Expect.Call(matrixOnOtherPerson.Person).Return(_baseLineData.Person2);

				Expect.Call(matrixOnOtherPeriod.Person).Return(_baseLineData.Person1);
				Expect.Call(matrixOnOtherPeriod.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 26)));

				Expect.Call(matrixOnPersonAndPeriod.Person).Return(_baseLineData.Person1);
				Expect.Call(matrixOnPersonAndPeriod.SchedulePeriod).Return(schedulePeriod);
				Expect.Call(schedulePeriod.DateOnlyPeriod).Return(new DateOnlyPeriod(new DateOnly(2013, 2, 27), new DateOnly(2013, 2, 27)));
			}

			using (_mocks.Playback())
			{
				ITeamInfo result = _target.CreateTeamInfo(_baseLineData.Person1, new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 27)), allMatrixesInScheduler);
				IList<IScheduleMatrixPro> matrixesForGroupMember0 = result.MatrixesForGroupMember(0).ToList();
				Assert.AreSame(matrixOnOtherPeriod, matrixesForGroupMember0[0]);
				Assert.AreSame(matrixOnPersonAndPeriod, matrixesForGroupMember0[1]);
				Assert.AreEqual(2, result.MatrixesForGroup().Count());
			}
		}

		[Test]
		public void ShouldNotReturnTeamInfoWithoutAnyPerson()
		{
			IScheduleMatrixPro matrixOnOtherPerson = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnOtherPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnPersonAndPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IGroupPerson groupPerson = _mocks.StrictMock<IGroupPerson>();

			var allMatrixesInScheduler = new List<IScheduleMatrixPro> { matrixOnOtherPerson, matrixOnOtherPeriod, matrixOnPersonAndPeriod };
			
			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(_baseLineData.Person1, new DateOnly(2013, 2, 26))).Return(groupPerson);
				Expect.Call(groupPerson.GroupMembers).Return(new ReadOnlyCollection<IPerson>(new List<IPerson>()));
			}

			using (_mocks.Playback())
			{
				ITeamInfo result = _target.CreateTeamInfo(_baseLineData.Person1, new DateOnly(2013, 2, 26), allMatrixesInScheduler);
				Assert.IsNull(result);
			}
		}

        [Test]
        public void ShouldReturnIfTeamBlockInfoNull()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                _target.CreateTeamInfo(_baseLineData.Person1,
                                       new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 27)), null));
        }

        [Test]
        public void ShouldReturnIfShiftProjectionCacheNull()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                _target.CreateTeamInfo(_baseLineData.Person1,new DateOnly(2013, 2, 26), null));
        }
	}
}