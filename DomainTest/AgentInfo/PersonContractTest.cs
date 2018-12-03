using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    /// <summary>
    /// Class for PersonContract tests
    /// </summary>
    [TestFixture]
    public class PersonContractTest
    {
        private IPersonContract _PersonContract;
        private IContract _contract;
        private IPartTimePercentage _partTimePercentage;
        private IContractSchedule _contractSchedule;


        /// <summary>
        /// Runs once per test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _contract = ContractFactory.CreateContract("Hourly");
            _contractSchedule = ContractScheduleFactory.CreateContractSchedule("Dummy");
            _partTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Full time");

            _PersonContract = new PersonContract(_contract,_partTimePercentage,_contractSchedule);
        }

        /// <summary>
        /// Determines whether this instance can be created and properties are set.
        /// </summary>
        [Test]
        public void CanCreateAndPropertiesAreSet()
        {
            Assert.IsNotNull(_PersonContract);
            Assert.AreEqual(_contract, _PersonContract.Contract);
            Assert.AreEqual(_contractSchedule, _PersonContract.ContractSchedule);
            Assert.AreEqual(_partTimePercentage, _PersonContract.PartTimePercentage);
        }

        /// <summary>
        /// Verifies the properties can be set.
        /// </summary>
        [Test]
        public void VerifyPropertiesCanBeSet()
        {
            IContract newContract = ContractFactory.CreateContract("Student");
            IContractSchedule newContractSchedule = ContractScheduleFactory.CreateContractSchedule("Dummy2");
            IPartTimePercentage newPartTimePercentage = PartTimePercentageFactory.CreatePartTimePercentage("Part time 30%");
                     
            _PersonContract.Contract = newContract;
            _PersonContract.ContractSchedule = newContractSchedule;
            _PersonContract.PartTimePercentage = newPartTimePercentage;

            Assert.IsNotNull(_PersonContract);
            Assert.AreEqual(newContract, _PersonContract.Contract);
            Assert.AreEqual(newContractSchedule, _PersonContract.ContractSchedule);
            Assert.AreEqual(newPartTimePercentage, _PersonContract.PartTimePercentage);
        }

        /// <summary>
        /// Verifies the can get part time average work time per day.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-23
        /// </remarks>
        [Test]
        public void VerifyCanGetPartTimeAverageWorkTimePerDay()
        {
            IPartTimePercentage partTime = PartTimePercentageFactory.CreatePartTimePercentage("75%");
            partTime.Percentage = new Percent(0.75d);

            Assert.AreEqual(TimeSpan.FromHours(8d), _PersonContract.AverageWorkTimePerDay); //Eight hours is default value for contracts
            _PersonContract.PartTimePercentage = partTime;
            Assert.AreEqual(TimeSpan.FromHours(6d),_PersonContract.AverageWorkTimePerDay);
        }

        [Test]
        public void VerifyClone()
        {
            IPersonContract personContract = (IPersonContract) _PersonContract.Clone();
            Assert.AreEqual(personContract.AverageWorkTimePerDay,_PersonContract.AverageWorkTimePerDay);
            Assert.AreEqual(personContract.Contract,_PersonContract.Contract);
            Assert.AreEqual(personContract.ContractSchedule,_PersonContract.ContractSchedule);
            Assert.AreEqual(personContract.PartTimePercentage,_PersonContract.PartTimePercentage);
            Assert.AreNotEqual(personContract,_PersonContract);
        }

        /// <summary>
        /// Protected constructor works.
        /// </summary>
        [Test]
        public void ProtectedConstructorWorks()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_PersonContract.GetType()));
        }

        [Test]
        public void VerifyPropertyContractMustNotSetNull()
        {
            Assert.Throws<ArgumentNullException>(() => _PersonContract.Contract = null);
        }

        [Test]
        public void VerifyPropertyPartTimePercentageMustNotSetNull()
        {
			Assert.Throws<ArgumentNullException>(() => _PersonContract.PartTimePercentage = null);
        }

        [Test]
        public void VerifyPropertyContractScheduleMustNotSetNull()
        {
			Assert.Throws<ArgumentNullException>(() => _PersonContract.ContractSchedule = null);
        }
    }
}