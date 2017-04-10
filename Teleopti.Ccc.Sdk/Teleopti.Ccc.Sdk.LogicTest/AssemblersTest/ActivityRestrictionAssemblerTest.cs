using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ActivityRestrictionAssemblerTest
    {
	    [Test]
	    public void ShouldMapDomainEntityToDto()
	    {
		    var activity = ActivityFactory.CreateActivity("activity").WithId();
		    var fakeActivityRepository = new FakeActivityRepository();
		    fakeActivityRepository.Add(activity);

		    var activityAssembler = new ActivityAssembler(fakeActivityRepository);
		    var activityRestrictionDomainObjectCreator = new ActivityRestrictionDomainObjectCreator();
		    var target = new ActivityRestrictionAssembler<IActivityRestriction>(activityRestrictionDomainObjectCreator,
			    activityAssembler);

		    var activityRestriction = new ActivityRestriction(activity);
		    activityRestriction.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18));
		    activityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6),
			    TimeSpan.FromHours(10));
		    activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6),
			    TimeSpan.FromHours(10));

		    var dto = target.DomainEntityToDto(activityRestriction);

		    Assert.That(dto.EndTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.EndTimeLimitation.EndTime));
		    Assert.That(dto.EndTimeLimitation.MinTime, Is.EqualTo(activityRestriction.EndTimeLimitation.StartTime));
		    Assert.That(dto.StartTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.StartTimeLimitation.EndTime));
		    Assert.That(dto.StartTimeLimitation.MinTime,
			    Is.EqualTo(activityRestriction.StartTimeLimitation.StartTime));
		    Assert.That(dto.WorkTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.WorkTimeLimitation.EndTime));
		    Assert.That(dto.WorkTimeLimitation.MinTime, Is.EqualTo(activityRestriction.WorkTimeLimitation.StartTime));
		    Assert.That(dto.Activity.Id, Is.EqualTo(activityRestriction.Activity.Id));
	    }

	    [Test]
        public void ShouldMapDtoToDomainEntity()
        {
			var activity = new Activity("Lunch").WithId();
		    var activityRepository = new FakeActivityRepository();
			activityRepository.Add(activity);

		    var activityAssembler = new ActivityAssembler(activityRepository);
			var activityRestrictionDomainObjectCreator =new ActivityRestrictionDomainObjectCreator();
			var target = new ActivityRestrictionAssembler<IActivityRestriction>(activityRestrictionDomainObjectCreator, activityAssembler);
			
            var activityRestrictionDto = new ActivityRestrictionDto();
            activityRestrictionDto.Id = Guid.NewGuid();
            activityRestrictionDto.Activity = new ActivityDto();
            activityRestrictionDto.Activity.Id = activity.Id.GetValueOrDefault();
            activityRestrictionDto.StartTimeLimitation = new TimeLimitationDto
                                                             {
                MinTime = TimeSpan.FromHours(6),
                MaxTime = TimeSpan.FromHours(11)
            };
            activityRestrictionDto.EndTimeLimitation = new TimeLimitationDto
                                                           {
                                                               MinTime = TimeSpan.FromHours(16),
                                                               MaxTime = TimeSpan.FromHours(17)
                                                           };
            activityRestrictionDto.WorkTimeLimitation = new TimeLimitationDto
                                                            {
                MinTime = TimeSpan.FromHours(1),
                MaxTime = TimeSpan.FromHours(2)
            };
			
            var domainEntity = target.DtoToDomainEntity(activityRestrictionDto);

            Assert.That(domainEntity.EndTimeLimitation.EndTime, Is.EqualTo(activityRestrictionDto.EndTimeLimitation.MaxTime));
            Assert.That(domainEntity.EndTimeLimitation.StartTime, Is.EqualTo(activityRestrictionDto.EndTimeLimitation.MinTime));
            Assert.That(domainEntity.StartTimeLimitation.EndTime, Is.EqualTo(activityRestrictionDto.StartTimeLimitation.MaxTime));
            Assert.That(domainEntity.StartTimeLimitation.StartTime, Is.EqualTo(activityRestrictionDto.StartTimeLimitation.MinTime));
            Assert.That(domainEntity.WorkTimeLimitation.EndTime, Is.EqualTo(activityRestrictionDto.WorkTimeLimitation.MaxTime));
            Assert.That(domainEntity.WorkTimeLimitation.StartTime, Is.EqualTo(activityRestrictionDto.WorkTimeLimitation.MinTime));
            Assert.That(domainEntity.Activity.Id, Is.EqualTo(activityRestrictionDto.Activity.Id));
        }
    }
}