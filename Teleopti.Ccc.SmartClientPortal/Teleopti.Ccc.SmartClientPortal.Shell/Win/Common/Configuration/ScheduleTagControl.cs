using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{
	public partial class ScheduleTagControl : BaseUserControl, ISettingPage
	{
		private List<IScheduleTag> _scheduleTags;
		private const short itemDifference = 1;
		private const short invalidItemIndex = -1;
		private const short firstItemIndex = 0;
		public ScheduleTagRepository Repository { get; private set; }
		public IUnitOfWork UnitOfWork { get; private set; }
		private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();

		public int LastItemIndex
		{
			get { return comboBoxTags.Items.Count - itemDifference; }
		}

		public IScheduleTag SelectedTag
		{
			get { return comboBoxTags.SelectedItem as IScheduleTag; }
		}

		public ScheduleTagControl()
		{
			InitializeComponent();
		}

		void textBoxNameTextChanged(object sender, EventArgs e)
		{
			changeTagName();
		}

		private void comboBoxTagsSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
		{
			e.Cancel = !isWithinRange(e.NewIndex);
		}

		private void comboBoxTagsSelectedIndexChanged(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;
			textBoxName.TextChanged -= textBoxNameTextChanged;
			selectTag();
			if (SelectedTag != null)
				changedInfo();
			Cursor.Current = Cursors.Default;
			textBoxName.TextChanged += textBoxNameTextChanged;
		}        

		private void buttonNewClick(object sender, EventArgs e)
		{
			//if (SelectedTag == null) return;
			Cursor.Current = Cursors.WaitCursor;

			addNewTag();

			Cursor.Current = Cursors.Default;
		}

		private void buttonDeleteClick(object sender, EventArgs e)
		{
			if (SelectedTag == null) return;
			var culture = TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.Culture;
			string text = string.Format(
				culture,
				Resources.AreYouSureYouWantToDeleteItem,
				SelectedTag.Description
				);

			var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

			var response = ViewBase.ShowConfirmationMessage(text, caption);

			if (response != DialogResult.Yes) return;
			Cursor.Current = Cursors.WaitCursor;

			deleteTag();

			Cursor.Current = Cursors.Default;
		}

		private void textBoxNameValidating(object sender, CancelEventArgs e)
		{
			if (SelectedTag != null)
			{
				e.Cancel = !validateTag();
			}
		}

		private void textBoxNameValidated(object sender, EventArgs e)
		{
			changeTagName();
		}

		private void setColors()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
			tableLayoutPanelBody.BackColor = ColorHelper.WizardBackgroundColor();

			gradientPanelHeader.BackColor = ColorHelper.OptionsDialogHeaderBackColor();
			labelHeader.ForeColor = ColorHelper.OptionsDialogHeaderForeColor();

			tableLayoutPanelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
			labelSubHeader1.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
		}

		private IScheduleTag createTag()
		{
			var description = PageHelper.CreateNewName(_scheduleTags, "Description", Resources.NewTagExample);
			var len = description.Name.Length > 15 ? 15 : description.Name.Length;
			var newTag = new ScheduleTag { Description = description.Name.Substring(0,len) };
			Repository.Add(newTag);

			return newTag;
		}

		private void addNewTag()
		{
			_scheduleTags.Add(createTag());
			loadTags();
			comboBoxTags.SelectedIndex = LastItemIndex;
			textBoxName.Focus();
			textBoxName.SelectAll();
		}

		private void deleteTag()
		{
			if (SelectedTag == null) return;
			Repository.Remove(SelectedTag);
			_scheduleTags.Remove(SelectedTag);

			loadTags();
		}

		private void loadTags()
		{
			if (Disposing) return;
			if (_scheduleTags == null)
			{
				_scheduleTags = new List<IScheduleTag>();
				_scheduleTags.AddRange(Repository.FindAllScheduleTags());
			}

			if (_scheduleTags.IsEmpty())
			{
				_scheduleTags.Add(createTag());
			}

			var selected = comboBoxTags.SelectedIndex;
			if (!isWithinRange(selected))
			{
				selected = firstItemIndex;
			}

			comboBoxTags.DataSource = null;
			comboBoxTags.DisplayMember = "Description";
			comboBoxTags.DataSource = _scheduleTags;

			comboBoxTags.SelectedIndex = selected;
		}

		private void selectTag()
		{
			if (SelectedTag != null)
			{
				textBoxName.Text = SelectedTag.Description;
			}
		}

		private void changeTagName()
		{
			if (SelectedTag == null) return;
			SelectedTag.Description = textBoxName.Text;
			comboBoxTags.Invalidate();
			loadTags();
		}

		private bool validateTag()
		{
			var failed = string.IsNullOrEmpty(textBoxName.Text);
			if (failed)
			{
				textBoxName.Text = SelectedTag.Description;
			}

			return !failed;
		}

		private bool isWithinRange(int index)
		{
			return index > invalidItemIndex && index < _scheduleTags.Count && comboBoxTags.DataSource != null;
		}

		protected override void SetCommonTexts()
		{
			base.SetCommonTexts();
			toolTip1.SetToolTip(buttonDelete, Resources.DeleteTag);
			toolTip1.SetToolTip(buttonNew, Resources.NewTag);
		}

		public void InitializeDialogControl()
		{
			setColors();
			SetTexts();
		}

		public void LoadControl()
		{
			loadTags();
		}

		public void SaveChanges()
		{}

		public void Unload()
		{
			_scheduleTags = null;
		}

		public TreeFamily TreeFamily()
		{
			return new TreeFamily(Resources.Scheduling);
		}

		public string TreeNode()
		{
			return Resources.ScheduleTags;
		}

		public void OnShow()
		{
		}

		public void SetUnitOfWork(IUnitOfWork value)
		{
			UnitOfWork = value;
			Repository = ScheduleTagRepository.DONT_USE_CTOR(UnitOfWork);
		}

		public void Persist()
		{}

		private void changedInfo()
		{
			autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
			autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
			var changed = _localizer.UpdatedByText(SelectedTag, Resources.UpdatedByColon);
			autoLabelInfoAboutChanges.Text = changed;
		}

		public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
		{
			throw new NotImplementedException();
		}

		public ViewType ViewType
		{
			get { return ViewType.ScheduleTag; }
		}
	}
}
