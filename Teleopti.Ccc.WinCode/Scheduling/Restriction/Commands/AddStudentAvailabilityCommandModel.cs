using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands
{
    public class AddStudentAvailabilityCommandModel : ApplicationCommandModel
    {
        private readonly RestrictionEditorViewModel _target;

        public AddStudentAvailabilityCommandModel(RestrictionEditorViewModel model)
            : base(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction)
        {
            _target = model;
            Command = RestrictionEditorRoutedCommands.AddStudentAvailability;
        }

        public override string Text
        {
            get { return UserTexts.Resources.AddStudentAvailability; }
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
                IScheduleDay part = _target.SchedulePart;
                IStudentAvailabilityRestriction restriction = new StudentAvailabilityRestriction();
                ICccTimeZoneInfo timeZoneInfo = part.TimeZone;

                var agentLocalDate = new DateOnly(timeZoneInfo.ConvertTimeFromUtc(part.Period.StartDateTime, timeZoneInfo));
                var studRestriction = new StudentAvailabilityDay(part.Person, agentLocalDate, new  List<IStudentAvailabilityRestriction>{ restriction});
                _target.AddStudentAvailabilityDay(studRestriction, part);
        }

        protected override bool CanExecute
        {
            get
            {
                if (_target.SchedulePart == null) return false;
                var restrictions = _target.RestrictionModels;
                return restrictions.OfType<AvailableRestrictionViewModel>().IsEmpty();

            }
        }
    }
}
