using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PersonRequestAuthorizationCheckerConfigurable : IPersonRequestCheckAuthorization
	{
		public bool HasEditPermission { get; set; }
		public bool HasViewPermission { get; set; }
		public bool HasCancelPermission { get; set; }

		public PersonRequestAuthorizationCheckerConfigurable()
		{
			HasEditPermission = HasViewPermission = HasCancelPermission = true;
		}

		public void RevokeCancelRequestPermission()
		{
			HasCancelPermission = false;
		}

		public void VerifyEditRequestPermission(IPersonRequest personRequest)
		{
		}

		public bool HasEditRequestPermission(IPersonRequest personRequest)
		{
			return HasEditPermission;
		}

		public bool HasViewRequestPermission(IPersonRequest personRequest)
		{
			return HasViewPermission;
		}

		public bool HasCancelRequestPermission(IPersonRequest personRequest)
		{
			return HasCancelPermission;
		}
	}
}