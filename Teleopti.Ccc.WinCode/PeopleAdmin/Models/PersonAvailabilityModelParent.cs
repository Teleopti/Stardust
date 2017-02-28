using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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

		public bool AdapterOrChildCanBold()
		{
			if (CanBold) return true;

			if (GridControl != null)
			{
				var childAdapters = GridControl.Tag as IList<PersonAvailabilityModelChild>;

				if (childAdapters != null)
				{
					for (var i = 0; i < childAdapters.Count; i++)
					{
						if (childAdapters[i].CanBold) return true;
					}
				}
			}

			return false;
		}


	}
}
