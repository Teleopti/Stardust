using System;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework;

namespace NodeTest.Attributes
{
	public class BaseTestsAttribute : Attribute, ITestAction
	{
		private IContainer _container;
		private object _fixture;
		private Type _fixtureType;

		public void BeforeTest(TestDetails test)
		{
			_fixture = test.Fixture;
			_fixtureType = _fixture.GetType();

			BuildContainer();
			InjectMembers();
		}

		public void AfterTest(TestDetails test)
		{
			//throw new NotImplementedException();
		}

		public ActionTargets Targets
		{
			get { return ActionTargets.Test; }
		}

		private void InjectMembers()
		{
			InjectMembers(GetType(),
			              this);

			InjectMembers(_fixtureType,
			              _fixture);
		}

		private void InjectMembers(IReflect type,
		                           object instance)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite);

			foreach (var propertyInfo in properties)
			{
				propertyInfo.SetValue(instance,
				                      _container.Resolve(propertyInfo.PropertyType),
				                      null);
			}

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach (var fieldsInfo in fields)
			{
				fieldsInfo.SetValue(instance,
				                    _container.Resolve(fieldsInfo.FieldType));
			}
		}

		private void BuildContainer()
		{
			var builder = new ContainerBuilder();
			SetUp(builder);
			_container = builder.Build();
		}

		protected virtual void SetUp(ContainerBuilder builder)
		{
		}
	}
}