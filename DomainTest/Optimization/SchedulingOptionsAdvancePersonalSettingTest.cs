using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using Rhino.Mocks;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulingOptionsAdvancedPersonalSettingTest
    {
        private SchedulingOptionsAdvancedPersonalSetting _target;
        private ISchedulingOptions _schedulingOptions;
        private MockRepository _mocks;
        //private IList<IScheduleTag> _scheduleTags;
        private IList<IGroupPage> _groupPages;
        private IShiftCategory _shiftCategory;
        private IGroupPage _groupPage;
        private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
           // _scheduleTag = _mocks.StrictMock<IScheduleTag>();
            _shiftCategory = _mocks.StrictMock<IShiftCategory>();
            _groupPage = _mocks.StrictMock<IGroupPage>();
            _groupPages = new List<IGroupPage> { _groupPage };
            _target = new SchedulingOptionsAdvancedPersonalSetting();
            _guid = new Guid();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
        }

        [Test]
        public void ShouldMap()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptions.ShiftCategory ).Return(_shiftCategory );
                Expect.Call(_shiftCategory.Id).Return(_guid);
                MapFromExpectations();

                Expect.Call(_shiftCategory.Id).Return(_guid);
                Expect.Call(_schedulingOptions.ShiftCategory).Return(_shiftCategory);
                Expect.Call(() => _schedulingOptions.ShiftCategory = _shiftCategory);
                MapToExpectations();
            }

            using (_mocks.Playback())
            {
                //_scheduleTags = new List<IScheduleTag> { _scheduleTag };

                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, new List<IShiftCategory > { _shiftCategory }, _groupPages);
            }
        }

        [Test]
        public void ShouldSetTagToNullScheduleInstanceWhenNoTag()
        {
            using (_mocks.Record())
            {
                Expect.Call(_schedulingOptions.ShiftCategory).Return(_shiftCategory );
                Expect.Call(_shiftCategory .Id).Return(null);
                MapFromExpectations();

                Expect.Call(_schedulingOptions.TagToUseOnScheduling).Return(null);
                Expect.Call(() => _schedulingOptions.TagToUseOnScheduling = NullScheduleTag.Instance);
                MapToExpectations();
            }

            using (_mocks.Playback())
            {
                //_scheduleTags = new List<IScheduleTag>();
                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, new List<IShiftCategory> { _shiftCategory }, _groupPages);
            }
        }

        private void MapFromExpectations()
        {
            Expect.Call(_schedulingOptions.UseMinimumPersons).Return(true);
            Expect.Call(_schedulingOptions.UseMaximumPersons).Return(true);
            Expect.Call(_schedulingOptions.UseMaxSeats).Return(true);
            Expect.Call(_schedulingOptions.DoNotBreakMaxSeats).Return(true);
           
        }


        private void MapToExpectations()
        {
            Expect.Call(_schedulingOptions.UseMinimumPersons).Return(true);
            Expect.Call(_schedulingOptions.UseMaximumPersons).Return(true);
            Expect.Call(_schedulingOptions.UseMaxSeats).Return(true);
            Expect.Call(_schedulingOptions.DoNotBreakMaxSeats).Return(true);
        }
    }
}
