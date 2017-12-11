using System.Collections.Generic;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.SmartParts
{
    public static class SmartPartInvoker
    {
		public static SmartPartCommand SmartPartCommand { get; set; }
		
		public static void ShowSmartPart(SmartPartInformation smartPartInfo, IList<SmartPartParameter> parameters)
        {
            SmartPartCommand.ShowSmartPart(smartPartInfo, parameters);
        }
		
        public static void ClearAllSmartParts()
        {
            SmartPartCommand.ClearAllSmartParts();
        }
    }
}
