using System.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiIdentity : GenericIdentity, ITeleoptiIdentity
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public TeleoptiIdentity(string name,
		                        IDataSource dataSource,
		                        IBusinessUnit businessUnit,
		                        WindowsIdentity windowsIdentity)
			: base(name)
		{
			DataSource = dataSource;
			BusinessUnit = businessUnit;
			WindowsIdentity = windowsIdentity;
		}

		public IDataSource DataSource { get; set; }
		public IBusinessUnit BusinessUnit { get; set; }
		public string Ticket { get; set; }
		public WindowsIdentity WindowsIdentity { get; set; }
	}
}