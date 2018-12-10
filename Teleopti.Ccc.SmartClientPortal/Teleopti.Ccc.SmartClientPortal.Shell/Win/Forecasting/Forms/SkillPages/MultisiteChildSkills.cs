using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Forecasting.SkillPages;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Forecasting.Forms.SkillPages
{
	public partial class MultisiteChildSkills : BaseUserControl, IPropertyPage
	{
		private readonly IStaffingCalculatorServiceFacade _staffingCalculatorServiceFacade;
		private MultisiteSkill _skill;
		private TextBox _editBox;
		private int _itemSelected;
		private bool _childSkillNameUpdated;

		public MultisiteChildSkills()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		public MultisiteChildSkills(IStaffingCalculatorServiceFacade staffingCalculatorServiceFacade)
		{
			_staffingCalculatorServiceFacade = staffingCalculatorServiceFacade;
			InitializeComponent();
			if (!DesignMode) SetTexts();
		}

		public void Populate(IAggregateRoot aggregateRoot)
		{
			var skill = aggregateRoot as MultisiteSkill;
			_skill = skill;
			reloadListBox();
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			addAHiddenTextBox();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void addAHiddenTextBox()
		{
			_editBox = new TextBox {Location = new Point(0, 0), Size = new Size(0, 0), MaxLength = 50};
			_editBox.Hide();
			listBoxChildSkills.Controls.AddRange(new Control[] { _editBox });
			_editBox.BackColor = listBoxChildSkills.BackColor;
			_editBox.ForeColor = listBoxChildSkills.ForeColor;
			_editBox.BorderStyle = BorderStyle.FixedSingle;
		}

		private void reloadListBox()
		{
			var childSkills = new TypedBindingCollection<ChildSkill>();
			foreach (var skill in _skill.ChildSkills)
			{
				childSkills.Add((ChildSkill)skill);
			}
			listBoxChildSkills.DataSource = null;
			listBoxChildSkills.DisplayMember = "Name";
			listBoxChildSkills.DataSource = childSkills;
		}

		public bool Depopulate(IAggregateRoot aggregateRoot)
		{
			return true;
		}

		#region IPropertyPage Members

		public string PageName
		{
			get { return UserTexts.Resources.SubSkills; }
		}

		public void SetEditMode()
		{
		}

		#endregion

		private void buttonAdvAddClick(object sender, EventArgs e)
		{
			var childSkill = new ChildSkill(
							UserTexts.Resources.SubSkill, 
							UserTexts.Resources.SubSkill,
							_skill.DisplayColor,
							_skill);

			new SkillWizardPages(childSkill, null, null, _staffingCalculatorServiceFacade).SetSkillDefaultSettings(childSkill);
			DialogResult result;
			using (var swp = new SkillWizardPages(childSkill,new RepositoryFactory(),UnitOfWorkFactory.Current, _staffingCalculatorServiceFacade))
			{
				swp.Initialize(PropertyPagesHelper.GetSkillPages(false, swp, true,false), new LazyLoadingManagerWrapper());
				using (var wizard = new Wizard(swp))
				{
					result = wizard.ShowDialog(this);
				}
			}
			if (result == DialogResult.Cancel)
			{
				_skill.ClearChildSkill(childSkill);
			}
			else
			{
				_skill.AddChildSkill(childSkill);
			}
			reloadListBox();
		}

		private void buttonAdvManageDayTemplatesClick(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			var childSkill = (ChildSkill)listBoxChildSkills.SelectedItem;

			var skillDayTemplateTool = new SkillDayTemplates(childSkill, !_childSkillNameUpdated);
			skillDayTemplateTool.ShowDialog(this);
		}

		private void buttonAdvRemoveClick(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			var childSkill = (ChildSkill)listBoxChildSkills.SelectedItem;

			bool templateExists = (from t in _skill.TemplateMultisiteWeekCollection.Values
								   from p in t.TemplateMultisitePeriodCollection
								   select p).Any(p => 
									   p.Distribution.ContainsKey(childSkill) && 
									   p.Distribution[childSkill].Value > 0);

			if (!templateExists)
			{
				var questionString = string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.QuestionDeleteTheSkillTwoParameters, "\"", childSkill.Name);
				if (ViewBase.ShowConfirmationMessage(questionString, UserTexts.Resources.Delete) == DialogResult.Yes)
				{
					_skill.RemoveChildSkill(childSkill);
					reloadListBox();
				}
			}
			else
			{
				ViewBase.ShowErrorMessage(UserTexts.Resources.SubSkillHasValueInTemplate,
						UserTexts.Resources.SubSkillDistributions);
			}
		}

		private void listBoxChildSkillsDoubleClick(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			showEditBox();
		}

		private void showEditBox()
		{
			_itemSelected = listBoxChildSkills.SelectedIndex;
			var rect = listBoxChildSkills.GetItemRectangle(_itemSelected);
			var itemText = ((ChildSkill)listBoxChildSkills.Items[_itemSelected]).Name;
			_editBox.Location = new Point(rect.X, rect.Y);
			_editBox.Size = new Size(rect.Width, rect.Height);
			_editBox.Show();
			_editBox.Text = itemText;
			_editBox.Focus();
			_editBox.SelectAll();
			_editBox.PreviewKeyDown += editBoxPreviewKeyDown;
			_editBox.LostFocus += focusOver;
		}

		private void editBoxPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					{
						hideEditBox();
						listBoxChildSkills.ClearSelected();
						break;
					}
				case Keys.Enter:
					{
						e.IsInputKey = true;
						focusOver(null, null);
						break;
					}
			}
		}

		private void hideEditBox()
		{
			_editBox.Hide();
		}

		private void focusOver(object sender, EventArgs e)
		{
			var text = _editBox.Text.TrimEnd((char)13);
			if (!string.IsNullOrEmpty(text.Trim()))
			{
				((ChildSkill)listBoxChildSkills.Items[_itemSelected]).ChangeName(text);
				_childSkillNameUpdated = true;
			}
			hideEditBox();
			reloadListBox();
			listBoxChildSkills.ClearSelected();
			listBoxChildSkills.SetSelected(_itemSelected, true);
		}

		private void buttonAdvRenameClick(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			showEditBox();
		}
	}
}
