﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Intraday;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Messaging.Events;

namespace Teleopti.Ccc.WinCodeTest.Intraday
{
	[TestFixture]
	public class OnEventForecastMessageCommandTest
	{
		private MockRepository mocks;
		private IIntradayView _view;
		private ISchedulingResultLoader _schedulingResultLoader;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private OnEventForecastDataMessageCommand target;
		private IScenario _scenario;
		private IEnumerable<IPerson> _persons;
		private ISchedulerStateHolder _schedulerStateHolder;
		private readonly DateTimePeriod _period = DateTimeFactory.CreateDateTimePeriod(new DateTime(2008, 10, 20, 0, 0, 0, DateTimeKind.Utc), 1);
		private IUnitOfWork _unitOfWork;
		private IScheduleDictionary _scheduleDictionary;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_view = mocks.StrictMock<IIntradayView>();
			_schedulingResultLoader = mocks.DynamicMock<ISchedulingResultLoader>();
			_unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();

			_scenario = mocks.StrictMock<IScenario>();
			_persons = new List<IPerson> { mocks.StrictMock<IPerson>() };
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, _period, _persons);

			_scheduleDictionary = mocks.StrictMock<IScheduleDictionary>();
			_unitOfWork = mocks.DynamicMock<IUnitOfWork>();

			target = new OnEventForecastDataMessageCommand(_view,_schedulingResultLoader,_unitOfWorkFactory);
		}

		[Test]
		public void VerifyOnEventForecastDataMessageHandler()
		{
			var skill = mocks.StrictMock<ISkill>();
			
			Expect.Call(_view.SelectedSkill).Return(skill);
			Expect.Call(() => _view.SelectSkillTab(skill));
			Expect.Call(_view.SetupSkillTabs);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(() => _schedulingResultLoader.ReloadForecastData(_unitOfWork));
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

			mocks.ReplayAll();

			_schedulerStateHolder.SchedulingResultState.Skills.Add(skill);
			_schedulerStateHolder.SchedulingResultState.Schedules = _scheduleDictionary;
			target.Execute(new EventMessage());

			mocks.VerifyAll();

			Assert.IsTrue(_schedulerStateHolder.SchedulingResultState.Skills.Contains(skill));
			Assert.AreSame(_scheduleDictionary, _schedulerStateHolder.SchedulingResultState.Schedules);
		}

		[Test]
		public void VerifyOnEventForecastDataMessageHandlerWithoutSkills()
		{

			Expect.Call(() => _schedulingResultLoader.ReloadForecastData(_unitOfWork));
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

			Expect.Call(_view.SelectedSkill).Return(null);

			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);

			mocks.ReplayAll();

			_schedulerStateHolder.SchedulingResultState.Schedules = _scheduleDictionary;
			target.Execute(new EventMessage());

			mocks.VerifyAll();

			Assert.AreSame(_scheduleDictionary, _schedulerStateHolder.SchedulingResultState.Schedules);
		}
	}
}