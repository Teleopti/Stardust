﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation.GroupScheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class ShiftCategoryFairnessAggregateManagerTest
	{
		private MockRepository _mocks;
		private ISchedulingResultStateHolder _resultStateHolder;
		private ShiftCategoryFairnessAggregateManager _target;
		private IShiftCategoryFairnessComparer _fairnessComparer;
		private IShiftCategoryFairnessAggregator _fairnessAggregator;
		private IShiftCategoryFairnessGroupPersonHolder _groupPersonHolder;
		private IScheduleDictionary _dic;
		private IScheduleDateTimePeriod _schedDateTimePeriod;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_resultStateHolder = _mocks.DynamicMock<ISchedulingResultStateHolder>();
			_dic = _mocks.DynamicMock<IScheduleDictionary>();
			_schedDateTimePeriod = _mocks.DynamicMock<IScheduleDateTimePeriod>();
			_fairnessComparer = _mocks.DynamicMock<IShiftCategoryFairnessComparer>();
			_fairnessAggregator = _mocks.DynamicMock<IShiftCategoryFairnessAggregator>();
			_groupPersonHolder = _mocks.DynamicMock<IShiftCategoryFairnessGroupPersonHolder>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnComparisonOnPersonAndTheGroup()
		{
			var date = new DateOnly(2012, 9, 12);
			var person = PersonFactory.CreatePerson("per");
			person.AddPersonPeriod(new PersonPeriod(date,
				   new PersonContract(new Contract("contract"),
					   new PartTimePercentage("percentage"),
					   new ContractSchedule("contractschedule")),
					   new Team()));
			var groupPage = new GroupPageLight();
			
			var dateTime = new DateTime(2012, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddDays(5));
			var groupPerson = new GroupPerson(new List<IPerson>{person}, date, "perras");
			var fairnessResult = new ShiftCategoryFairnessCompareResult();
			Expect.Call(_resultStateHolder.Schedules).Return(_dic);
			Expect.Call(_dic.Period).Return(_schedDateTimePeriod);
			Expect.Call(_schedDateTimePeriod.VisiblePeriodMinusFourWeeksPeriod()).Return(dateTimePeriod);
			Expect.Call(_dic.Keys).Return(new List<IPerson> {person});
			Expect.Call(_groupPersonHolder.GroupPersons(new List<DateOnly>(), groupPage, date, new Collection<IPerson> {person})).
				IgnoreArguments().Return(new List<IGroupPerson> { groupPerson });
			Expect.Call(_fairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson>())).IgnoreArguments().
				Repeat.Twice();
			Expect.Call(_fairnessComparer.Compare(null, null, null)).IgnoreArguments().Return(fairnessResult);
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessAggregateManager(_resultStateHolder, _fairnessComparer, _fairnessAggregator, _groupPersonHolder);
			var result = _target.GetPerPersonAndGroup(person, groupPage, date);
			Assert.That(result, Is.EqualTo(fairnessResult));
			_mocks.VerifyAll();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void ShouldReturnComparisonOnGroupAndOtherGroup()
		{
			var date = new DateOnly(2012, 9, 12);
			var person = PersonFactory.CreatePerson("per");
			var person2 = PersonFactory.CreatePerson("per");
			person.AddPersonPeriod(new PersonPeriod(date,
				   new PersonContract(new Contract("contract"),
					   new PartTimePercentage("percentage"),
					   new ContractSchedule("contractschedule")),
					   new Team()));
			person2.AddPersonPeriod(new PersonPeriod(date,
				   new PersonContract(new Contract("contract"),
					   new PartTimePercentage("percentage"),
					   new ContractSchedule("contractschedule")),
					   new Team()));
			var groupPage = new GroupPageLight();

			var dateTime = new DateTime(2012, 9, 12, 0, 0, 0, 0, DateTimeKind.Utc);
			var dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddDays(5));
			var groupPerson = new GroupPerson(new List<IPerson> { person }, date, "perras");
			var groupPerson2 = new GroupPerson(new List<IPerson> { person2 }, date, "peckas");
			var fairnessResult = new ShiftCategoryFairnessCompareResult();
			Expect.Call(_resultStateHolder.Schedules).Return(_dic);
			Expect.Call(_dic.Period).Return(_schedDateTimePeriod);
			Expect.Call(_schedDateTimePeriod.VisiblePeriodMinusFourWeeksPeriod()).Return(dateTimePeriod);
			Expect.Call(_dic.Keys).Return(new List<IPerson> { person });
			Expect.Call(_groupPersonHolder.GroupPersons(new List<DateOnly>(), groupPage, date, new Collection<IPerson> { person })).
				IgnoreArguments().Return(new List<IGroupPerson> { groupPerson, groupPerson2 });
			Expect.Call(_fairnessAggregator.GetShiftCategoryFairnessForPersons(_dic, new List<IPerson>())).IgnoreArguments().
				Repeat.Twice();
			Expect.Call(_fairnessComparer.Compare(null, null, null)).IgnoreArguments().Return(fairnessResult);
			_mocks.ReplayAll();
			_target = new ShiftCategoryFairnessAggregateManager(_resultStateHolder, _fairnessComparer, _fairnessAggregator, _groupPersonHolder);
			var result = _target.GetPerGroupAndOtherGroup(person,groupPage,date);
			Assert.That(result, Is.EqualTo(fairnessResult));
			_mocks.VerifyAll();
		}
	}
}