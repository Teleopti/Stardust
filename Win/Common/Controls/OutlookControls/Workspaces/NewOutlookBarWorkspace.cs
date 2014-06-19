using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.Win.Common.Controls.OutlookControls.Workspaces
{
    public partial class NewOutlookBarWorkspace : UserControl, IOutlookBarWorkspaceView 
    {
        private readonly OutlookBarWorkspacePresenter _presenter;
        private readonly IClientPortalCallback _clientPortalCallback;

        public NewOutlookBarWorkspace(IClientPortalCallback clientPortalCallback, OutlookBarWorkspaceModel model)
        {
            _clientPortalCallback = clientPortalCallback;
            _presenter = new OutlookBarWorkspacePresenter(this, model);
            InitializeComponent();
			//groupBarModules.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
			//groupBarModules.ForeColor = Color.Violet;

        }

        public int NumberOfVisibleGroupBars
        {
            get
            {
                int count = 0;
                foreach (GroupBarItem item in groupBarModules.GroupBarItems)
                {
                    if (!item.InNavigationPane) { count++; }
                }
                return count;
            }
        }

		

        public void SetNumberOfVisibleGroupBars(int visibleCount)
        {
            visibleCount = Math.Min(visibleCount, groupBarModules.GroupBarItems.Count);
            for (var i = 0; i < visibleCount; i++)
                groupBarModules.GroupBarItems[i].InNavigationPane = false;
        }

        private void NewOutlookBarWorkspaceLoad(object sender, EventArgs e)
		{
			groupBarModules.GroupBarItemSelected += GroupBarModulesGroupBarItemSelected;
			groupBarModules.MouseUp += GroupBarModulesMouseUp;

			_presenter.Initialize();
        }

		void GroupBarModulesMouseUp(object sender, MouseEventArgs e)
		{
			var item = groupBarModules.PointToItem(e.Location);
			if (item == null) return;

            var selectedGroupBarItem = groupBarModules.VisibleGroupBarItems[groupBarModules.SelectedItem];

			if (selectedGroupBarItem == item)
				_clientPortalCallback.SelectedModule(selectedGroupBarItem, _startup);
		}

        void GroupBarModulesGroupBarItemSelected(object sender, EventArgs e)
        {
            var selectedGroupBarItem = groupBarModules.VisibleGroupBarItems[groupBarModules.SelectedItem];
            LastModule = selectedGroupBarItem.Tag.ToString();
            _clientPortalCallback.SelectedModule(selectedGroupBarItem, _startup);
        }

        public string LastModule { get; set; }

        public void StartupModule(string module)
        {
        	_startup = true;

        	groupBarModules.SelectedItem = -1;
            if(module==null)
            {
				if(groupBarModules.GroupBarItems.Count>0)
				{
					groupBarModules.SelectedItem = 0;
				}
            	_startup = false;
            	return;
            }

            var index = 0; 
            foreach (GroupBarItem item in groupBarModules.GroupBarItems)
            {
                if (module.Equals(item.Tag))
                {
                    groupBarModules.SelectedItem = index;
                }
                index++;
            }
        	_startup = false;
        }

        // fulfix because when changing Theme in Windows these will be created again
        private bool _created;
    	private bool _startup;

    	public void CreateGroupBars()
        {
            if(_created) return;
            groupBarModules.GroupBarItems.AddRange(_presenter.GetItems().Select(CreateGroupBar).ToArray());
            _created = true;
        }

        private GroupBarItem CreateGroupBar(OutlookBarInfo outlookBarSmartPartInfo)
        {
            using (var groupBarItem = new GroupBarItem())
            {
                groupBarItem.Padding = 4;
                groupBarItem.Enabled = outlookBarSmartPartInfo.Enable;
                groupBarItem.Text = outlookBarSmartPartInfo.Title;
                groupBarItem.Icon = _presenter.ConvertToIcon(outlookBarSmartPartInfo.Icon, outlookBarSmartPartInfo.Icon.Height, true);
                groupBarItem.Tag = outlookBarSmartPartInfo;
                groupBarItem.BackColor = Color.White;
                groupBarItem.Tag = outlookBarSmartPartInfo.EventTopicName;
				groupBarItem.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 0);
	            groupBarItem.ForeColor = groupBarModules.HeaderForeColor;
				groupBarItem.ClientBorderColors = new BorderColors(Color.White, Color.FromArgb(22, 165, 220), Color.FromArgb(22, 165, 220), Color.White);
                groupBarItem.InNavigationPane = true;

                return groupBarItem;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ParentForm.Closing += ParentForm_Closing;
        }

        void ParentForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _presenter.Close();
        }
    }

    public interface IOutlookBarWorkspaceView
    {
        int NumberOfVisibleGroupBars { get; }
        void SetNumberOfVisibleGroupBars(int visibleCount);
        string LastModule { get; set; }
        void StartupModule(string module);
        void CreateGroupBars();
    }

    public interface IClientPortalCallback
    {
        void SelectedModule(GroupBarItem selectedItem, bool startup);
    }
}