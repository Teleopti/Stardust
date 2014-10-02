using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.ServerTest
{
    [TestFixture]
    public class CurrentAndNextLayerExtractorTest
    {
        private ICurrentAndNextLayerExtractor _target;
        private readonly DateTime _dateTime = DateTime.UtcNow;

        [SetUp]
        public void Setup()
        {
            _target = new CurrentAndNextLayerExtractor();
        }

        [Test]
        public void VerifyCurrentLayerAndNext()
        {
            var dateTime = DateTime.UtcNow;
            var scheduleLayers = new List<ScheduleLayer>
                {
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddMinutes(-5),
                            EndDateTime = dateTime.AddMinutes(5)
                        },
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddMinutes(5),
                            EndDateTime = dateTime.AddMinutes(10)
                        }
                };
            var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
            Assert.That(result.Item1.StartDateTime, Is.EqualTo(dateTime.AddMinutes(-5)));
            Assert.That(result.Item2.EndDateTime, Is.EqualTo(dateTime.AddMinutes(10)));
        }

        [Test]
        public void VerifyCurrentLayerAndNextNoLayerNow()
        {
            var dateTime = DateTime.UtcNow;
            var scheduleLayers = new List<ScheduleLayer>
                {
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddMinutes(5),
                            EndDateTime = dateTime.AddMinutes(10)
                        },
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddMinutes(10),
                            EndDateTime = dateTime.AddMinutes(20)
                        }
                };
            var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
            Assert.IsNull(result.Item1);
            Assert.That(result.Item2.EndDateTime, Is.EqualTo(dateTime.AddMinutes(10)));
        }

        [Test]
        public void VerifyCurrentLayerAndNextLastAssignment()
        {
            var dateTime = DateTime.UtcNow;
            var scheduleLayers = new List<ScheduleLayer>
                {
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddMinutes(-5),
                            EndDateTime = dateTime.AddMinutes(5)
                        },
                    new ScheduleLayer
                        {
                            StartDateTime = dateTime.AddDays(1),
                            EndDateTime = dateTime.AddDays(1)
                        }
                };
            var result = _target.CurrentLayerAndNext(dateTime, scheduleLayers);
            Assert.IsNull(result.Item2);
        }

        [Test]
        public void ShouldWorkWithNoReadModel()
        {
            var result = _target.CurrentLayerAndNext(DateTime.Now, new List<ScheduleLayer>());
            Assert.IsNull(result.Item1);
            Assert.IsNull(result.Item2);
        }

        [Test]
        public void ShouldWorkWhenReadModelIsOld()
        {
            var layers = new List<ScheduleLayer>
                {
                    new ScheduleLayer
                        {
                            EndDateTime = _dateTime.AddDays(-1)
                        },
                    new ScheduleLayer
                        {
                            EndDateTime = _dateTime.AddDays(-1)
                        }
                };
            var result = _target.CurrentLayerAndNext(_dateTime, layers);
            Assert.IsNull(result.Item1);
            Assert.IsNull(result.Item2);
        }
    }
}