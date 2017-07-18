using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity;

namespace Teleopti.Ccc.WinCodeTest.Configuration
{
    [TestFixture, SetUICulture("en-US")]
    public class MasterActivityPresenterTest
    {
        private MockRepository _mocks;
        private IMasterActivityView _view;
        private MasterActivityPresenter _target;
        private IMasterActivityViewModel _viewModel;
        private LocalizedUpdateInfo _localizer;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _view = _mocks.StrictMock<IMasterActivityView>();
            _viewModel = _mocks.StrictMock<IMasterActivityViewModel>();
            _localizer = new LocalizedUpdateInfo();
            _target = new MasterActivityPresenter(_view, _viewModel);
        }

        [Test]
        public void ShouldGetAllMasterActivitiesFromModelAndShowOnLoad()
        {
            Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(new List<IMasterActivityModel>());
            Expect.Call(() => _view.LoadComboWithMasterActivities(new List<IMasterActivityModel>()));
            _mocks.ReplayAll();

            _target.LoadAllMasterActivities();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldSetNameShortNameColorUpdateInfoAndLoadTwoListWhenOnMasterActivityIsSelected()
        {
            var selectedMaster = _mocks.StrictMock<IMasterActivityModel>();
            
            Expect.Call(_viewModel.AllNotDeletedActivities).Return(new List<IActivityModel>());
            Expect.Call(selectedMaster.Activities).Return(new List<IActivityModel>());
            Expect.Call(() => _view.LoadTwoList(new List<IActivityModel>(), new List<IActivityModel>()));
            Expect.Call(selectedMaster.Name).Return("MyNiceName");
            Expect.Call(() => _view.LongName = "MyNiceName");
            Expect.Call(selectedMaster.Color).Return(Color.DeepPink);
            Expect.Call(() => _view.Color = Color.DeepPink);
            Expect.Call(selectedMaster.UpdateInfo).Return("hejhej");
            Expect.Call(() => _view.SetUpdateInfo("hejhej"));
            _mocks.ReplayAll();

            _target.OnMasterActivitySelected(selectedMaster);

            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldUpdateNameShortNameColorActivitiesAndReloadCombo()
        {
            var selectedMasterActivity = _mocks.StrictMock<IMasterActivityModel>();
            var masters = new List<IMasterActivityModel>();
            IList<IActivityModel> activities = new List<IActivityModel>();
            Expect.Call(_view.LongName).Return("NewName");
            Expect.Call(() => selectedMasterActivity.Name = "NewName");
            Expect.Call(_view.Color).Return(Color.Firebrick);
            Expect.Call(() => selectedMasterActivity.Color = Color.Firebrick);
            Expect.Call(_view.Activities).Return(activities);
            Expect.Call(() => selectedMasterActivity.UpdateActivities(activities));
            Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(masters);
            Expect.Call(() => _view.LoadComboWithMasterActivities(masters));
            Expect.Call(() => _view.SelectMaster(selectedMasterActivity));
            _mocks.ReplayAll();

            _target.OnMasterActivityPropertyChanged(selectedMasterActivity);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateNewMasterAddToListAndSelectOnAdd()
        {//Description = new Description{ Name = "NyMaster", ShortName = "NM", Color = Color.DodgerBlue}
            var newMaster = new MasterActivityModel(new MasterActivity { Description = new Description("New Master1", ""), DisplayColor = Color.Firebrick }, _localizer);
            var lstActivities = new List<IMasterActivityModel> { newMaster };
            Expect.Call(_viewModel.CreateNewMasterActivity("New Master2")).Return(newMaster);
            Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(lstActivities).Repeat.Twice();
            Expect.Call(() => _view.LoadComboWithMasterActivities(lstActivities));
            Expect.Call(() => _view.SelectMaster(newMaster));
            Expect.Call(() => _view.LongName = "New Master1");
            Expect.Call(() => _view.Color = Color.Firebrick);
            Expect.Call(_viewModel.AllNotDeletedActivities).Return(new List<IActivityModel>());
            Expect.Call(() => _view.LoadTwoList(new List<IActivityModel>(), new List<IActivityModel>()));
            Expect.Call(() => _view.SetUpdateInfo(""));
            _mocks.ReplayAll();

            _target.OnAddNew();
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallDeleteOnModelAndRefreshComboOnDelete()
        {
            var master1 = _mocks.StrictMock<IMasterActivityModel>();
            var master2 = new MasterActivityModel(new MasterActivity { Description = new Description("NyMaster", ""), DisplayColor = Color.DodgerBlue }, _localizer);
            var lstActivities = new List<IMasterActivityModel> { master2 };
            Expect.Call(_view.ConfirmDelete()).Return(true);
            Expect.Call(() => _viewModel.DeleteMasterActivity(master1));
            Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(lstActivities).Repeat.Twice();
            Expect.Call(() => _view.LoadComboWithMasterActivities(lstActivities)).Repeat.Twice();
            Expect.Call(() => _view.SelectMaster(master2));
            Expect.Call(() => _view.LongName = "NyMaster");
            Expect.Call(() => _view.Color = Color.DodgerBlue);
            Expect.Call(_viewModel.AllNotDeletedActivities).Return(new List<IActivityModel>());
            Expect.Call(() => _view.LoadTwoList(new List<IActivityModel>(), new List<IActivityModel>()));
            Expect.Call(() => _view.SetUpdateInfo(""));
            _mocks.ReplayAll();

            _target.OnDeleteMasterActivity(master1);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCallDeleteOnModelAndEmptyViewWhenLastMasterDeleted()
        {
            var master1 = _mocks.StrictMock<IMasterActivityModel>();
            var lstActivities = new List<IMasterActivityModel> ();
            Expect.Call(_view.ConfirmDelete()).Return(true);
            Expect.Call(() => _viewModel.DeleteMasterActivity(master1));
            Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(lstActivities);
            Expect.Call(() => _view.LoadComboWithMasterActivities(lstActivities));
            Expect.Call(() => _view.LongName = "");
            Expect.Call(() => _view.Color = Color.Empty);
            Expect.Call(() => _view.LoadTwoList(new List<IActivityModel>(), new List<IActivityModel>()));
            Expect.Call(() => _view.SetUpdateInfo(""));
            _mocks.ReplayAll();

            _target.OnDeleteMasterActivity(master1);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCancelDeleteIfConfirmationIsNegative()
        {
            var master1 = _mocks.StrictMock<IMasterActivityModel>();
            Expect.Call(_view.ConfirmDelete()).Return(false);
            _mocks.ReplayAll();

            _target.OnDeleteMasterActivity(master1);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldCreateNewUniqueName()
        {
            var master1 = new MasterActivityModel(new MasterActivity { Description = new Description("NewMaster1", ""), DisplayColor = Color.DodgerBlue }, _localizer);
            var master2 = new MasterActivityModel(new MasterActivity { Description = new Description("NewMaster2", ""), DisplayColor = Color.DodgerBlue }, _localizer);
            var lstActivities = new List<IMasterActivityModel> {master1, master2 };
            var newName = MasterActivityPresenter.GetNewName(lstActivities, "NewMaster");
            Assert.That(newName.Equals("NewMaster3"));
        }

        [Test]
        public void ShouldThrowIfCashedModelsIsNull()
        {
			Assert.Throws<ArgumentNullException>(() => MasterActivityPresenter.GetNewName(null, "NewMaster"));
        }

        [Test]
        public void ShouldDoNothingIfSelectedIsNull()
        {
            _target.OnMasterActivityPropertyChanged(null);
        }

		[Test]
		public void ShouldUseExistingNameIfNameIsEmptyInView()
		{
			var master = new MasterActivityModel(new MasterActivity { Description = new Description("master", ""), DisplayColor = Color.DodgerBlue }, _localizer);
			var masters = new List<IMasterActivityModel>();

			using (_mocks.Record())
			{
				Expect.Call(_view.LongName).Return(string.Empty);
				Expect.Call(_view.Color).Return(Color.BlueViolet);
				Expect.Call(_view.Activities).Return(new List<IActivityModel>());
				Expect.Call(() => _view.LoadComboWithMasterActivities(masters));
				Expect.Call(_viewModel.AllNotDeletedMasterActivities).Return(masters);
				Expect.Call(() => _view.SelectMaster(master));
				
			}

			using (_mocks.Playback())
			{
				_target.OnMasterActivityPropertyChanged(master);
				Assert.AreEqual("master", master.Name);
			}
		}
    }
}
