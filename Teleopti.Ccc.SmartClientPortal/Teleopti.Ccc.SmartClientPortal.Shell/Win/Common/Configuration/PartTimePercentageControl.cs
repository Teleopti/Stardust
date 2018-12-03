using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{

    public partial class PartTimePercentageControl : BaseUserControl, ISettingPage
    {
        private List<IPartTimePercentage> _partTimePercentageList;
        private readonly LocalizedUpdateInfo _localizer = new LocalizedUpdateInfo();
        private const short invalidItemIndex = -1;
        private const short firstItemIndex = 0;
        private const short itemDiffernce = 1;
        public PartTimePercentageRepository Repository { get; private set; }

        public IUnitOfWork UnitOfWork { get; private set; }

        private IPartTimePercentage SelectedPartTimePercentage
        {
            get
            {
                return comboBoxPercentageCollection.SelectedItem as IPartTimePercentage;
            }
        }

        public int LastItemIndex
        {
            get { return comboBoxPercentageCollection.Items.Count - itemDiffernce; }
        }

        public PartTimePercentageControl()
        {
            InitializeComponent();

            // Binds events.
            comboBoxPercentageCollection.SelectedIndexChanging += comboBoxPercentageCollectionSelectedIndexChanging;
            comboBoxPercentageCollection.SelectedIndexChanged += comboBoxPercentageCollectionSelectedIndexChanged;
            textBoxDescription.Validating += textBoxDescriptionValidating;
            textBoxDescription.Validated += textBoxDescriptionValidated;
            doubleTextBox1.Validated += doubleTextBox1Validated;
            buttonNew.Click += buttonNewClick;
            buttonDelete.Click += buttonDeleteClick;
        }

        void doubleTextBox1Validated(object sender, EventArgs e)
        {
            SelectedPartTimePercentage.Percentage = new Percent(doubleTextBox1.DoubleValue/ 100);
        }



        private void textBoxDescriptionValidating(object sender, CancelEventArgs e)
        {
            if (SelectedPartTimePercentage != null)
            {
                e.Cancel = !validatePartTimePercentageDescription();
            }
        }

        private void textBoxDescriptionValidated(object sender, EventArgs e)
        {
            if (SelectedPartTimePercentage != null)
            {
                changePartTimePercentageDescription();
            }
        }

        private void comboBoxPercentageCollectionSelectedIndexChanged(object sender, EventArgs e)
        {
        	if (SelectedPartTimePercentage == null) return;
        	selectPartTimePercentage();
        	changedInfo();
        }

        private void comboBoxPercentageCollectionSelectedIndexChanging(object sender, SelectedIndexChangingArgs e)
        {
            e.Cancel = !isWithinRange(e.NewIndex);
        }

        private void buttonNewClick(object sender, EventArgs e)
        {
        	if (SelectedPartTimePercentage == null) return;
        	Cursor.Current = Cursors.WaitCursor;
        	addNewPartTimePercentage();
        	Cursor.Current = Cursors.Default;
        }

        private void buttonDeleteClick(object sender, EventArgs e)
        {
        	if (SelectedPartTimePercentage == null) return;
        	var text = string.Format(
        		CurrentCulture,
        		Resources.AreYouSureYouWantToDeletePartTimePercentage,
        		SelectedPartTimePercentage.Description
        		);

        	var caption = string.Format(CurrentCulture, Resources.ConfirmDelete);

        	var response = ViewBase.ShowConfirmationMessage(text, caption);

        	if (response != DialogResult.Yes) return;
        	Cursor.Current = Cursors.WaitCursor;
        	deletePartTimePercentage();
        	Cursor.Current = Cursors.Default;
        }

        public void SetUnitOfWork(IUnitOfWork value)
        {
            UnitOfWork = value;
            Repository = new PartTimePercentageRepository(UnitOfWork);
        }

        public void Persist()
        {}

        public TreeFamily TreeFamily()
        {
            return new TreeFamily(Resources.Contract);
        }

        public string TreeNode()
        {
            return Resources.PartTimepercentages;
        }

    	public void OnShow()
    	{
    	}

    	public void InitializeDialogControl()
        {
            setColors();
            SetTexts();
        }

        public void LoadControl()
        {
            //Bind data to combo box
            loadPartTimePercentages();
        }

        public void Unload()
        {
        }

        public void SaveChanges()
        {}

        protected override void SetCommonTexts()
        {
            base.SetCommonTexts();
            toolTip1.SetToolTip(buttonDelete, Resources.DeletePartTimePercentage);
            toolTip1.SetToolTip(buttonNew, Resources.NewPartTimePercentage);
        }

        private void changedInfo()
        {
            autoLabelInfoAboutChanges.ForeColor = ColorHelper.ChangeInfoTextColor();
            autoLabelInfoAboutChanges.Font = ColorHelper.ChangeInfoTextFontStyleItalic(autoLabelInfoAboutChanges.Font);
            string changed = _localizer.UpdatedByText(SelectedPartTimePercentage, Resources.UpdatedByColon);
            autoLabelInfoAboutChanges.Text = changed;
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

            tableLayoutPanelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.BackColor = ColorHelper.OptionsDialogSubHeaderBackColor();
            labelSubHeader2.ForeColor = ColorHelper.OptionsDialogSubHeaderForeColor();
        }

        private void changePartTimePercentageDescription()
        {
            SelectedPartTimePercentage.Description = new Description(textBoxDescription.Text);

            loadPartTimePercentages();
        }

        private bool validatePartTimePercentageDescription()
        {
					bool failed = string.IsNullOrWhiteSpace(textBoxDescription.Text);
	        if (failed)
		        textBoxDescription.Text = SelectedPartTimePercentage.Description.Name;

	        return !failed;
        }

        private void addNewPartTimePercentage()
        {
            _partTimePercentageList.Add(createPartTimePercentage());

            loadPartTimePercentages();

            comboBoxPercentageCollection.SelectedIndex = LastItemIndex;
        }

        private void deletePartTimePercentage()
        {
            Repository.Remove(SelectedPartTimePercentage);
            _partTimePercentageList.Remove(SelectedPartTimePercentage);

            loadPartTimePercentages();
        }

        private bool isWithinRange(int index)
        {
            return index > invalidItemIndex && index < _partTimePercentageList.Count && comboBoxPercentageCollection.DataSource != null;
        }

        private void loadPartTimePercentages()
        {
            if (_partTimePercentageList == null)
            {
                _partTimePercentageList = new List<IPartTimePercentage>(Repository.FindAllPartTimePercentageByDescription());
            }

            if (_partTimePercentageList.IsEmpty())
            {
                _partTimePercentageList.Add(createPartTimePercentage());
            }

            // Removes binding from comboBoxAdvPercentageCollection.
            int selected = comboBoxPercentageCollection.SelectedIndex;
            if (!isWithinRange(selected))
            {
                selected = firstItemIndex;
            }

            // Rebinds list to comboBoxAdvPercentageCollection.
            comboBoxPercentageCollection.DataSource = null;
            comboBoxPercentageCollection.DisplayMember = "Description";
            comboBoxPercentageCollection.DataSource = _partTimePercentageList;

            comboBoxPercentageCollection.SelectedIndex = selected;

        }

        private IPartTimePercentage createPartTimePercentage()
        {
            Description description = PageHelper.CreateNewName(_partTimePercentageList,
                "Description.Name",
                Resources.NewPartTimePercentage);

            IPartTimePercentage newParTimePercentage = new PartTimePercentage(description.Name) { Description = description };
            Repository.Add(newParTimePercentage);

            return newParTimePercentage;
        }

        private void selectPartTimePercentage()
        {
        	if (SelectedPartTimePercentage == null) return;
        	textBoxDescription.Text = SelectedPartTimePercentage.Description.Name;
            doubleTextBox1.DoubleValue = SelectedPartTimePercentage.Percentage.Value*100;
        }

        public void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity)
        {
            throw new NotImplementedException();
        }

        public ViewType ViewType
        {
            get { return ViewType.PartTimePercentage; }
        }
    }
}
