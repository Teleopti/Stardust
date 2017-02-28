using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Models
{

    /// <summary>
    /// The wrapper class which encapsulates properties that are required to 
    /// pupulate and manipulate PersonRotation view.
    /// </summary>
    /// <remarks>
    /// Created by: Shiran Ginige
    /// Created date: 2008-07-23
    /// </remarks>
    public class PersonRotationModelChild : PersonRotationModelBase 
    {
        /// <summary>
        /// The public constructor
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public PersonRotationModelChild(IPerson person, CommonNameDescriptionSetting commonNameDescription)
            : base(person, commonNameDescription)
        {
        }

        public override string PersonFullName
        {
            get;
            set;
        }
    }
}
