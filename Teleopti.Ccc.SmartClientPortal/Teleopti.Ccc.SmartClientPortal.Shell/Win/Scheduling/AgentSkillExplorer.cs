using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling
{
	public partial class AgentSkillExplorer : Form
	{
		private readonly IList<AgentSkillExplorerRow> _rows = new List<AgentSkillExplorerRow>();
		private readonly IList<ISkill> _skills = new List<ISkill>();
 
		public AgentSkillExplorer()
		{
			InitializeComponent();
		}

		public void Setup(ISchedulerStateHolder stateHolder, ILifetimeScope container)
		{
			var singleSkillDictionary = container.Resolve<ISingleSkillDictionary>();
			singleSkillDictionary.Create(stateHolder.SchedulingResultState.LoadedAgents.ToList(), stateHolder.RequestedPeriod.DateOnlyPeriod);
			listView.View = View.Details;
			listView.GridLines = true;
			listView.FullRowSelect = true;
			SetupColumns(stateHolder);
			SetupRows(stateHolder, singleSkillDictionary);
			SetupComboSkills();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "xxAll"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "Teleopti.Ccc.Win.Scheduling.AgentSkillExplorerComboItem.set_Text(System.String)")]
		public void SetupComboSkills()
		{
			var item = new AgentSkillExplorerComboItem();
			item.Text = "xxAll";
			item.Value = null;
			comboBox.Items.Add(item);

			foreach (var skill in _skills)
			{
				var skillItem = new AgentSkillExplorerComboItem();
				skillItem.Text = skill.Name;
				skillItem.Value = skill;
				comboBox.Items.Add(skillItem);
			}

			comboBox.SelectedIndex = 0;
			comboBox.SelectedIndexChanged += new System.EventHandler(comboBox_SelectedIndexChanged);
		}

		void comboBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(checkBox.Checked)
				AddRows(SingleSkillOnDoubleSkillList());

		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ListView+ColumnHeaderCollection.Add(System.String,System.String)")]
		public void SetupColumns(ISchedulerStateHolder stateHolder)
		{
			listView.Columns.Add("Name", "Name");

			foreach (var skill in stateHolder.SchedulingResultState.Skills)
			{
				if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
				{
					//listView.Columns.Add(skill.Id.ToString(), skill.Name + "(*)");
				}
				else
				{
					listView.Columns.Add(skill.Id.ToString(), skill.Name);
					_skills.Add(skill);
				}
			}	
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void SetupRows(ISchedulerStateHolder stateHolder, ISingleSkillDictionary singleSkillDictionary)
		{
			_rows.Clear();
				
			var periods = new List<IPersonPeriod>();

			var period = stateHolder.RequestedPeriod.DateOnlyPeriod;

			foreach (var person in stateHolder.SchedulingResultState.LoadedAgents)
			{
				foreach (var day in period.DayCollection())
				{
					var personPeriod = person.Period(day);
					if (personPeriod == null) continue;
					if (periods.Count == 0 || !periods.Last().Equals(personPeriod))
					{
						periods.Add(personPeriod);
						var skills = Skills((personPeriod));

						var hasOpenSkill = skills.Any(skill => _skills.Contains(skill));
						if (hasOpenSkill)
						{
							var isSingleSkill = singleSkillDictionary.IsSingleSkill(person, day);
							var row = new AgentSkillExplorerRow(person, personPeriod.Period, skills, isSingleSkill);

							_rows.Add(row);
						}
					}
				}
			}

			AddRows(SortedList());
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Windows.Forms.ListViewItem+ListViewSubItemCollection.Add(System.String)")]
		private void AddRows(IEnumerable<AgentSkillExplorerRow> list)
		{
			listView.Items.Clear();

			foreach (var row in list)
			{
				var newItem = new ListViewItem(row.Name);
				if (row.TrueSingleSkill) newItem.BackColor = Color.LightGreen;
				else if (row.SingleSkill() != null) newItem.BackColor = Color.LightSkyBlue;
				else newItem.BackColor = Color.LightCoral;

				foreach (var skill in _skills)
				{
					if(row.Skills.Contains(skill))
					{
						newItem.SubItems.Add("X");
					}
					else
					{
						newItem.SubItems.Add("");
					}
				}

				listView.Items.Add(newItem);
			}	
		}

		private IEnumerable<AgentSkillExplorerRow> SingleSkillOnDoubleSkillList()
		{
			var singleSkillRows = new List<AgentSkillExplorerRow>();
			var singleDoubleSkillRows = new List<AgentSkillExplorerRow>();

			foreach (var agentSkillExplorerRow in _rows)
			{
				if(agentSkillExplorerRow.SingleSkill() != null)
					singleSkillRows.Add(agentSkillExplorerRow);
			}

			foreach (var singleSkill in singleSkillRows)
			{
				var selectedItem = comboBox.SelectedItem as AgentSkillExplorerComboItem;
				if (selectedItem.Value != null && !singleSkill.SingleSkill().Equals(selectedItem.Value))
					continue;

				foreach (var agentSkillExplorerRow in _rows)
				{
					if (singleSkillRows.Contains(agentSkillExplorerRow))
						continue;

					if (agentSkillExplorerRow.Skills.Count > 0)
					{
						if (agentSkillExplorerRow.Skills.Contains(singleSkill.SingleSkill()))
						{
							if (!singleDoubleSkillRows.Contains(agentSkillExplorerRow))
								singleDoubleSkillRows.Add(agentSkillExplorerRow);
						}
					}
				}
			}

			return singleDoubleSkillRows;
		}

		private IEnumerable<AgentSkillExplorerRow> SortedList()
		{	
			var sorted = _rows.OrderByDescending(s => s.Skills.Count).ThenBy(n => n.Name).ToList();
			return sorted;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		private IList<ISkill> Skills(IPersonPeriod personPeriod)
		{
			var skills = new List<ISkill>();

			var activePersonSkills = (from a in personPeriod.PersonSkillCollection
									  where a.Active && !((IDeleteTag)a.Skill).IsDeleted
									  select a).ToList();

			foreach (var personSkill in activePersonSkills)
			{
				skills.Add(personSkill.Skill);	
			}

			//foreach (var personSkill in personPeriod.PersonMaxSeatSkillCollection)
			//{
			//    skills.Add(personSkill.Skill);
			//}

			return skills;
		}

		private void buttonClose_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void checkBox_CheckedChanged(object sender, System.EventArgs e)
		{
			if(!checkBox.Checked)
				AddRows(SortedList());
			else
			{
				AddRows(SingleSkillOnDoubleSkillList());
			}
		}
	}

	public class AgentSkillExplorerComboItem
	{
		public string Text { get; set; }
		public object Value { get; set; }

		public override string ToString()
		{
			return Text;
		}	
	}

	public class AgentSkillExplorerRow
	{
		private readonly IPerson _person;

		public AgentSkillExplorerRow(IPerson person, DateOnlyPeriod period, IList<ISkill> skills, bool trueSingleSkill)
		{
			_person = person;
			Skills = skills;
			TrueSingleSkill = trueSingleSkill;
			Period = period;
		}

		public ISkill SingleSkill()
		{
			ISkill singleSkill = null;
			var count = 0;

			foreach (var skill in Skills)
			{
				//if (skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
				if(skill.Activity != null)
				{
					singleSkill = skill;
					count++;
				}
			}

			if (count == 1)
				return singleSkill;

			return null;
		}

		public string Name
		{
			get { return _person.Name.FirstName + " " + _person.Name.LastName + " " + Period.DateString + "(" + Skills.Count + ")"; }
		}

		public IList<ISkill> Skills { get; private set; }
		public bool TrueSingleSkill { get; private set; }
		public DateOnlyPeriod Period { get; private set; }
	}
}
