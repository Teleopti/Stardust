using System;
using Autofac;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIsolate
	{
		ITestDoubleFor UseTestDouble<TTestDouble>() where TTestDouble : class;
		ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance) where TTestDouble : class;
		ITestDoubleFor UseTestDoubleForType(Type type);
	}

	public interface IExtend
	{
		void AddService<TService>(bool instancePerLifeTimeScope = false);
		void AddService<TService>(TService instance) where TService : class;
		void AddModule(Module module);
		void AddService<TService, TServiceName>(string name);
	}
}