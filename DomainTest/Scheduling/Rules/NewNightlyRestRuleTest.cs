using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    [TestFixture]
    public class NewNightlyRestRuleTest
    {
        private INewBusinessRule _target;
        private IDictionary<IPerson, IScheduleRange> _ranges;
        private IScheduleDay _scheduleDay;
        private IScheduleDay _yesterday;
        private IScheduleDay _today;
        private IScheduleDay _tomorrow;
        private IList<IScheduleDay> _scheduleDays;
        private MockRepository _mocks;
        private IPerson _person;
        private IScheduleRange _range;
        private IEnumerable<IBusinessRuleResponse> _responsList;
    	private IWorkTimeStartEndExtractor _workTimeStartEndExtractor;

    	[SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _scheduleDay = _mocks.StrictMock<IScheduleDay>();
            _yesterday = _mocks.StrictMock<IScheduleDay>();
            
            _today = _mocks.StrictMock<IScheduleDay>();
            _tomorrow = _mocks.StrictMock<IScheduleDay>();

            _scheduleDays = new List<IScheduleDay>{_scheduleDay};
            _person = PersonFactory.CreatePersonWithPersonPeriod(new DateOnly(), new List<ISkill>());
            _person.SetId(Guid.NewGuid());
            _range = _mocks.StrictMock<IScheduleRange>();
            _ranges = new Dictionary<IPerson, IScheduleRange> {{_person, _range}};
        	_workTimeStartEndExtractor = _mocks.StrictMock<IWorkTimeStartEndExtractor>();
            _target = new NewNightlyRestRule(_workTimeStartEndExtractor);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.IsFalse(_target.IsMandatory);
            Assert.IsTrue(_target.HaltModify);
            _target.HaltModify = false;
            Assert.IsFalse(_target.HaltModify);
            Assert.AreEqual(string.Empty, _target.ErrorMessage);
        }

        [Test]
        public void VerifyNoPersonPeriod()
        {
            _person.RemoveAllPersonPeriods();
            var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            using (_mocks.Record())
            {
                //currentScheduleDay
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any(); 
				Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.AtLeastOnce();
				Expect.Call(_range.Person).Return(_person).Repeat.Any();
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3))).Return(new[] { _yesterday, _today, _tomorrow });
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();
				Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				mockShift(_yesterday, new DateTimePeriod());
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();
				Expect.Call(_today.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				mockShift(_today, new DateTimePeriod());
				Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 3), TimeZoneInfo.Utc)).Repeat.Any();
				Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				mockShift(_tomorrow, new DateTimePeriod());
			}

            using (_mocks.Playback())
            {
                _responsList = _target.Validate(_ranges, _scheduleDays);
            }

            Assert.AreEqual(0, _responsList.Count());
        }

        [Test]
        public void VerifyNoSchedules()
        {
            var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            using (_mocks.Record())
            {
                
                //currentScheduleDay
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any();
                Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.AtLeastOnce();
                Expect.Call(_range.Person).Return(_person).Repeat.Any();

                //yesterday
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3))).Return(new[] { _yesterday, _today, _tomorrow });
                Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_yesterday, new DateTimePeriod());

                //today
                Expect.Call(_today.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_today, new DateTimePeriod());

                //tomorrow
                Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.None).Repeat.Any();
				Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 3), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_tomorrow, new DateTimePeriod());

                Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>()).
                   Repeat.Any();
            }

            using (_mocks.Playback())
            {
                _responsList = _target.Validate(_ranges, _scheduleDays);
            }
            
            Assert.AreEqual(0, _responsList.Count());
        }

        [Test]
        public void VerifyScheduleBreaksRule()
        {
            var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            using (_mocks.Record())
            {

                //currentScheduleDay
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any();
                Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.Any();
                Expect.Call(_range.Person).Return(_person).Repeat.Any();
                Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>()).
                    Repeat.Any();

                //yesterday
                Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1,2010,1,3))).Return(new[]{_yesterday,_today,_tomorrow});
                Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();

                mockShift(_yesterday, new DateTimePeriod(new DateTime(2010, 1, 1, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 1, 19, 0, 0, DateTimeKind.Utc)));

                //today
                Expect.Call(_today.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_today, new DateTimePeriod(new DateTime(2010, 1, 2, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 2, 19, 0, 0, DateTimeKind.Utc)));
				
                //tomorrow
                Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
                Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010,1,3), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_tomorrow, new DateTimePeriod(new DateTime(2010, 1, 3, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 3, 19, 0, 0, DateTimeKind.Utc)));

            }

            using (_mocks.Playback())
            {
                _responsList = _target.Validate(_ranges, _scheduleDays);
            }

            Assert.AreEqual(4, _responsList.Count());
            Assert.IsFalse(_responsList.First().Mandatory);
            Assert.AreEqual(_target.HaltModify, _responsList.First().Error);

        }

        [Test]
        public void VerifyScheduleOk()
        {
            var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
            using (_mocks.Record())
            {

                //currentScheduleDay
                Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
                Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any();
                Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.Any();
                Expect.Call(_range.Person).Return(_person).Repeat.Any();

                //yesterday
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3))).Return(new[] { _yesterday, _today, _tomorrow });
                Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_yesterday, new DateTimePeriod(new DateTime(2010, 1, 1, 6, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 1, 19, 0, 0, DateTimeKind.Utc)));

                //today
                Expect.Call(_today.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();
                
				mockShift(_today, new DateTimePeriod(new DateTime(2010, 1, 2, 6, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 2, 19, 0, 0, DateTimeKind.Utc)));
				
                //tomorrow
                Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.MainShift).Repeat.Any();
				Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 3), TimeZoneInfo.Utc)).Repeat.Any();
                
				mockShift(_tomorrow, new DateTimePeriod(new DateTime(2010, 1, 3, 6, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 3, 19, 0, 0, DateTimeKind.Utc)));

                Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>()).
                    Repeat.Any();

            }

            using (_mocks.Playback())
            {
                _responsList = _target.Validate(_ranges, _scheduleDays);
            }

            Assert.AreEqual(0, _responsList.Count());
        }

        [Test]
        public void VerifyRuleRemoveRangeBusinessRuleResponse()
        {
            var doPeriod = new DateOnlyPeriod(2010, 1, 3, 2010, 1, 3);
            DateTimePeriod period = doPeriod.ToDateTimePeriod(_person.PermissionInformation.DefaultTimeZone());
            IList<IBusinessRuleResponse> list = new List<IBusinessRuleResponse>();

            doPeriod = new DateOnlyPeriod(2010, 1, 3, 2010, 1, 4);
            IBusinessRuleResponse thisResponse = new BusinessRuleResponse(typeof (NewNightlyRestRule), "", true, true,
                                                                          period, _person, doPeriod);
            IBusinessRuleResponse anotherResponse = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true,
                                                                          period.MovePeriod(TimeSpan.FromDays(10)), _person, doPeriod);

            IBusinessRuleResponse thirdResponse = new BusinessRuleResponse(typeof(NewNightlyRestRule), "", true, true,
                                                                          period, PersonFactory.CreatePerson(), doPeriod);
            list.Add(thisResponse);
            list.Add(anotherResponse);
            list.Add(thirdResponse);
        	var target = new NewNightlyRestRule(_workTimeStartEndExtractor);
            target.ClearMyResponses(list, _person, new DateOnly(2010, 1, 3));
            Assert.AreEqual(2, list.Count);

        }

		[Test]
		public void ShouldCalculateOvertime()
		{
			var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			using (_mocks.Record())
			{

				//currentScheduleDay
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any();
				Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.Any();
				Expect.Call(_range.Person).Return(_person).Repeat.Any();
				Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>()).
					Repeat.Any();

				//yesterday
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3))).Return(new[] { _yesterday, _today, _tomorrow });
				Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.Any();
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_yesterday, new DateTimePeriod(new DateTime(2010, 1, 1, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 1, 19, 0, 0, DateTimeKind.Utc)));

				//today
				Expect.Call(_today.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.Any();
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_today, new DateTimePeriod(new DateTime(2010, 1, 2, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 2, 19, 0, 0, DateTimeKind.Utc)));

				//tomorrow
				Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.Any();
				Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 3), TimeZoneInfo.Utc)).Repeat.Any();

				mockShift(_tomorrow, new DateTimePeriod(new DateTime(2010, 1, 3, 5, 0, 0, DateTimeKind.Utc), new DateTime(2010, 1, 3, 19, 0, 0, DateTimeKind.Utc)));

			}

			using (_mocks.Playback())
			{
				_responsList = _target.Validate(_ranges, _scheduleDays);
			}

			Assert.AreEqual(4, _responsList.Count());
			Assert.IsFalse(_responsList.First().Mandatory);
			Assert.AreEqual(_target.HaltModify, _responsList.First().Error);
		}

		[Test]
		public void ShouldNotCalculateAbsence()
		{
			var dateOnlyAsDateTimePeriod = _mocks.StrictMock<IDateOnlyAsDateTimePeriod>();
			using (_mocks.Record())
			{

				//currentScheduleDay
				Expect.Call(_scheduleDay.Person).Return(_person).Repeat.Any();
				Expect.Call(_scheduleDay.DateOnlyAsPeriod).Return(dateOnlyAsDateTimePeriod).Repeat.Any();
				Expect.Call(dateOnlyAsDateTimePeriod.DateOnly).Return(new DateOnly(2010, 1, 2)).Repeat.Any();
				Expect.Call(_range.Person).Return(_person).Repeat.Any();
				Expect.Call(_range.BusinessRuleResponseInternalCollection).Return(new List<IBusinessRuleResponse>()).
					Repeat.Any();

				//yesterday
				Expect.Call(_range.ScheduledDayCollection(new DateOnlyPeriod(2010, 1, 1, 2010, 1, 3))).Return(new[] { _yesterday, _today, _tomorrow });
				Expect.Call(_yesterday.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.Any();
				Expect.Call(_yesterday.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 1), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_yesterday, new DateTimePeriod());

				//today
				Expect.Call(_today.SignificantPart()).Return(SchedulePartView.FullDayAbsence).Repeat.Any();
				Expect.Call(_today.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 2), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_today, new DateTimePeriod());

				//tomorrow
				Expect.Call(_tomorrow.SignificantPart()).Return(SchedulePartView.Overtime).Repeat.Any();
				Expect.Call(_tomorrow.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(new DateOnly(2010, 1, 3), TimeZoneInfo.Utc)).Repeat.Any();
				mockShift(_tomorrow,new DateTimePeriod());
			}

			using (_mocks.Playback())
			{
				_responsList = _target.Validate(_ranges, _scheduleDays);
			}

			Assert.AreEqual(0, _responsList.Count());
		}

		private void mockShift(IScheduleDay scheduleDay, DateTimePeriod period)
        {
            var projectionService = _mocks.StrictMock<IProjectionService>();
            var visualLayerCollection = _mocks.StrictMock<IVisualLayerCollection>();
            Expect.Call(scheduleDay.ProjectionService()).Return(projectionService).Repeat.Any();
            Expect.Call(projectionService.CreateProjection()).Return(visualLayerCollection).Repeat.Any();
			Expect.Call(_workTimeStartEndExtractor.WorkTimeStart(visualLayerCollection)).Return(period.StartDateTime);
			Expect.Call(_workTimeStartEndExtractor.WorkTimeEnd(visualLayerCollection)).Return(period.EndDateTime);
        }
    }
}
