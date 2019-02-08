using System;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly)]
	public class DefaultDataAttribute : Attribute
	{
	}
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class NoDefaultDataAttribute : Attribute{}
	
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Assembly)]
	public class AddDatasourceId : Attribute
	{
	}
}