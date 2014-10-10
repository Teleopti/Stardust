using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class MultiplicatorsetForPasteSpecialFilterTest
	{
		private MultiplicatorsetForPasteSpecialFilter _target;
		private MockRepository _mocks;
		private IEnumerable<IMultiplicatorDefinitionSet> _availableMultiplicatorDefinitionSets;
		private IEnumerable<IScheduleDay> _scheduleDays;
		private IScheduleDay _scheduleDay1;
		private IScheduleDay _scheduleDay2;
		private IScheduleDay _scheduleDay3;
		private IContract _contract1;
		private IContract _contract2;
		private IContract _contract3;
		private IPersonContract _personContract1;
		private IPersonContract _personContract2;
		private IPersonContract _personContract3;
		private IMultiplicatorDefinitionSet _multiplicator1;
		private IMultiplicatorDefinitionSet _multiplicator2;
		private IMultiplicatorDefinitionSet _multiplicator3;
		private IMultiplicatorDefinitionSet _multiplicator4;
		private IPerson _person1;
		private IPerson _person2;
		private IPerson _person3;
		private IPersonPeriod _personPeriod1;
		private IPersonPeriod _personPeriod2;
		private IPersonPeriod _personPeriod3;
		private DateOnly _dateOnly;
		private IDateOnlyAsDateTimePeriod _dateOnlyAsDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_multiplicator1 = _mocks.Stub<IMultiplicatorDefinitionSet>();
			_multiplicator2 = _mocks.Stub<IMultiplicatorDefinitionSet>();
			_multiplicator3 = _mocks.Stub<IMultiplicatorDefinitionSet>();
			_multiplicator4 = _mocks.Stub<IMultiplicatorDefinitionSet>();
			_scheduleDay1 = _mocks.Stub<IScheduleDay>();
			_scheduleDay2 = _mocks.Stub<IScheduleDay>();
			_scheduleDay3 = _mocks.Stub<IScheduleDay>();
			_dateOnlyAsDateTimePeriod = _mocks.Stub<IDateOnlyAsDateTimePeriod>();
			_dateOnly = new DateOnly(2010, 03, 01);
			_person1 = _mocks.Stub<IPerson>();
			_person2 = _mocks.Stub<IPerson>();
			_person3 = _mocks.Stub<IPerson>();
			_personPeriod1 = _mocks.Stub<IPersonPeriod>();
			_personPeriod2 = _mocks.Stub<IPersonPeriod>();
			_personPeriod3 = _mocks.Stub<IPersonPeriod>();
			_personContract1 = _mocks.Stub<IPersonContract>();
			_personContract2 = _mocks.Stub<IPersonContract>();
			_personContract3 = _mocks.Stub<IPersonContract>();
			_contract1 = _mocks.Stub<IContract>();
			_contract2 = _mocks.Stub<IContract>();
			_contract3 = _mocks.Stub<IContract>();
			_scheduleDays = new List<IScheduleDay> { _scheduleDay1, _scheduleDay2, _scheduleDay3 };
			_availableMultiplicatorDefinitionSets = new List<IMultiplicatorDefinitionSet> {_multiplicator1, _multiplicator2};
			_target = new MultiplicatorsetForPasteSpecialFilter();
		}


		[Test]
		public void ShouldHaveTheMultiplicatorWhichAllPeopleHave()
		{

			using (_mocks.Record())
			{
				Expect.Call(_multiplicator1.MultiplicatorType).Return(MultiplicatorType.Overtime);
				Expect.Call(_multiplicator2.MultiplicatorType).Return(MultiplicatorType.Overtime);
				Expect.Call(_multiplicator3.MultiplicatorType).Return(MultiplicatorType.Overtime);
				Expect.Call(_multiplicator4.MultiplicatorType).Return(MultiplicatorType.OBTime);
				Expect.Call(_scheduleDay1.Person).Return(_person1);
				Expect.Call(_scheduleDay2.Person).Return(_person2); 
				Expect.Call(_scheduleDay3.Person).Return(_person3);
				Expect.Call(_scheduleDay1.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_scheduleDay2.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_scheduleDay3.DateOnlyAsPeriod).Return(_dateOnlyAsDateTimePeriod);
				Expect.Call(_dateOnlyAsDateTimePeriod.DateOnly).Return(_dateOnly);
				Expect.Call(_person1.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new List<IPersonPeriod>{_personPeriod1 });
				Expect.Call(_person2.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new List<IPersonPeriod> { _personPeriod2 });
				Expect.Call(_person3.PersonPeriods(new DateOnlyPeriod(_dateOnly, _dateOnly))).Return(new List<IPersonPeriod> { _personPeriod3 });
				_personPeriod1.PersonContract = _personContract1;
				_personPeriod2.PersonContract = _personContract2;
				_personPeriod3.PersonContract = _personContract3;
				_personContract1.Contract = _contract1;
				_personContract2.Contract = _contract2;
				_personContract3.Contract = _contract3;
				Expect.Call(_contract1.MultiplicatorDefinitionSetCollection)
					.Return(new ReadOnlyCollection<IMultiplicatorDefinitionSet>(new List<IMultiplicatorDefinitionSet>
						{
							_multiplicator1,
							_multiplicator2
						}));
				Expect.Call(_contract2.MultiplicatorDefinitionSetCollection)
					.Return(new ReadOnlyCollection<IMultiplicatorDefinitionSet>(new List<IMultiplicatorDefinitionSet>
						{
							_multiplicator1
						}));
				Expect.Call(_contract3.MultiplicatorDefinitionSetCollection)
						.Return(new ReadOnlyCollection<IMultiplicatorDefinitionSet>(new List<IMultiplicatorDefinitionSet>
						{
							_multiplicator1
						}));
			}
			using (_mocks.Playback())
			{
				var result = _target.FilterAvailableMultiplicatorSet(_availableMultiplicatorDefinitionSets, _scheduleDays);
				Assert.AreEqual(1, result.ToList().Count);
				Assert.AreEqual(_multiplicator1, result.ToList()[0]);
			}


		}
	}
}
