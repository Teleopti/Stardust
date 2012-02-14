using System.Collections;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.AgentPortalCode.AgentSchedule;
using Teleopti.Ccc.Sdk.Client.SdkServiceReference;

namespace Teleopti.Ccc.AgentPortalCodeTest.AgentSchedule
{
    [TestFixture]
    public class LegendsTest
    {
        readonly MockRepository mocker = new MockRepository();
        private LegendsPresenter target;//
        private ILegendsView view;
        private ITeleoptiSchedulingService sdk;

        [SetUp]
        public void Setup()
        {
            view = mocker.StrictMock<ILegendsView>();
            sdk = mocker.StrictMock<ITeleoptiSchedulingService>();
            target = new LegendsPresenter(view, sdk);
        }

        //Hmm crappy test
        [Test]
        public void CanInitializeView()
        {
            using (mocker.Record())
            {
                var absenceDtos = new[]
                                   {
                                       new AbsenceDto {Name = "One", DisplayColor = new ColorDto()},
                                       new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                                       new AbsenceDto {Name = "Three", DisplayColor = new ColorDto()}
                                   };
                
                var activityDtos = new[]
                                   {
                                       new ActivityDto {Description = "One", DisplayColor = new ColorDto()},
                                       new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                       new ActivityDto {Description = "Three", DisplayColor = new ColorDto()}
                                   };

                Expect.Call(sdk.GetAbsences(new AbsenceLoadOptionDto())).Return(absenceDtos).IgnoreArguments();
                Expect.Call(sdk.GetActivities(new LoadOptionDto())).Return(activityDtos).IgnoreArguments();
                Expect.Call(view.AbsenceDataSource).PropertyBehavior().IgnoreArguments();
                Expect.Call(view.ActivityDataSource).PropertyBehavior().IgnoreArguments();

                view.AbsenceDataSource = new ArrayList(absenceDtos);
                view.ActivityDataSource = new ArrayList(activityDtos);

                view.ActivityHeight = 81;
                view.AbsenceHeight = 81;
                Expect.Call(view.AbsenceHeight).Return(81);
                Expect.Call(view.ActivityHeight).Return(81);
            }

            using (mocker.Playback())
            {
                target.Initialize();
                Assert.AreEqual(170, target.Height());
            }
        }

        [Test]
        public void CheckMaxHeight()
        {
            using(mocker.Record())
            {
                var absenceDtos = GetManyAbsenceDtos();
                var activityDtos = getManyActivityDtos();

                Expect.Call(sdk.GetAbsences(new AbsenceLoadOptionDto())).Return(absenceDtos).IgnoreArguments();
                Expect.Call(sdk.GetActivities(new LoadOptionDto())).Return(activityDtos).IgnoreArguments();
                Expect.Call(view.AbsenceDataSource).PropertyBehavior().IgnoreArguments();
                Expect.Call(view.ActivityDataSource).PropertyBehavior().IgnoreArguments();

                view.AbsenceDataSource = new ArrayList(absenceDtos);
                view.ActivityDataSource = new ArrayList(activityDtos);

                view.ActivityHeight = 336;
                view.AbsenceHeight = 353;
                Expect.Call(view.AbsenceHeight).Return(353);
                Expect.Call(view.ActivityHeight).Return(353);
            }

            using(mocker.Playback())
            {
                target.Initialize();
                Assert.AreEqual(714, target.Height());
            }
        }

        private ActivityDto[] getManyActivityDtos()
        {
            return new[]
                               {
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "One", DisplayColor = new ColorDto()}, 
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "Two", DisplayColor = new ColorDto()},
                                   new ActivityDto {Description = "Three", DisplayColor = new ColorDto()}
                               };
        }

        private AbsenceDto[] GetManyAbsenceDtos()
        {
            return new []
                       {
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "One", DisplayColor = new ColorDto()}, 
                           new AbsenceDto {Name = "Two", DisplayColor = new ColorDto()},
                           new AbsenceDto {Name = "Three",DisplayColor = new ColorDto()}
                       };
        }
    }
}
