using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock.FairnessOptimization.Seniority
{
	[TestFixture]
	public class ShiftCategoryPointInfoExtractorTest
	{
		private MockRepository _mock;
		private ShiftCategoryPointExtractor _target;
		private IShiftCategory _shiftCategory1;
		private IShiftCategory _shiftCategory2;
		private IDictionary<IShiftCategory, int> _shiftCategoryPointsDic;
		private ITeamBlockInfo _teamBlockInfo1;
		private IList<ITeamBlockInfo> _teamBlockInfos;
		private ITeamInfo _teamInfo1;
		private IBlockInfo _blockInfo1;
		private DateOnlyPeriod _dateOnlyPeriod;
		private IScheduleMatrixPro _scheduleMatrixPro1;
		private IScheduleDayPro _scheduleDayPro1;
		private IScheduleDayPro _scheduleDayPro2;
		private IList<IScheduleMatrixPro> _scheduleMatrixPros1;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IPersonAssignment _personAssignment1;
		private IPersonAssignment _personAssignment2;
		private IShiftCategoryPoints _shiftCategoryPoints;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_teamInfo1 = _mock.StrictMock<ITeamInfo>();
			_teamBlockInfo1 = _mock.StrictMock<ITeamBlockInfo>();
			_blockInfo1 = _mock.StrictMock<IBlockInfo>();
			_teamBlockInfos = new List<ITeamBlockInfo>{_teamBlockInfo1};
			_shiftCategory1 = ShiftCategoryFactory.CreateShiftCategory("AA");
			_shiftCategory2 = ShiftCategoryFactory.CreateShiftCategory("BB");
            _shiftCategoryPointsDic = new Dictionary<IShiftCategory, int>();
            _shiftCategoryPointsDic.Add(_shiftCategory2, 1);
            _shiftCategoryPointsDic.Add(_shiftCategory1, 2);
			_dateOnlyPeriod = new DateOnlyPeriod(2013, 1, 1, 2013, 1, 2);
			_scheduleMatrixPro1 = _mock.StrictMock<IScheduleMatrixPro>();
			_scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleDayPro2 = _mock.StrictMock<IScheduleDayPro>();
			_scheduleMatrixPros1 = new List<IScheduleMatrixPro> { _scheduleMatrixPro1 };
			_scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_scheduleDay2 = _mock.StrictMock<IScheduleDay>();
			_personAssignment1 = _mock.StrictMock<IPersonAssignment>();
			_personAssignment2 = _mock.StrictMock<IPersonAssignment>();
			_shiftCategoryPoints = _mock.StrictMock<IShiftCategoryPoints>();
			_target = new ShiftCategoryPointExtractor(_shiftCategoryPoints);
		}

		[Test]
		public void ShouldExtractShiftCategoryInfos()
		{
			using (_mock.Record())
			{
				Expect.Call(_shiftCategoryPoints.ExtractShiftCategoryPoints(new List<IShiftCategory>())).Return(_shiftCategoryPointsDic);
				Expect.Call(_teamBlockInfo1.TeamInfo).Return(_teamInfo1);
				Expect.Call(_teamBlockInfo1.BlockInfo).Return(_blockInfo1);
				Expect.Call(_blockInfo1.BlockPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_teamInfo1.MatrixesForGroupAndPeriod(_dateOnlyPeriod)).Return(_scheduleMatrixPros1);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnlyPeriod.StartDate)).Return(_scheduleDayPro1);
				Expect.Call(_scheduleMatrixPro1.GetScheduleDayByKey(_dateOnlyPeriod.EndDate)).Return(_scheduleDayPro2);
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(_scheduleDayPro2.DaySchedulePart()).Return(_scheduleDay2);
				Expect.Call(_scheduleDay1.PersonAssignment()).Return(_personAssignment1);
				Expect.Call(_scheduleDay2.PersonAssignment()).Return(_personAssignment2);
				Expect.Call(_personAssignment1.ShiftCategory).Return(_shiftCategory1);
				Expect.Call(_personAssignment2.ShiftCategory).Return(_shiftCategory2);
			}

			using (_mock.Playback())
			{
				var result = _target.ExtractShiftCategoryInfos(_teamBlockInfos, new List<IShiftCategory>());
				//Assert.AreEqual(3, result[_teamBlockInfo1].Points);
			}
		}
	}
}
