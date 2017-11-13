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
			// This is really ugly
			var packageDir = new DirectoryInfo(Path.Combine(TestContext.CurrentContext.TestDirectory, @"..\..\..\..\packages"))
				.GetDirectories("Teleopti.Payroll.TestDlls.*").Last().FullName;
			var pathToDlls = Path.Combine(packageDir, "PayrollTestDlls");
			searchPath = MockRepository.GenerateMock<ISearchPath>();
			searchPath.Stub(x => x.Path).Return(pathToDlls);

			var domainAssemblyResolver = new DomainAssemblyResolver(new AssemblyFileLoader(searchPath));

			target = new PlugInLoader(domainAssemblyResolver, searchPath);
		}

		[Test]
		public void VerifyPayrollDllDtosCanBeLoaded()
		{
			//If fail, fix assembly redirect in SDK and contact Anders so he let support guys know about new version. Custom payroll may be broken!
			runWithExceptionHandling(() => target.LoadDtos());
		}

		[Test]
		public void VerifyPayrollDllsCanBeLoaded()
		{
			//If fail, fix assembly redirect in SDK and contact Anders so he let support guys know about new version. Custom payroll may be broken!
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