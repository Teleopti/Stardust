using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Rhino.Mocks;
using NUnit.Framework;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
    [TestFixture]
    public class RestoreSchedulePartServiceTest
    {
        private RestoreSchedulePartService _target;
        private ISchedulePartModifyAndRollbackService _rollbackService;
        private IScheduleDay _destination;
        private IScheduleDay _source;
        private IPersonAssignment _personAssignment;
        private IPersonAbsence _personAbsence;
        private ReadOnlyCollection<IPersonAssignment> _personAssignments;
        private ReadOnlyCollection<IPersonAbsence> _personAbsences;
        private MockRepository _mocks;
        
        [SetUp]
        public void Setup()
        {
            _mocks = new MockRepository();
            _destination = _mocks.StrictMock<IScheduleDay>();
            _source = _mocks.StrictMock<IScheduleDay>();
            _personAssignment = _mocks.StrictMock<IPersonAssignment>();
            _personAbsence = _mocks.StrictMock<IPersonAbsence>();
            _personAssignments = new ReadOnlyCollection<IPersonAssignment>(new List<IPersonAssignment>{_personAssignment});
            _personAbsences = new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence> { _personAbsence });
            _rollbackService = _mocks.StrictMock<ISchedulePartModifyAndRollbackService>();
            _target = new RestoreSchedulePartService(_rollbackService, _destination, _source);
        }

        [Test]
        public void ShouldRestore()
        {
            using (_mocks.Record())
            {
                Expect.Call(() => _destination.Clear<IPersonAssignment>());
            	Expect.Call(_destination.PersonAbsenceCollection()).Return(
            		new ReadOnlyCollection<IPersonAbsence>(new List<IPersonAbsence>()));
                Expect.Call(() => _destination.Clear<IPersonDayOff>());

                Expect.Call(_source.PersonAssignmentCollectionDoNotUse()).Return(_personAssignments);
                Expect.Call(_source.PersonAbsenceCollection()).Return(_personAbsences);

                Expect.Call(_personAssignment.NoneEntityClone()).Return(_personAssignment);
                Expect.Call(_personAbsence.NoneEntityClone()).Return(_personAbsence);

                Expect.Call(() =>_destination.Add(_personAssignment));
                Expect.Call(() => _destination.Add(_personAbsence));

                Expect.Call(() => _rollbackService.Modify(_destination));
            }

            using(_mocks.Playback())
            {
                _target.Restore();
            }
        }
    }
}
