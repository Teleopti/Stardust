using System;
using Teleopti.Interfaces.Domain;
using System.Drawing;


namespace Teleopti.Ccc.WinCode.Common.ScheduleFilter
{
    public interface IScheduleFilterView
    {
        void ButtonOkText(string buttonText);
        void ButtonCancelText(string buttonText);
        void CreateAndAddTreeNode(CccTreeNode node);
        void CloseFilterForm();
        void SetColor();
        void SetTexts();
        void ClearTabsAndTrees();
        void ClearSelectedTabControls();
        void AddTabPages(IGroupPage page);
        void AddTabPages(string displayText);
        void AddSelectedIndexChangedHandler();
        void RemoveSelectedIndexChangedHandler();
        DateTime CurrentFilterDate { get; }
        void OpenContextMenu(Point point);
        void DisplaySearch();
        object SelectedTabTag();
        bool IsAnyNodeChecked(IPerson person);
        // not used: void UpdateNodesChecked(IPerson person, bool selected);

    }
}