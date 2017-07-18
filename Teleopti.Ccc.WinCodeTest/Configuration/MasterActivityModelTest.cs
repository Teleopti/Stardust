using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture]
    public class MasterActivityModelTest
    {
        private MockRepository _mocks;
        private IMasterActivityViewModel _target;
        private IActivityRepository _activityRepository;
        private IMasterActivityRepository _masterActivityRepository;
        private MasterActivityModel _model;
        private ActivityModel _activityModel;
        private LocalizedUpdateInfo _localizer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _activityRepository = _mocks.StrictMock<IActivityRepository>();
            _masterActivityRepository = _mocks.StrictMock<IMasterActivityRepository>();
            _localizer = new LocalizedUpdateInfo();
            _target = new MasterActivityViewModel(_activityRepository, _masterActivityRepository);
        }

        [Test]
        public void CanUpdatePropertiesOnMasterActivity()
        {
            var master = new MasterActivity { Description = new Description("OrgName", ""), DisplayColor = Color.Green};
            _model = new MasterActivityModel(master,_localizer) {Name = "NewName", Color = Color.Blue};

            Assert.That(_model.Entity.Description.Name, Is.EqualTo("NewName"));
            Assert.That(_model.Entity.DisplayColor, Is.EqualTo(Color.Blue));

        }

        [Test]
        public void ShouldLoadAllNotDeletedFromRepository()
        {
            var deleted = new MasterActivity();
            deleted.SetDeleted();
            var lst = new List<IMasterActivity> { deleted, new MasterActivity()  };
            Expect.Call(_masterActivityRepository.LoadAll()).Return(lst);
            _mocks.ReplayAll();
            var ret = _target.AllNotDeletedMasterActivities;
            Assert.That(ret.Count, Is.EqualTo(1));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldAddToRepositoryAndCashedListWhenCreateNewMasterActivity()
        {
            var deleted = new MasterActivity();
            deleted.SetDeleted();
            var lst = new List<IMasterActivity> { deleted, new MasterActivity() };
            Expect.Call(_masterActivityRepository.LoadAll()).Return(lst);
            Expect.Call(() => _masterActivityRepository.Add(new MasterActivity())).IgnoreArguments();

            _mocks.ReplayAll();
            var retLst = _target.AllNotDeletedMasterActivities;
            Assert.That(retLst.Count, Is.EqualTo(1));

            _target.CreateNewMasterActivity("NyMaster");
            var lstNow = _target.AllNotDeletedMasterActivities;
            Assert.That(lstNow.Count, Is.EqualTo(2));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldRemoveFromRepositoryAndCashedListWhenDeletingMasterActivity()
        {
            var masterToRemove = new MasterActivity();
            var lst = new List<IMasterActivity>
                          {
                              new MasterActivity(),
                              new MasterActivity(),
                              masterToRemove
                          };
            Expect.Call(_masterActivityRepository.LoadAll()).Return(lst);
            Expect.Call(() => _masterActivityRepository.Remove(masterToRemove)).IgnoreArguments();

            _mocks.ReplayAll();
            var retLst = _target.AllNotDeletedMasterActivities;
            Assert.That(retLst.Count, Is.EqualTo(3));

            _target.DeleteMasterActivity(retLst[2]);
            var lstNow = _target.AllNotDeletedMasterActivities;
            Assert.That(lstNow.Count, Is.EqualTo(2));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldLoadAllNotDeletedActivitiesFromActivityRepositoryOnce()
        {
            var lst = new List<IActivity> { new Activity("a"), new Activity("b") };
            var deletedActivity = new Activity("deleted");
            deletedActivity.SetDeleted();
            lst.Add(deletedActivity);
            Expect.Call(_activityRepository.LoadAll()).Return(lst);
            _mocks.ReplayAll();
            var ret = _target.AllNotDeletedActivities;
            Assert.That(ret.Count, Is.EqualTo(2));
            _mocks.VerifyAll();

        }

        [Test]
        public void ActivityModelShouldSayIsDeletedIfActivityNotRequiresSkill()
        {
            var activity = new Activity("noskill") {RequiresSkill = false};
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.IsDeleted);
        }

        [Test]
        public void ActivityModelShouldSayIsDeletedIfActivityNotInContractTime()
        {
            var activity = new Activity("notConntract") { RequiresSkill = true , InContractTime = false};
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.IsDeleted);
        }

        [Test]
        public void ActivityModelShouldSayNotDeletedIfActivityInContractTimeRequiresSkillAndNotDeleted()
        {
            var activity = new Activity("skill&Contract") { RequiresSkill = true, InContractTime = true };
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.IsDeleted, Is.False);
        }

        [Test]
        public void ActivityModelShouldReturnNameOfActivity()
        {
            var activity = new Activity("MyFineActivity") ;
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.Name, Is.EqualTo("MyFineActivity"));
        }

        [Test]
        public void SetDeletedOnActivityModelShouldNotWork()
        {
            var activity = new Activity("MyFineActivity") { RequiresSkill = true, InContractTime = true };
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.IsDeleted, Is.False);
            _activityModel.SetDeleted();
            Assert.That(_activityModel.IsDeleted, Is.False);
        }

        [Test]
        public void EqualsOnActivityModelOnOtherTypeOfObjectShouldReturnFalse()
        {
            var activity = new Activity("MyFineActivity") { RequiresSkill = true, InContractTime = true };
            _activityModel = new ActivityModel(activity);
            var other = new MasterActivity();
            Assert.That(_model.Equals(other),Is.False);
        }

        [Test]
        public void EqualsOnActivityModelOnOtherModelButSameActivityShouldReturnTrue()
        {
            var activity = new Activity("MyFineActivity") { RequiresSkill = true, InContractTime = true };
            _activityModel = new ActivityModel(activity);
            var other = new ActivityModel(activity);
            Assert.That(_activityModel.Equals(other));
        }

        [Test]
        public void EqualsShouldReturnFalseWhenCalledWithNull()
        {
            var master = new MasterActivity();
            _model = new MasterActivityModel(master, _localizer);
            Assert.That(_model.Equals(null), Is.False);
        }

        [Test]
        public void EqualsShouldReturnTrueWhenCalledWithSameModel()
        {
            var master = new MasterActivity();
            _model = new MasterActivityModel(master, _localizer);
            Assert.That(_model.Equals(_model));
        }

        [Test]
        public void ShouldReturnTrueOnEqualsIfDifferentModelButSameMaster()
        {
            var master = new MasterActivity();
            _model = new MasterActivityModel(master, _localizer);
            var model2 = new MasterActivityModel(master, _localizer);

            Assert.That(_model.Equals(model2));
        }

        [Test]
        public void ShouldReturnSameHashCodeAsMaster()
        {
            var master = new MasterActivity();
            _model = new MasterActivityModel(master, _localizer);
            Assert.That(master.GetHashCode(),Is.EqualTo(_model.GetHashCode()));
        }

        [Test]
        public void ShouldCallUpdateActivitiesOnMasterWhenUpdateActivitiesIsCalledOnModel()
        {
            var master = _mocks.StrictMock<IMasterActivity>();
            _model = new MasterActivityModel(master, _localizer);
            var activity = new Activity("dkldfk");
            var lst = new List<IActivity> {activity};
            var actModel = new ActivityModel(activity);
            Expect.Call(() => master.UpdateActivityCollection(lst));
            _mocks.ReplayAll();
            _model.UpdateActivities(new List<IActivityModel> { actModel });
            _mocks.VerifyAll();
        }

        [Test]
        public void ActivityModelShouldReturnSameHashCodeAsActivity()
        {
            var activity = new Activity("d");
            _activityModel = new ActivityModel(activity);
            Assert.That(activity.GetHashCode(), Is.EqualTo(_activityModel.GetHashCode()));
        }

        [Test, Culture("sv-SE")]
        public void ShouldShouldReturnUpdateInfoFromMasterActivity()
        {
            var master = new MasterActivity();
			ReflectionHelper.SetUpdatedOn(master,new DateTime(2001,1,1));
			ReflectionHelper.SetUpdatedBy(master,PersonFactory.CreatePerson());

			_model = new MasterActivityModel(master,_localizer);
			
            _mocks.ReplayAll();
            Assert.That(_model.UpdateInfo, Is.EqualTo("Updated by: arne arne 2001-01-01 01:00:00"));
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldThrowIfMasterActivityToDeleteIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target.DeleteMasterActivity(null));
        }

        [Test]
        public void EqualsShouldReturnFalseOnNull()
        {
            var activity = new Activity("MyFineActivity") { RequiresSkill = true, InContractTime = true };
            _activityModel = new ActivityModel(activity);
            Assert.That(_activityModel.Equals(null),Is.False);
        }
    }

}
