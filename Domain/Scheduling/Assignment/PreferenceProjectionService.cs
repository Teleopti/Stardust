using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class PreferenceProjectionService : ScheduleProjectionService
    {
        private bool _hasMainShift;

        public PreferenceProjectionService(IPerson person, DateTimePeriod period)
            : base(person, period)
        {
        }

        protected override IList<IVisualLayer> CreateAssignmentProjection(IPersonAssignment personAssignment)
        {
            if (_hasMainShift)
                return base.CreateAssignmentProjection(personAssignment);
            List<IVisualLayer> retList = new List<IVisualLayer>();
            foreach (IPersonalShift personalShift in personAssignment.PersonalShiftCollection)
            {
                retList.AddRange(((VisualLayerCollection)personalShift.ProjectionService().CreateProjection()).UnMergedCollection);
            }
            return retList;
        }

        protected override IList<IVisualLayer> CreateMeetingProjection(IPersonMeeting personMeeting, IEnumerable<IVisualLayer> assignmentProjection)
        {
            if (_hasMainShift)
                return base.CreateMeetingProjection(personMeeting, assignmentProjection);
            IActivity activity = personMeeting.BelongsToMeeting.Activity;

            return new List<IVisualLayer> { personMeeting.CreateVisualLayerFactory().CreateShiftSetupLayer(activity,personMeeting.Period) };
        }

        public override IVisualLayerCollection CreateProjection()
        {
            checkHasMainShift();
            return base.CreateProjection();
        }

        private void checkHasMainShift()
        {
            _hasMainShift = false;
            foreach (IPersonAssignment personAssignment in PersonAssignmentCollection)
            {
                if(personAssignment.MainShift!=null)
                    _hasMainShift = true;
            }
        }
    }
}
