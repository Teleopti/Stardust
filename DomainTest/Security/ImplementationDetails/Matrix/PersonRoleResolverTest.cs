using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.Matrix
{
	[TestFixture]
	public class PersonRoleResolverTest
	{
		private PersonRoleResolver _target;
		private MockRepository _mockRepository;
		private IPerson _person;
		private ITeamResolver _teamResolverRole;
		private IApplicationFunctionResolver _applicationFunctionResolver;
		private IApplicationRole _applicationRole1;
		private IApplicationRole _applicationRole2;
		private IApplicationFunction _applicationFunction;
		private DateOnly _dateOnly;
		private ITeam _team1;
		private ITeam _team2;

		[SetUp]
		public void Setup()
		{
			_mockRepository = new MockRepository();
			_person = PersonFactory.CreatePerson("AA");
			_applicationRole1 = ApplicationRoleFactory.CreateRole("Role1", "Role1");
			_applicationRole2 = ApplicationRoleFactory.CreateRole("Role2", "Role2");
			_person.PermissionInformation.AddApplicationRole(_applicationRole1);
			_person.PermissionInformation.AddApplicationRole(_applicationRole2);
			_applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("APP");
			_teamResolverRole =
					_mockRepository.StrictMock<ITeamResolver>();
			_applicationFunctionResolver = _mockRepository.StrictMock<IApplicationFunctionResolver>();
			_target = new PersonRoleResolver(_person, _teamResolverRole, _applicationFunctionResolver);
			_dateOnly = new DateOnly(2008, 1, 1);
			_team1 = TeamFactory.CreateSimpleTeam("Team1");
			_team2 = TeamFactory.CreateSimpleTeam("Team2");
		}

		[Test]
		public void VerifyResolveNoDuplicates()
		{
			var itemFromRoleResolver1 = new MatrixPermissionHolder(_person, _team1, false);
			var itemFromRoleResolver2 = new MatrixPermissionHolder(_person, _team2, false);

			var itemFromResolver1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);
			var itemFromResolver2 = new MatrixPermissionHolder(_person, _team2, false, _applicationFunction);

			var resultFromRole1 = new HashSet<MatrixPermissionHolder> { itemFromRoleResolver1 };
			var resultFromFunctionResolver1 = new HashSet<MatrixPermissionHolder> { itemFromResolver1 };

			var resultFromRole2 = new HashSet<MatrixPermissionHolder> { itemFromRoleResolver2 };
			var resultFromFunctionResolver2 = new HashSet<MatrixPermissionHolder> { itemFromResolver2 };

			ICollection<MatrixPermissionHolder> result;

			var itemExpectedResult1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);
			var itemExpectedResult2 = new MatrixPermissionHolder(_person, _team2, false, _applicationFunction);

			using (_mockRepository.Record())
			{
				Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole1, _dateOnly))
						.Return(resultFromRole1).Repeat.Once();

				Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole1, null))
						.Return(resultFromFunctionResolver1).Repeat.Once();


				Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole2, _dateOnly))
						.Return(resultFromRole2).Repeat.Once();

				Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole2, _applicationRole2, null))
						.Return(resultFromFunctionResolver2).Repeat.Once();
			}
			using (_mockRepository.Playback())
			{
				result = _target.Resolve(_dateOnly, null);
			}

			Assert.AreEqual(2, result.Count);
			Assert.IsTrue(result.Contains(itemExpectedResult1));
			Assert.IsTrue(result.Contains(itemExpectedResult2));

		}

		[Test]
		public void VerifyResolveWithDuplicates()
		{
			var itemFromRoleResolver1 = new MatrixPermissionHolder(_person, _team1, false);

			var itemFromResolver1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);

			var resultFromRole1 = new HashSet<MatrixPermissionHolder> { itemFromRoleResolver1 };
			var resultFromFunctionResolver1 = new HashSet<MatrixPermissionHolder> { itemFromResolver1 };

			ICollection<MatrixPermissionHolder> result;

			var itemExpectedResult1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);

			using (_mockRepository.Record())
			{
				Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole1, _dateOnly))
						.Return(resultFromRole1).Repeat.Once();

				Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole1, null))
						.Return(resultFromFunctionResolver1).Repeat.Once();

				Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole2, _dateOnly))
						.Return(resultFromRole1).Repeat.Once();

				Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole2, null))
						.Return(resultFromFunctionResolver1).Repeat.Once();
			}
			using (_mockRepository.Playback())
			{
				result = _target.Resolve(_dateOnly, null);
			}

			Assert.AreEqual(1, result.Count);
			Assert.IsTrue(result.Contains(itemExpectedResult1));
		}
	}
}