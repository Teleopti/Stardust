using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Intraday;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.WpfControls.Controls.Intraday
{
    /// <summary>
    /// Interaction logic for PinnedLayerView.xaml
    /// </summary>
    public partial class AgentStateLayerView
    {
        private readonly ObservableCollection<AgentStateViewAdapter> _agentStateViewAdapterCollection = new ObservableCollection<AgentStateViewAdapter>();
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        private DateTimePeriod _nowPeriod = new DateTimePeriod(DateTime.UtcNow, DateTime.UtcNow.AddMinutes(1));

        public AgentStateLayerView()
        {
            InitializeComponent();
            
            var view = (ListCollectionView)CollectionViewSource.GetDefaultView(mainGrid.ItemsSource);
            view.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            view.SortDescriptions.Add(new SortDescription() {PropertyName = "Sort"});
        }

        public ObservableCollection<AgentStateViewAdapter> AgentStateViewAdapterCollection
        {
            get { return _agentStateViewAdapterCollection; }
        }

        public void SetAgentStateViewAdapterCollection(IEnumerable<AgentStateViewAdapter> agentStateViewAdapters)
        {
            _agentStateViewAdapterCollection.Clear();
            foreach (var agentStateViewAdapter in agentStateViewAdapters)
            {
                _agentStateViewAdapterCollection.Add(agentStateViewAdapter);
            }
        }

        public DateTimePeriod NowPeriod
        {
            get { return _nowPeriod; }
            set
            {
                if (value == _nowPeriod) return;
                _nowPeriod = value;
                _agentStateViewAdapterCollection.ForEach(
                    a => a.Refresh());
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            string helpContext = HelpProvider.GetHelpString(this);
            if (string.IsNullOrEmpty(helpContext) && Parent == null) return;
            var elementParent = Parent as FrameworkElement;
            if (elementParent == null || elementParent.Parent == null) return;
            HelpProvider.SetHelpString(elementParent.Parent, helpContext);
        }
    }
}