using System;
using System.Drawing;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    public class MeetingPayload : Payload, IMeetingPayload
    {
        private readonly IMeeting _meeting;

        public MeetingPayload(IMeeting meeting)
        {
            _meeting = meeting;
        }

        public IMeeting Meeting
        {
            get { return _meeting; }
        }


        public override ITracker Tracker
        {
            get
            {
                return _meeting.Activity.Tracker;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool InWorkTime
        {
            get
            {
                return _meeting.Activity.InWorkTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool InPaidTime
        {
            get
            {
                return _meeting.Activity.InPaidTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

		public Description ConfidentialDescription(IPerson assignedPerson)
        {
            return _meeting.Activity.ConfidentialDescription(assignedPerson);
        }

		public Color ConfidentialDisplayColor(IPerson assignedPerson)
        {
            return _meeting.Activity.ConfidentialDisplayColor(assignedPerson);
        }

        public override IPayload UnderlyingPayload
        {
            get { return _meeting.Activity; }
        }

        public override bool InContractTime
        {
            get
            {
                return _meeting.Activity.InContractTime;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        public override Guid? Id
        {
            get
            {
                return _meeting.Id;
            }
        }
    }
}