using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.AgentPortalCode.Requests.ShiftTrade;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCodeTest.Requests.ShiftTrade
{
    [TestFixture]
    public class ShiftTradeDetailModelTest
    {
        private ShiftTradeDetailModel _target;
        private ShiftTradeSwapDetailDto _shiftTradeSwapDetailDto;
        private PersonDto _loggedOnPerson;
        private PersonDto _otherPerson;
        private SchedulePartDto _schedulePartFrom;
        private SchedulePartDto _schedulePartTo;

        [SetUp]
        public void Setup()
        {
            PreparePersons();
            PrepareScheduleParts();
            _shiftTradeSwapDetailDto = new ShiftTradeSwapDetailDto();
            _shiftTradeSwapDetailDto.PersonFrom = _loggedOnPerson;
            _shiftTradeSwapDetailDto.PersonTo = _otherPerson;
            _shiftTradeSwapDetailDto.DateFrom = new DateOnlyDto {DateTime = new DateTime(2009, 11, 17), DateTimeSpecified = true};
            _shiftTradeSwapDetailDto.DateTo = _shiftTradeSwapDetailDto.DateFrom;
            _shiftTradeSwapDetailDto.SchedulePartFrom = _schedulePartFrom;
            _shiftTradeSwapDetailDto.SchedulePartTo = _schedulePartTo;
            _target = new ShiftTradeDetailModel(_shiftTradeSwapDetailDto, _schedulePartFrom, _loggedOnPerson, _loggedOnPerson);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_shiftTradeSwapDetailDto,_target.ShiftTradeSwapDetailDto);

            Assert.AreEqual(UserTexts.Resources.Me,_target.Person);
            Assert.AreEqual(_shiftTradeSwapDetailDto.DateFrom.DateTime,_target.TradeDate);

            Assert.IsNotNull(_target.VisualProjection);
            Assert.AreEqual(3,_target.VisualProjection.LayerCollection.Count);
            Assert.AreEqual(TimeSpan.FromHours(20),_target.VisualProjection.Period().Value.StartTime);
            Assert.AreEqual(TimeSpan.FromHours(27), _target.VisualProjection.Period().Value.EndTime);
        	Assert.IsTrue(_target.VisualProjectionContainsAbsence);

            _target = new ShiftTradeDetailModel(_shiftTradeSwapDetailDto, _schedulePartTo, _otherPerson, _loggedOnPerson);
            Assert.AreEqual(_otherPerson.Name, _target.Person);
            Assert.IsNotNull(_target.VisualProjection);
            Assert.AreEqual(0,_target.VisualProjection.LayerCollection.Count);
            Assert.AreEqual("Da dayoff",_target.VisualProjection.DayOffName);
            Assert.IsTrue(_target.VisualProjection.IsDayOff);
        }

        private void PrepareScheduleParts()
        {
            List<ProjectedLayerDto> projectedLayersFrom = new List<ProjectedLayerDto>();
            projectedLayersFrom.Add(new ProjectedLayerDto
                                        {
                                            Description = "Act1",
                                            DisplayColor = new ColorDto(),
                                            Period =
                                                new DateTimePeriodDto
                                                    {
                                                        LocalStartDateTime = new DateTime(2009, 11, 17, 20, 0, 0),
                                                        LocalEndDateTime = new DateTime(2009, 11, 17, 23, 0, 0)
                                                    }
                                        });
            projectedLayersFrom.Add(new ProjectedLayerDto
            {
                Description = "Act2",
                DisplayColor = new ColorDto(),
                Period =
                    new DateTimePeriodDto
                    {
                        LocalStartDateTime = new DateTime(2009, 11, 17, 23, 0, 0),
                        LocalEndDateTime = new DateTime(2009, 11, 18, 02, 0, 0)
                    }
            });
            projectedLayersFrom.Add(new ProjectedLayerDto
            {
				IsAbsence = true,
                Description = "Act3",
                DisplayColor = new ColorDto(),
                Period =
                    new DateTimePeriodDto
                    {
                        LocalStartDateTime = new DateTime(2009, 11, 18, 02, 0, 0),
                        LocalEndDateTime = new DateTime(2009, 11, 18, 03, 0, 0)
                    }
            });
            _schedulePartFrom = new SchedulePartDto();
            _schedulePartFrom.ProjectedLayerCollection = projectedLayersFrom.ToArray();
            _schedulePartFrom.Date = new DateOnlyDto { DateTime = new DateTime(2009, 11, 17) };
            _schedulePartTo = new SchedulePartDto();
            _schedulePartTo.ProjectedLayerCollection = new ProjectedLayerDto[0];
            _schedulePartTo.Date = new DateOnlyDto { DateTime = new DateTime(2009, 11, 17) };
            _schedulePartTo.PersonDayOff = new PersonDayOffDto {Name = "Da dayoff"};
        }

        private void PreparePersons()
        {
            _loggedOnPerson = new PersonDto();
            _loggedOnPerson.Id = Guid.NewGuid().ToString();
            _loggedOnPerson.Name = "Ashley Andeen";
            _otherPerson = new PersonDto();
            _otherPerson.Id = Guid.NewGuid().ToString();
            _otherPerson.Name = "Prashant Arora";
        }
    }
}
