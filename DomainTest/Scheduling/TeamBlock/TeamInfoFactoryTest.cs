using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock
{
	[TestFixture]
	public class TeamInfoFactoryTest
	{
		private MockRepository _mocks;
		private ITeamInfoFactory _target;
		private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
		private IGroupPersonBuilderWrapper _groupPersonBuilderWrapper;
		private BaseLineData _baseLineData;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
			_groupPersonBuilderWrapper = _mocks.StrictMock<IGroupPersonBuilderWrapper>();
			_target = new TeamInfoFactory(_groupPersonBuilderWrapper);
			_baseLineData = new BaseLineData();
		}

		[Test]
		public void ShouldReturnNewTeamInfoFromDate()
		{
			
			IScheduleMatrixPro matrixOnOtherPerson = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnOtherPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			IScheduleMatrixPro matrixOnPersonAndPeriod = _mocks.StrictMock<IScheduleMatrixPro>();
			Group group = new Group(new List<IPerson> { _baseLineData.Person1 }, "");

			var allMatrixesInScheduler = new List<IScheduleMatrixPro> { matrixOnOtherPerson, matrixOnOtherPeriod, matrixOnPersonAndPeriod };

			IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderWrapper.ForOptimization()).Return(_groupPersonBuilderForOptimization);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroup(null, _baseLineData.Person1, new DateOnly(2013, 2, 26))).Return(group);

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
				ITeamInfo result = _target.CreateTeamInfo(null, _baseLineData.Person1, new DateOnly(2013, 2, 26), allMatrixesInScheduler);
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
			Group group = new Group(new List<IPerson> { _baseLineData.Person1 }, "");

			var allMatrixesInScheduler = new List<IScheduleMatrixPro> { matrixOnOtherPerson, matrixOnOtherPeriod, matrixOnPersonAndPeriod };

			IVirtualSchedulePeriod schedulePeriod = _mocks.StrictMock<IVirtualSchedulePeriod>();

			using (_mocks.Record())
			{
				Expect.Call(_groupPersonBuilderWrapper.ForOptimization()).Return(_groupPersonBuilderForOptimization);
				Expect.Call(_groupPersonBuilderForOptimization.BuildGroup(null, _baseLineData.Person1, new DateOnly(2013, 2, 26))).Return(group);
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
				ITeamInfo result = _target.CreateTeamInfo(null, _baseLineData.Person1, new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 27)), allMatrixesInScheduler);
				IList<IScheduleMatrixPro> matrixesForGroupMember0 = result.MatrixesForGroupMember(0).ToList();
				Assert.AreSame(matrixOnOtherPeriod, matrixesForGroupMember0[0]);
				Assert.AreSame(matrixOnPersonAndPeriod, matrixesForGroupMember0[1]);
				Assert.AreEqual(2, result.MatrixesForGroup().Count());
			}
		}

		
        [Test]
        public void ShouldReturnIfTeamBlockInfoNull()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                _target.CreateTeamInfo(null, _baseLineData.Person1,
                                       new DateOnlyPeriod(new DateOnly(2013, 2, 26), new DateOnly(2013, 2, 27)), null));
        }

        [Test]
        public void ShouldReturnIfShiftProjectionCacheNull()
        {
            Assert.Throws<ArgumentNullException>(
                () =>
                _target.CreateTeamInfo(null, _baseLineData.Person1,new DateOnly(2013, 2, 26), null));
        }
	}
}