using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Legacy;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;
using Teleopti.Ccc.TestCommon.FakeData;


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
		private readonly DateOnlyPeriod _period = new DateOnlyPeriod(2008, 10, 20,2008,10,21);
		private IUnitOfWork _unitOfWork;
		private IScheduleDictionary _scheduleDictionary;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			_view = mocks.StrictMock<IIntradayView>();
			_schedulingResultLoader = mocks.DynamicMock<ISchedulingResultLoader>();
			_unitOfWorkFactory = mocks.StrictMock<IUnitOfWorkFactory>();

			_scenario = ScenarioFactory.CreateScenarioAggregate();
			_persons = new List<IPerson> { PersonFactory.CreatePersonWithId() };
			_schedulerStateHolder = new SchedulerStateHolder(_scenario, new DateOnlyPeriodAsDateTimePeriod(_period, TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone), _persons, mocks.DynamicMock<IDisableDeletedFilter>(), new SchedulingResultStateHolder(), new TimeZoneGuard());

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
		    Expect.Call(_view.DrawSkillGrid);
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
			Expect.Call(() => _schedulingResultLoader.ReloadForecastData(_unitOfWork));
			Expect.Call(_schedulingResultLoader.SchedulerState).Return(_schedulerStateHolder).Repeat.AtLeastOnce();

			mocks.ReplayAll();

			_schedulerStateHolder.SchedulingResultState.AddSkills(skill);
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