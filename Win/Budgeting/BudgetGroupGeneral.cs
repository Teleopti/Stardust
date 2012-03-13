using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Win.Common;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.PropertyPageAndWizard;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting
{
	public partial class BudgetGroupGeneral : BaseUserControl, IPropertyPage
	{
		public BudgetGroupGeneral()
		{
			InitializeComponent();
			if (!DesignMode) SetTexts();
			SetColor();
		}

		private void SetColor()
		{
			BackColor = ColorHelper.WizardBackgroundColor();
		}

		public void Populate(IAggregateRoot aggregateRoot)
		{
			var budgetGroup = aggregateRoot as BudgetGroup;
			if (budgetGroup == null) return;
			
			textBoxName.Text = budgetGroup.Name;
			textBoxDaysPerYear.Text = string.Format(CurrentCulture,"{0}",budgetGroup.DaysPerYear);
			InitializeTimeZones(budgetGroup);
			InitializeSkills();
			InitializeAddedSkills(budgetGroup);
		}

		private void InitializeTimeZones(BudgetGroup budgetGroup)
		{
			comboBoxAdvTimeZones.DisplayMember = "DisplayName";
			foreach (var timeZoneInfo in TimeZoneInfo.GetSystemTimeZones())
			{
				var tzi = (new CccTimeZoneInfo(timeZoneInfo));
				comboBoxAdvTimeZones.Items.Add(tzi);
				if (budgetGroup.TimeZone.Id == tzi.Id)
					comboBoxAdvTimeZones.SelectedItem = tzi;
			}
		}

		private void InitializeAddedSkills(IBudgetGroup budgetGroup)
		{
			foreach (var skill in budgetGroup.SkillCollection)
			{
				listBoxSkills.Items.Remove(skill);
				listBoxAddedSkills.Items.Add(skill);
			}
		}

		private void InitializeSkills()
		{
			listBoxSkills.DisplayMember = "Name";
			listBoxAddedSkills.DisplayMember = "Name";
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var repFac = new RepositoryFactory();
			    var skills = repFac.CreateSkillRepository(uow).FindAllWithoutMultisiteSkills();
				listBoxSkills.Items.AddRange(skills.ToArray());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UserTexts"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "BudgetGroupNameIsInvalid"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Syncfusion.Windows.Forms.MessageBoxAdv.Show(System.String,System.String,System.Windows.Forms.MessageBoxButtons,System.Windows.Forms.MessageBoxIcon,System.Windows.Forms.MessageBoxDefaultButton,System.Windows.Forms.MessageBoxOptions)")]
		public bool Depopulate(IAggregateRoot aggregateRoot)
		{
			IBudgetGroup budgetGroup = aggregateRoot as BudgetGroup;
			try
			{
				if (budgetGroup != null)
				{
					budgetGroup.Name = textBoxName.Text.Trim();
					budgetGroup.TrySetDaysPerYear(Convert.ToInt32(textBoxDaysPerYear.Text.Trim(),CurrentCulture));
					budgetGroup.TimeZone = (ICccTimeZoneInfo) comboBoxAdvTimeZones.SelectedItem;

					budgetGroup.RemoveAllSkills();
					foreach (var item in listBoxAddedSkills.Items)
					{
						budgetGroup.AddSkill((ISkill)item);
					}
				}
			}
			catch (ArgumentException ex)
			{
				Syncfusion.Windows.Forms.MessageBoxAdv.Show(string.Concat("UserTexts.Resources.BudgetGroupNameIsInvalid", "  "), "", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, (RightToLeft == System.Windows.Forms.RightToLeft.Yes ? MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign : 0));
				Trace.WriteLine(ex.Message);
				return false;
			}

			return true;
		}

		public void SetEditMode()
		{

		}

		public string PageName
		{
			get { return UserTexts.Resources.NewBudgetGroup; }
		}

		private void buttonAdvAddAll_Click(object sender, EventArgs e)
		{
			listBoxAddedSkills.Items.AddRange(listBoxSkills.Items);
			listBoxSkills.Items.Clear();
		}

		private void buttonAdvAddSelected_Click(object sender, EventArgs e)
		{
			AddSelectedSkills();
		}

		private void AddSelectedSkills()
		{
			foreach (var item in listBoxSkills.SelectedItems)
			{
				listBoxAddedSkills.Items.Add(item);
			}

			foreach (var item in listBoxAddedSkills.Items)
			{
				listBoxSkills.Items.Remove(item);
			}
		}

		private void buttonAdvRemoveSelected_Click(object sender, EventArgs e)
		{
			RemoveSelectedSkills();
		}

		private void RemoveSelectedSkills()
		{
			foreach (var selectedItem in listBoxAddedSkills.SelectedItems)
			{
				listBoxSkills.Items.Add(selectedItem);
			}

			foreach (var item in listBoxSkills.Items)
			{
				listBoxAddedSkills.Items.Remove(item);
			}
		}

		private void buttonAdvRemoveAll_Click(object sender, EventArgs e)
		{
			listBoxSkills.Items.AddRange(listBoxAddedSkills.Items);
			listBoxAddedSkills.Items.Clear();
		}

		private void listBoxSkills_DoubleClick(object sender, EventArgs e)
		{
			AddSelectedSkills();
		}

		private void listBoxAddedSkills_DoubleClick(object sender, EventArgs e)
		{
			RemoveSelectedSkills();
		}
	}
}
