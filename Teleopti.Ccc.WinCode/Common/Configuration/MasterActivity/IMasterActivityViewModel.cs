using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity
{
    public interface IMasterActivityViewModel
    {
        IList<IMasterActivityModel> AllNotDeletedMasterActivities { get; }
        IList<IActivityModel> AllNotDeletedActivities { get; }
        IMasterActivityModel CreateNewMasterActivity(string newName);
        void DeleteMasterActivity(IMasterActivityModel masterActivityModel);
    }
}