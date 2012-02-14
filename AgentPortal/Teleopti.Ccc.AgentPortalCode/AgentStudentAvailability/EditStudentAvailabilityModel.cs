using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortalCode.AgentStudentAvailability
{
    public class EditStudentAvailabilityModel : IEditStudentAvailabilityModel
    {
        private readonly IPermissionService _permissionService;
        private readonly IApplicationFunctionHelper _applicationFunctionHelper;

        public EditStudentAvailabilityModel(IPermissionService permissionService, IApplicationFunctionHelper applicationFunctionHelper)
        {
            _permissionService = permissionService;
            _applicationFunctionHelper = applicationFunctionHelper;
        }

        public TimeSpan? StartTimeLimitation { get; set; }
        public TimeSpan? EndTimeLimitation { get; set; }
        public bool EndTimeLimitationNextDay { get; set; }
        public TimeSpan? SecondStartTimeLimitation { get; set; }
        public TimeSpan? SecondEndTimeLimitation { get; set; }
        public bool SecondEndTimeLimitationNextDay { get; set; }

        public IList<StudentAvailabilityRestriction> StudentAvailabilityRestrictions { get; private set; }

        public bool CreateStudentAvailabilityIsPermitted
        {
            get
            {
                return
                 _permissionService.IsPermitted(
                     _applicationFunctionHelper.DefinedApplicationFunctionPaths.CreateStudentAvailability);
            }
        }


        public void SetStudentAvailabilityRestrictions(IList<StudentAvailabilityRestriction> studentAvailabilityRestrictions)
        {
            StudentAvailabilityRestrictions = studentAvailabilityRestrictions;
            SetFieldValues();
        }

        public static StudentAvailabilityRestriction CreateEmptyStudentAvailabilityRestriction()
        {
            var studentAvailabilityRestriction = new StudentAvailabilityRestriction();
            studentAvailabilityRestriction.StartTimeLimitation = new TimeLimitation(new TimeOfDayValidator(false));
            studentAvailabilityRestriction.EndTimeLimitation = new TimeLimitation(new TimeOfDayValidator(true));
            return studentAvailabilityRestriction;
        }

        public void SetValuesToStudentAvailabilityRestrictions()
        {
            StudentAvailabilityRestrictions = new List<StudentAvailabilityRestriction> {CreateEmptyStudentAvailabilityRestriction()};

            StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime = StartTimeLimitation;
            StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime = StartTimeLimitation;

            var endLimitation = EndTimeLimitation;
            if (endLimitation.HasValue && EndTimeLimitationNextDay)
                endLimitation = endLimitation.Value.Add(TimeSpan.FromDays(1));
            StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime = endLimitation;
            
            if (SecondStartTimeLimitation.HasValue && SecondEndTimeLimitation.HasValue)
            {
                var studentAvailabilityRestriction = CreateEmptyStudentAvailabilityRestriction();
                studentAvailabilityRestriction.StartTimeLimitation.MinTime = SecondStartTimeLimitation;
                studentAvailabilityRestriction.EndTimeLimitation.MaxTime = SecondStartTimeLimitation;
                var secondEndLimitation = SecondEndTimeLimitation;
                if (SecondEndTimeLimitationNextDay)
                    secondEndLimitation = secondEndLimitation.Value.Add(TimeSpan.FromDays(1));
                studentAvailabilityRestriction.EndTimeLimitation.MaxTime = secondEndLimitation;
                StudentAvailabilityRestrictions.Add(studentAvailabilityRestriction);
            }
        }

        private void SetFieldValues()
        {
            StartTimeLimitation = null;
            EndTimeLimitation = null;
            EndTimeLimitationNextDay = false;

            SecondStartTimeLimitation = null;
            SecondEndTimeLimitation = null;
            SecondEndTimeLimitationNextDay = false;

            if (StudentAvailabilityRestrictions == null) return;
            if (StudentAvailabilityRestrictions.Count > 0)
            {
                if (StudentAvailabilityRestrictions[0].StartTimeLimitation != null)
                {
                    StartTimeLimitation = StudentAvailabilityRestrictions[0].StartTimeLimitation.MinTime;
                }
                if (StudentAvailabilityRestrictions[0].EndTimeLimitation != null)
                {
                    EndTimeLimitation = StudentAvailabilityRestrictions[0].EndTimeLimitation.MaxTime;
                    if (EndTimeLimitation.HasValue &&
                        EndTimeLimitation.Value >= TimeSpan.FromDays(1))
                    {
                        EndTimeLimitationNextDay = true;
                        EndTimeLimitation = EndTimeLimitation.Value.Add(TimeSpan.FromDays(-1));
                    }
                }
            }
            if (StudentAvailabilityRestrictions.Count == 2)
            {
                if (StudentAvailabilityRestrictions[1].StartTimeLimitation != null)
                {
                    SecondStartTimeLimitation = StudentAvailabilityRestrictions[1].StartTimeLimitation.MinTime;
                }
                if (StudentAvailabilityRestrictions[1].EndTimeLimitation != null)
                {
                    SecondEndTimeLimitation = StudentAvailabilityRestrictions[1].EndTimeLimitation.MaxTime;
                    if (SecondEndTimeLimitation.HasValue && SecondEndTimeLimitation.Value >= TimeSpan.FromDays(1))
                    {
                        SecondEndTimeLimitationNextDay = true;
                        SecondEndTimeLimitation = SecondEndTimeLimitation.Value.Add(TimeSpan.FromDays(-1));
                    }
                }
            }
        }
    }
}
