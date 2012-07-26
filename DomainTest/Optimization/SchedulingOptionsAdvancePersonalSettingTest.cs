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
        private IShiftCategory _shiftCategory;
        private Guid _guid;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _schedulingOptions = _mocks.StrictMock<ISchedulingOptions>();
            _shiftCategory = _mocks.StrictMock<IShiftCategory>();
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
               
                _target.MapFrom(_schedulingOptions);
                _target.MapTo(_schedulingOptions, new List<IShiftCategory > { _shiftCategory });
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
            Expect.Call(_schedulingOptions.UseMinimumPersons = true);
            Expect.Call(_schedulingOptions.UseMaximumPersons = true);
            Expect.Call(_schedulingOptions.UseMaxSeats = true );
            Expect.Call(_schedulingOptions.DoNotBreakMaxSeats = true );
        }
    }
}
