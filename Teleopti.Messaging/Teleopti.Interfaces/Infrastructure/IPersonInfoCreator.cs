using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPersonInfoCreator
	{
		void CreateAndPersistPersonInfo(IPersonInfoModel personInfo);
	}
}