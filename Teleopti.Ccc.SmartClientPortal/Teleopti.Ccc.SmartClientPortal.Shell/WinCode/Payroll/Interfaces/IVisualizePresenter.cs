using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Payroll.Interfaces
{
    public interface IVisualizePresenter : ICommon<VisualPayloadInfo>, IPresenterBase
    {
        /// <summary>
        /// Loads the model.
        /// </summary>
        /// <param name="selectedDate">The date.</param>
        /// <param name="timeZoneInfo">The time zone info.</param>
        void LoadModel(DateOnly selectedDate, TimeZoneInfo timeZoneInfo);
    }
}
