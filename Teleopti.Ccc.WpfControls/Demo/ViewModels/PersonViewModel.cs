using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WpfControls.Demo.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WpfControls.Demo.ViewModels
{
    public class PersonViewModel:DataModel
    {
        #region properties & fields
        private IPerson _person;
        private ObservableCollection<PersonAccountDayViewModel> _accounts = new ObservableCollection<PersonAccountDayViewModel>();
        
        public string FirstName
        {
            get { return _person.Name.FirstName; }
        }
        public string LastName
        {
            get { return _person.Name.LastName; }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ObservableCollection<PersonAccountDayViewModel> Accounts
        {
            get { return _accounts; }
            set { _accounts = value; }
        }

        #endregion //properties & fields

        #region ctor
        public PersonViewModel()
        {
            _person = new Person();
            _person.Name = new Name("xxFirstName","xxLastName");
            State = ModelState.Fetching;
        }

        public PersonViewModel(IPerson person):base()
        {
            _person = person;
           
           foreach (IPersonAccount pAcc in _person.PersonAccountCollection)
           {
               if (pAcc != null) Accounts.Add(new PersonAccountDayViewModel((PersonAccountDay)pAcc));
           }
        }
        #endregion //ctor




    }
}
