using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBusTest.Payroll
{
	[TestFixture]
	public class PlugInLoaderTest
	{
		private PlugInLoader target;
		private ISearchPath searchPath;

		[SetUp]
		public void Setup()
		{
			var pathToDlls = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\", "PayrollTestDlls");
			searchPath = MockRepository.GenerateMock<ISearchPath>();
			searchPath.Stub(x => x.Path).Return(pathToDlls);

			var domainAssemblyResolver = new DomainAssemblyResolver(new AssemblyFileLoader(searchPath));

			target = new PlugInLoader(domainAssemblyResolver, searchPath);
		}

		[Test]
		public void VerifyPayrollDllDtosCanBeLoaded()
		{
			runWithExceptionHandling(() => target.LoadDtos());
		}

		[Test]
		public void VerifyPayrollDllsCanBeLoaded()
		{
			runWithExceptionHandling(() => target.Load());
		}

		private static void runWithExceptionHandling(Action action)
		{
			try
			{
				action();
			}
			catch (ReflectionTypeLoadException e)
			{
				Assert.Fail($"Failed to load payroll with messages (probably due to missing assembly binding redirect): {string.Join("\n", e.LoaderExceptions.Select(x => x.Message))}");
			}
		}
	}
}