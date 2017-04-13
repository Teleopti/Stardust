using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Forecasting.Cascading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_CascadingSkillsGUI_40018)]
	public partial class CascadingSkillsView : BaseDialogForm
	{
		private readonly CascadingSkillPresenter _presenter;
		private BindingSource _bindingSourceNonCascading;
		private BindingSource _bindingSourceCascading;
		private int _cascadingItemMaxLength;

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
					_presenter.MoveUpCascadingSkills(skill.First());
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
					_presenter.MoveDownCascadingSkills(skill.First());
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
				_presenter.MakeNonCascading(skill.First());
			}

			_cascadingItemMaxLength = 0;
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
					_cascadingItemMaxLength = Math.Max(_cascadingItemMaxLength, TextRenderer.MeasureText(outputText, e.Font).Width);
					listBoxCascading.HorizontalExtent = _cascadingItemMaxLength + 10;
					e.Graphics.DrawString(outputText, e.Font, textBrush, e.Bounds, StringFormat.GenericDefault);
				}
			}
		}

		private void buttonAdvEqualClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();
			var selectedItems = listBoxCascading.SelectedItems.Cast<IList<ISkill>>().ToList();

			if (selectedIndices.Count <= 1) return;
			var masterSkill = selectedItems.First().First();

			foreach (var selectedItem in selectedItems)
			{
				if (selectedItem.Equals(selectedItems.First())) continue;
				_presenter.MakeParalell(masterSkill, selectedItem.First());
			}

			_bindingSourceCascading.ResetBindings(false);
			listBoxCascading.SelectedItems.Clear();
			listBoxCascading.SetSelected(selectedIndices.First(), true);
		}

		private void buttonAdvUnEqualClick(object sender, EventArgs e)
		{
			var selectedIndices = listBoxCascading.SelectedIndices.Cast<int>().ToList();
			var selectedItems = listBoxCascading.SelectedItems.Cast<IList<ISkill>>().ToList();

			if (selectedIndices.Count <= 0) return;
			foreach (var selectedItem in selectedItems)
			{
				if (selectedItem.Count <= 1) continue;
				_presenter.Unparalell(selectedItem.First());
			}

			_bindingSourceCascading.ResetBindings(false);
			_cascadingItemMaxLength = 0;
			listBoxCascading.SelectedItems.Clear();
			listBoxCascading.SetSelected(selectedIndices.First(), true);
		}
	}
}
