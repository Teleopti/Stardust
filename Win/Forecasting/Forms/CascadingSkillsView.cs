using System;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Forecasting.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public partial class CascadingSkillsView : BaseDialogForm
    {
	    private readonly CascadingSkillPresenter _presenter;
	    private BindingSource _bindingSourceNonCascading;
	    private BindingSource _bindingSourceCascading;

		public CascadingSkillsView(CascadingSkillPresenter presenter)
        {
		    _presenter = presenter;
		    InitializeComponent();
				
            if (DesignMode) return;
            SetTexts();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (DesignMode)return;
			
	        _bindingSourceNonCascading = new BindingSource {DataSource = _presenter.NonCascadingSkills};
			_bindingSourceCascading = new BindingSource { DataSource = _presenter.CascadingSkills};

			listBoxNonCascading.DataSource = _bindingSourceNonCascading;
	        listBoxNonCascading.DisplayMember = "Name";

	        listBoxCascading.DataSource = _bindingSourceCascading;
	        listBoxCascading.DisplayMember = "Name";
        }

		private void buttonAdvMoveUpClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();

			if (selectedIndices.Count > 0 &&  selectedIndices[0] > 0)
			{
				foreach (var selectedItem in listBoxCascading.SelectedItems)
				{ 
					var skill = selectedItem as ISkill;
					_presenter.MoveUpCascadingSkill(skill);
				}

				_bindingSourceCascading.ResetBindings(false);
				listBoxCascading.SelectedItems.Clear();

				foreach (var selectedIndex in selectedIndices)
				{
					var index = ((int) selectedIndex) - 1;
					listBoxCascading.SetSelected(index, true);
				}
			}
		}

		private void buttonAdvMoveDownClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();
			var selectedItems = listBoxCascading.SelectedItems.Cast<ISkill>().ToList();

			if(selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count - 1] < _presenter.CascadingSkills.Count() - 1)
			{
				foreach (var skill in selectedItems.Reverse<ISkill>())
				{
					_presenter.MoveDownCascadingSkill(skill);
				}

				_bindingSourceCascading.ResetBindings(false);
				listBoxCascading.SelectedItems.Clear();

				foreach (var selectedIndex in selectedIndices)
				{
					var index = selectedIndex + 1;
					listBoxCascading.SetSelected(index, true);
				}
			}
		}

		private void buttonAdvMakeCascadingClick(object sender, EventArgs e)
		{
			foreach (var selectedItem in listBoxNonCascading.SelectedItems)
			{
				var skill = selectedItem as ISkill;
				_presenter.MakeCascading(skill);
			}

			_bindingSourceCascading.ResetBindings(false);
			_bindingSourceNonCascading.ResetBindings(false);
		}

		private void buttonAdvMakeNonCascadingClick(object sender, EventArgs e)
		{
			foreach (var selectedItem in listBoxCascading.SelectedItems)
			{
				var skill = selectedItem as ISkill;
				_presenter.MakeNonCascading(skill);
			}

			_bindingSourceCascading.ResetBindings(false);
			_bindingSourceNonCascading.ResetBindings(false);
		}
	}
}
