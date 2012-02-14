namespace Teleopti.Ccc.Win.Common
{
    public interface IHelpHelper
    {
        void GetHelp(IHelpForm form, IHelpContext helpContext, bool local);
		void GetHelp(BaseUserControl userControl, IHelpContext helpContext, bool local);
    }
}