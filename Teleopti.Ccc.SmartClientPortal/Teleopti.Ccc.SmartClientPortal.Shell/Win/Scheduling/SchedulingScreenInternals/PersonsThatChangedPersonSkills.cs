using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.Domain.AgentInfo;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public partial class PersonsThatChangedPersonSkills : Form
	{
		private readonly IList<PersonsThatChangedPersonSkillsDuringPeriodFinder.PersonsThatChangedPersonSkillsDuringPeriodFinderResult> _results;

		public PersonsThatChangedPersonSkills()
		{
			InitializeComponent();
		}

		public PersonsThatChangedPersonSkills(IList<PersonsThatChangedPersonSkillsDuringPeriodFinder.PersonsThatChangedPersonSkillsDuringPeriodFinderResult> results)
		{
			_results = results;
			InitializeComponent();
		}

		private void personsThatChangedPersonSkillsDuringPeriodFinderViewLoad(object sender, System.EventArgs e)
		{
			foreach (var personsThatChangedPersonSkillsDuringPeriodFinderResult in _results)
			{
				foreach (var addedSkill in personsThatChangedPersonSkillsDuringPeriodFinderResult.AddedSkills)
				{
					var item = new ListViewItem(personsThatChangedPersonSkillsDuringPeriodFinderResult.Person.Name.ToString());
					item.SubItems.Add(personsThatChangedPersonSkillsDuringPeriodFinderResult.Date.ToShortDateString());
					item.SubItems.Add(addedSkill.Name);
					listView1.Items.Add(item);
				}
				foreach (var removedSkill in personsThatChangedPersonSkillsDuringPeriodFinderResult.RemovedSkills)
				{
					var item = new ListViewItem(personsThatChangedPersonSkillsDuringPeriodFinderResult.Person.Name.ToString());
					item.SubItems.Add(personsThatChangedPersonSkillsDuringPeriodFinderResult.Date.ToShortDateString());
					item.SubItems.Add("");
					item.SubItems.Add(removedSkill.Name);
					listView1.Items.Add(item);
				}
			}
		}
	}
}
