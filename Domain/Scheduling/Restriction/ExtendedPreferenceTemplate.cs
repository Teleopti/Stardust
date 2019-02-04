using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restriction
{
    public class ExtendedPreferenceTemplate : AggregateRoot_Events_ChangeInfo, IExtendedPreferenceTemplate
    {
        private readonly Color _displayColor;
        private readonly IPerson _person;
        private readonly IPreferenceRestrictionTemplate _restriction;
        private readonly string _name;

        protected ExtendedPreferenceTemplate(){}

        public ExtendedPreferenceTemplate(IPerson person, IPreferenceRestrictionTemplate preferenceRestrictionTemplate, string name, Color color)
            : this()
        {
            _person = person;
            _restriction = preferenceRestrictionTemplate;
            _name = name;
            _displayColor = color;
            _restriction.SetParent(this);
        }

        public virtual IPreferenceRestrictionTemplate Restriction
        {
            get { return _restriction; }
        }

        public virtual Color DisplayColor
        {
            get { return _displayColor; }
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual IPerson Person
        {
            get { return _person; }
        }
    }
}