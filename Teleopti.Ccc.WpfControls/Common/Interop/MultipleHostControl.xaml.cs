using System.Windows.Controls;
using Teleopti.Ccc.WinCode.Common.Interop;
using Teleopti.Ccc.WpfControls.Controls.Editor;
using Teleopti.Ccc.WpfControls.Controls.Notes;
using Teleopti.Ccc.WpfControls.Controls.Restriction;

namespace Teleopti.Ccc.WpfControls.Common.Interop
{
    /// <summary>
    /// Interaction logic for MultipleHostControl.xaml
    /// </summary>
    public partial class MultipleHostControl : UserControl
    {
        private IMultipleHostViewModel Model { get; set; }

        public MultipleHostControl()
        {
            InitializeComponent();
            Model = new MultipleHostViewModel();
        }

        public void AddItem(object header, object content)
        {
            Model.Add(header, content);
            DataContext = Model.Items;
        }

        public void UpdateItems()
        {
            var current = Model.Current;
            ClearHeaders();
            UpdateHeaders(current);
        }

        private void ClearHeaders()
        {
            foreach (var item in Model.Items)
            {
                var itemHostViewModel = item;

                if (!itemHostViewModel.ModelHeader.ToString().Contains("(")) continue;
                var name = itemHostViewModel.ModelHeader.ToString().Split(new[] { '(' });
                itemHostViewModel.UpdateItem(name[0].TrimEnd(new[] { ' ' }), itemHostViewModel.ModelContent);
            }
        }

        private void UpdateHeaders(HostViewModel current)
        {
            foreach (var item in Model.Items)
            {
                var itemHostViewModel = item;
                if (itemHostViewModel.ModelContent != current.ModelContent) continue;
                string agentName;
                if (itemHostViewModel.ModelContent is WpfShiftEditor)
                {
                    var wpfShiftEditor = current.ModelContent as WpfShiftEditor;
                    if (wpfShiftEditor != null)
                        if (wpfShiftEditor.SchedulePart != null)
                        {
                            agentName = wpfShiftEditor.SchedulePart.Person.Name + " " +
                                        wpfShiftEditor.SchedulePart.Period.LocalStartDateTime.ToShortDateString();
                            SplitAndUpdate(itemHostViewModel, agentName);
                        }
                }
                else if (itemHostViewModel.ModelContent is RestrictionEditor)
                {
                    var restrictionEditor = current.ModelContent as RestrictionEditor;
                    if (restrictionEditor != null)
                        if (restrictionEditor.SchedulePart != null)
                        {
                            agentName = restrictionEditor.SchedulePart.Person.Name + " " +
                                        restrictionEditor.SchedulePart.Period.LocalStartDateTime.ToShortDateString();
                            SplitAndUpdate(itemHostViewModel, agentName);
                        }
                }
                else if (itemHostViewModel.ModelContent is NotesEditor)
                {
                    var notesEditor = current.ModelContent as NotesEditor;
                    if (notesEditor != null)
                        if (notesEditor.SchedulePart != null)
                        {
                            agentName = notesEditor.SchedulePart.Person.Name + " " +
                                        notesEditor.SchedulePart.Period.LocalStartDateTime.ToShortDateString();
                            SplitAndUpdate(itemHostViewModel, agentName);
                        }
                }
            }
        }

        private void SplitAndUpdate(HostViewModel itemHostViewModel, string agentName)
        {
            var split = itemHostViewModel.ModelHeader.ToString().Split(new[] { '(' });
            itemHostViewModel.UpdateItem(split[0].TrimEnd(new[] { ' ' }) + " (" + agentName + ")", Model.CurrentItem);
        }
    }
}
