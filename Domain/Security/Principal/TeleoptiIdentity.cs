using System;
using System.Runtime.Serialization;
using System.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	[DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2012/05/")]
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
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
			BusinessUnit = businessUnit;
			if (BusinessUnit != null)
				BusinessUnitId = businessUnit.Id.Value;
			WindowsIdentity = windowsIdentity;
			TeleoptiAuthenticationType = teleoptiAuthenticationType;
		}

		[DataMember]
		public string DataSourceName { get; set; }
		public IDataSource DataSource { get; set; }

		[DataMember]
		public Guid BusinessUnitId { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }

		[DataMember]
		public AuthenticationTypeOption TeleoptiAuthenticationType { get; private set; }
		[DataMember]
		public string Ticket { get; set; }

		public WindowsIdentity WindowsIdentity { get; set; }

	}
}