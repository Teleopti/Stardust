using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Ccc.WinCode.Scheduling.Restriction;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler.Restriction
{
	[TestFixture]
	public class RestrictionViewModelTest
	{
		private TimeSpan _startLimit;
		private TimeSpan _endLimit;
		private WorkTimeLimitation _workLimit;
		private IPreferenceRestriction _preferenceRestriction;
		private IStudentAvailabilityDay _studDayRestriction;
		private PreferenceRestrictionViewModel _target;
		private MockRepository _mockRep;
		private CrossThreadTestRunner _testRunner;
		private IList<IActivity> _activities;
		private IList<IShiftCategory> _shiftCategories;
		private IList<IDayOffTemplate> _dayOffTemplates;
		private TesterForCommandModels _models;
		private IRestrictionAltered _parentToAlter;
		private IScheduleDataRestriction _scheduleDataRestriction;

		[SetUp]
		public void Setup()
		{
			_dayOffTemplates = new List<IDayOffTemplate>();
			_models = new TesterForCommandModels();
			_activities = new List<IActivity>();
			_shiftCategories = new List<IShiftCategory>();
			_mockRep = new MockRepository();
			_startLimit = TimeSpan.FromHours(8);
			_endLimit = TimeSpan.FromHours(12);
			_workLimit = new WorkTimeLimitation(TimeSpan.FromHours(4), TimeSpan.FromHours(8));
			_preferenceRestriction = new PreferenceRestriction();
			_target = new PreferenceRestrictionViewModel(_preferenceRestriction, null);
			_studDayRestriction = _mockRep.StrictMock<StudentAvailabilityDay>();
			_scheduleDataRestriction = _mockRep.StrictMock<IScheduleDataRestriction>();
			_testRunner = new CrossThreadTestRunner();
			_parentToAlter = _mockRep.StrictMock<IRestrictionAltered>();
		}

        [Test]
        public void VerifyCollectionsUpdatesTargetWithDeletedItems()
        {
            IList<string> firedProperties = new List<string>();
            var shiftCategory1 = new ShiftCategory("for test 1");
            var shiftCategory2 = new ShiftCategory("for test 2");
            var dayOffTemplate1 = new DayOffTemplate(new Description("for test 1"));
            var dayOffTemplate2 = new DayOffTemplate(new Description("for test 2"));
            IList<IShiftCategory> categories = new List<IShiftCategory> { shiftCategory2 };
            IList<IDayOffTemplate> dayOffTemplates = new List<IDayOffTemplate> { dayOffTemplate2 };

            _preferenceRestriction.ShiftCategory = shiftCategory1;
            _preferenceRestriction.DayOffTemplate = dayOffTemplate1;
            var preferenceDay = new PreferenceDay(new Person(), new DateOnly(), _preferenceRestriction);

            shiftCategory1.SetDeleted();
            dayOffTemplate1.SetDeleted();

            using (_mockRep.Record())
            {
                Expect.Call(_parentToAlter.ShiftCategories).Return(categories).Repeat.AtLeastOnce();
                Expect.Call(_parentToAlter.Activities).Return(new List<IActivity>()).Repeat.AtLeastOnce();
                Expect.Call(_parentToAlter.DayOffTemplates).Return(dayOffTemplates).Repeat.AtLeastOnce();
                Expect.Call(_parentToAlter.RestrictionIsAltered = true).Repeat.AtLeastOnce();
                Expect.Call(() => _parentToAlter.RestrictionAltered()).Repeat.AtLeastOnce();
            }
            _target = (PreferenceRestrictionViewModel)RestrictionViewModel.CreateViewModel(_parentToAlter, preferenceDay);
            _target.PropertyChanged += ((sender, e) => firedProperties.Add(e.PropertyName));

            Assert.AreEqual(shiftCategory1, _target.Categories.View.CurrentItem);
            Assert.AreEqual(dayOffTemplate1, _target.DayOffTemplates.View.CurrentItem);

            _target.Categories.View.MoveCurrentTo(shiftCategory2);
            _target.DayOffTemplates.View.MoveCurrentTo(dayOffTemplate2);

            _target.CommitChanges();

            Assert.AreEqual(shiftCategory2, _preferenceRestriction.ShiftCategory);
            Assert.AreEqual(dayOffTemplate2, _preferenceRestriction.DayOffTemplate);
            Assert.IsTrue(_target.HasDayOff);

            _target.Categories.View.MoveCurrentToFirst();
            _target.DayOffTemplates.View.MoveCurrentToFirst();

            _target.CommitChanges();

            Assert.IsNull(_preferenceRestriction.ShiftCategory);
            Assert.IsNull(_preferenceRestriction.DayOffTemplate);
            Assert.IsFalse(_target.HasDayOff);

            Assert.IsTrue(firedProperties.Contains("HasDayOff"), "Verify PropertyChanged has fired on HasDayOff");
        }
		[Test]
		public void VerifyCollectionsUpdatesTarget()
		{
			IList<string> firedProperties = new List<string>();
			var shiftCategory1 = new ShiftCategory("for test 1");
			var shiftCategory2 = new ShiftCategory("for test 2");
			var dayOffTemplate1 = new DayOffTemplate(new Description("for test 1"));
			var dayOffTemplate2 = new DayOffTemplate(new Description("for test 2"));
			IList<IShiftCategory> categories = new List<IShiftCategory> { shiftCategory1, shiftCategory2 };
			IList<IDayOffTemplate> dayOffTemplates = new List<IDayOffTemplate> { dayOffTemplate1, dayOffTemplate2 };

			_preferenceRestriction.ShiftCategory = shiftCategory1;
			_preferenceRestriction.DayOffTemplate = dayOffTemplate1;
			var preferenceDay = new PreferenceDay(new Person(), new DateOnly(), _preferenceRestriction);

			using (_mockRep.Record())
			{
				Expect.Call(_parentToAlter.ShiftCategories).Return(categories).Repeat.AtLeastOnce();
                Expect.Call(_parentToAlter.Activities).Return(new List<IActivity>()).Repeat.AtLeastOnce();
				Expect.Call(_parentToAlter.DayOffTemplates).Return(dayOffTemplates).Repeat.AtLeastOnce();
				Expect.Call(_parentToAlter.RestrictionIsAltered = true).Repeat.AtLeastOnce();
				Expect.Call(() => _parentToAlter.RestrictionAltered()).Repeat.AtLeastOnce();
			}
			_target = (PreferenceRestrictionViewModel)RestrictionViewModel.CreateViewModel(_parentToAlter, preferenceDay);
			_target.PropertyChanged += ((sender, e) => firedProperties.Add(e.PropertyName));
		
            Assert.AreEqual(shiftCategory1, _target.Categories.View.CurrentItem);
			Assert.AreEqual(dayOffTemplate1, _target.DayOffTemplates.View.CurrentItem);

			_target.Categories.View.MoveCurrentTo(shiftCategory2);
			_target.DayOffTemplates.View.MoveCurrentTo(dayOffTemplate2);

			_target.CommitChanges();

			Assert.AreEqual(shiftCategory2, _preferenceRestriction.ShiftCategory);
			Assert.AreEqual(dayOffTemplate2, _preferenceRestriction.DayOffTemplate);
			Assert.IsTrue(_target.HasDayOff);

			_target.Categories.View.MoveCurrentToFirst();
			_target.DayOffTemplates.View.MoveCurrentToFirst();

			_target.CommitChanges();

		    Assert.IsNull(_preferenceRestriction.ShiftCategory);
			Assert.IsNull(_preferenceRestriction.DayOffTemplate);
			Assert.IsFalse(_target.HasDayOff);

			Assert.IsTrue(firedProperties.Contains("HasDayOff"), "Verify PropertyChanged has fired on HasDayOff");
		}

		[Test]
		public void VerifyCanCreatePreferenceRestrictionWithNullCollections()
		{
			//Activity activity = new Activity("Activity") { Description = new Description("Activity") };
			var shiftCategory = new ShiftCategory("ShiftCategory");
			IPreferenceRestriction prefRestriction =
				new PreferenceRestriction
				{
					ShiftCategory = shiftCategory,
					DayOffTemplate = null,
					//Activity = activity,
					StartTimeLimitation = new StartTimeLimitation(_startLimit, _startLimit),
					EndTimeLimitation = new EndTimeLimitation(_endLimit, _endLimit),
					WorkTimeLimitation = _workLimit
				};
			var preferenceDay = new PreferenceDay(new Person(), new DateOnly(), prefRestriction);
			using (_mockRep.Record())
			{
				Expect.Call(_parentToAlter.Activities).Return(null);
				Expect.Call(_parentToAlter.ShiftCategories).Return(null);
				Expect.Call(_parentToAlter.DayOffTemplates).Return(null);
			}

			using (_mockRep.Playback())
			{
				var target = (PreferenceRestrictionViewModel)RestrictionViewModel.CreateViewModel(_parentToAlter, preferenceDay);
				Assert.IsNotNull(target);

			}
		}

		[Test]
		public void VerifyIsValidIfAllModelsAreValid()
		{
			var model = new RestrictionViewModelForTest();
			var trueModel = _mockRep.StrictMock<ILimitationViewModel>();
			var falseModel = _mockRep.StrictMock<ILimitationViewModel>();

			using (_mockRep.Record())
			{
				Expect.Call(trueModel.Invalid).Return(true).Repeat.Any();
				Expect.Call(falseModel.Invalid).Return(false).Repeat.Any();
			}

			using (_mockRep.Playback())
			{
				model.SetTimeLimitations(falseModel, falseModel, falseModel);
				Assert.IsTrue(model.IsValid());

				model.SetTimeLimitations(trueModel, falseModel, falseModel);
				Assert.IsFalse(model.IsValid());

				model.SetTimeLimitations(falseModel, trueModel, falseModel);
				Assert.IsFalse(model.IsValid());

				model.SetTimeLimitations(falseModel, falseModel, trueModel);
				Assert.IsFalse(model.IsValid());
			}
		}

		[Test]
		public void VerifyGeneratesAvailableRestrictionViewModelAndPropertiesAreSet()
		{
			var startLimitation = new StartTimeLimitation(_startLimit, _startLimit);
			var endLimitation = new EndTimeLimitation(_endLimit, _endLimit);
			var avRestriction =
				new StudentAvailabilityRestriction
					{
						StartTimeLimitation = startLimitation,
						EndTimeLimitation = endLimitation,
						WorkTimeLimitation = _workLimit
					};
			var availabilityRestrictions = new List<IStudentAvailabilityRestriction> { avRestriction };
			_studDayRestriction = new StudentAvailabilityDay(new Person(),new DateOnly(), availabilityRestrictions);

				var target = (AvailableRestrictionViewModel)RestrictionViewModel.CreateViewModel(null, _studDayRestriction);
				Assert.AreEqual(true, target.Available);
				Assert.AreEqual(UserTexts.Resources.StudentAvailability, target.Description);
				
				Assert.IsTrue(target.WorkTimeLimits.Editable);
				Assert.AreEqual(startLimitation, target.StartTimeLimits.Limitation);
				Assert.AreEqual(endLimitation, target.EndTimeLimits.Limitation);
		}

		[Test]
		public void VerifyGeneratesAvailableRestrictionForStudentAvailabilityCorrectTextIsReturned()
		{
			var startLimitation = new StartTimeLimitation(_startLimit, _startLimit);
			var endLimitation = new EndTimeLimitation(_endLimit, _endLimit);
			var avRestriction =
				new StudentAvailabilityRestriction
				{
					StartTimeLimitation = startLimitation,
					EndTimeLimitation = endLimitation,
					WorkTimeLimitation = _workLimit
				};
			var availabilityRestrictions = new List<IStudentAvailabilityRestriction> { avRestriction };
			_studDayRestriction = new StudentAvailabilityDay(new Person(), new DateOnly(), availabilityRestrictions);
			
				var target = (AvailableRestrictionViewModel)RestrictionViewModel.CreateViewModel(null, _studDayRestriction);
				Assert.AreEqual(true, target.Available);
				Assert.AreEqual(UserTexts.Resources.StudentAvailability, target.Description);
				_studDayRestriction.NotAvailable = true;
				Assert.IsFalse(target.Available);
				Assert.IsTrue(target.WorkTimeLimits.Editable);
				Assert.AreEqual(startLimitation, target.StartTimeLimits.Limitation);
				Assert.AreEqual(endLimitation, target.EndTimeLimits.Limitation);

		}

		[Test]
		public void VerifyGeneratesAvailableRestrictionForAvailabilityCorrectTextIsReturned()
		{
			var startLimitation = new StartTimeLimitation(_startLimit, _startLimit);
			var endLimitation = new EndTimeLimitation(_endLimit, _endLimit);
			IAvailabilityRestriction avRestriction =
				new AvailabilityRestriction
				{
					StartTimeLimitation = startLimitation,
					EndTimeLimitation = endLimitation,
					WorkTimeLimitation = _workLimit
				};
			using (_mockRep.Record())
			{
				Expect.Call(_scheduleDataRestriction.Restriction).Return(avRestriction);
			}
			var target = (AvailabilityRestrictionViewModel)RestrictionViewModel.CreateViewModel(null, _scheduleDataRestriction);
			target.CommitChanges(); //Does nothing on rotations
			Assert.AreEqual(false, target.HasDayOff);
			Assert.AreEqual(UserTexts.Resources.Availability, target.Description);
			var targetWithoutDayOff = new AvailabilityRestrictionViewModel(avRestriction) {HasDayOff = true};
		    Assert.IsFalse(targetWithoutDayOff.HasDayOff);
		}

		[Test]
		public void VerifyGeneratesRotationRestrictionViewModelAndPropertiesAreSet()
		{
			IRotationRestriction rotRestriction =
				new RotationRestriction
					{
						ShiftCategory = new ShiftCategory("ShiftCat"),
						DayOffTemplate = new DayOffTemplate(new Description("DayOff")),
						StartTimeLimitation = new StartTimeLimitation(_startLimit, _startLimit),
						EndTimeLimitation = new EndTimeLimitation(_endLimit, _endLimit),
						WorkTimeLimitation = _workLimit
					};

			using (_mockRep.Record())
			{
				Expect.Call(_scheduleDataRestriction.Restriction).Return(rotRestriction);
			}
			var target = (RotationRestrictionViewModel)RestrictionViewModel.CreateViewModel(null, _scheduleDataRestriction);
			target.CommitChanges(); //Does nothing on rotations
			Assert.AreEqual(true, target.DayOff);
			Assert.AreEqual(UserTexts.Resources.Rotation, target.Description);
			target.ShiftCategory = "empty setter";
			Assert.AreEqual("ShiftCat", target.ShiftCategory);
			rotRestriction.DayOffTemplate = null;
			var targetWithoutDayOff = new RotationRestrictionViewModel(rotRestriction) {DayOff = true};
		    Assert.IsFalse(targetWithoutDayOff.DayOff);


		}

		[Test]
		public void VerifyGeneratesPreferenceRestrictionViewModelAndPropertiesAreSet()
		{
			const string activityString = "Activity";
			const string shiftCategoryString = "ShiftCategory";
			const string dayOffTemplateString = "DayOffTemplate";
			IActivity activity = new Activity("ActivityName") { Description = new Description(activityString) };
			IShiftCategory shiftCategory = new ShiftCategory(shiftCategoryString);
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description(dayOffTemplateString));
			IPreferenceRestriction prefRestriction =
				new PreferenceRestriction
					{
						ShiftCategory = shiftCategory,
						DayOffTemplate = dayOffTemplate,
						//Activity = activity,
						StartTimeLimitation = new StartTimeLimitation(_startLimit, _startLimit),
						EndTimeLimitation = new EndTimeLimitation(_endLimit, _endLimit),
						WorkTimeLimitation = _workLimit
					};
			prefRestriction.AddActivityRestriction(new ActivityRestriction(activity));
			_activities.Add(activity);
			_shiftCategories.Add(shiftCategory);
			_dayOffTemplates.Add(dayOffTemplate);
			var preferenceDay = new PreferenceDay(new Person(), new DateOnly(), prefRestriction);
			using (_mockRep.Record())
			{
				Expect.Call(_parentToAlter.Activities).Return(_activities).Repeat.Any();
				Expect.Call(_parentToAlter.ShiftCategories).Return(_shiftCategories).Repeat.Any();
				Expect.Call(_parentToAlter.DayOffTemplates).Return(_dayOffTemplates).Repeat.Any();
			}

			using (_mockRep.Playback())
			{
				var target = (PreferenceRestrictionViewModel)RestrictionViewModel.CreateViewModel(_parentToAlter, preferenceDay);
				Assert.AreEqual(UserTexts.Resources.Preference, target.Description);
				Assert.AreEqual(shiftCategoryString, target.ShiftCategory);
				Assert.AreEqual(activityString, target.Activity);
				Assert.AreEqual(dayOffTemplateString, target.DayOff);
				Assert.IsTrue(target.HasDayOff, "HasDayOff must be set at creation");
				Assert.IsTrue(target.HasShiftCategory, "HasShiftCategory must be set at creation");
			}
		}

		[Test]
		public void VerifyPersonRestrictionIsSetWhenCreated()
		{
			var personRestriction = _mockRep.StrictMock<IScheduleDataRestriction>();
			IRotationRestriction rotRestriction = new RotationRestriction();
			using (_mockRep.Record())
			{
				Expect.Call(personRestriction.Restriction).Return(rotRestriction);
			}
			var target = (RotationRestrictionViewModel)RestrictionViewModel.CreateViewModel(null, personRestriction);
			Assert.AreEqual(personRestriction, target.PersistableScheduleData);
            Assert.IsTrue(target.BelongsToPart());
		}

		[Test]
		public void VerifyGeneratesReadOnlyRestrictionViewModelAndPropertiesAreSet()
		{
			const string shiftCategoryString = "ShiftCategory";
			const string dayOffTemplateString = "DayOffTemplate";
			
			IShiftCategory shiftCategory = new ShiftCategory(shiftCategoryString);
			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description(dayOffTemplateString));
			IPreferenceRestriction prefRestriction =
				new PreferenceRestriction
					{
						ShiftCategory = shiftCategory,
						DayOffTemplate = dayOffTemplate,
						//Activity = activity,
						StartTimeLimitation = new StartTimeLimitation(_startLimit, _startLimit),
						EndTimeLimitation = new EndTimeLimitation(_endLimit, _endLimit),
						WorkTimeLimitation = _workLimit
					};

			var target = (ReadOnlyRestrictionViewModel)RestrictionViewModel.CreateReadOnlyViewModel(prefRestriction);
			
			target.CommitChanges(); //Does nothing on readonly
			Assert.AreEqual(true, target.DayOff);
			target.DayOff = false;
			Assert.AreEqual(true, target.DayOff);
			Assert.AreEqual(UserTexts.Resources.Preference, target.Description);
			target.ShiftCategory = "empty setter";
			Assert.AreEqual("ShiftCategory", target.ShiftCategory);
			
			 var startLimitation = new StartTimeLimitation(_startLimit, _startLimit);
			var endLimitation = new EndTimeLimitation(_endLimit, _endLimit);
			var avRestriction =
				new StudentAvailabilityRestriction
					{
						StartTimeLimitation = startLimitation,
						EndTimeLimitation = endLimitation,
						WorkTimeLimitation = _workLimit
					};

			target = (ReadOnlyRestrictionViewModel)RestrictionViewModel.CreateReadOnlyViewModel(avRestriction);
			// has no parent yet
			Assert.AreEqual(false, target.DayOff);

			var availabilityRestrictions = new List<IStudentAvailabilityRestriction> { avRestriction };
			_studDayRestriction = new StudentAvailabilityDay(new Person(), new DateOnly(), availabilityRestrictions)
			                          {NotAvailable = true};

		    Assert.AreEqual(true, target.DayOff);
			Assert.AreEqual(UserTexts.Resources.StudentAvailability, target.Description);
			Assert.IsTrue(string.IsNullOrEmpty(target.ShiftCategory));
		}

        [Test]
		public void VerifyCommitChangesOnAvailableRestriction()
		{
			var parent = _mockRep.StrictMock<IRestrictionAltered>();
			var restriction = new StudentAvailabilityRestriction();
			var target = new AvailableRestrictionViewModel(restriction, parent);

			using (_mockRep.Record())
			{
				Expect.Call(parent.RestrictionIsAltered = true);
				Expect.Call(parent.RestrictionAltered);
			}
			using (_mockRep.Playback())
			{
				target.UpdateCommandModel.OnExecute(null, _models.CreateExecutedRoutedEventArgs()); //Fires the updateCommand
			}
		}

		[Test]
		public void VerifyUpdateCommand()
		{
			var parent = _mockRep.StrictMock<IRestrictionAltered>();
			var model = new AvailableRestrictionViewModel(new StudentAvailabilityRestriction(), parent);
			CommandModel command = model.UpdateCommandModel;
			Assert.AreEqual(UserTexts.Resources.Update, command.Text);

			using (_mockRep.Record())
			{
				Expect.Call(parent.RestrictionAltered);
				Expect.Call(parent.RestrictionIsAltered = true);//UpdateCommand should set the updateflag to true
			}
			_testRunner.RunInSTA(
				delegate
				{
					var element = new Grid(); //Used as ICommandTarget
					CreateCommandBinding.SetCommand(element, command);
					element.CommandBindings.Add(new CommandBinding(command.Command));
					Assert.IsTrue(command.Command.CanExecute(null, element));
					using (_mockRep.Playback())
					{
						command.Command.Execute(null, element);
					}

				});
		}

		[Test]
		public void VerifyCanDeleteRestrictionViewModel()
		{

			var fakeModel = _mockRep.StrictMock<IRestrictionViewModel>();

			using (_mockRep.Record())
			{
				Expect.Call(_parentToAlter.Activities).Return(null);
				Expect.Call(_parentToAlter.ShiftCategories).Return(null);
				Expect.Call(_parentToAlter.DayOffTemplates).Return(null);
				Expect.Call(() => _parentToAlter.RestrictionRemoved(fakeModel)).IgnoreArguments(); //TODO: use real model instead without Ignore
			}


			using (_mockRep.Playback())
			{
				var model = new PreferenceRestrictionViewModel(_preferenceRestriction, _parentToAlter);
				CanExecuteRoutedEventArgs arg = _models.CreateCanExecuteRoutedEventArgs();
				model.DeleteCommandModel.OnQueryEnabled(model, arg);
				Assert.IsTrue(arg.Handled);
				Assert.IsTrue(arg.CanExecute);
				Assert.AreEqual(UserTexts.Resources.Delete, model.DeleteCommandModel.Text);
				model.DeleteCommandModel.OnExecute(model, _models.CreateExecutedRoutedEventArgs());
			}

		}

		[Test]
		public void VerifyCanOnlyDeleteAvailabilityIfStudentAvailability()
		{
			var parent = _mockRep.StrictMock<IRestrictionAltered>();
			var restriction1 = _mockRep.StrictMock<StudentAvailabilityRestriction>();
			var restriction2 = _mockRep.StrictMock<IAvailabilityRestriction>();
			CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = _models.CreateCanExecuteRoutedEventArgs();


			using (_mockRep.Record())
			{
				Expect.Call(restriction1.StartTimeLimitation).Return(new StartTimeLimitation(TimeSpan.FromHours(8),
																							 TimeSpan.FromHours(15)));
				Expect.Call(restriction1.EndTimeLimitation).Return(new EndTimeLimitation(TimeSpan.FromHours(15),
																							TimeSpan.FromHours(20)));
				Expect.Call(restriction1.WorkTimeLimitation).Return(new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(10)));
				Expect.Call(restriction2.StartTimeLimitation).Return(new StartTimeLimitation(TimeSpan.FromHours(8),
																							TimeSpan.FromHours(15)));
				Expect.Call(restriction2.EndTimeLimitation).Return(new EndTimeLimitation(TimeSpan.FromHours(15),
																							TimeSpan.FromHours(20)));
				Expect.Call(restriction2.WorkTimeLimitation).Return(new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(10)));
				
				Expect.Call(restriction1.Parent).Return(_studDayRestriction).Repeat.Any();
				Expect.Call(restriction2.Parent ).Return(_studDayRestriction).Repeat.Any();
				Expect.Call(_studDayRestriction.NotAvailable).Return(false).Repeat.Any();
				
			}
			using (_mockRep.Playback())
			{
				var modelWithStudentAvailability = new AvailableRestrictionViewModel(restriction1,
																										parent);
				var modelWithAvailability = new AvailabilityRestrictionViewModel(
					restriction2);
				modelWithStudentAvailability.DeleteCommandModel.OnQueryEnabled(modelWithStudentAvailability, canExecuteRoutedEventArgs);
				Assert.IsTrue(canExecuteRoutedEventArgs.CanExecute);
				
				modelWithAvailability.DeleteCommandModel.OnQueryEnabled(modelWithAvailability,
																		   canExecuteRoutedEventArgs);
				Assert.IsFalse(canExecuteRoutedEventArgs.CanExecute);
			}
		}

		[Test]
		public void VerifyDeleteRotationViewModelsIsDisabled()
		{
			var restriction = _mockRep.StrictMock<IRotationRestriction>();

			CanExecuteRoutedEventArgs canExecuteRoutedEventArgs = _models.CreateCanExecuteRoutedEventArgs();


			using (_mockRep.Record())
			{
				Expect.Call(restriction.StartTimeLimitation).Return(new StartTimeLimitation(TimeSpan.FromHours(8),
																							 TimeSpan.FromHours(15)));
				Expect.Call(restriction.EndTimeLimitation).Return(new EndTimeLimitation(TimeSpan.FromHours(15),
																							TimeSpan.FromHours(20)));
				Expect.Call(restriction.WorkTimeLimitation).Return(new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(10)));

			}
			using (_mockRep.Playback())
			{
				var model = new RotationRestrictionViewModel(restriction);
				model.DeleteCommandModel.OnQueryEnabled(model, canExecuteRoutedEventArgs);
				Assert.IsFalse(canExecuteRoutedEventArgs.CanExecute);
			}
		}

		[Test]
		public void VerifyUpdateCommandForEvents()
		{
			bool changedFired = false;//this is for fxcop

			var model = new RestrictionViewModelForTest {ChangesCommited = false};
		    var validModel = _mockRep.StrictMock<ILimitationViewModel>();
			var inValidModel = _mockRep.StrictMock<ILimitationViewModel>();

			using (_mockRep.Record())
			{
				Expect.Call(validModel.Invalid).Return(false).Repeat.Any();
				Expect.Call(inValidModel.Invalid).Return(true).Repeat.Any();
			}
			using (_mockRep.Playback())
			{
				ICommand eventCommand = model.UpdateOnEventCommand;
				eventCommand.CanExecuteChanged += delegate { changedFired = true; };

				model.SetTimeLimitations(inValidModel, inValidModel, inValidModel);
				Assert.IsFalse(eventCommand.CanExecute(null), "Cannot execute if invalid state");
				eventCommand.Execute(null);
				Assert.IsFalse(model.ChangesCommited, "Does not execute if CanExeute is false, changes are not commited");

				model.SetTimeLimitations(validModel, validModel, validModel);
				Assert.IsTrue(eventCommand.CanExecute(null), "Can Execute if valid state");

				eventCommand.Execute(null);
				Assert.IsTrue(model.ChangesCommited);
				Assert.IsFalse(changedFired);
			}
		}

        [Test]
        public void ShouldKeepTrackIfRestrictionIsConnectedToScheduleDay()
        {
            var part = _mockRep.StrictMock<IScheduleDay>();
            Expect.Call(part.PersonRestrictionCollection()).Return(
                new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));

            var prefDay = new PreferenceDay(new Person(), new DateOnly(), (IPreferenceRestriction)_target.Restriction);
            Expect.Call(part.PersonRestrictionCollection()).Return(
                new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { prefDay }));

            
            _mockRep.ReplayAll();
            
            Assert.That(_target.BelongsToPart(), Is.False);
            _target.ScheduleDay = part;
            Assert.That(_target.BelongsToPart(), Is.False);

            Assert.That(_target.BelongsToPart(), Is.True);
            _mockRep.VerifyAll();
        }
        

        [Test]
        public void ShouldKeepTrackIfRestrictionIsConnectedToScheduleDayOnAvailable()
        {
            var startLimitation = new StartTimeLimitation(_startLimit, _startLimit);
			var endLimitation = new EndTimeLimitation(_endLimit, _endLimit);
			var avRestriction =
				new StudentAvailabilityRestriction
					{
						StartTimeLimitation = startLimitation,
						EndTimeLimitation = endLimitation,
						WorkTimeLimitation = _workLimit
					};

            var viewModel = new AvailableRestrictionViewModel(avRestriction, null);
            
            var part = _mockRep.StrictMock<IScheduleDay>();
            Expect.Call(part.PersonRestrictionCollection()).Return(
                new ReadOnlyCollection<IScheduleData>(new List<IScheduleData>()));
            var restrictions = new List<IStudentAvailabilityRestriction> {avRestriction};

            var studDay = new StudentAvailabilityDay(new Person(), new DateOnly(), restrictions);
            Expect.Call(part.PersonRestrictionCollection()).Return(
                new ReadOnlyCollection<IScheduleData>(new List<IScheduleData> { studDay }));


            _mockRep.ReplayAll();

            Assert.That(viewModel.BelongsToPart(), Is.False);
            viewModel.ScheduleDay = part;
            Assert.That(viewModel.BelongsToPart(), Is.False);

            Assert.That(viewModel.BelongsToPart(), Is.True);
            _mockRep.VerifyAll();
        }

		private class RestrictionViewModelForTest : RestrictionViewModel
		{
			public bool ChangesCommited { get; set; }

			public void SetTimeLimitations(ILimitationViewModel start, ILimitationViewModel end, ILimitationViewModel work)
			{
				StartTimeLimits = start;
				EndTimeLimits = end;
				WorkTimeLimits = work;
			}

			public override string Description
			{
				get { return "description"; }
			}

			public override void CommitChanges()
			{
				ChangesCommited = true;
			}
		}
	}
}