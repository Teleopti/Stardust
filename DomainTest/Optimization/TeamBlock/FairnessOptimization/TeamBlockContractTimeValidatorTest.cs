using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization
{
	[TestFixture]
	public class TeamBlockContractTimeValidatorTest
	{
		private MockRepository _mock;
		private ITeamBlockInfo _teamBlockInfo1;
		private ITeamBlockInfo _teamBlockInfo2;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleMatrixPro _scheduleMatrixPro2;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IProjectionService _projectionService1;
		private IProjectionService _projectionService2;
		private IVisualLayerCollection _visualLayerCollection1;
		private IVisualLayerCollection _visualLayerCollection2;
		private TeamBlockContractTimeValidator _target;
		private IBlockInfo _blockInfo1;
		private IBlockInfo _blockInfo2;
		private ITeamInfo _teamInfo1;
		private ITeamInfo _teamInfo2;
		private DateOnlyPeriod _dateOnlyPeriod;
		
		[SetUp]
		public void Setup()
		{
			_mock = new MockRepository();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_teamBlockInfo2 = _mock.StrictMock<ITeamBlockInfo>();
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleMatrixPro2 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_projectionService1 = _mock.StrictMock<IProjectionService>();
			_projectionService2 = _mock.StrictMock<IProjectionService>();
			_visualLayerCollection1 = _mock.StrictMock<IVisualLayerCollection>();
			_visualLayerCollection2 = _mock.StrictMock<IVisualLayerCollection>();
			_blockInfo1 = _mock.StrictMock<IBlockInfo>();
			_blockInfo2 = _mock.StrictMock<IBlockInfo>();
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 1);
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamInfo2 = _mock.StrictMock<ITeamInfo>();
			_target = new TeamBlockContractTimeValidator();
		}

		[Test]
		public void ShouldReturnTrueWhenSameContractTime()
		{
			IList<IScheduleMatrixPro> matrixPros1 = new List<IScheduleMatrixPro>{_scheduleMatrixPro1};
			IList<IScheduleMatrixPro> matrixPros2 = new List<IScheduleMatrixPro>{_scheduleMatrixPro2};
			var contractTime = TimeSpan.FromHours(1);
			
			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(matrixPros1);
				Expect.Call(_teamInfo2.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(matrixPros2);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnlyPeriod.StartDate)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnlyPeriod.StartDate)).Return(_scheduleDayPro2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.HasProjection()).Return(true);
				Expect.Call(_scheduleDay2.HasProjection()).Return(true);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection1.ContractTime()).Return(contractTime);
				Expect.Call(_visualLayerCollection2.ContractTime()).Return(contractTime);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWhenDifferentContractTime()
		{
			IList<IScheduleMatrixPro> matrixPros1 = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			IList<IScheduleMatrixPro> matrixPros2 = new List<IScheduleMatrixPro> { _scheduleMatrixPro2 };
			var contractTime = TimeSpan.FromHours(1);
			var contractTimeDifferent = contractTime.Add(TimeSpan.FromHours(1));

			using (_mock.Record())
			{
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_teamBlockInfo2.BlockInfo).Return(_blockInfo2);
				Expect.Call(_blockInfo1.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_blockInfo2.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo2.TeamInfo).Return(_teamInfo2);
				Expect.Call(_teamInfo1.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(matrixPros1);
				Expect.Call(_teamInfo2.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(matrixPros2);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnlyPeriod.StartDate)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro2.GetScheduleDayByKey(_dateOnlyPeriod.StartDate)).Return(_scheduleDayPro2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.HasProjection()).Return(true);
				Expect.Call(_scheduleDay2.HasProjection()).Return(true);
				Expect.Call(_scheduleDay1.ProjectionService()).Return(_projectionService1);
				Expect.Call(_scheduleDay2.ProjectionService()).Return(_projectionService2);
				Expect.Call(_projectionService1.CreateProjection()).Return(_visualLayerCollection1);
				Expect.Call(_projectionService2.CreateProjection()).Return(_visualLayerCollection2);
				Expect.Call(_visualLayerCollection1.ContractTime()).Return(contractTime);
				Expect.Call(_visualLayerCollection2.ContractTime()).Return(contractTimeDifferent);
			}

			using (_mock.Playback())
			{
				var result = _target.ValidateContractTime(_teamBlockInfo1, _teamBlockInfo2);
				Assert.IsFalse(result);
			}	
		}
	}
}
