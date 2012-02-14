using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
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
        private MatrixPermissionHolder _itemFromRoleResolver1;
        private MatrixPermissionHolder _itemFromRoleResolver2;
        private MatrixPermissionHolder _itemFromResolver1;
        private MatrixPermissionHolder _itemFromResolver2;
        private MatrixPermissionHolder _itemExpectedResult1;
        private MatrixPermissionHolder _itemExpectedResult2;
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
            _itemFromRoleResolver1 = new MatrixPermissionHolder(_person, _team1, false);
            _itemFromRoleResolver2 = new MatrixPermissionHolder(_person, _team2, false);

            _itemFromResolver1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);
            _itemFromResolver2 = new MatrixPermissionHolder(_person, _team2, false, _applicationFunction);

            HashSet<MatrixPermissionHolder> resultFromRole1 = new HashSet<MatrixPermissionHolder> {_itemFromRoleResolver1};
            HashSet<MatrixPermissionHolder> resultFromFunctionResolver1 = new HashSet<MatrixPermissionHolder> {_itemFromResolver1};

            HashSet<MatrixPermissionHolder> resultFromRole2 = new HashSet<MatrixPermissionHolder> { _itemFromRoleResolver2 };
            HashSet<MatrixPermissionHolder> resultFromFunctionResolver2 = new HashSet<MatrixPermissionHolder> {_itemFromResolver2};

            ICollection<MatrixPermissionHolder> result;

            _itemExpectedResult1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);
            _itemExpectedResult2 = new MatrixPermissionHolder(_person, _team2, false, _applicationFunction);

            using (_mockRepository.Record())
            {

                Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole1, _dateOnly))
                    .Return(resultFromRole1).Repeat.Once();

                Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole1,null))
                    .Return(resultFromFunctionResolver1).Repeat.Once();


                Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole2, _dateOnly))
                    .Return(resultFromRole2).Repeat.Once();

                Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole2, _applicationRole2,null))
                    .Return(resultFromFunctionResolver2).Repeat.Once();

            }
            using (_mockRepository.Playback())
            {
                result = _target.Resolve(_dateOnly,null);
            }

            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains(_itemExpectedResult1));
            Assert.IsTrue(result.Contains(_itemExpectedResult2));
            
        }

        [Test]
        public void VerifyResolveWithDuplicates()
        {
            _itemFromRoleResolver1 = new MatrixPermissionHolder(_person, _team1, false);

            _itemFromResolver1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);

            HashSet<MatrixPermissionHolder> resultFromRole1 = new HashSet<MatrixPermissionHolder> { _itemFromRoleResolver1 };
            HashSet<MatrixPermissionHolder> resultFromFunctionResolver1 = new HashSet<MatrixPermissionHolder> { _itemFromResolver1 };

            ICollection<MatrixPermissionHolder> result;

            _itemExpectedResult1 = new MatrixPermissionHolder(_person, _team1, false, _applicationFunction);

            using (_mockRepository.Record())
            {

                Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole1, _dateOnly))
                    .Return(resultFromRole1).Repeat.Once();

                Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole1,null))
                    .Return(resultFromFunctionResolver1).Repeat.Once();


                Expect.Call(_teamResolverRole.ResolveTeams(_applicationRole2, _dateOnly))
                    .Return(resultFromRole1).Repeat.Once();

                Expect.Call(_applicationFunctionResolver.ResolveApplicationFunction(resultFromRole1, _applicationRole2, null))
                    .Return(resultFromFunctionResolver1).Repeat.Once();

            }
            using (_mockRepository.Playback())
            {
                result = _target.Resolve(_dateOnly,null);
            }

            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.Contains(_itemExpectedResult1));
        }
    }
}