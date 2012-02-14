using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.WinCode.PersonAdmin;

namespace Teleopti.Ccc.WinCodeTest.PersonAdmin
{
    /// <summary>
    /// Tests for ContractScheduleMapper
    /// </summary>
    [TestFixture]
    public class ContractScheduleMapperTest
    {
        private ContractScheduleMapper _target;

        [SetUp]
        public void Setup()
        {
            _target = new ContractScheduleMapper();
            ContractSchedule contractSchedule = new ContractSchedule("TestContractSchedule");
            ((IEntity)contractSchedule).SetId(Guid.NewGuid());
            _target.ContainedEntity = contractSchedule;
        }

        /// <summary>
        /// Verifies the properties not null.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-04-18
        /// </remarks>
        [Test]
        public void VerifyPropertiesNotNull()
        {
            Assert.IsNotNull(_target.Id);
            Assert.IsNotEmpty(_target.Name);
        }

    }
}
