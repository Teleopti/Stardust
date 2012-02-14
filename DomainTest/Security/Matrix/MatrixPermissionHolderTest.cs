using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
    [TestFixture]
    public class MatrixPermissionHolderTest
    {
        private MatrixPermissionHolder _target;
        private IPerson _person;
        private ITeam _team;
        private IApplicationFunction _applicationFunction;

        [SetUp]
        public void Setup()
        {

            _person = PersonFactory.CreatePerson("AA");
            _team = TeamFactory.CreateSimpleTeam();
            _applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("APP");
            _target = new MatrixPermissionHolder(_person, _team, false);
        }

        [Test]
        public void VerifyHashCodeWithoutApplicationFunction()
        {
            ICollection<MatrixPermissionHolder> list = new HashSet<MatrixPermissionHolder> { _target };

            MatrixPermissionHolder sameObject = new MatrixPermissionHolder(_person, _team, false);
            list.Add(sameObject);
            Assert.AreEqual(_target.GetHashCode(), sameObject.GetHashCode());
            Assert.AreEqual(1, list.Count());

            IPerson person2 = PersonFactory.CreatePerson("BB");
            MatrixPermissionHolder differentObject = new MatrixPermissionHolder(person2, _team, false);
            list.Add(differentObject);
            Assert.AreEqual(2, list.Count());
        }

        [Test]
        public void VerifyHashCodeWithApplicationFunction()
        {
            _target = new MatrixPermissionHolder(_person, _team, false, _applicationFunction);

            ICollection<MatrixPermissionHolder> list = new HashSet<MatrixPermissionHolder> { _target };

            MatrixPermissionHolder sameObject = new MatrixPermissionHolder(_person, _team, false, _applicationFunction);
            list.Add(sameObject);
            Assert.AreEqual(_target.GetHashCode(), sameObject.GetHashCode());
            Assert.AreEqual(1, list.Count());

            IApplicationFunction applicationFunction = ApplicationFunctionFactory.CreateApplicationFunction("2");
            MatrixPermissionHolder differentObject = new MatrixPermissionHolder(_person, _team, false, applicationFunction);

            list.Add(differentObject);
            Assert.AreEqual(2, list.Count());
        }

    }
}