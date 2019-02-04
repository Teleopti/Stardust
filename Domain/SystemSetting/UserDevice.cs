using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SystemSetting
{
	public class UserDevice : AggregateRoot, IUserDevice
	{
		private IPerson _owner;
		private string _token;

		public virtual IPerson Owner
		{
			get { return _owner; }
			set { _owner = value; }
		}

		public virtual string Token
		{
			get { return _token; }
			set { _token = value; }
		}

	}
}