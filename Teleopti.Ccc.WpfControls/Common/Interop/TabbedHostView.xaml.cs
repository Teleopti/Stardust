using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.GuiHelpers;
using Teleopti.Ccc.WinCode.Common.Interop;
using Teleopti.Ccc.WpfControls.Controls.Editor;
using Teleopti.Ccc.WpfControls.Controls.Notes;

namespace Teleopti.Ccc.WpfControls.Common.Interop
{
    /// <summary>
    /// Interaction logic for TabbedHostView.xaml
    /// </summary>
    public partial class TabbedHostView : UserControl
    {
        public TabbedHostView()
        {
            InitializeComponent();
        }

        private void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            var count = tabControl.Items.Count;
            for (int i = 0; i < count; i++)
            {
                var itemHostViewModel = tabControl.Items[i] as HostViewModel;
                if (itemHostViewModel==null) continue;
                var helpStringForChild =
                    HelpProvider.GetHelpString((DependencyObject)itemHostViewModel.ModelContent);
                if (string.IsNullOrEmpty(helpStringForChild)) continue;
                var container = tabControl.ItemContainerGenerator.ContainerFromIndex(i);
                HelpProvider.SetHelpString(container,helpStringForChild);
            }
        }

        private void tabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HostViewModel current = null;
            if (tabControl.SelectedItem is HostViewModel)
                current = tabControl.SelectedItem as HostViewModel;
            ClearHeaders();
            UpdateHeaders(current);
        }

        private void ClearHeaders()
        {
            foreach (var item in tabControl.Items)
            {
                var itemHostViewModel = item as HostViewModel;

                if (itemHostViewModel != null)
                    if (itemHostViewModel.ModelHeader.ToString().Contains("("))
                    {
                        var name = itemHostViewModel.ModelHeader.ToString().Split(new[] { '(' });
                        itemHostViewModel.UpdateItem(name[0].TrimEnd(new[] { ' ' }), itemHostViewModel.ModelContent);
                    }
            }
        }

        private void UpdateHeaders(HostViewModel current)
        {
            foreach (var item in tabControl.Items)
            {
                var itemHostViewModel = item as HostViewModel;
                if (itemHostViewModel != current) continue;
                if (current == null) continue;
                WpfShiftEditor wpfShiftEditor;
                NotesEditor notesEditor;
                string agentName;
                if (current.ModelContent is WpfShiftEditor)
                {
                    wpfShiftEditor = current.ModelContent as WpfShiftEditor;
                    if (wpfShiftEditor.SchedulePart != null)
                    {
                        agentName = wpfShiftEditor.SchedulePart.Person.Name + " " +
									wpfShiftEditor.SchedulePart.DateOnlyAsPeriod.DateOnly.ToShortDateString();
                        SplitAndUpdate(current, agentName);
                    }
                }
                else if (current.ModelContent is NotesEditor)
                {
                    notesEditor = current.ModelContent as NotesEditor;
                    if (notesEditor.SchedulePart != null)
                    {
                        agentName = notesEditor.SchedulePart.Person.Name + " " +
									notesEditor.SchedulePart.DateOnlyAsPeriod.DateOnly.ToShortDateString();
                        SplitAndUpdate(current, agentName);
                    }
                }
            }
        }

        private static void SplitAndUpdate(HostViewModel current, string agentName)
        {
            var split = current.ModelHeader.ToString().Split(new[] { '(' });
            current.UpdateItem(split[0] + " (" + agentName + ")", current.ModelContent);
        }
    }
}
