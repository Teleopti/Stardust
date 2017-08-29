using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.TestCommon.Services
{
	public class PersonRequestAuthorizationCheckerForTest : IPersonRequestCheckAuthorization
    {
        public void VerifyEditRequestPermission(IPersonRequest personRequest)
        {
        }

        public bool HasEditRequestPermission(IPersonRequest personRequest)
        {
            return true;
        }

        public bool HasViewRequestPermission(IPersonRequest personRequest)
        {
            return true;
        }

		public bool HasCancelRequestPermission (IPersonRequest personRequest)
		{
			return true;
		}
    }
}