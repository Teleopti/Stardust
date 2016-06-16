using System;
using System.Collections.Generic;
using System.Drawing;
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
			if (DesignMode) return;

			_bindingSourceNonCascading = new BindingSource { DataSource = _presenter.NonCascadingSkills };
			_bindingSourceCascading = new BindingSource { DataSource = _presenter.CascadingSkills };
			listBoxNonCascading.DataSource = _bindingSourceNonCascading;
			listBoxNonCascading.DisplayMember = "Name";
			listBoxCascading.DataSource = _bindingSourceCascading;
			listBoxCascading.DrawMode = DrawMode.OwnerDrawFixed;
		}

		private void buttonAdvMoveUpClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();

			if (selectedIndices.Count > 0 && selectedIndices[0] > 0)
			{
				foreach (var selectedItem in listBoxCascading.SelectedItems)
				{
					var skill = (IList<ISkill>)selectedItem;
					_presenter.MoveUpCascadingSkills(skill.Single()); //will throw when multiple
				}

				_bindingSourceCascading.ResetBindings(false);
				listBoxCascading.SelectedItems.Clear();

				foreach (var selectedIndex in selectedIndices)
				{
					var index = selectedIndex - 1;
					listBoxCascading.SetSelected(index, true);
				}
			}
		}

		private void buttonAdvMoveDownClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();
			var selectedItems = listBoxCascading.SelectedItems.Cast<IList<ISkill>>().ToList();

			if (selectedIndices.Count > 0 && selectedIndices[selectedIndices.Count - 1] < _presenter.CascadingSkills.Count() - 1)
			{
				foreach (var skill in selectedItems.Reverse<IList<ISkill>>())
				{
					_presenter.MoveDownCascadingSkills(skill.Single()); //will throw when multiple
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
				var skill = (ISkill) selectedItem;
				_presenter.MakeCascading(skill);
			}

			_bindingSourceCascading.ResetBindings(false);
			_bindingSourceNonCascading.ResetBindings(false);
		}

		private void buttonAdvMakeNonCascadingClick(object sender, EventArgs e)
		{
			foreach (var selectedItem in listBoxCascading.SelectedItems)
			{
				var skill = (IList<ISkill>)selectedItem;
				_presenter.MakeNonCascading(skill.Single()); //will throw when multiple
			}

			_bindingSourceCascading.ResetBindings(false);
			_bindingSourceNonCascading.ResetBindings(false);
		}

		private void listBoxCascadingDrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index <= -1)
				return;

			var isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
			var color = isSelected ? SystemColors.Highlight : Color.White;
			using (var textBrush = new SolidBrush(e.ForeColor))
			{
				using (var backgroundBrush = new SolidBrush(color))
				{
					e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
					var outputText = string.Join(", ", ((IList<ISkill>)listBoxCascading.Items[e.Index]).Select(x => x.Name));
					e.Graphics.DrawString(outputText, e.Font, textBrush, e.Bounds, StringFormat.GenericDefault);
				}
			}
		}
	}
}
