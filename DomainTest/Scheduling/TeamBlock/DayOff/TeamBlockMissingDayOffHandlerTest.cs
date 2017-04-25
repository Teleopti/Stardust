using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.DayOff
{
    [TestFixture]
    public class TeamBlockMissingDayOffHandlerTest
    {
        private MockRepository _mock;
        private ITeamBlockMissingDayOffHandler _target;
		private IMissingDayOffBestSpotDecider _missingDayOffBestSpotDecider;
        private IMatrixDataListCreator _matrixDataListCreator;
        private IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _matrix1;
        private IScheduleMatrixPro _matrix2;
        private SchedulingOptions _schedulingOptions;
        private IList<IMatrixData> _matrixDataList;
        private IMatrixData _matrixData1;
        private IMatrixData _matrixData2;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IDayOffTemplate _dayOffTemplate;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay1;
        private ISplitSchedulePeriodToWeekPeriod _splitSchedulePeriodToWeekPeriod;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private DateOnlyPeriod _dateOnlyPeriod;
	    private IPerson _person;

	    [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();

            _dateOnlyPeriod = new DateOnlyPeriod(2014, 02, 27, 2014, 03, 03);
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
			_missingDayOffBestSpotDecider = _mock.StrictMock<IMissingDayOffBestSpotDecider>();
            _matrixDataListCreator = _mock.StrictMock<IMatrixDataListCreator>();
            _matrixDataWithToFewDaysOff = _mock.StrictMock<IMatrixDataWithToFewDaysOff>();
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = new SchedulingOptions();
            _matrixList = new List<IScheduleMatrixPro> {_matrix1, _matrix2};
			_splitSchedulePeriodToWeekPeriod = _mock.StrictMock<ISplitSchedulePeriodToWeekPeriod>();
            _target = new TeamBlockMissingDayOffHandler(_missingDayOffBestSpotDecider,_matrixDataListCreator,_matrixDataWithToFewDaysOff,_splitSchedulePeriodToWeekPeriod);
            _matrixData1 = _mock.StrictMock<IMatrixData>();
            _matrixData2 = _mock.StrictMock<IMatrixData>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _dayOffTemplate = _mock.StrictMock<IDayOffTemplate>();
            _matrixDataList = new List<IMatrixData> {_matrixData1, _matrixData2};
		    _person = PersonFactory.CreatePerson();
        }

        void _target_DayScheduled(object sender, SchedulingServiceBaseEventArgs e)
        {
            e.Cancel = true;
        }

        [Test]
        public void ShouldExecuteIfNoMatrixFound()
        {
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(new List<IMatrixData>());
            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }
            
        }

        [Test]
        public void ShouldNotExecuteIfResultingDateIsNull()
        {
            IList<IMatrixData> matrixData1List = new List<IMatrixData>() { _matrixData1 };
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);
                
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
                      .IgnoreArguments()
                      .Return(matrixData1List);
	            Expect.Call(_matrixData1.Matrix).Return(_matrix1);
	            Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod);			
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_matrix1.Person).Return(_person);

	            Expect.Call(_splitSchedulePeriodToWeekPeriod.Split(_dateOnlyPeriod, _person.FirstDayOfWeek))
		            .Return(new List<DateOnlyPeriod> {_dateOnlyPeriod});

	            Expect.Call(_missingDayOffBestSpotDecider.Find(matrixData1List[0],
		            new List<DateOnlyPeriod> {_dateOnlyPeriod}, new List<DateOnly>())).IgnoreArguments().Return(null);

            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }

        }

        [Test, Ignore("Talk to Robin")]
        public void ShouldNotExecuteIfCanceled()
        {
            IList<IMatrixData> matrixData1List = new List<IMatrixData>() { _matrixData1 };

            DateOnly? today = DateOnly.Today;
            _target.DayScheduled += _target_DayScheduled;
            using (_mock.Record())
            {
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);

				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
					  .IgnoreArguments()
					  .Return(matrixData1List);
				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_matrix1.Person).Return(_person);

				Expect.Call(_splitSchedulePeriodToWeekPeriod.Split(_dateOnlyPeriod, _person.FirstDayOfWeek))
					.Return(new List<DateOnlyPeriod> { _dateOnlyPeriod });

				Expect.Call(_missingDayOffBestSpotDecider.Find(matrixData1List[0],
					new List<DateOnlyPeriod> { _dateOnlyPeriod }, new List<DateOnly>())).IgnoreArguments().Return(today);
 
                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Twice();
                Expect.Call(_matrix1.GetScheduleDayByKey(today.Value)).Return(_scheduleDayPro1);
                Expect.Call(_matrix1.UnlockedDays).Return(new [] { _scheduleDayPro1 });
				Expect.Call(_schedulingOptions.DayOffTemplate).Return(_dayOffTemplate);
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(() => _scheduleDay1.CreateAndAddDayOff(_dayOffTemplate)).IgnoreArguments();
                Expect.Call(() => _rollbackService.Modify(_scheduleDay1)).IgnoreArguments();
            }

            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }
            _target.DayScheduled -= _target_DayScheduled;
        }

        [Test]
        public void ShouldExecuteSuccessfully()
        {
            IList<IMatrixData> matrixData1List = new List<IMatrixData> { _matrixData1 };
            DateOnly? today = DateOnly.Today;
            using (_mock.Record())
            {
				Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);

				Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
					  .IgnoreArguments()
					  .Return(matrixData1List);
				Expect.Call(_matrixData1.Matrix).Return(_matrix1);
				Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod);
				Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);
				Expect.Call(_matrix1.Person).Return(_person);

				Expect.Call(_splitSchedulePeriodToWeekPeriod.Split(_dateOnlyPeriod, _person.FirstDayOfWeek))
					.Return(new List<DateOnlyPeriod> { _dateOnlyPeriod });

				Expect.Call(_missingDayOffBestSpotDecider.Find(matrixData1List[0],
					new List<DateOnlyPeriod> { _dateOnlyPeriod }, new List<DateOnly>())).IgnoreArguments().Return(today);

				Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Twice();
				Expect.Call(_matrix1.GetScheduleDayByKey(today.Value)).Return(_scheduleDayPro1);
				Expect.Call(_matrix1.UnlockedDays).Return(new [] { _scheduleDayPro1 });
				Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
				Expect.Call(() => _scheduleDay1.CreateAndAddDayOff(_dayOffTemplate)).IgnoreArguments();
				Expect.Call(() => _rollbackService.Modify(_scheduleDay1)).IgnoreArguments();
	            IScheduleDayData scheduleDayData;
	            Expect.Call(_matrixData1.TryGetValue(today.Value, out scheduleDayData)).Return(true).OutRef(new ScheduleDayData(today.Value));
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List)).IgnoreArguments().Return(new List<IMatrixData>());
                
            }
            using (_mock.Playback())
            {
	            _schedulingOptions.DayOffTemplate = _dayOffTemplate;

	            _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }
        }    
    }
}
