﻿using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{
    public class PersonAvailabilityModelChild : PersonAvailabilityModelBase
    {

        public PersonAvailabilityModelChild(IPerson person, IPersonAvailability personAvailability, CommonNameDescriptionSetting commonNameDescription)
            : base(person, personAvailability,  commonNameDescription)
        {
            
        }

        public override string PersonFullName
        {
            get; 
            set;
        }


    }
}
