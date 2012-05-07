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
			if (DataSource != null)
				DataSourceName = DataSource.DataSourceName;
			WindowsIdentity = windowsIdentity;
			TeleoptiAuthenticationType = teleoptiAuthenticationType;
			BusinessUnit = businessUnit;
		}

		[DataMember]
		public string DataSourceName { get; set; }
		public IDataSource DataSource { get; set; }

		[DataMember]
		public AuthenticationTypeOption TeleoptiAuthenticationType { get; private set; }
		[DataMember]
		public string Ticket { get; set; }

		public WindowsIdentity WindowsIdentity { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }
	}
}