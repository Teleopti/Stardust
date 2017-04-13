using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Configuration.MasterActivity
{
    public interface IMasterActivityModel
    {
        IList<IActivityModel> Activities { get; }
        Color Color { set; get; }
        string Name { get; set; }
        string UpdateInfo { get; }
        IMasterActivity Entity { get; }
        void UpdateActivities(IList<IActivityModel> activities);
    }
}