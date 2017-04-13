using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class OvertimePreferencesDialogPresenter
	{
		private readonly IOvertimePreferencesDialog _view;

		public OvertimePreferencesDialogPresenter(IOvertimePreferencesDialog view)
		{
			_view = view;
		}

		public void SetStateButtons(IList<IMultiplicatorDefinitionSet> definitionSets)
		{
			if (definitionSets.IsEmpty()) _view.SetStateOkButtonDisabled();
		}

		public void ButtonOkClick()
		{
			_view.GetDataFromControls();
			_view.SavePersonalSettings();
		}
	}
}
