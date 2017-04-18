using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.PeopleAdmin.Models
{

    /// <summary>
    /// The wrapper class which encapsulates properties that are required to 
    /// pupulate and manipulate PersonRotation view.
    /// </summary>
    /// <remarks>
    /// Created by: Shiran Ginige
    /// Created date: 2008-07-23
    /// </remarks>
    public class PersonRotationModelParent : PersonRotationModelBase
    {
        /// <summary>
        /// The public constructor
        /// </summary>
        /// <remarks>
        /// Created by: Shiran Ginige
        /// Created date: 2008-07-23
        /// </remarks>
        public PersonRotationModelParent(IPerson person, CommonNameDescriptionSetting commonNameDescription)
            : base(person, commonNameDescription)
        {
        }

        public int RotationCount { get; set; }

        public void ResetCanBoldPropertyOfChildAdapters()
        {
            if (GridControl != null)
            {
                IList<PersonRotationModelChild> childAdapters = GridControl.Tag as
                                                                          IList<PersonRotationModelChild>;

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
				var childAdapters = GridControl.Tag as IList<PersonRotationModelChild>;

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
