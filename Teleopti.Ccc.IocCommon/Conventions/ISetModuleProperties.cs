using System.Reflection;
using Module = Autofac.Module;

namespace Teleopti.Ccc.IocCommon.Conventions
{
	public interface ISetModuleProperties
	{
		void SetPropertyValue(Module module, PropertyInfo propertyInfo);
	}
}