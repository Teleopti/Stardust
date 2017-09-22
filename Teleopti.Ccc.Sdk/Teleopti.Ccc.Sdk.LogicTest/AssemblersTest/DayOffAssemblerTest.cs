using System;
using System.Drawing;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class DayOffAssemblerTest
    {
        [Test]
        public void VerifyDomainEntityToDto()
        {
			var dayOffDomain = new DayOffTemplate(new Description("Day Off"))
			{
				DisplayColor = Color.Blue,
				Anchor = TimeSpan.FromHours(12),
				PayrollCode = "payrollcode007"
			}.WithId();
			dayOffDomain.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(10));

			var dayOffRep = new FakeDayOffTemplateRepository();
			dayOffRep.Add(dayOffDomain);
			var target = new DayOffAssembler(dayOffRep);
			
			var dayOffInfoDto = target.DomainEntityToDto(dayOffDomain);

            Assert.AreEqual(dayOffDomain.Id, dayOffInfoDto.Id);
            Assert.AreEqual(dayOffDomain.Description.Name, dayOffInfoDto.Name);
            Assert.AreEqual(dayOffDomain.Description.ShortName, dayOffInfoDto.ShortName);
			Assert.AreEqual(dayOffDomain.PayrollCode, dayOffInfoDto.PayrollCode);
            Assert.IsFalse(dayOffInfoDto.IsDeleted);
        }

	    [Test]
	    public void VerifyDtoToDomainEntity()
	    {
		    var dayOffDomain = new DayOffTemplate(new Description("Day Off"))
		    {
			    DisplayColor = Color.Blue,
			    Anchor = TimeSpan.FromHours(12),
			    PayrollCode = "payrollcode007"
		    }.WithId();
		    dayOffDomain.SetTargetAndFlexibility(TimeSpan.FromHours(36), TimeSpan.FromHours(10));

		    var dayOffRep = new FakeDayOffTemplateRepository();
		    dayOffRep.Add(dayOffDomain);
		    var target = new DayOffAssembler(dayOffRep);
			
		    var dayOffIntoDto = new DayOffInfoDto {Id = dayOffDomain.Id};

		    IDayOffTemplate dayOffTemplate = target.DtoToDomainEntity(dayOffIntoDto);
		    Assert.IsNotNull(dayOffTemplate);
	    }
    }
}
