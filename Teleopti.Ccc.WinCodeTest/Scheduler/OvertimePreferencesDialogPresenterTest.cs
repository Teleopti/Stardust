using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	public class OvertimePreferencesDialogPresenterTest
	{
		private OvertimePreferencesDialogPresenter _target;
		private IOvertimePreferencesDialog _view;
		private MockRepository _mock;
		private IList<IMultiplicatorDefinitionSet> _definitionSets;
		private IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;

		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_view = _mock.StrictMock<IOvertimePreferencesDialog>();
			_multiplicatorDefinitionSet = _mock.StrictMock<IMultiplicatorDefinitionSet>();
			_definitionSets = new List<IMultiplicatorDefinitionSet>{_multiplicatorDefinitionSet};
			_target = new OvertimePreferencesDialogPresenter(_view);
		}

		[Test]
		public void ShouldNotSetStateOkButtonDisabledfMultiplicatorDefinitionSetsExists()
		{
			_target.SetStateButtons(_definitionSets);		
		}

		[Test]
		public void ShouldSetStateOkButtonDisabledIfMultiplicatorDefinitionSetsDontExists()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _view.SetStateOkButtonDisabled());
			}

			using (_mock.Playback())
			{
				_definitionSets.Clear();
				_target.SetStateButtons(_definitionSets);
			}
		}

		[Test]
		public void ShouldGetDataFromControlsAndSavePersonalSettingsWhenOkClicked()
		{
			using (_mock.Record())
			{
				Expect.Call(() => _view.GetDataFromControls());
				Expect.Call(() => _view.SavePersonalSettings());
			}

			using (_mock.Playback())
			{
				_target.ButtonOkClick();
			}
		}
	}
}
