using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/05/")]
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		public TeleoptiIdentity(string name,
		                        IDataSource dataSource,
		                        IBusinessUnit businessUnit,
		                        WindowsIdentity windowsIdentity,
		                        AuthenticationTypeOption teleoptiAuthenticationType)
			: base(name)
		{
			DataSource = dataSource;
			WindowsIdentity = windowsIdentity;
			TeleoptiAuthenticationType = teleoptiAuthenticationType;
			BusinessUnit = businessUnit;
		}

		[DataMember]
		public AuthenticationTypeOption TeleoptiAuthenticationType { get; private set; }
		[DataMember]
		public string Ticket { get; set; }

		public WindowsIdentity WindowsIdentity { get; set; }
		public IDataSource DataSource { get; private set; }
		public IBusinessUnit BusinessUnit { get; private set; }
	}
}