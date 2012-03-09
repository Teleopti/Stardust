using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class HasDayOffUnderFullDayAbsenceTest
	{
		private HasDayOffUnderFullDayAbsence _target;
		private IPerson _person;
		private IPersonPeriod _personPeriod;
		private IPersonContract _personContract;
		private IContract _contract;
		private IContractSchedule _contractSchedule;
		private IScheduleDay _scheduleDay;
		private IPersonDayOff _personDayOff;

		[SetUp]
		public void SetUp()
		{
			_person = MockRepository.GenerateMock<IPerson>();
			_personPeriod = MockRepository.GenerateMock<IPersonPeriod>();
			_personContract = MockRepository.GenerateMock<IPersonContract>();
			_contract = MockRepository.GenerateMock<IContract>();
			_contractSchedule = MockRepository.GenerateMock<IContractSchedule>();
			_scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			_personDayOff = MockRepository.GenerateMock<IPersonDayOff>();
		}

		[Test]
		public void ShouldReturnTrueWhenContractDayOffUnderFullDayAbsence()
		{
			_scheduleDay.Stub(x => x.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new[] { _personDayOff }));
			_scheduleDay.Stub(x => x.Person).Return(_person);
			_scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence);
			_person.Stub(x => x.Period(DateOnly.Today)).Return(_personPeriod);
			_personPeriod.Stub(x => x.PersonContract).Return(_personContract);
			_personContract.Stub(x => x.Contract).Return(_contract);
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
			_personContract.Stub(x => x.ContractSchedule).Return(_contractSchedule);
			_contractSchedule.Stub(x => x.IsWorkday(Arg<DateOnly>.Is.Anything, Arg<DateOnly>.Is.Anything)).Return(false);

			_target = new HasDayOffUnderFullDayAbsence();

			_target.HasDayOff(_scheduleDay).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnTrueWhenPersonDayOffUnderFullDayAbsence()
		{
			_scheduleDay.Stub(x => x.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new[] { _personDayOff }));
			_scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence);

			_target = new HasDayOffUnderFullDayAbsence();

			_target.HasDayOff(_scheduleDay).Should().Be.EqualTo(true);
		}

		[Test]
		public void ShouldReturnFalseWhenNoAbsence()
		{
			_scheduleDay.Stub(x => x.PersonDayOffCollection()).Return(new ReadOnlyCollection<IPersonDayOff>(new[] { _personDayOff }));
			_scheduleDay.Stub(x => x.Person).Return(_person);
			_scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(SchedulePartView.MainShift);
			_person.Stub(x => x.Period(DateOnly.Today)).Return(_personPeriod);
			_personPeriod.Stub(x => x.PersonContract).Return(_personContract);
			_personContract.Stub(x => x.Contract).Return(_contract);
			_contract.Stub(x => x.EmploymentType).Return(EmploymentType.FixedStaffDayWorkTime);
			_personContract.Stub(x => x.ContractSchedule).Return(_contractSchedule);
			_contractSchedule.Stub(x => x.IsWorkday(Arg<DateOnly>.Is.Anything, Arg<DateOnly>.Is.Anything)).Return(false);

			_target = new HasDayOffUnderFullDayAbsence();

			_target.HasDayOff(_scheduleDay).Should().Be.EqualTo(false);
		}

		[Test]
		public void ShouldReturnFalseWhenNoDayOff()
		{
			_scheduleDay.Stub(x => x.SignificantPartForDisplay()).Return(SchedulePartView.FullDayAbsence);

			_target = new HasDayOffUnderFullDayAbsence();

			_target.HasDayOff(_scheduleDay).Should().Be.EqualTo(false);
		}
	}
}
