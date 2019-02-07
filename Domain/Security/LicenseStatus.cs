using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Security
{
    public class LicenseStatus : AggregateRoot, ILicenseStatus 
    {
        private string _xmlString;

        public virtual string XmlString
        {
            get { return _xmlString; }
            set { _xmlString = value; }
        }

    }
}
