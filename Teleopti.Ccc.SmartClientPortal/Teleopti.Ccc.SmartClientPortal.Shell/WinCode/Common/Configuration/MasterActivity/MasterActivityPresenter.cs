using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity
{
	public class MasterActivityPresenter
	{
		private readonly IMasterActivityView _view;
		private readonly IMasterActivityViewModel _viewModel;


		public MasterActivityPresenter(IMasterActivityView view, IMasterActivityViewModel viewModel)
		{
			_view = view;
			_viewModel = viewModel;
		}

		public void LoadAllMasterActivities()
		{
			var lst = _viewModel.AllNotDeletedMasterActivities;
			_view.LoadComboWithMasterActivities(lst);

		}

		public void OnMasterActivitySelected(IMasterActivityModel selectedMasterActivityModel)
		{
			SetPropertiesInView(selectedMasterActivityModel);
		}

		private void SetPropertiesInView(IMasterActivityModel selectedMasterActivityModel)
		{
			if (selectedMasterActivityModel != null)
			{
				_view.LongName = selectedMasterActivityModel.Name;
				_view.Color = selectedMasterActivityModel.Color;
				_view.LoadTwoList(_viewModel.AllNotDeletedActivities, selectedMasterActivityModel.Activities);
				_view.SetUpdateInfo(selectedMasterActivityModel.UpdateInfo);
			}
			else
			{
				_view.LongName = "";
				_view.Color = Color.Empty;
				_view.LoadTwoList(new List<IActivityModel>(), new List<IActivityModel>());
				_view.SetUpdateInfo("");
			}
		}
		public void OnMasterActivityPropertyChanged(IMasterActivityModel selectedMasterActivityModel)
		{
			if (selectedMasterActivityModel == null)
				return;

			var longName = _view.LongName;
			if (!string.IsNullOrWhiteSpace(longName))
				selectedMasterActivityModel.Name = longName;
			selectedMasterActivityModel.Color = _view.Color;
			selectedMasterActivityModel.UpdateActivities(_view.Activities);
			_view.LoadComboWithMasterActivities(_viewModel.AllNotDeletedMasterActivities);
			_view.SelectMaster(selectedMasterActivityModel);
		}

		public void OnAddNew()
		{
			// get from resource
			var newMasterActivity = _viewModel.CreateNewMasterActivity(GetNewName(_viewModel.AllNotDeletedMasterActivities, UserTexts.Resources.NewMasterActivity));
			_view.LoadComboWithMasterActivities(_viewModel.AllNotDeletedMasterActivities);
			SetPropertiesInView(newMasterActivity);
			_view.SelectMaster(newMasterActivity);
		}

		public void OnDeleteMasterActivity(IMasterActivityModel masterActivityModel)
		{
			if (!_view.ConfirmDelete()) return;

			_viewModel.DeleteMasterActivity(masterActivityModel);
			var masters = _viewModel.AllNotDeletedMasterActivities;
			_view.LoadComboWithMasterActivities(masters);
			if (!masters.IsEmpty())
			{
				_view.SelectMaster(masters.First());
				SetPropertiesInView(masters.First());
				LoadAllMasterActivities();
			}
			else
			{
				SetPropertiesInView(null);
			}
		}

		public static string GetNewName(IList<IMasterActivityModel> cashedModels, string newName)
		{
			if (cashedModels == null) throw new ArgumentNullException("cashedModels");
			var number = 1;
			string theNew;
			bool exists;
			do
			{
				exists = false;
				theNew = newName + number;
				foreach (var masterActivityModel in cashedModels)
				{
					if (masterActivityModel.Entity.Name.Equals(theNew))
					{
						exists = true;
						break;
					}
				}
				number += 1;
			} while (exists);

			return theNew;
		}
	}
}