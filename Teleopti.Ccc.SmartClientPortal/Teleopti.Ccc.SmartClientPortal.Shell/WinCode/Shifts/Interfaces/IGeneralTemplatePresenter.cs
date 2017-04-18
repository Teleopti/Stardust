using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Shifts.Interfaces
{
    public interface IGeneralTemplatePresenter : ICommon<IGeneralTemplateViewModel>, 
                                                 IPresenterBase
    {
		event EventHandler<EventArgs> OnlyForRestrictionsChanged;
    	void InvokeOnlyForRestrictionsCellChanged();
    }
}
