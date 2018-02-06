using System;
using Autofac;
using BoDi;
using TechTalk.SpecFlow.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.SpecFlowPlugin
{
	public class AutofacTestObjectResolver : ITestObjectResolver
	{
		public object ResolveBindingInstance(Type bindingType, IObjectContainer scenarioContainer)
		{
			return scenarioContainer
				.Resolve<IComponentContext>()
				.Resolve(bindingType);
		}
	}
}