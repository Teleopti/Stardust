using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.LogicTest.AssemblersTest
{
    [TestFixture]
    public class ActivityRestrictionAssemblerTest
    {
        private ActivityRestrictionAssembler<IActivityRestriction> _target;
        private MockRepository _mocks;
        private IAssembler<IActivity, ActivityDto> _activityAssembler;
        private IActivityRestrictionDomainObjectCreator<IActivityRestriction> _activityRestrictionDomainObjectCreator;

        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _activityAssembler = _mocks.StrictMock<IAssembler<IActivity,ActivityDto>>();
            _activityRestrictionDomainObjectCreator =
                _mocks.StrictMock<IActivityRestrictionDomainObjectCreator<IActivityRestriction>>();
            _target = new ActivityRestrictionAssembler<IActivityRestriction>(_activityRestrictionDomainObjectCreator, _activityAssembler);
        }

        [Test]
        public void ShouldMapDomainEntityToDto()
        {
            IActivity activity = ActivityFactory.CreateActivity("activity");
            activity.SetId(Guid.NewGuid());
            var activityRestriction = new ActivityRestriction(activity);
            activityRestriction.EndTimeLimitation = new EndTimeLimitation(TimeSpan.FromHours(17), TimeSpan.FromHours(18));
            activityRestriction.StartTimeLimitation = new StartTimeLimitation(TimeSpan.FromHours(6),
                                                                              TimeSpan.FromHours(10));
            activityRestriction.WorkTimeLimitation = new WorkTimeLimitation(TimeSpan.FromHours(6),
                                                                            TimeSpan.FromHours(10));
            using (_mocks.Record())
            {
                Expect.Call(_activityAssembler.DomainEntityToDto(activity)).Return(new ActivityDto {Id = activity.Id});
            }
            using (_mocks.Playback())
            {
                var dto = _target.DomainEntityToDto(activityRestriction);

                Assert.That(dto.EndTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.EndTimeLimitation.EndTime));
                Assert.That(dto.EndTimeLimitation.MinTime, Is.EqualTo(activityRestriction.EndTimeLimitation.StartTime));
                Assert.That(dto.StartTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.StartTimeLimitation.EndTime));
                Assert.That(dto.StartTimeLimitation.MinTime,
                            Is.EqualTo(activityRestriction.StartTimeLimitation.StartTime));
                Assert.That(dto.WorkTimeLimitation.MaxTime, Is.EqualTo(activityRestriction.WorkTimeLimitation.EndTime));
                Assert.That(dto.WorkTimeLimitation.MinTime, Is.EqualTo(activityRestriction.WorkTimeLimitation.StartTime));
                Assert.That(dto.Activity.Id, Is.EqualTo(activityRestriction.Activity.Id));
            }
        }

        [Test]
        public void ShouldMapDtoToDomainEntity()
        {
            _mocks.Record();

            IActivity activity = new Activity("Lunch");
            Guid activityGuid = Guid.NewGuid();
            activity.SetId(activityGuid);
            var activityRestrictionDto = new ActivityRestrictionDto();
            activityRestrictionDto.Id = Guid.NewGuid();
            activityRestrictionDto.Activity = new ActivityDto();
            activityRestrictionDto.Activity.Id = activityGuid;
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

            Expect.Call(_activityAssembler.DtoToDomainEntity(activityRestrictionDto.Activity))
                .Return(activity)
                .Repeat.Once();
            Expect.Call(_activityRestrictionDomainObjectCreator.CreateNewDomainObject(activity)).Return(
                new ActivityRestriction(activity));

            _mocks.ReplayAll();

            var domainEntity = _target.DtoToDomainEntity(activityRestrictionDto);

            _mocks.VerifyAll();

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