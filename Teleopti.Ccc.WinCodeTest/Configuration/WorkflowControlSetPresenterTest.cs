using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.Tracking;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WinCode.Common.Configuration;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class WorkflowControlSetPresenterTest
    {
        private WorkflowControlSetPresenter _target;
        private MockRepository _mocks;
        private IWorkflowControlSetRepository _repository;
        private IWorkflowControlSetView _view;
        private IRepositoryFactory _repositoryFactory;
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IUnitOfWork _unitOfWork;
        private IList<IAbsence> _absenceList;
        private IList<IActivity> _activityList;
        private IList<IShiftCategory> _categories;
        private IList<IDayOffTemplate> _dayOffTemplates;
        private WorkflowControlSetModel _workflowControlSetModel;
        private IWorkflowControlSet _workflowControlSet;
        private readonly CultureInfo _culture = TeleoptiPrincipal.Current.Regional.Culture;
        private List<ISkill> _skillList;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _repository = _mocks.StrictMock<IWorkflowControlSetRepository>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
            _unitOfWork = _mocks.StrictMock<IUnitOfWork>();
            _view = _mocks.StrictMock<IWorkflowControlSetView>();
            _absenceList = new List<IAbsence>();
            _activityList = new List<IActivity>();
            _categories = new List<IShiftCategory>();
            _dayOffTemplates = new List<IDayOffTemplate>();
            _skillList = new List<ISkill>();
            _workflowControlSet = new WorkflowControlSet("My Workflow Control Set");
            _workflowControlSet.SetId(Guid.NewGuid());
            _workflowControlSetModel = new WorkflowControlSetModel(_workflowControlSet);

            _target = new WorkflowControlSetPresenter(_view, _unitOfWorkFactory, _repositoryFactory);
        }

        [Test]
        public void VerifyInitializeWithExistingModels()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
                Assert.AreEqual(1, _target.RequestableAbsenceCollection.Count);
                Assert.IsTrue(_target.DoRequestableAbsencesExist);
            }
        }

        [Test]
        public void VerifyInitializeWithNoExistingModels()
        {
            using (_mocks.Record())
            {
                ExpectInitialize(new List<IWorkflowControlSet>());
                _view.DisableAllButAdd();
                LastCall.IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
                Assert.IsFalse(_target.DoRequestableAbsencesExist);
            }
        }

        [Test]
        public void VerifyItemsToBeDeletedAreHidden()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                _view.DisableAllButAdd();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.DeleteWorkflowControlSet();
                Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
            }
        }

        [Test]
        public void VerifyNothingHappensWhenDeletingWithoutSelectedItem()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
                _target.DeleteWorkflowControlSet(); //Not explicitly selected, which makes the function return without doing anything
                Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
                var item = _target.WorkflowControlSetModelCollection.First();
                Assert.IsTrue(item.Id.HasValue);
                Assert.IsFalse(item.ToBeDeleted);
                Assert.IsFalse(item.IsNew);
            }
        }

        [Test]
        public void VerifyAddNew()
        {
            using (_mocks.Record())
            {
                _view.FillWorkloadControlSetCombo(_target.WorkflowControlSetModelCollection, "Name");
                LastCall.IgnoreArguments();
                _view.SelectWorkflowControlSet(_workflowControlSetModel);
                LastCall.IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(0, _target.WorkflowControlSetModelCollection.Count());
                _target.AddWorkflowControlSet();
                Assert.AreEqual(1, _target.WorkflowControlSetModelCollection.Count());
                var item = _target.WorkflowControlSetModelCollection.First();
                Assert.IsFalse(item.Id.HasValue);
                Assert.IsTrue(item.IsNew);
                Assert.IsFalse(item.ToBeDeleted);
                Assert.AreEqual(Resources.NewWorkflowControlSet, item.Name);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), Test]
        public void VerifySaveChanges()
        {
            IWorkflowControlSet workflowControlSet1 = new WorkflowControlSet("to delete");
            workflowControlSet1.SetId(Guid.NewGuid());
            IWorkflowControlSet workflowControlSet2 = new WorkflowControlSet("to edit");
            workflowControlSet2.SetId(Guid.NewGuid());
            IWorkflowControlSet workflowControlSet3 = new WorkflowControlSet("new one");
            IWorkflowControlSet workflowControlSet4 = new WorkflowControlSet("new to delete");
            IWorkflowControlSet workflowControlSet5 = workflowControlSet2.EntityClone();
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet>
                                                                  {
                                                                      workflowControlSet1,
                                                                      workflowControlSet2,
                                                                      workflowControlSet3,
                                                                      workflowControlSet4
                                                                  };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                Expect.Call(() => _unitOfWork.Dispose());
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_unitOfWork);
                Expect.Call(_repositoryFactory.CreateWorkflowControlSetRepository(_unitOfWork)).Return(_repository);
                Expect.Call(() => _repository.Remove(workflowControlSet1));
                Expect.Call(() => _repository.Add(workflowControlSet3)).IgnoreArguments();
                Expect.Call(() => _unitOfWork.Reassociate(null)).IgnoreArguments();
                Expect.Call(_unitOfWork.Merge(workflowControlSet2)).Return(workflowControlSet5);
                Expect.Call(_unitOfWork.PersistAll()).Return(new List<IRootChangeInfo>());
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.AreEqual(4, _target.WorkflowControlSetModelCollection.Count());
                _target.WorkflowControlSetModelCollection.ElementAt(1).Name = "new name";
                _target.WorkflowControlSetModelCollection.ElementAt(3).ToBeDeleted = true;
                _target.WorkflowControlSetModelCollection.ElementAt(0).ToBeDeleted = true;
                Assert.AreEqual(2, _target.WorkflowControlSetModelCollection.Count());
                _target.SaveChanges();
                Assert.AreSame(workflowControlSet5,
                               _target.WorkflowControlSetModelCollection.ElementAt(0).OriginalDomainEntity);
            }
        }

        [Test]
        public void VerifySelectedModel()
        {
            ExpectSetSelectedWorkflowControlSetModel();

            Assert.IsNull(_target.SelectedModel);

            _mocks.ReplayAll();
            _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);
            Assert.IsNotNull(_target.SelectedModel);
            Assert.AreEqual(_workflowControlSetModel.Name, _target.SelectedModel.Name);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanChangePeriodType()
        {
            IAbsence absence = AbsenceFactory.CreateAbsence("Holiday");
            _workflowControlSetModel.DomainEntity.AddOpenAbsenceRequestPeriod(new AbsenceRequestOpenRollingPeriod { Absence = absence });
            _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

            var currentModels = _workflowControlSetModel.AbsenceRequestPeriodModels;
            var chosenAbsenceRequestOpenPeriod = currentModels[0];
            _target.SetPeriodType(chosenAbsenceRequestOpenPeriod,
                                  new AbsenceRequestPeriodTypeModel(new AbsenceRequestOpenDatePeriod(), "From-To"));
            Assert.AreEqual(1, _workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods.Count);
            Assert.IsTrue(_workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods[0] is AbsenceRequestOpenDatePeriod);
            Assert.AreSame(chosenAbsenceRequestOpenPeriod, currentModels[0]);
            Assert.AreSame(chosenAbsenceRequestOpenPeriod.DomainEntity, _workflowControlSetModel.DomainEntity.AbsenceRequestOpenPeriods[0]);
            Assert.AreEqual(absence, chosenAbsenceRequestOpenPeriod.Absence);
        }

        [Test]
        public void VerifyAddNewDateOpenPeriod()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateRequestableAbsence("Holiday", "Ho", Color.Red));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetOpenPeriodsGridRowCount(_workflowControlSetModel.AbsenceRequestPeriodModels.Count)).IgnoreArguments();
                _view.RefreshOpenPeriodsGrid();
            }

            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

                Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
                _target.AddOpenDatePeriod();
                Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
                IAbsenceRequestOpenPeriod absenceRequestOpenPeriod = _target.SelectedModel.AbsenceRequestPeriodModels[0].DomainEntity;
                Assert.IsNotNull(absenceRequestOpenPeriod.Absence);
                Assert.AreEqual(_target.RequestableAbsenceCollection[0], absenceRequestOpenPeriod.Absence);
                Assert.IsNull(absenceRequestOpenPeriod.Absence.Tracker);
                Assert.AreEqual(new AbsenceRequestNoneValidator(), absenceRequestOpenPeriod.PersonAccountValidator);
            }
        }

        [Test]
        public void VerifyAddNewRollingOpenPeriod()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            IAbsence absenceWithTracker = AbsenceFactory.CreateRequestableAbsence("Holiday", "Ho", Color.Red);
            absenceWithTracker.Tracker = Tracker.CreateDayTracker();
            _absenceList.Add(absenceWithTracker);

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetOpenPeriodsGridRowCount(_workflowControlSetModel.AbsenceRequestPeriodModels.Count)).IgnoreArguments();
                _view.RefreshOpenPeriodsGrid();
            }

            using (_mocks.Playback())
            {
                _target.Initialize();
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
        }

        [Test]
        public void VerifyDeleteAbsenceRequestPeriodCanDelete()
        {
            _workflowControlSet.AddOpenAbsenceRequestPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(_view.ConfirmDeleteOfAbsenceRequestPeriod()).Return(true);
                _view.SetOpenPeriodsGridRowCount(0);
                _view.RefreshOpenPeriodsGrid();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

                Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
                _target.DeleteAbsenceRequestPeriod(
                    new ReadOnlyCollection<AbsenceRequestPeriodModel>(_target.SelectedModel.AbsenceRequestPeriodModels));
                Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
            }
        }

        [Test]
        public void VerifyCanCancelDeleteAbsenceRequestPeriod()
        {
            _workflowControlSet.AddOpenAbsenceRequestPeriod(WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item);
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(_view.ConfirmDeleteOfAbsenceRequestPeriod()).Return(false);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

                Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
                _target.DeleteAbsenceRequestPeriod(
                    new ReadOnlyCollection<AbsenceRequestPeriodModel>(_target.SelectedModel.AbsenceRequestPeriodModels));
                Assert.AreEqual(1, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
            }
        }

        [Test]
        public void VerifyCanGetDefaultPeriod()
        {
            DateTime startDate = DateTime.Today;
            startDate = DateHelper.GetFirstDateInMonth(startDate, CultureInfo.CurrentCulture);
            DateTime endDate = CultureInfo.CurrentCulture.Calendar.AddMonths(startDate, 3).AddDays(-1);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.RefreshOpenPeriodsGrid()).Repeat.Twice();
            }
            using (_mocks.Playback())
            {
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
        }

        [Test]
        public void VerifyDeleteAbsenceRequestPeriodNothingToDelete()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());

                Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
                _target.DeleteAbsenceRequestPeriod(_target.SelectedModel.AbsenceRequestPeriodModels);
                Assert.AreEqual(0, _target.SelectedModel.AbsenceRequestPeriodModels.Count);
            }
        }

        [Test]
        public void VerifyMove()
        {
            var openDatePeriod =
                (AbsenceRequestOpenDatePeriod)WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[0].Item;
            ((IEntity)openDatePeriod).SetId(Guid.NewGuid());
            var openRollingPeriod =
                (AbsenceRequestOpenRollingPeriod)WorkflowControlSetModel.DefaultAbsenceRequestPeriodAdapters[1].Item;
            ((IEntity)openRollingPeriod).SetId(Guid.NewGuid());
            _workflowControlSet.AddOpenAbsenceRequestPeriod(openDatePeriod);
            _workflowControlSet.AddOpenAbsenceRequestPeriod(openRollingPeriod);
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetOpenPeriodsGridRowCount(2)).IgnoreArguments().Repeat.Twice();
                Expect.Call(() => _view.RefreshOpenPeriodsGrid()).Repeat.Twice();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
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
        }
		
        [Test]
        public void VerifyActivityListContainsNullValueFirstAfterFillAllowedPreferenceActivityCombo()
        {
            IActivity firstActivity = ActivityFactory.CreateActivity("Lunch");
            IActivity secondActivity = ActivityFactory.CreateActivity("Administration");

            _activityList.Add(firstActivity);
            _activityList.Add(secondActivity);

            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
            }
            Assert.AreEqual(3, _target.ActivityCollection.Count);
            Assert.IsNull(_target.ActivityCollection[0]);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetAllowedPreferenceActivity()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
            }
            IActivity activity = ActivityFactory.CreateActivity("Lunch");
            Assert.IsNull(_target.SelectedModel.AllowedPreferenceActivity);
            _target.SetSelectedAllowedPreferenceActivity(activity);
            Assert.AreEqual(activity, _target.SelectedModel.AllowedPreferenceActivity);
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetWriteProtectedDays()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetWriteProtectedDays(null);
                Assert.AreEqual(null, _target.SelectedModel.WriteProtection);
                _target.SetWriteProtectedDays(1);
                Assert.AreEqual(1, _target.SelectedModel.WriteProtection);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetCalendarCultureInfo()
        {
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetCalendarCultureInfo(cultureInfo));
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetCulture();
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetPublishedToDateWhenDateNotNull()
        {
            DateTime publishedToDate = DateTime.Today;
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetPublishedToDate(publishedToDate);
            }
            _mocks.VerifyAll();

            Assert.AreEqual(publishedToDate, _target.SelectedModel.SchedulePublishedToDate);
            Assert.AreEqual(publishedToDate, _target.SelectedModel.DomainEntity.SchedulePublishedToDate);
        }

        [Test]
        public void VerifySetPublishedToDateWhenDateIsNull()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetPublishedToDate(null);
            }
            _mocks.VerifyAll();

            Assert.IsNull(_target.SelectedModel.SchedulePublishedToDate);
            Assert.IsNull(_target.SelectedModel.DomainEntity.SchedulePublishedToDate);
        }

        [Test]
        public void VerifyDefaultValuesForPreferencePeriodAreSetWhenCreatingNew()
        {
            var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
            var preferencePeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

            using (_mocks.Record())
            {
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetPreferencePeriods(insertPeriod, preferencePeriod));
                Expect.Call(() => _view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
                Expect.Call(() => _view.SelectWorkflowControlSet(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.AddWorkflowControlSet();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.DefaultPreferencePeriods(_workflowControlSetModel, new DateTime(2010, 3, 24));
            }
            _mocks.VerifyAll();
        }
        
        [Test]
        public void VerifyDefaultValuesForStudentAvailabilityPeriodAreSetWhenCreatingNew()
        {
			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var studentAvailabilityPeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

            using (_mocks.Record())
            {
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetStudentAvailabilityPeriods(insertPeriod, studentAvailabilityPeriod));
                Expect.Call(() => _view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
                Expect.Call(() => _view.SelectWorkflowControlSet(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.AddWorkflowControlSet();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.DefaultStudentAvailabilityPeriods(_workflowControlSetModel, new DateTime(2010, 3, 24));
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetPreferencePeriods()
        {
            var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var preferencePeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetPreferencePeriod(preferencePeriod);
                _target.SetPreferenceInputPeriod(insertPeriod);
                Assert.AreEqual(insertPeriod, _target.SelectedModel.DomainEntity.PreferenceInputPeriod);
                Assert.AreEqual(preferencePeriod, _target.SelectedModel.DomainEntity.PreferencePeriod);
            }
            _mocks.VerifyAll();
        } 
        
        [Test]
        public void VerifySetStudentAvailabilityPeriods()
        {
			var insertPeriod = new DateOnlyPeriod(2010, 4, 1, 2010, 4, 30);
			var studentAvailabilityPeriod = new DateOnlyPeriod(2010, 5, 1, 2010, 5, 31);

            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetStudentAvailabilityPeriod(studentAvailabilityPeriod);
                _target.SetStudentAvailabilityInputPeriod(insertPeriod);
                Assert.AreEqual(insertPeriod, _target.SelectedModel.DomainEntity.StudentAvailabilityInputPeriod);
                Assert.AreEqual(studentAvailabilityPeriod, _target.SelectedModel.DomainEntity.StudentAvailabilityPeriod);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyBasicVisualizerWriteProtectionPeriods()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

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
        }


        [Test]
        public void VerifyBasicVisualizerPreferencePeriods()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

                var expected = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
                _target.SetPreferencePeriod(expected);

				var result = _target.BasicVisualizerPreferencePeriods();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(expected, result[0]);
            }

        }

        [Test]
        public void VerifyBasicVisualizerStudentAvailabilityPeriods()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

				var expected = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
                _target.SetStudentAvailabilityPeriod(expected);

				var result = _target.BasicVisualizerStudentAvailabilityPeriods();
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual(expected, result[0]);
            }

        }

        [Test]
        public void VerifyBasicVisualizerPublishedPeriodsWithNull()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

                var expected1 = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
                _target.SetPreferencePeriod(expected1);
                _target.SetStudentAvailabilityPeriod(expected1);
                _target.SetPublishedToDate(null);

                var expected2 = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(DateHelper.MinSmallDateTime));

                var result = _target.BasicVisualizerPublishedPeriods();
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(expected2, result[0]);
                Assert.AreEqual(expected1, result[1]);
            }
        }

        [Test]
        public void VerifyBasicVisualizerPublishedPeriodsWithNotNull()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            _absenceList.Add(AbsenceFactory.CreateAbsence("Holiday"));

            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);

				var expected1 = new DateOnlyPeriod(new DateOnly(2010, 7, 1), new DateOnly(2010, 7, 31));
                _target.SetPreferencePeriod(expected1);
                _target.SetStudentAvailabilityPeriod(expected1);
                _target.SetPublishedToDate(new DateOnly(2010, 5, 31));

				var expected2 = new DateOnlyPeriod(new DateOnly(DateHelper.MinSmallDateTime), new DateOnly(2010, 5, 31));

                IList<DateOnlyPeriod> result = _target.BasicVisualizerPublishedPeriods();
                Assert.AreEqual(3, result.Count);
                Assert.AreEqual(expected2, result[0]);
                Assert.AreEqual(expected1, result[1]);
            }
        }

        #region Shift trade

        [Test]
        public void VerifySetShiftTradeOpenPeriodDaysForwardMinimum()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
			var tradePeriodDays = new MinMax<int>(20, 100);
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetWriteProtectedDays(null);
                Assert.AreEqual(null, _target.SelectedModel.WriteProtection);
                _target.SetOpenShiftTradePeriod(tradePeriodDays);
                Assert.AreEqual(tradePeriodDays, _target.SelectedModel.ShiftTradeOpenPeriodDays);
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyDefaultShiftTradePeriodDaysWhenCreatingNew()
        {
			var periodDays = new MinMax<int>(2, 17);

            using (_mocks.Record())
            {
                ExpectSetSelectedWorkflowControlSetModel();
                Expect.Call(() => _view.SetShiftTradePeriodDays(periodDays));
                Expect.Call(() => _view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
                Expect.Call(() => _view.SelectWorkflowControlSet(null)).IgnoreArguments();
            }
            using (_mocks.Playback())
            {
                _target.AddWorkflowControlSet();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.DefaultShiftTradePeriodDays(periodDays);
            }
            _mocks.VerifyAll();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity"), Test]
        public void VerifyShiftTradeTargetTimeFlexibilityIsSetWhenCreatingNew()
        {
			var flexibility = new TimeSpan(0, 0, 0);

            using (_mocks.Record())
            {
                Expect.Call(() => _view.SetName("name")).IgnoreArguments();
                Expect.Call(() => _view.SetUpdatedInfo("")).IgnoreArguments();
                Expect.Call(
                    () => _view.SetOpenPeriodsGridRowCount(_workflowControlSetModel.AbsenceRequestPeriodModels.Count)).IgnoreArguments();
                Expect.Call(() => _view.SetWriteProtectedDays(null));
                Expect.Call(() => _view.SetShiftTradeTargetTimeFlexibility(flexibility));
                Expect.Call(() => _view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
                Expect.Call(() => _view.SelectWorkflowControlSet(null)).IgnoreArguments();
                Expect.Call(_view.EnableAllAuthorized);
                Expect.Call(() => _view.SetMatchingSkills(null)).IgnoreArguments();
                Expect.Call(() => _view.SetAllowedDayOffs(null)).IgnoreArguments();
                Expect.Call(() => _view.SetAllowedShiftCategories(null)).IgnoreArguments();
                Expect.Call(() => _view.SetAllowedAbsences(null)).IgnoreArguments();
                Expect.Call(() => _view.LoadDateOnlyVisualizer()).Repeat.Any();
                Expect.Call(() => _view.SetAutoGrant(false));
                Expect.Call(() => _view.SetUseShiftCategoryFairness(false));
            }
            using (_mocks.Playback())
            {
                _target.AddWorkflowControlSet();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifySetShiftTradeTargetTimeFlexibility()
        {
			var flexibility = new TimeSpan(0, 34, 0);
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                _target.SetShiftTradeTargetTimeFlexibility(flexibility);
                Assert.AreEqual(flexibility, _target.SelectedModel.ShiftTradeTargetTimeFlexibility);
            }
            _mocks.VerifyAll();
        }


        #endregion

        [Test]
        public void VerifyAddRemovePreferenceDayOff()
        {

            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("sglk"));
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
                _target.AddAllowedPreferenceDayOff(dayOffTemplate);
                Assert.AreEqual(1, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
                _target.RemoveAllowedPreferenceDayOff(dayOffTemplate);
                Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceDayOffs.Count());
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyAddRemovePreferenceShiftCategory()
        {

            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                IShiftCategory shiftCategory = new ShiftCategory("cat");
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
                Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
                _target.AddAllowedPreferenceShiftCategory(shiftCategory);
                Assert.AreEqual(1, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
                _target.RemoveAllowedPreferenceShiftCategory(shiftCategory);
                Assert.AreEqual(0, _target.SelectedModel.DomainEntity.AllowedPreferenceShiftCategories.Count());
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void VerifyCanGetListsOffDayOffsAndShiftCategories()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                Assert.IsNotNull(_target.ShiftCategoriesCollection());
                Assert.IsNotNull(_target.DayOffCollection());
            }
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateModelOnRadioButtonFairnessCheckChange()
        {
            IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
            using (_mocks.Record())
            {
                ExpectInitialize(repositoryCollection);
                ExpectSetSelectedWorkflowControlSetModel();
            }
            using (_mocks.Playback())
            {
                _target.Initialize();
                _target.SetSelectedWorkflowControlSetModel(_workflowControlSetModel);
                _target.OnRadioButtonAdvFairnessEqualCheckChanged(true);
                Assert.IsTrue(_workflowControlSetModel.UseShiftCategoryFairness);
                _target.OnRadioButtonAdvFairnessPointsCheckChanged(true);
                Assert.IsFalse(_workflowControlSetModel.UseShiftCategoryFairness);
            }

            _mocks.VerifyAll();
        }

	    [Test]
	    public void VerifySetMinTimePerWeek()
	    {
			 var minTimePerWeek = new TimeSpan(0, 28, 0, 0);
			 IList<IWorkflowControlSet> repositoryCollection = new List<IWorkflowControlSet> { _workflowControlSet };
			 using (_mocks.Record())
			 {
				 ExpectInitialize(repositoryCollection);
				 ExpectSetSelectedWorkflowControlSetModel();
			 }
			 using (_mocks.Playback())
			 {
				 _target.Initialize();
				 _target.SetSelectedWorkflowControlSetModel(_target.WorkflowControlSetModelCollection.First());
				 _target.SetMinTimePerWeek(minTimePerWeek);
				 Assert.AreEqual(minTimePerWeek, _target.SelectedModel.MinTimePerWeek);
			 }
			 _mocks.VerifyAll();
	    }

        public void ExpectInitialize(IList<IWorkflowControlSet> repositoryCollection)
        {
            ExpectInitialize(_mocks, _view, _unitOfWorkFactory, _repositoryFactory, repositoryCollection, _categories, _dayOffTemplates, _absenceList,
                             _activityList, _skillList, _culture);
        }

        public static void ExpectInitialize(MockRepository mocks, IWorkflowControlSetView view, IUnitOfWorkFactory unitOfWorkFactory, IRepositoryFactory repositoryFactory, IList<IWorkflowControlSet> workflowControlSets, IList<IShiftCategory> shiftCategories, IList<IDayOffTemplate> dayOffTemplates, IList<IAbsence> absences, IList<IActivity> activities, IList<ISkill> skills, CultureInfo culture)
        {
            var unitOfWork = mocks.StrictMock<IUnitOfWork>();
            var repository = mocks.StrictMock<IWorkflowControlSetRepository>();
            var absenceRepository = mocks.StrictMock<IAbsenceRepository>();
            var activityRepository = mocks.StrictMock<IActivityRepository>();
            var categoryRepository = mocks.StrictMock<IShiftCategoryRepository>();
            var dayOffRepository = mocks.StrictMock<IDayOffTemplateRepository>();
            var skillRepository = mocks.StrictMock<ISkillRepository>();

            Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            Expect.Call(() => unitOfWork.DisableFilter(QueryFilter.Deleted));
            Expect.Call(repositoryFactory.CreateWorkflowControlSetRepository(unitOfWork)).Return(repository);
            Expect.Call(repository.LoadAllSortByName()).Return(workflowControlSets).Repeat.Once();
            Expect.Call(repositoryFactory.CreateAbsenceRepository(unitOfWork)).Return(absenceRepository);
            Expect.Call(repositoryFactory.CreateActivityRepository(unitOfWork)).Return(activityRepository);
            Expect.Call(repositoryFactory.CreateSkillRepository(unitOfWork)).Return(skillRepository);
            Expect.Call(repositoryFactory.CreateShiftCategoryRepository(unitOfWork)).Return(categoryRepository);
            Expect.Call(repositoryFactory.CreateDayOffRepository(unitOfWork)).Return(dayOffRepository);
            Expect.Call(categoryRepository.LoadAll()).Return(shiftCategories);
            Expect.Call(dayOffRepository.LoadAll()).Return(dayOffTemplates);
            Expect.Call(absenceRepository.LoadRequestableAbsence()).Return(absences);
            Expect.Call(absenceRepository.LoadAll()).Return(absences);
            Expect.Call(activityRepository.LoadAllSortByName()).Return(activities);
            Expect.Call(skillRepository.FindAllWithoutMultisiteSkills()).Return(skills);
            Expect.Call(unitOfWork.Dispose);
            Expect.Call(() => view.SetCalendarCultureInfo(culture));

            if (absences.Count > 0)
            {
                Expect.Call(() => view.EnableHandlingOfAbsenceRequestPeriods(true));
            }
            else
            {
                Expect.Call(() => view.EnableHandlingOfAbsenceRequestPeriods(false));
            }
            Expect.Call(() => view.FillAllowedPreferenceActivityCombo(null, "Name")).IgnoreArguments();
            LastCall.IgnoreArguments();
            if (!workflowControlSets.IsEmpty())
            {
                Expect.Call(() => view.FillWorkloadControlSetCombo(null, "Name")).IgnoreArguments();
                LastCall.IgnoreArguments();
            }
            Expect.Call(view.InitializeView);
        }

        public void ExpectSetSelectedWorkflowControlSetModel()
        {
            ExpectSetSelectedWorkflowControlSetModel(_view);
        }

        public static void ExpectSetSelectedWorkflowControlSetModel(IWorkflowControlSetView view)
        {
            Expect.Call(() => view.SetName("name")).IgnoreArguments();
            Expect.Call(() => view.SetUpdatedInfo("")).IgnoreArguments();
            Expect.Call(() => view.SetOpenPeriodsGridRowCount(1)).IgnoreArguments();
            Expect.Call(() => view.SetWriteProtectedDays(0)).IgnoreArguments();
            Expect.Call(view.LoadDateOnlyVisualizer).Repeat.Any();
            Expect.Call(() => view.SetMatchingSkills(null)).IgnoreArguments();
            Expect.Call(() => view.SetAllowedDayOffs(null)).IgnoreArguments();
            Expect.Call(() => view.SetAllowedShiftCategories(null)).IgnoreArguments();
            Expect.Call(() => view.SetAllowedAbsences(null)).IgnoreArguments();
            Expect.Call(() => view.SetShiftTradeTargetTimeFlexibility(new TimeSpan()));
            Expect.Call(() => view.SetAutoGrant(false)).IgnoreArguments();
            Expect.Call(() => view.SetUseShiftCategoryFairness(false)).IgnoreArguments();
            Expect.Call(() => view.SetMinTimePerWeek(new TimeSpan()));
            Expect.Call(view.EnableAllAuthorized);
        }
    }
}
