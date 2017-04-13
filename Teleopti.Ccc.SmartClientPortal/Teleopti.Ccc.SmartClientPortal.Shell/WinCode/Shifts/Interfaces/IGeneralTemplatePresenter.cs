using System;

namespace Teleopti.Ccc.WinCode.Shifts.Interfaces
{
    public interface IGeneralTemplatePresenter : ICommon<IGeneralTemplateViewModel>, 
                                                 IPresenterBase
    {
		event EventHandler<EventArgs> OnlyForRestrictionsChanged;
    	void InvokeOnlyForRestrictionsCellChanged();
    }
}
