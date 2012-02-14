using System.Linq;
using System.Windows.Input;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.WinCode.Common.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction.Commands
{
    public class AddPreferenceRestrictionCommandModel : ApplicationCommandModel
    {
        private readonly RestrictionEditorViewModel _target;

        public AddPreferenceRestrictionCommandModel(RestrictionEditorViewModel model)
            : base(DefinedRaptorApplicationFunctionPaths.ModifyPersonRestriction)
        {
            _target = model;
            Command = RestrictionEditorRoutedCommands.AddPreferenceRestriction;
           
        }

        public override string Text
        {
            get { return UserTexts.Resources.AddPreferenceRestriction; }
        }

        public override void OnExecute(object sender, ExecutedRoutedEventArgs e)
        {
            IScheduleDay part = _target.SchedulePart;

            IPreferenceRestriction restriction = new PreferenceRestriction();
            ICccTimeZoneInfo timeZoneInfo = part.TimeZone;
            var agentLocalDate = new DateOnly(timeZoneInfo.ConvertTimeFromUtc(part.Period.StartDateTime, timeZoneInfo));
            var preferenceDay = new PreferenceDay(part.Person, agentLocalDate, restriction);

            _target.AddPreferenceDay(preferenceDay, part);
        }

        protected override bool CanExecute
        {
            get
            {
             if (_target.SchedulePart == null) return false;

                var restrictions = _target.RestrictionModels;
                return restrictions.OfType<PreferenceRestrictionViewModel>().IsEmpty();

            }
        }
    }
}
