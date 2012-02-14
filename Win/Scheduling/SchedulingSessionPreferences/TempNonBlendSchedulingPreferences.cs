using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class TempNonBlendSchedulingPreferences : Form
    {
        private readonly ISchedulingOptions _defaultOptions;
        private readonly IList<IGroupPage> _groupPages;
        private IList<IActivity> _activities;
        //

        public TempNonBlendSchedulingPreferences(ISchedulingOptions defaultOptions, IEnumerable<IGroupPage> groupPages, IList<IActivity> activities):this()
        {
            _defaultOptions = defaultOptions;
            // inte skill här heller va??
            var specification = new NotSkillGroupSpecification();
            _groupPages = new List<IGroupPage>(groupPages).FindAll(specification.IsSatisfiedBy);
            _activities = (from a in activities where a.RequiresSkill select a).ToList();
            initGroupPages();
            initActivities();
        }

        public TempNonBlendSchedulingPreferences()
        {
            InitializeComponent();
        }

        private void initGroupPages()
        {
            comboBoxGrouping.DataSource = _groupPages;
            comboBoxGrouping.DisplayMember = "Description";

            if (_defaultOptions.GroupOnGroupPage != null)
            {
                comboBoxGrouping.SelectedItem = _defaultOptions.GroupOnGroupPage;
            }
        }

        private void initActivities()
        {
            comboBoxActivities.DataSource = _activities;
            comboBoxActivities.DisplayMember = "Description";
        }

        public IActivity SelectedActivity()
        {
            return (IActivity)comboBoxActivities.SelectedItem;
        }

        public int Demand()
        {
            return (int)integerTextBox1.IntegerValue;
        }

        private void Button1Click(object sender, System.EventArgs e)
        {
            if (comboBoxGrouping.Items.Count > 0)
                _defaultOptions.GroupOnGroupPage = (IGroupPage)comboBoxGrouping.SelectedItem;
            Close();
        }

        
    }
}
