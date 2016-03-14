using System;
using Autofac;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface ISystem
	{
		ITestDoubleFor UseTestDouble<TTestDouble>() where TTestDouble : class;
		ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance) where TTestDouble : class;
		ITestDoubleFor UseTestDoubleForType(Type type);

		void AddService<TService>(bool instancePerLifeTimeScope = false);
		void AddService<TService>(TService instance) where TService : class;

		void AddModule(Module module);
	}
}