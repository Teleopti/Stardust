using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
    [TestFixture]
    public class MatrixPermissionsResolverTest
    {
        private MatrixPermissionsResolverTestClass _target;
        private MockRepository _mockRepository;
        private IPerson _person1;
        private IPerson _person2;
        private IPersonRoleResolver _personRoleResolver1;
        private IPersonRoleResolver _personRoleResolver2;
        private DateOnly _queryDate;
        private ITeam _team;
        private IApplicationFunction _applicationFunction;

        [SetUp]
        public void Setup()
        {

            _mockRepository = new MockRepository();
            _person1 = PersonFactory.CreatePerson("AA");
            _person2 = PersonFactory.CreatePerson("BB");
            _queryDate = new DateOnly(2008, 01, 01);
            _team = TeamFactory.CreateSimpleTeam("Team");
            _applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("APP");

            _personRoleResolver1 = _mockRepository.StrictMock<IPersonRoleResolver>();
            _personRoleResolver2 = _mockRepository.StrictMock<IPersonRoleResolver>();

            _target = new MatrixPermissionsResolverTestClass(new List<IPersonRoleResolver> { _personRoleResolver1, _personRoleResolver2 });

        }

        [Test]
        public void VerifyConstructor1()
        {
            var personProvider = _mockRepository.StrictMock<IPersonRepository>();
            
            using(_mockRepository.Record())
            {
                Expect.Call(personProvider.FindAllSortByName(true)).Return(new List<IPerson> { _person1, _person2 });
            }
            using(_mockRepository.Playback())
            {
                MatrixPermissionsResolver target = new MatrixPermissionsResolver(personProvider,
                                                                                 new FunctionsForRoleProvider(
                                                                                     new LicensedFunctionsProvider(
                                                                                         new DefinedRaptorApplicationFunctionFactory
                                                                                             ()),
                                                                                     new ExternalFunctionsProvider(
                                                                                         new RepositoryFactory())),null);
                Assert.IsNotNull(target);
            }
        }

        [Test]
        public void VerifyResolve()
        {

            MatrixPermissionHolder itemInResolver1 = new MatrixPermissionHolder(_person1, _team, false, _applicationFunction);
            MatrixPermissionHolder itemInResolver2 = new MatrixPermissionHolder(_person2, _team, false, _applicationFunction);

            HashSet<MatrixPermissionHolder> resultFromResolver1 = new HashSet<MatrixPermissionHolder> { itemInResolver1 };
            HashSet<MatrixPermissionHolder> resultFromResolver2 = new HashSet<MatrixPermissionHolder> { itemInResolver2 };

            using (_mockRepository.Record())
            {
                Expect.Call(_personRoleResolver1.Resolve(_queryDate,null)).Return(resultFromResolver1).Repeat.Once();
                Expect.Call(_personRoleResolver2.Resolve(_queryDate,null)).Return(resultFromResolver2).Repeat.Once();
            }

            IList<MatrixPermissionHolder> result = _target.ResolvePermission(_queryDate,null);

            Assert.AreEqual(2, result.Count);
        }
    }

    public class MatrixPermissionsResolverTestClass : MatrixPermissionsResolver
    {
        public MatrixPermissionsResolverTestClass(IList<IPersonRoleResolver> personRoleResolvers)
            : base(personRoleResolvers)
        {
            //
        }
    }
}