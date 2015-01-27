using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
	public class ApplicationFunctionsProviderTest
	{
		const string myDatasource = "my datasource";

		[Test]
		public void ShouldCombineLicensedFunctionsWithAvailableFunctions()
		{
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();
			var currentDataSource = new FakeCurrentDatasource(myDatasource);
			var applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();
			
			licensedFunctionsProvider.Stub(x => x.LicensedFunctions(myDatasource))
				.Return(new List<IApplicationFunction> {new ApplicationFunction("Code1"){ForeignSource = DefinedForeignSourceNames.SourceRaptor}});
			applicationFunctionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { new ApplicationFunction("Code1") { ForeignSource = DefinedForeignSourceNames.SourceRaptor }, new ApplicationFunction("Code2") { ForeignSource = DefinedForeignSourceNames.SourceRaptor } });

			var target = new ApplicationFunctionsProvider(applicationFunctionRepository, licensedFunctionsProvider, currentDataSource);

			var result = target.AllFunctions();
			result.FindByFunctionPath("Code1").IsLicensed.Should().Be.True();
			result.FindByFunctionPath("Code2").IsLicensed.Should().Be.False();
		}

		[Test]
		public void ShouldAlwaysTreatAllAsLicensed()
		{
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();
			var currentDataSource = new FakeCurrentDatasource(myDatasource);
			var applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions(myDatasource))
				.Return(new List<IApplicationFunction>());
			applicationFunctionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { new ApplicationFunction("All") { ForeignSource = DefinedForeignSourceNames.SourceRaptor } });

			var target = new ApplicationFunctionsProvider(applicationFunctionRepository, licensedFunctionsProvider,
				currentDataSource);

			var result = target.AllFunctions();
			result.FindByFunctionPath("All").IsLicensed.Should().Be.True();
		}

		[Test]
		public void ShouldIgnoreLicenseCheckForFunctionsFromOtherSources()
		{
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();
			var currentDataSource = new FakeCurrentDatasource(myDatasource);
			var applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions(myDatasource))
				.Return(new List<IApplicationFunction>());
			applicationFunctionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { new ApplicationFunction("Code1") { ForeignSource = DefinedForeignSourceNames.SourceMatrix } });

			var target = new ApplicationFunctionsProvider(applicationFunctionRepository, licensedFunctionsProvider,
				currentDataSource);

			var result = target.AllFunctions();
			result.FindByFunctionPath("Code1").IsLicensed.Should().Be.True();
		}

		[Test]
		public void ShouldHidePreliminaryFunction()
		{
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();
			var currentDataSource = new FakeCurrentDatasource(myDatasource);
			var applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions(myDatasource))
				.Return(new List<IApplicationFunction>());
			applicationFunctionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { new ApplicationFunction("Code1") { ForeignSource = DefinedForeignSourceNames.SourceRaptor, IsPreliminary = true} });

			var target = new ApplicationFunctionsProvider(applicationFunctionRepository, licensedFunctionsProvider, currentDataSource);

			var result = target.AllFunctions();
			result.FindByFunctionPath("Code1").Hidden.Should().Be.True();
		}

		[Test]
		public void ShouldCombineLicensedFunctionsWithAvailableFunctionsInNestedHierarchy()
		{
			var licensedFunctionsProvider = MockRepository.GenerateMock<ILicensedFunctionsProvider>();
			var currentDataSource = new FakeCurrentDatasource(myDatasource);
			var applicationFunctionRepository = MockRepository.GenerateMock<IApplicationFunctionRepository>();

			licensedFunctionsProvider.Stub(x => x.LicensedFunctions(myDatasource))
				.Return(new List<IApplicationFunction>
				{
					new ApplicationFunction("Code1") {ForeignSource = DefinedForeignSourceNames.SourceRaptor},
					new ApplicationFunction("Code1/Code2") {ForeignSource = DefinedForeignSourceNames.SourceRaptor}
				});
			var function1 = new ApplicationFunction("Code1") { ForeignSource = DefinedForeignSourceNames.SourceRaptor };
			var function2 = new ApplicationFunction("Code2", function1) { ForeignSource = DefinedForeignSourceNames.SourceRaptor };
			var function3 = new ApplicationFunction("Code3", function2) { ForeignSource = DefinedForeignSourceNames.SourceRaptor };
			applicationFunctionRepository.Stub(x => x.GetAllApplicationFunctionSortedByCode())
				.Return(new List<IApplicationFunction> { function1, function2, function3 });

			var target = new ApplicationFunctionsProvider(applicationFunctionRepository, licensedFunctionsProvider, currentDataSource);

			var result = target.AllFunctions();
			result.FindByFunctionPath("Code1").IsLicensed.Should().Be.True();
			result.FindByFunctionPath("Code1/Code2").IsLicensed.Should().Be.True();
			result.FindByFunctionPath("Code1/Code2/Code3").IsLicensed.Should().Be.False();
		}
	}
}