﻿using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    /// <summary>
    /// Testing delete service for schedule parts
    /// </summary>
    [TestFixture]
    public class DeleteSchedulePartServiceTest
    {
        private MockRepository _mocks;
        private DeleteSchedulePartService _deleteService;
        private IList<IScheduleDay> _list;
        private IScheduleDay _part1;
        private IScheduleDay _part2;
        private IScheduleDay _part3;
        private DeleteOption _deleteOption;
    	private ISchedulePartModifyAndRollbackService _rollbackService;
        private NoSchedulingProgress _schedulingProgress;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _deleteService = new DeleteSchedulePartService();
        	_rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
			_part1 = _mocks.StrictMock<IScheduleDay>();
			_part2 = _mocks.StrictMock<IScheduleDay>();
			_part3 = _mocks.StrictMock<IScheduleDay>();
            _list = new List<IScheduleDay>{_part1, _part2};
            _deleteOption = new DeleteOption();
            _schedulingProgress = new NoSchedulingProgress();
        }

		[Test]
        public void VerifyCanCreateObject()
        {
            Assert.IsNotNull(_deleteService);
        }

        [Test]
        public void VerifyDeleteMainShift()
        {
            using (_mocks.Record())
            {
                _part3.DeleteMainShift();
                _part3.DeleteMainShift();
                Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
                Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
            }

            using (_mocks.Playback())
            {
                _deleteOption.MainShift = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

		[Test]
		public void VerifyDeleteMainShiftSpecial()
		{
			using (_mocks.Record())
			{
				_part3.DeleteMainShiftSpecial();
				_part3.DeleteMainShiftSpecial();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

			using (_mocks.Playback())
			{
				_deleteOption.MainShiftSpecial = true;
				_deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
			}
		}

        [Test]
        public void VerifyDeletePersonalStuff()
        {
            using (_mocks.Record())
            {
                _part3.DeletePersonalStuff();
                _part3.DeletePersonalStuff();


				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

            using (_mocks.Playback())
            {
                _deleteOption.PersonalShift = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteDayOff()
        {
            using (_mocks.Record())
            {
                _part3.DeleteDayOff();
                _part3.DeleteDayOff();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.DayOff = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void ShouldNotDeleteDayOffUnderFullDayAbsence()
        {
            using (_mocks.Record())
            {
            	Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();

                Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();


				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.Default = true;

                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteFullDayAbsence()
        {
            using (_mocks.Record())
            {
                _part3.DeleteFullDayAbsence(_part3);
                _part3.DeleteFullDayAbsence(_part3);

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
			}

            using (_mocks.Playback())
            {
                _deleteOption.Absence = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeleteDefault()
        {
            using(_mocks.Record())
            {
                Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Absence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.PersonalShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.DayOff).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Overtime).Repeat.Twice();

                Expect.Call(() => _part3.DeleteMainShift()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeletePersonalStuff()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteDayOff()).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteAbsence(false)).Repeat.AtLeastOnce();
                Expect.Call(() => _part3.DeleteOvertime()).Repeat.AtLeastOnce();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

            using (_mocks.Playback())
            {
                _deleteOption.Default = true;

                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

		[Test]
		public void VerifyDeleteDefaultWithoutBackgroundWorker()
		{
			using (_mocks.Record())
			{
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.MainShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Absence).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.PersonalShift).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.DayOff).Repeat.Twice();
				Expect.Call(_part3.SignificantPartForDisplay()).Return(SchedulePartView.Overtime).Repeat.Twice();

				Expect.Call(() => _part3.DeleteMainShift()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeletePersonalStuff()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteDayOff()).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteFullDayAbsence(_part3)).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteAbsence(false)).Repeat.AtLeastOnce();
				Expect.Call(() => _part3.DeleteOvertime()).Repeat.AtLeastOnce();

				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();

			}

			using (_mocks.Playback())
			{

				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
				_deleteService.Delete(_list, _rollbackService);
			}
		}

        [Test]
        public void VerifyDeleteOvertime()
        {
            _list = new List<IScheduleDay> { _part1};
            using (_mocks.Record())
            {
                Expect.Call(_part1.ReFetch()).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.DeleteOvertime).Repeat.AtLeastOnce();

				Expect.Call(() => _rollbackService.Modify(_part1));
            }

            using (_mocks.Playback())
            {
                _deleteOption.Overtime = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyDeletePreference()
        {
            _list = new List<IScheduleDay> { _part1 };

            using (_mocks.Record())
            {
                Expect.Call(_part1.ReFetch()).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.DeletePreferenceRestriction).Repeat.Once();
				Expect.Call(() => _rollbackService.Modify(_part1));
            }

            using (_mocks.Playback())
            {
                _deleteOption.Preference = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

        [Test]
        public void VerifyStudentAvailabilityRestriction()
        {
            _list = new List<IScheduleDay> { _part1 };

            using (_mocks.Record())
            {
                Expect.Call(_part1.ReFetch()).Return(_part1).Repeat.AtLeastOnce();
                Expect.Call(_part1.DeleteStudentAvailabilityRestriction).Repeat.Once();
            	Expect.Call(() => _rollbackService.Modify(_part1));
            }

            using (_mocks.Playback())
            {
                _deleteOption.StudentAvailability = true;
                _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            }
        }

		[Test]
		public void ShouldDeleteWithSpecifiedRules()
		{
			_list = new List<IScheduleDay> { _part1, _part2 };
			var rules = NewBusinessRuleCollection.Minimum();

			using (_mocks.Record())
			{
				Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3, rules)).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part2, rules));
			}

			var ret = _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress, rules);
			Assert.AreEqual(2, ret.Count);
		}

        [Test]
        public void VerifyDeleteIsReturningListOfNewScheduleParts()
        {
            _list = new List<IScheduleDay>{_part1, _part2};

            using (_mocks.Record())
            {
                Expect.Call(_part1.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
                Expect.Call(_part2.ReFetch()).Return(_part3).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part3)).Repeat.AtLeastOnce();
				Expect.Call(() => _rollbackService.Modify(_part2));
            }

            IList<IScheduleDay> ret = _deleteService.Delete(_list, _deleteOption, _rollbackService, _schedulingProgress);
            Assert.AreEqual(2, ret.Count);
        }
    }
}
