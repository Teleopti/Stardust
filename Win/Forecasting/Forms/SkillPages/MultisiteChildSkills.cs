using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.Win.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Ccc.WinCode.Forecasting.SkillPages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.SkillPages
{
    public partial class MultisiteChildSkills : BaseUserControl, IPropertyPage
    {
        private MultisiteSkill _skill;
    	private TextBox _editBox;
    	private int _itemSelected;
    	private bool _childSkillNameUpdated;

    	public MultisiteChildSkills()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }

        public void Populate(IAggregateRoot aggregateRoot)
        {
            var skill = aggregateRoot as MultisiteSkill;
            _skill = skill;
            ReloadListBox();
        }
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddAHiddenTextBox();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void AddAHiddenTextBox()
    	{
    		_editBox = new TextBox {Location = new Point(0, 0), Size = new Size(0, 0)};
			_editBox.MaxLength = 50;
    		_editBox.Hide();
    		listBoxChildSkills.Controls.AddRange(new Control[] { _editBox });
    		_editBox.BackColor = listBoxChildSkills.BackColor;
    		_editBox.ForeColor = listBoxChildSkills.ForeColor;
    		_editBox.BorderStyle = BorderStyle.FixedSingle;
    	}

    	private void ReloadListBox()
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

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-02-19
        /// </remarks>
        public void SetEditMode()
        {
        }

        #endregion

        private void buttonAdvAdd_Click(object sender, EventArgs e)
        {
            var childSkill = new ChildSkill(
                            UserTexts.Resources.SubSkill, 
                            UserTexts.Resources.SubSkill,
                            _skill.DisplayColor,
                            _skill.DefaultResolution,
                            _skill.SkillType)
                             	{
                             		MidnightBreakOffset = _skill.MidnightBreakOffset
                             	};

        	SkillWizardPages.SetSkillDefaultSettings(childSkill);
            childSkill.TimeZone = _skill.TimeZone;
            childSkill.Activity = _skill.Activity;
            childSkill.SetParentSkill(_skill);
            DialogResult result;
            using (var swp = new SkillWizardPages(childSkill,new RepositoryFactory(),UnitOfWorkFactory.Current))
            {
				swp.Initialize(PropertyPagesHelper.GetSkillPages(false, swp, true), new LazyLoadingManagerWrapper());
                using (var wizard = new Wizard(swp))
                {
                    result = wizard.ShowDialog(this);
                }
            }
            if (result == DialogResult.Cancel)
            {
                _skill.RemoveChildSkill(childSkill);
            }
            else
            {
                _skill.AddChildSkill(childSkill);
            }
            ReloadListBox();
        }

        private void buttonAdvManageDayTemplates_Click(object sender, EventArgs e)
        {
            if (listBoxChildSkills.SelectedItem == null) return;
            var childSkill = (ChildSkill)listBoxChildSkills.SelectedItem;

			var skillDayTemplateTool = new SkillDayTemplates(childSkill, !_childSkillNameUpdated);
			skillDayTemplateTool.ShowDialog(this);
        }

        private void buttonAdvRemove_Click(object sender, EventArgs e)
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
                    ReloadListBox();
                }
            }
            else
            {
                ViewBase.ShowErrorMessage(UserTexts.Resources.SubSkillHasValueInTemplate,
                        UserTexts.Resources.SubSkillDistributions);
            }
        }

		private void listBoxChildSkills_DoubleClick(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			ShowEditBox();
		}

		private void ShowEditBox()
    	{
			_itemSelected = listBoxChildSkills.SelectedIndex;
			var rect = listBoxChildSkills.GetItemRectangle(_itemSelected);
			var itemText = ((ChildSkill)listBoxChildSkills.Items[_itemSelected]).Name;
			_editBox.Location = new System.Drawing.Point(rect.X, rect.Y);
			_editBox.Size = new System.Drawing.Size(rect.Width, rect.Height);
			_editBox.Show();
			_editBox.Text = itemText;
			_editBox.Focus();
			_editBox.SelectAll();
			_editBox.PreviewKeyDown += EditBox_PreviewKeyDown;
			_editBox.LostFocus += FocusOver;
    	}

    	private void EditBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Escape:
					{
						HideEditBox();
						listBoxChildSkills.ClearSelected();
						break;
					}
				case Keys.Enter:
					{
						e.IsInputKey = true;
						FocusOver(null, null);
						break;
					}
			}
    	}

    	private void HideEditBox()
		{
			_editBox.Hide();
		}

    	private void FocusOver(object sender, EventArgs e)
    	{
			var text = _editBox.Text.TrimEnd((char)13);
			if (!string.IsNullOrEmpty(text.Trim()))
			{
				((ChildSkill) listBoxChildSkills.Items[_itemSelected]).Name = text;
				_childSkillNameUpdated = true;
			}
    		HideEditBox();
			ReloadListBox();
			listBoxChildSkills.ClearSelected();
			listBoxChildSkills.SetSelected(_itemSelected, true);
    	}

		private void buttonAdvRename_Click(object sender, EventArgs e)
		{
			if (listBoxChildSkills.SelectedItem == null) return;
			ShowEditBox();
		}
    }
}
