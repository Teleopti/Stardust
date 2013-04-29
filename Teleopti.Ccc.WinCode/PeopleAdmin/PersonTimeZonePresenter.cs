using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.PeopleAdmin
{
	public class PersonTimeZonePresenter
	{
		private readonly IPersonTimeZoneView _view;
		private readonly IList<IPerson> _persons;

		public PersonTimeZonePresenter(IPersonTimeZoneView view, IList<IPerson> persons)
		{
			_view = view;
			_persons = persons;
		}

		public void OnButtonAdvOkClick()
		{
			foreach (var person in _persons)
			{
				_view.SetPersonTimeZone(person);	
			}
			
			_view.HideDialog();
		}

		public void OnButtonAdvCancelClick()
		{
			_view.Cancel();		
		}
	}
}
