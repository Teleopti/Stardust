using System;
using System.Linq;
using Teleopti.Ccc.Domain.Budgeting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.SmartClientPortal.Shell.Win.Common;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.PropertyPageAndWizard;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Budgeting
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
				var tzi = ((timeZoneInfo));
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
                if (skill is IMultisiteSkill) continue;
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

		public bool Depopulate(IAggregateRoot aggregateRoot)
		{
			IBudgetGroup budgetGroup = aggregateRoot as BudgetGroup;
			if (budgetGroup != null)
			{
				budgetGroup.Name = textBoxName.Text.Trim();
				budgetGroup.TrySetDaysPerYear((int)textBoxDaysPerYear.IntegerValue);
				budgetGroup.TimeZone = (TimeZoneInfo) comboBoxAdvTimeZones.SelectedItem;

				budgetGroup.RemoveAllSkills();
				foreach (var item in listBoxAddedSkills.Items)
				{
					budgetGroup.AddSkill((ISkill) item);
				}
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
