using System;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Infrastructure.Aop;
using Teleopti.Ccc.IocCommon;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface ISetupConfiguration
	{
		void SetupConfiguration(IocArgs args);
	}

	public interface ISetup
	{
		void Setup(ISystem system, IIocConfiguration configuration);
	};

	public interface ISystem
	{
		ITestDoubleFor UseTestDouble<TTestDouble>() where TTestDouble : class;
		ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance) where TTestDouble : class;
		ITestDoubleFor UseTestDoubleForType(Type type);

		void AddService<TService>();
		void AddService<TService>(TService instance) where TService : class;

		void AddModule(Module module);
	}

	public interface ITestDoubleFor
	{
		void For<T>();
		void For<T1, T2>();
		void For<T1, T2, T3>();
		void For<T1, T2, T3, T4>();
		void For<T1, T2, T3, T4, T5, T6, T7>();
	}

	public class IgnoringTestDoubles : SystemImpl
	{
		public IgnoringTestDoubles(ContainerBuilder builder, TestDoubles testDoubles) : base(builder, testDoubles)
		{
		}

		public override ITestDoubleFor UseTestDouble<TTestDouble>()
		{
			return new IgnoreTestDouble();
		}

		public override ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance)
		{
			return new IgnoreTestDouble();
		}

	}

	public class SystemImpl : ISystem
	{
		private readonly ContainerBuilder _builder;
		private readonly TestDoubles _testDoubles;

		public SystemImpl(ContainerBuilder builder, TestDoubles testDoubles)
		{
			_builder = builder;
			_testDoubles = testDoubles;
		}

		public virtual ITestDoubleFor UseTestDouble<TTestDouble>() where TTestDouble : class
		{
			return new TestDoubleFor<TTestDouble>(_builder, _testDoubles, null);
		}

		public virtual ITestDoubleFor UseTestDouble<TTestDouble>(TTestDouble instance) where TTestDouble : class
		{
			return new TestDoubleFor<TTestDouble>(_builder, _testDoubles, instance);
		}

		public virtual ITestDoubleFor UseTestDoubleForType(Type type)
		{
			return new TestDoubleFor<object>(_builder, _testDoubles, type);
		}

		public void AddService<TService>()
		{
			_builder
				.RegisterType<TService>()
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance()
				.ApplyAspects();
		}

		public void AddService<TService>(TService instance) where TService : class
		{
			_builder
				.RegisterInstance(instance)
				.AsSelf()
				.AsImplementedInterfaces()
				.SingleInstance();
		}

		public void AddModule(Module module)
		{
			_builder.RegisterModule(module);
		}
	}

	public class IgnoreTestDouble : ITestDoubleFor
	{
		public void For<T>()
		{
		}

		public void For<T1, T2>()
		{
		}

		public void For<T1, T2, T3>()
		{
		}

		public void For<T1, T2, T3, T4>()
		{
		}

		public void For<T1, T2, T3, T4, T5, T6, T7>()
		{
		}
	}

	public class TestDoubles
	{
		public class Registration
		{
			public Action<ContainerBuilder> Action;
		}

		private readonly List<Registration> _registrations = new List<Registration>();

		public Registration Register(Action<ContainerBuilder> action)
		{
			var registration = new Registration
			{
				Action = action
			};
			_registrations.Add(registration);
			return registration;
		}

		public void RegisterFromPreviousContainer(ContainerBuilder builder)
		{
			_registrations.ForEach(r =>
			{
				if (r.Action != null)
					r.Action.Invoke(builder);
			});
		}
	}

	public class TestDoubleFor<TTestDouble> : ITestDoubleFor where TTestDouble : class
	{
		private readonly ContainerBuilder _builder;
		private readonly TestDoubles _testDoubles;
		private readonly Type _type;
		private readonly object _instance;

		public TestDoubleFor(ContainerBuilder builder, TestDoubles testDoubles, object instance)
		{
			_builder = builder;
			_testDoubles = testDoubles;
			_instance = instance;
		}

		public TestDoubleFor(ContainerBuilder builder, TestDoubles testDoubles, Type type)
		{
			_builder = builder;
			_testDoubles = testDoubles;
			_type = type;
		}

		public void For<T>()
		{
			register(typeof(T));
		}

		public void For<T1, T2>()
		{
			register(typeof(T1), typeof(T2));
		}

		public void For<T1, T2, T3>()
		{
			register(typeof(T1), typeof(T2), typeof(T3));
		}

		public void For<T1, T2, T3, T4>()
		{
			register(typeof(T1), typeof(T2), typeof(T3), typeof(T4));
		}

		public void For<T1, T2, T3, T4, T5, T6, T7>()
		{
			register(typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
		}

		private void register(params Type[] asTypes)
		{
			if (_instance == null)
			{
				if (_type != null)
				{
					var registration = _testDoubles.Register(b =>
					{
						b.RegisterType(_type)
							.SingleInstance()
							.AsSelf()
							.As(asTypes);
					});
					_builder
						.RegisterType(_type)
						.SingleInstance()
						.AsSelf()
						.As(asTypes)
						.ExternallyOwned()
						.OnActivated(c =>
						{
							registration.Action = b =>
							{
								b.RegisterInstance(c.Instance)
									.SingleInstance()
									.AsSelf()
									.As(asTypes);
							};
						});
				}
				else
				{
					var registration = _testDoubles.Register(b =>
					{
						b.RegisterType<TTestDouble>()
							.SingleInstance()
							.AsSelf()
							.As(asTypes);
					});
					_builder
						.RegisterType<TTestDouble>()
						.SingleInstance()
						.AsSelf()
						.As(asTypes)
						.ExternallyOwned()
						.OnActivated(c =>
						{
							registration.Action = b =>
							{
								b.RegisterInstance(c.Instance)
									.SingleInstance()
									.AsSelf()
									.As(asTypes);
							};
						});
				}
			}
			else
			{
				_testDoubles.Register(b =>
				{
					b.RegisterInstance(_instance)
						.SingleInstance()
						.AsSelf()
						.As(asTypes);
				});
				_builder
					.RegisterInstance(_instance)
					.SingleInstance()
					.AsSelf()
					.As(asTypes)
					.ExternallyOwned()
					;
			}
		}
	}

}