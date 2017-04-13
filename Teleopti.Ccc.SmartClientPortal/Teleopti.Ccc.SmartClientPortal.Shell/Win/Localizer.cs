using System.Globalization;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win
{
	public class Localizer : ILocalizationProvider
	{
		public string GetLocalizedString(CultureInfo culture, string name, object ctrl)
		{
			
			 switch (name)
			 {
				 #region MessageBoxAdv
				 case ResourceIdentifiers.OK:
					 return Resources.Ok;
				 case ResourceIdentifiers.Cancel:
					 return Resources.Cancel;
				 case ResourceIdentifiers.Yes:
					 return Resources.Yes;
				 case ResourceIdentifiers.No:
					 return Resources.No;
				 #endregion

				 default:
					 return string.Empty;	
			 }
		}
	}
}