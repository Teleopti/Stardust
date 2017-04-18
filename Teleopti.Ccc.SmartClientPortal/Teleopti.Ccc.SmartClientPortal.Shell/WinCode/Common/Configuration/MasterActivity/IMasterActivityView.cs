using System.Collections.Generic;
using System.Drawing;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration.MasterActivity
{
    public interface IMasterActivityView
    {
        void LoadComboWithMasterActivities(IList<IMasterActivityModel> masterActivities);
        void LoadTwoList(IList<IActivityModel> allActivities, IList<IActivityModel> selectedActivities);
        Color Color { set; get; }
        string LongName { get; set; }
        IList<IActivityModel> Activities { get; }
        void SetUpdateInfo(string infoText);
        void SelectMaster(IMasterActivityModel masterActivityModel);
        bool ConfirmDelete();
    }
}