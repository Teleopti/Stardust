using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.WinCodeTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class WorkflowControlSetPresenterTest : IIsolateSystem
	{
		public FakeWorkflowControlSetRepository WorkflowControlSetRepository;
		public FakeSkillRepository SkillRepository;
		public FakeUnitOfWorkFactory UnitOfWorkFactory;
		public FakeRepositoryFactory RepositoryFactory;
		public FakeAbsenceRepository AbsenceRepository;
		public FakeToggleManager ToggleManager;
		public FakeActivityRepository ActivityRepository;

		private WorkflowControlSetPresenter _target;
		private WorkflowControlSetView _view;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
		}

		[TearDown]
		public void Clean()
		{
			_view?.Dispose();
		}

		[Test]
		public void VerifyInitializeWithExistingModels()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			WorkflowControlSetRepository.Add(workflowControlSet);

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			absence.Requestable = true;
			AbsenceRepository.Add(absence);

			initialize();

			Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
			Assert.AreEqual(1, _target.RequestableAbsenceCollection.Count);
			Assert.IsTrue(_target.DoRequestableAbsencesExist);
		}

		[Test]
		public void VerifyInitializeWithNoExistingModels()
		{
			initialize();
			_view.DisableAllButAdd();

			Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
			Assert.IsFalse(_target.DoRequestableAbsencesExist);
		}

		[Test]
		public void VerifyItemsToBeDeletedAreHidden()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();
			_view.DisableAllButAdd();

			Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.DeleteWorkflowControlSet();
			Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
		}

		[Test]
		public void VerifyNothingHappensWhenDeletingWithoutSelectedItem()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());

			_target.SelectedModel = null;
			_target.DeleteWorkflowControlSet();
			Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());

			var item = _target.WorkflowControlSetModelCollection.First();
			Assert.IsTrue(item.Id.HasValue);
			Assert.IsFalse(item.ToBeDeleted);
			Assert.IsFalse(item.IsNew);
		}

		[Test]
		public void VerifyAddNew()
		{
			initialize();

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			_view.FillWorkloadControlSetCombo(_target.WorkflowControlSetModelCollection, "Name");
			_view.SelectWorkflowControlSet(new WorkflowControlSetModel(workflowControlSet));

			Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
			_target.AddWorkflowControlSet();
			Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());

			var item = _target.WorkflowControlSetModelCollection.First();
			Assert.IsFalse(item.Id.HasValue);
			Assert.IsTrue(item.IsNew);
			Assert.IsFalse(item.ToBeDeleted);
			Assert.AreEqual(Resources.NewWorkflowControlSet, item.Name);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
		public void VerifySaveChanges()
		{
			IWorkflowControlSet workflowControlSet1 = new WorkflowControlSet("to delete");
			workflowControlSet1.SetId(Guid.NewGuid());
			IWorkflowControlSet workflowControlSet2 = new WorkflowControlSet("to edit");
			workflowControlSet2.SetId(Guid.NewGuid());
			IWorkflowControlSet workflowControlSet5 = workflowControlSet2.EntityClone();

			WorkflowControlSetRepository.Add(workflowControlSet1);
			WorkflowControlSetRepository.Add(workflowControlSet2);

			initialize();

			_target.AddWorkflowControlSet();
			_target.SelectedModel.Name = "new one";

			_target.AddWorkflowControlSet();
			_target.SelectedModel.Name = "new to delete";

			Assert.AreEqual(4, _target.WorkflowControlSetModelCollection.Count());
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.ElementAt(1));
			_target.SelectedModel.Name = "new name";

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.ElementAt(0));
			_target.DeleteWorkflowControlSet();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.Last());
			_target.DeleteWorkflowControlSet();

			Assert.AreEqual(2, _target.WorkflowControlSetModelCollection.Count());
			_target.SaveChanges();
			Assert.AreEqual(workflowControlSet5,
				_target.WorkflowControlSetModelCollection.ElementAt(0).OriginalDomainEntity);
		}

		[Test]
		public void VerifySelectedModel()
		{
			initialize();

			Assert.IsNull(_target.SelectedModel);

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			var workflowControlSetModel = new WorkflowControlSetModel(workflowControlSet);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);

			Assert.IsNotNull(_target.SelectedModel);
			Assert.AreEqual(workflowControlSetModel.Name, _target.SelectedModel.Name);
		}

		[Test]
		public void VerifyCanChangePeriodType()
		{
			initialize();

			var absence = AbsenceFactory.CreateAbsence("Holiday");
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			var workflowControlSetModel = new WorkflowControlSetModel(workflowControlSet);
			workflowControlSetModel.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod
			{
				Absence = absence
			});
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);

			var currentModels = workflowControlSetModel.AbsenceRequestPeriodModels;
			var chosenAbsenceRequestOpenPeriod = currentModels[0];
			_target.SetPeriodType(chosenAbsenceRequestOpenPeriod,
				new AbsenceRequestPeriodTypeModel(new AbsenceRequestOpenDatePeriod(), "From-To"));
			Assert.AreEqual(1, workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods.Count);
			Assert.IsTrue(
				workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods[0] is AbsenceRequestOpenDatePeriod);
			Assert.AreSame(chosenAbsenceRequestOpenPeriod, currentModels[0]);
			Assert.AreSame(chosenAbsenceRequestOpenPeriod.DomainEntity,
				workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods[0]);
			Assert.AreEqual(absence, chosenAbsenceRequestOpenPeriod.Absence);
		}

		[Test]
		public void VerifyAddNewDateOpenPeriod()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateRequestableAbsence("Holiday", "Ho", Color.Red));

			initialize();

			_view.RefreshOpenPeriodsGrid();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			_target.AddOpenDatePeriod();
			Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			IAbsenceRequestOpenPeriod absenceRequestOpenPeriod =
				_target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity;
			Assert.IsNotNull(absenceRequestOpenPeriod.Absence);
			Assert.AreEqual(_target.RequestableAbsenceCollection[0], absenceRequestOpenPeriod.Absence);
			Assert.IsNull(absenceRequestOpenPeriod.Absence.Tracker);
			Assert.AreEqual(new AbsenceRequestNoneValidator(), absenceRequestOpenPeriod.PersonAccountValidator);
		}

		[Test]
		public void VerifyAddNewRollingOpenPeriod()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			IAbsence absenceWithTracker = AbsenceFactory.CreateRequestableAbsence("Holiday", "Ho", Color.Red);
			absenceWithTracker.Tracker = Tracker.CreateDayTracker();
			AbsenceRepository.Add(absenceWithTracker);

			initialize();

			_view.RefreshOpenPeriodsGrid();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			_target.AddOpenRollingPeriod();
			Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);

			IAbsenceRequestOpenPeriod absenceRequestOpenPeriod = _target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity;

			Assert.IsNotNull(absenceRequestOpenPeriod.Absence);
			Assert.AreEqual(_target.RequestableAbsenceCollection[0], absenceRequestOpenPeriod.Absence);
			Assert.IsNotNull(absenceRequestOpenPeriod.Absence.Tracker);
			Assert.AreEqual(new PersonAccountBalanceValidator(), absenceRequestOpenPeriod.PersonAccountValidator);
		}

		[Test]
		public void VerifyDeleteAbsenceRequestPeriodCanDelete()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			workflowControlSet.AddOpenAbsenceRequestPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);

			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new FixConfirmResultWorkflowControlSetView(ToggleManager, _target, true);
			_target.Initialize();

			_view.SetOpenPeriodsGridRowCount(0);
			_view.RefreshOpenPeriodsGrid();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			_target.DeleteAbsenceRequestPeriod(
				new ReadOnlyCollection<AbsenceRequestPeriodModel>(_target.SelectedModel.AbsenceRequestPeriodModels));
			Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyCanCancelDeleteAbsenceRequestPeriod()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			workflowControlSet.AddOpenAbsenceRequestPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);

			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new FixConfirmResultWorkflowControlSetView(ToggleManager, _target, false);
			_target.Initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			_target.DeleteAbsenceRequestPeriod(
				new ReadOnlyCollection<AbsenceRequestPeriodModel>(_target.SelectedModel.AbsenceRequestPeriodModels));
			Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyCanGetDefaultPeriod()
		{
			initialize();

			DateTime startDate = DateTime.Today;
			startDate = DateHelper.GetFirstDateInMonth(startDate, CultureInfo.CurrentCulture);
			DateTime endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 3).AddDays(-1);

			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), _target.ProjectionPeriod);

			_target.NextProjectionPeriod();

			startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 1);
			endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, 1);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
				_target.ProjectionPeriod);

			_target.PreviousProjectionPeriod();

			startDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, -1);
			endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(endDate, -1);
			Assert.AreEqual(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)),
				_target.ProjectionPeriod);
		}

		[Test]
		public void VerifyDeleteAbsenceRequestPeriodNothingToDelete()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
			_target.DeleteAbsenceRequestPeriod(_target.SelectedModel.AbsenceRequestPeriodModels);
			Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
		}

		[Test]
		public void VerifyMove()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			var openDatePeriod =
				(AbsenceRequestOpenDatePeriod)WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item;
			((IEntity)openDatePeriod).SetId(Guid.NewGuid());
			var openRollingPeriod =
				(AbsenceRequestOpenRollingPeriod)WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item;
			((IEntity)openRollingPeriod).SetId(Guid.NewGuid());
			workflowControlSet.AddOpenAbsenceRequestPeriod(openDatePeriod);
			workflowControlSet.AddOpenAbsenceRequestPeriod(openRollingPeriod);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(openDatePeriod, _target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.AbsenceRequestPeriodModels[1].DomainEntity);
			_target.MoveUp(_target.SelectedModel.AbsenceRequestPeriodModels[1]);
			Assert.AreEqual(openDatePeriod, _target.SelectedModel.AbsenceRequestPeriodModels[1].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity);
			_target.MoveDown(_target.SelectedModel.AbsenceRequestPeriodModels[0]);
			Assert.AreEqual(openDatePeriod, _target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity);
			Assert.AreEqual(openRollingPeriod, _target.SelectedModel.AbsenceRequestPeriodModels[1].DomainEntity);
		}

		[Test]
		public void VerifyActivityListContainsNullValueFirstAfterFillAllowedPreferenceActivityCombo()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			IActivity firstActivity = ActivityFactory.CreateActivity("Lunch");
			IActivity secondActivity = ActivityFactory.CreateActivity("Administration");

			ActivityRepository.Add(firstActivity);
			ActivityRepository.Add(secondActivity);

			initialize();

			Assert.AreEqual(3, _target.ActivityCollection.Count);
			Assert.IsNull(_target.ActivityCollection[0]);
		}

		[Test]
		public void VerifySetAllowedPreferenceActivity()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			IActivity activity = ActivityFactory.CreateActivity("Lunch");
			Assert.IsNull(_target.SelectedModel.AllowedPreferenceActivity);
			_target.SetSelectedAllowedPreferenceActivity(activity);
			Assert.AreEqual(activity, _target.SelectedModel.AllowedPreferenceActivity);
		}

		[Test]
		public void VerifySetWriteProtectedDays()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetWriteProtectedDays(null);
			Assert.AreEqual(null, _target.SelectedModel.WriteProtection);
			_target.SetWriteProtectedDays(1);
			Assert.AreEqual(1, _target.SelectedModel.WriteProtection);
		}

		[Test]
		public void VerifySetCalendarCultureInfo()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetCulture();
		}

		[Test]
		public void VerifySetPublishedToDateWhenDateNotNull()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			DateTime publishedToDate = DateTime.Today;

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetPublishedToDate(publishedToDate);

			Assert.AreEqual(publishedToDate, _target.SelectedModel.SchedulePublishedToDate);
			Assert.AreEqual(publishedToDate, _target.SelectedModel.DomainEntity.SchedulePublishedToDate);
		}

		[Test]
		public void VerifySetPublishedToDateWhenDateIsNull()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetPublishedToDate(null);

			Assert.IsNull(_target.SelectedModel.SchedulePublishedToDate);
			Assert.IsNull(_target.SelectedModel.DomainEntity.SchedulePublishedToDate);
		}

		[Test]
		public void VerifyDefaultValuesForPreferencePeriodAreSetWhenCreatingNew()
		{
			initialize();

			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var preferencePeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

			_target.AddWorkflowControlSet();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			var workflowControlSetModel = new WorkflowControlSetModel(workflowControlSet);
			_target.DefaultPreferencePeriods(workflowControlSetModel, new DateTime(2010, 3, 24));

			Assert.AreEqual(workflowControlSetModel.PreferencePeriod, preferencePeriod);
			Assert.AreEqual(workflowControlSetModel.PreferenceInputPeriod, insertPeriod);
		}

		[Test]
		public void VerifyDefaultValuesForStudentAvailabilityPeriodAreSetWhenCreatingNew()
		{
			initialize();

			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var studentAvailabilityPeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

			_target.AddWorkflowControlSet();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
			var workflowControlSetModel = new WorkflowControlSetModel(workflowControlSet);
			_target.DefaultStudentAvailabilityPeriods(workflowControlSetModel, new DateTime(2010, 3, 24));

			Assert.AreEqual(workflowControlSetModel.StudentAvailabilityPeriod, studentAvailabilityPeriod);
			Assert.AreEqual(workflowControlSetModel.StudentAvailabilityInputPeriod, insertPeriod);
		}

		[Test]
		public void VerifySetPreferencePeriods()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var preferencePeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetPreferencePeriod(preferencePeriod);
			_target.SetPreferenceInputPeriod(insertPeriod);
			Assert.AreEqual(insertPeriod, _target.SelectedModel.DomainEntity.PreferenceInputPeriod);
			Assert.AreEqual(preferencePeriod, _target.SelectedModel.DomainEntity.PreferencePeriod);
		}

		[Test]
		public void VerifySetStudentAvailabilityPeriods()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var studentAvailabilityPeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetStudentAvailabilityPeriod(studentAvailabilityPeriod);
			_target.SetStudentAvailabilityInputPeriod(insertPeriod);
			Assert.AreEqual(insertPeriod, _target.SelectedModel.DomainEntity.StudentAvailabilityInputPeriod);
			Assert.AreEqual(studentAvailabilityPeriod, _target.SelectedModel.DomainEntity.StudentAvailabilityPeriod);
		}

		[Test]
		public void VerifyBasicVisualizerWriteProtectionPeriods()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateAbsence("Holiday"));

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var expected = new DateOnlyPeriod(DateOnly.MinValue, DateOnly.MinValue);
			var result = _target.BasicVisualizerWriteProtectionPeriods(new DateOnly(2010, 6, 1));
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(expected, result[0]);

			_target.SetWriteProtectedDays(10);
			expected = new DateOnlyPeriod(DateOnly.MinValue, new DateOnly(2010, 6, 1).AddDays(-10));
			result = _target.BasicVisualizerWriteProtectionPeriods(new DateOnly(2010, 6, 1));
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(expected, result[0]);
		}

		[Test]
		public void VerifyBasicVisualizerPreferencePeriods()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateAbsence("Holiday"));

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var expected = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
			_target.SetPreferencePeriod(expected);

			var result = _target.BasicVisualizerPreferencePeriods();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(expected, result[0]);
		}

		[Test]
		public void VerifyBasicVisualizerStudentAvailabilityPeriods()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateAbsence("Holiday"));

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var expected = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
			_target.SetStudentAvailabilityPeriod(expected);

			var result = _target.BasicVisualizerStudentAvailabilityPeriods();
			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(expected, result[0]);
		}

		[Test]
		public void VerifyBasicVisualizerPublishedPeriodsWithNull()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateAbsence("Holiday"));

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var expected1 = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
			_target.SetPreferencePeriod(expected1);
			_target.SetStudentAvailabilityPeriod(expected1);
			_target.SetPublishedToDate(null);

			var expected2 = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime),
				new DateOnly(DateHelper.MinSmallDateTime));

			var result = _target.BasicVisualizerPublishedPeriods();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(expected2, result[0]);
			Assert.AreEqual(expected1, result[1]);
		}

		[Test]
		public void VerifyBasicVisualizerPublishedPeriodsWithNotNull()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			AbsenceRepository.Add(AbsenceFactory.CreateAbsence("Holiday"));

			initialize();

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			var expected1 = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
			_target.SetPreferencePeriod(expected1);
			_target.SetStudentAvailabilityPeriod(expected1);
			_target.SetPublishedToDate(new DateTime(2010, 5, 31));

			var expected2 = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(2010, 5, 31));

			IList<DateOnlyPeriod> result = _target.BasicVisualizerPublishedPeriods();
			Assert.AreEqual(2, result.Count);
			Assert.AreEqual(expected2, result[0]);
			Assert.AreEqual(expected1, result[1]);
		}

		[Test]
		public void VerifySetShiftTradeOpenPeriodDaysForwardMinimum()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var tradePeriodDays = new MinMax<int>(20, 100);

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetWriteProtectedDays(null);
			Assert.AreEqual(null, _target.SelectedModel.WriteProtection);
			_target.SetOpenShiftTradePeriod(tradePeriodDays);
			Assert.AreEqual(tradePeriodDays, _target.SelectedModel.ShiftTradeOpenPeriodDays);
		}

		[Test]
		public void VerifyDefaultShiftTradePeriodDaysWhenCreatingNew()
		{
			initialize();

			var periodDays = new MinMax<int>(2, 17);

			_target.AddWorkflowControlSet();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.DefaultShiftTradePeriodDays(periodDays);

			Assert.AreEqual(periodDays, _target.SelectedModel.ShiftTradeOpenPeriodDays);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
		public void VerifyShiftTradeTargetTimeFlexibilityIsSetWhenCreatingNew()
		{
			initialize();

			var flexibility = new TimeSpan(0, 0, 0);

			_target.AddWorkflowControlSet();
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

			Assert.AreEqual(flexibility, _target.SelectedModel.ShiftTradeTargetTimeFlexibility);
		}

		[Test]
		public void VerifySetShiftTradeTargetTimeFlexibility()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var flexibility = new TimeSpan(0, 34, 0);

			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			_target.SetShiftTradeTargetTimeFlexibility(flexibility);
			Assert.AreEqual(flexibility, _target.SelectedModel.ShiftTradeTargetTimeFlexibility);
		}

		[Test]
		public void VerifyAddRemovePreferenceDayOff()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("sglk"));
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
			_target.AddAllowedPreferenceDayOff(dayOffTemplate);
			Assert.AreEqual(1, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
			_target.RemoveAllowedPreferenceDayOff(dayOffTemplate);
			Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
		}

		[Test]
		public void VerifyAddRemovePreferenceShiftCategory()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			IShiftCategory shiftCategory = new ShiftCategory("cat");
			_target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
			Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
			_target.AddAllowedPreferenceShiftCategory(shiftCategory);
			Assert.AreEqual(1, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
			_target.RemoveAllowedPreferenceShiftCategory(shiftCategory);
			Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
		}

		[Test]
		public void VerifyCanGetListsOffDayOffsAndShiftCategories()
		{
			initialize();

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			Assert.IsNotNull(_target.ShiftCategoriesCollection());
			Assert.IsNotNull(_target.DayOffCollection());
		}
		
		[Test]
		public void VerifyIsUsingPrimaySkill()
		{
			var skill = new Skill("test");
			skill.SetCascadingIndex(1);
			SkillRepository.Add(skill);

			initialize();

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			Assert.IsTrue(_target.IsUsingPrimarySkill());
		}

		[Test]
		public void VerifyIsNotUsingPrimaySkill()
		{
			var skill = new Skill("test");
			SkillRepository.Add(skill);

			initialize();

			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			Assert.IsFalse(_target.IsUsingPrimarySkill());
		}

		[Test]
		public void ShouldUpdateModelOnRadioButtonFairnessCheckChange()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var workflowControlSetModel = _target.WorkflowControlSetModelCollection.ElementAt(0);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);
			_target.OnRadioButtonAdvFairnessEqualCheckChanged(true);
			Assert.AreEqual(FairnessType.EqualNumberOfShiftCategory, workflowControlSetModel.GetFairnessType());
			_target.OnRadioButtonAdvSeniorityCheckedChanged(true);
			Assert.AreEqual(FairnessType.Seniority, workflowControlSetModel.GetFairnessType());
		}

		[Test]
		public void ShouldUpdateModelOnOvertimeConfigurationCheckChanged()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var workflowControlSetModel = _target.WorkflowControlSetModelCollection.ElementAt(0);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);
			_target.SetOvertimeProbability(true);

			Assert.AreEqual(true, workflowControlSetModel.IsOvertimeProbabilityEnabled);
		}

		[Test]
		public void ShouldAddSkillToMatchList()
		{
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var workflowControlSetModel = _target.WorkflowControlSetModelCollection.ElementAt(0);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);

			var skill = SkillFactory.CreateSkill("5 finger death punch").WithId();
			_target.AddSkillToMatchList(skill);
			Assert.IsTrue(workflowControlSetModel.MustMatchSkills.Contains(skill));
		}

		[Test]
		public void ShouldRemoveSkillFromMatchList()
		{
			var skill = SkillFactory.CreateSkill("5 finger death punch").WithId();
			var workflowControlSet = new WorkflowControlSet("My Workflow Control Set").WithId();
			workflowControlSet.AddSkillToMatchList(skill);
			WorkflowControlSetRepository.Add(workflowControlSet);

			initialize();

			var workflowControlSetModel = _target.WorkflowControlSetModelCollection.ElementAt(0);
			_target.SetSelectedWorkflowControlSetModel(workflowControlSetModel);

			_target.RemoveSkillFromMatchList(skill);
			Assert.IsFalse(workflowControlSetModel.MustMatchSkills.Contains(skill));
		}

		private void initialize()
		{
			_target = new WorkflowControlSetPresenter(null, UnitOfWorkFactory, RepositoryFactory, ToggleManager);
			_view = new WorkflowControlSetView(ToggleManager, _target);
			_target.Initialize();
		}
	}
}