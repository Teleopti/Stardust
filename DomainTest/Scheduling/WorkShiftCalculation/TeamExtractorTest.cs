using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.WorkShiftCalculation
{
     [TestFixture]
    public class TeamExtractorTest
    {
         private MockRepository _mocks;
         private ITeamExtractor _target;

         [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _target = new TeamExtractor(new List<IScheduleMatrixPro>());
        }

        [Test]
        public void ShouldVerifyRandomTeamSelection()
        {
           
        }
    }

   
}
