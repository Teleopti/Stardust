using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Teleopti.Ccc.WinCode.Grouping;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingSessionPreferences
{
    public partial class TempNonBlendSchedulingPreferences : Form
    {
        private readonly ISchedulingOptions _defaultOptions;
    	private readonly ISchedulerGroupPagesProvider _groupPagesProvider;
    	private readonly IList<IGroupPageLight> _groupPages;
        private readonly IList<IActivity> _activities;
        //

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public TempNonBlendSchedulingPreferences(ISchedulingOptions defaultOptions, ISchedulerGroupPagesProvider groupPagesProvider, IList<IActivity> activities):this()
        {
            _defaultOptions = defaultOptions;
        	_groupPagesProvider = groupPagesProvider;
        	_groupPages = _groupPagesProvider.GetGroups(false);
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
                _defaultOptions.GroupOnGroupPage = (IGroupPageLight)comboBoxGrouping.SelectedItem;
            Close();
        }

        
    }
}
