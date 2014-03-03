using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.TeamBlock.DayOff
{
    [TestFixture]
    public class TeamBlockMissingDayOffHandlerTest
    {
        private MockRepository _mock;
        private ITeamBlockMissingDayOffHandler _target;
        private IBestSpotForAddingDayOffFinder _bestSpotForAddingDayOffFinder;
        private IMatrixDataListCreator _matrixDataListCreator;
        private IMatrixDataWithToFewDaysOff _matrixDataWithToFewDaysOff;
        private IList<IScheduleMatrixPro> _matrixList;
        private IScheduleMatrixPro _matrix1;
        private IScheduleMatrixPro _matrix2;
        private ISchedulingOptions _schedulingOptions;
        private IList<IMatrixData> _matrixDataList;
        private IMatrixData _matrixData1;
        private IMatrixData _matrixData2;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IList<IScheduleDayData> _scheduleDayDataList;
        private IScheduleDayData _scheduleDayData1;
        private IScheduleDayData _scheduleDayData2;
        private IDayOffTemplate _dayOffTemplate;
        private IScheduleDayPro _scheduleDayPro1;
        private IScheduleDay _scheduleDay1;
        private SplitSchedulePeriodToWeekPeriod _splitSchedulePeriodToWeekPeriod;
        private IValidNumberOfDayOffInAWeekSpecification _validNumberOfDayOffInAWeekSpecification;
        private IVirtualSchedulePeriod _virtualSchedulePeriod;
        private DateOnlyPeriod _dateOnlyPeriod;

        [SetUp]
        public void Setup()
        {
            _mock = new MockRepository();

            _dateOnlyPeriod = new DateOnlyPeriod(2014, 02, 27, 2014, 03, 03);
            _virtualSchedulePeriod = _mock.StrictMock<IVirtualSchedulePeriod>();
            _scheduleDayPro1 = _mock.StrictMock<IScheduleDayPro>();
            _scheduleDay1 = _mock.StrictMock<IScheduleDay>();
            _bestSpotForAddingDayOffFinder = _mock.StrictMock<IBestSpotForAddingDayOffFinder>();
            _matrixDataListCreator = _mock.StrictMock<IMatrixDataListCreator>();
            _matrixDataWithToFewDaysOff = _mock.StrictMock<IMatrixDataWithToFewDaysOff>();
            _matrix1 = _mock.StrictMock<IScheduleMatrixPro>();
            _matrix2 = _mock.StrictMock<IScheduleMatrixPro>();
            _schedulingOptions = _mock.StrictMock<ISchedulingOptions>();
            _matrixList = new List<IScheduleMatrixPro>() {_matrix1, _matrix2};
            _splitSchedulePeriodToWeekPeriod = new SplitSchedulePeriodToWeekPeriod();
            _validNumberOfDayOffInAWeekSpecification = _mock.StrictMock<IValidNumberOfDayOffInAWeekSpecification>();
            _target = new TeamBlockMissingDayOffHandler(_bestSpotForAddingDayOffFinder,_matrixDataListCreator,_matrixDataWithToFewDaysOff,_splitSchedulePeriodToWeekPeriod ,_validNumberOfDayOffInAWeekSpecification   );
            _matrixData1 = _mock.StrictMock<IMatrixData>();
            _matrixData2 = _mock.StrictMock<IMatrixData>();
            _rollbackService = _mock.StrictMock<ISchedulePartModifyAndRollbackService>();
            _scheduleDayData1 = _mock.StrictMock<IScheduleDayData>();
            _scheduleDayData2 = _mock.StrictMock<IScheduleDayData>();
            _scheduleDayDataList = new List<IScheduleDayData>() {_scheduleDayData1, _scheduleDayData2};
            _dayOffTemplate = _mock.StrictMock<IDayOffTemplate>();
            _matrixDataList = new List<IMatrixData>() {_matrixData1, _matrixData2};
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
            var scheduleDayCollection =
                new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList);
            DateOnly? today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
                      .IgnoreArguments()
                      .Return(matrixData1List);
                //spliting the period into weeks
                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Times(3);
                Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.Twice();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
                Expect.Call(_validNumberOfDayOffInAWeekSpecification.IsSatisfied(_matrix1, _dateOnlyPeriod, 2))
                      .IgnoreArguments()
                      .Return(false);

                //Filtering schedule day collection
                Expect.Call(_matrixData1.ScheduleDayDataCollection).Return(new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList));
                Expect.Call(_scheduleDayData1.DateOnly).Return(today.Value);
                Expect.Call(_scheduleDayData2.DateOnly).Return(today.Value);

                Expect.Call(_bestSpotForAddingDayOffFinder.Find(scheduleDayCollection)).IgnoreArguments().Return(null);

            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }

        }

        [Test]
        public void ShouldNotExecuteIfInLockedDays()
        {
            IList<IMatrixData> matrixData1List = new List<IMatrixData>() { _matrixData1 };
            var scheduleDayCollection =
                new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList);
            DateOnly? today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
                      .IgnoreArguments()
                      .Return(matrixData1List);
                //spliting the period into weeks
                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Times(3);
                Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.Twice();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
                Expect.Call(_validNumberOfDayOffInAWeekSpecification.IsSatisfied(_matrix1, _dateOnlyPeriod, 2))
                      .IgnoreArguments()
                      .Return(false);

                //Filtering schedule day collection
                Expect.Call(_matrixData1.ScheduleDayDataCollection).Return(new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList));
                Expect.Call(_scheduleDayData1.DateOnly).Return(today.Value);
                Expect.Call(_scheduleDayData2.DateOnly).Return(today.Value);

                //Shceudling day off
                Expect.Call(_bestSpotForAddingDayOffFinder.Find(scheduleDayCollection)).IgnoreArguments().Return(today);
                Expect.Call(_schedulingOptions.DayOffTemplate).Return(_dayOffTemplate);

                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Twice();
                Expect.Call(_matrix1.GetScheduleDayByKey(today.Value)).Return(_scheduleDayPro1);
                Expect.Call(_matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> ()));

                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List)).IgnoreArguments().Return(new List<IMatrixData>());

            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }
        }


        [Test]
        public void ShouldNotExecuteIfCanceled()
        {
            IList<IMatrixData> matrixData1List = new List<IMatrixData>() { _matrixData1 };
            var scheduleDayCollection =
                new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList);
            DateOnly? today = DateOnly.Today;
            _target.DayScheduled += _target_DayScheduled;
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
                      .IgnoreArguments()
                      .Return(matrixData1List);
                //spliting the period into weeks
                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Times(3);
                Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.Twice();
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
                Expect.Call(_validNumberOfDayOffInAWeekSpecification.IsSatisfied(_matrix1, _dateOnlyPeriod, 2))
                      .IgnoreArguments()
                      .Return(false);

                //Filtering schedule day collection
                Expect.Call(_matrixData1.ScheduleDayDataCollection).Return(new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList));
                Expect.Call(_scheduleDayData1.DateOnly).Return(today.Value);
                Expect.Call(_scheduleDayData2.DateOnly).Return(today.Value);

                //Shceudling day off
                Expect.Call(_bestSpotForAddingDayOffFinder.Find(scheduleDayCollection)).IgnoreArguments().Return(today);
                Expect.Call(_schedulingOptions.DayOffTemplate).Return(_dayOffTemplate);

                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Twice();
                Expect.Call(_matrix1.GetScheduleDayByKey(today.Value)).Return(_scheduleDayPro1);
                Expect.Call(_matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 }));
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(() => _scheduleDay1.CreateAndAddDayOff(_dayOffTemplate)).IgnoreArguments();
                Expect.Call(() => _rollbackService.Modify(_scheduleDay1)).IgnoreArguments();

                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List)).IgnoreArguments().Return(new List<IMatrixData>());

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
            IList<IMatrixData> matrixData1List = new List<IMatrixData>() { _matrixData1 };
            var scheduleDayCollection =
                new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList);
            DateOnly? today = DateOnly.Today;
            using (_mock.Record())
            {
                Expect.Call(_matrixDataListCreator.Create(_matrixList, _schedulingOptions)).IgnoreArguments().Return(_matrixDataList);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(_matrixDataList)).IgnoreArguments().Return(matrixData1List);
                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List))
                      .IgnoreArguments()
                      .Return(matrixData1List);
                //spliting the period into weeks
                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Times(3) ;
                Expect.Call(_matrix1.SchedulePeriod).Return(_virtualSchedulePeriod).Repeat.Twice() ;
                Expect.Call(_virtualSchedulePeriod.DateOnlyPeriod).Return(_dateOnlyPeriod);

                Expect.Call(_virtualSchedulePeriod.DaysOff()).Return(2);
                Expect.Call(_validNumberOfDayOffInAWeekSpecification.IsSatisfied(_matrix1, _dateOnlyPeriod, 2))
                      .IgnoreArguments()
                      .Return(false);

                //Filtering schedule day collection
                Expect.Call(_matrixData1.ScheduleDayDataCollection).Return(new ReadOnlyCollection<IScheduleDayData>(_scheduleDayDataList));
                Expect.Call(_scheduleDayData1.DateOnly).Return(today.Value);
                Expect.Call(_scheduleDayData2.DateOnly).Return(today.Value);

                //Shceudling day off
                Expect.Call(_bestSpotForAddingDayOffFinder.Find(scheduleDayCollection)).IgnoreArguments().Return(today);

                Expect.Call(_schedulingOptions.DayOffTemplate).Return(_dayOffTemplate);

                Expect.Call(_matrixData1.Matrix).Return(_matrix1).Repeat.Twice();
                Expect.Call(_matrix1.GetScheduleDayByKey(today.Value)).Return(_scheduleDayPro1);
                Expect.Call(_matrix1.UnlockedDays).Return(new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro> { _scheduleDayPro1 }));
                Expect.Call(_scheduleDayPro1.DaySchedulePart()).Return(_scheduleDay1);
                Expect.Call(() => _scheduleDay1.CreateAndAddDayOff(_dayOffTemplate)).IgnoreArguments();
                Expect.Call(() => _rollbackService.Modify(_scheduleDay1)).IgnoreArguments();

                Expect.Call(_matrixDataWithToFewDaysOff.FindMatrixesWithToFewDaysOff(matrixData1List)).IgnoreArguments().Return(new List<IMatrixData>()).Repeat.Twice();
                
            }
            using (_mock.Playback())
            {
                _target.Execute(_matrixList, _schedulingOptions, _rollbackService);
            }
        }
        
    }
    
}
