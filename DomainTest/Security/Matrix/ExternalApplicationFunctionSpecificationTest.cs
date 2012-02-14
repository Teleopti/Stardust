using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Matrix;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.Matrix
{
    /// <summary>
    /// Tests for ExternalApplicationFunctionSpecification
    /// </summary>
    [TestFixture]
    public class ExternalApplicationFunctionSpecificationTest
    {
        private ExternalApplicationFunctionSpecification _target;
        private IList<IApplicationFunction> _originalList; 

        [SetUp]
        public void Setup()
        {
            _originalList = ApplicationFunctionFactory.CreateApplicationFunctionWithMatrixReports();
            _target = new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix);
        }

        [Test]
        public void VerifyIsSatisfy()
        {
            Assert.IsFalse(_target.IsSatisfiedBy(_originalList[2]));
            Assert.IsTrue(_target.IsSatisfiedBy(_originalList[3]));
        }

        [Test]
        public void VerifyFilterList()
        {    
            IList<IApplicationFunction> filteredList = new List<IApplicationFunction>(_originalList)
                .FindAll(new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix).IsSatisfiedBy);

            Assert.AreEqual(3, filteredList.Count);
            Assert.AreSame(filteredList[0], _originalList[3]);
            Assert.AreSame(filteredList[1], _originalList[5]);
            Assert.AreSame(filteredList[2], _originalList[6]);
            
        }

    }
}