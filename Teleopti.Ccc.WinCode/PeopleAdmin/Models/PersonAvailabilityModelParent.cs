using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class PersonAvailabilityModelParent : PersonAvailabilityModelBase
    {
        public PersonAvailabilityModelParent(IPerson person, IPersonAvailability personAvailability, CommonNameDescriptionSetting commonNameDescription)
            : base(person, personAvailability, commonNameDescription)
        {

        }

        public int RotationCount
        {
            get;
            set;
        }

        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                IList<PersonAvailabilityModelChild> childAdapters = GridControl.Tag as
                                                                          IList<PersonAvailabilityModelChild>;

                if (childAdapters != null)
                {
                    for (int i = 0; i < childAdapters.Count; i++)
                    {
                        childAdapters[i].CanBold = false;
                    }
                }

                GridControl.Invalidate();
            }
        }


    }
}
