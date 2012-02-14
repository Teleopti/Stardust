using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
    [TestFixture]
    public class ApplicationFunctionResolverTest
    {
        private IApplicationFunctionResolver _target;
        private IApplicationRole _applicationRole;
        private IApplicationFunction _raptorApplicationFunction;
        private IApplicationFunction _matrixApplicationFunction;
        private MockRepository _mocks;
        private ITeam _personsTeam;
        private ITeam _otherTeam;
        private IPerson _person;
        private MatrixPermissionHolder _item1;
        private MatrixPermissionHolder _item2;
        private MatrixPermissionHolder _item3;
        private HashSet<MatrixPermissionHolder> _inputList;
        private IFunctionsForRoleProvider _functionsForRoleProvider;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _applicationRole = ApplicationRoleFactory.CreateRole("Role1", "Role1");
            _raptorApplicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("App1");
            _matrixApplicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("App2");
            _matrixApplicationFunction.ForeignSource = DefinedForeignSourceNames.SourceMatrix;
            _applicationRole.AddApplicationFunction(_raptorApplicationFunction);
            _applicationRole.AddApplicationFunction(_matrixApplicationFunction);
            _functionsForRoleProvider = _mocks.StrictMock<IFunctionsForRoleProvider>();
            _target = new ApplicationFunctionResolver(_functionsForRoleProvider);

            _person = PersonFactory.CreatePerson("AA");
            _personsTeam = TeamFactory.CreateSimpleTeam("TeamPerson");
            _otherTeam = TeamFactory.CreateSimpleTeam("TeamOther");

            _item1 = new MatrixPermissionHolder(_person, _personsTeam, true);
            _item2 = new MatrixPermissionHolder(_person, _personsTeam, false);
            _item3 = new MatrixPermissionHolder(_person, _otherTeam, false);
            _inputList = new HashSet<MatrixPermissionHolder> { _item1, _item2, _item3 };
        }

        [Test]
        public void VerifyResolveApplicationFunction()
        {
            using (_mocks.Record())
            {
                Expect.Call(_functionsForRoleProvider.AvailableFunctions(_applicationRole, null)).Return(
                    new[] {_raptorApplicationFunction,_matrixApplicationFunction,});
            }
            using (_mocks.Playback())
            {
                MatrixPermissionHolder expectedNotContained = new MatrixPermissionHolder(_person, _personsTeam, false,
                                                                                         _raptorApplicationFunction);
                MatrixPermissionHolder expectedContained = new MatrixPermissionHolder(_person, _personsTeam, false,
                                                                                      _matrixApplicationFunction);

                HashSet<MatrixPermissionHolder> result = _target.ResolveApplicationFunction(_inputList, _applicationRole,
                                                                                            null);
                Assert.AreEqual(3, result.Count);

                Assert.IsFalse(result.Contains(expectedNotContained));
                Assert.IsTrue(result.Contains(expectedContained));
            }
        }

        [Test]
        public void VerifyResolveApplicationFunctionWithAllFunction()
        {
            IApplicationFunction allApplicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("All");

            IList<IApplicationFunction> applicationFunctions =
                new List<IApplicationFunction>{_raptorApplicationFunction, _matrixApplicationFunction, allApplicationFunction};

            MatrixPermissionHolder expectedNotContained = new MatrixPermissionHolder(_person, _personsTeam, false, _raptorApplicationFunction);
            MatrixPermissionHolder expectedContained = new MatrixPermissionHolder(_person, _personsTeam, false, _matrixApplicationFunction);
            _applicationRole.AddApplicationFunction(allApplicationFunction);

            using(_mocks.Record())
            {
                Expect.Call(_functionsForRoleProvider.AvailableFunctions(_applicationRole,null))
                    .Return(applicationFunctions)
                    .Repeat.AtLeastOnce();
            }
            using (_mocks.Playback())
            {
                HashSet<MatrixPermissionHolder> result = _target.ResolveApplicationFunction(_inputList, _applicationRole,null);
                Assert.AreEqual(3, result.Count);

                Assert.IsFalse(result.Contains(expectedNotContained));
                Assert.IsTrue(result.Contains(expectedContained));
            }
        }
    }
}