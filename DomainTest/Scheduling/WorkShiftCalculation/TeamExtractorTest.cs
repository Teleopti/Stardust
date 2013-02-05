using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
     [TestFixture]
    public class TeamExtractorTest
    {
         private MockRepository _mocks;
         private ITeamExtractor _target;
         private IGroupPersonBuilderForOptimization _groupPersonBuilderForOptimization;
         private IScheduleMatrixPro _matrixPro;
         private IGroupPerson _groupPerson;

         [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
             _groupPersonBuilderForOptimization = _mocks.StrictMock<IGroupPersonBuilderForOptimization>();
             _matrixPro = _mocks.StrictMock<IScheduleMatrixPro>();
             _groupPerson = _mocks.StrictMock<IGroupPerson>();

             _target = new TeamExtractor(new List<IScheduleMatrixPro>{_matrixPro}, _groupPersonBuilderForOptimization);
        }

        [Test]
        public void ShouldReturnTheTeamForPerson()
        {
            var person = new Person();
            using (_mocks.Record())
            {
                Expect.Call(_matrixPro.Person).Return(person);
                Expect.Call(_groupPersonBuilderForOptimization.BuildGroupPerson(person, new DateOnly())).IgnoreArguments().Return(
                    _groupPerson);
            }
            using (_mocks.Playback())
            {
                Assert.AreEqual(_target.GetRandomTeam(new DateOnly( )), _groupPerson );
            }
        }
    }

   
}
