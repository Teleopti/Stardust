using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Presentation
{
    public class ReportSettingsScheduleAuditingModel
    {
        private readonly IList<IPerson> _modifiedBy = new List<IPerson>();
        private readonly IList<IPerson> _agents = new List<IPerson>();
        public DateOnlyPeriod ChangePeriod { get; set; }
        public DateOnlyPeriod SchedulePeriod { get; set; }
        public DateOnlyPeriod ChangePeriodDisplay { get; set; }
        public DateOnlyPeriod SchedulePeriodDisplay { get; set; }

        public void AddModifier(IPerson person)
        {
            if(!_modifiedBy.Contains(person))
                _modifiedBy.Add(person);
        }

        public void RemoveModifier(IPerson person)
        {
            _modifiedBy.Remove(person);
        }

        public IList<IPerson> ModifiedBy
        {
            get { return _modifiedBy; }
        }

        public void AddAgent(IPerson person)
        {
            if(!_agents.Contains(person))
                _agents.Add(person);
        }

        public void RemoveAgent(IPerson person)
        {
            _agents.Remove(person);
        }

        public IList<IPerson> Agents => _agents;

		public string AgentsNameCommaSeparated()
        {
            if (_agents == null || _agents.Count == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var sortedList = _agents.OrderBy(n => n.Name.FirstName).ToList();

            foreach (var person in sortedList)
            {
                if ((stringBuilder.Length + person.Name.ToString(NameOrderOption.FirstNameLastName).Length) > 650)
                {
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    return stringBuilder.Append("...").ToString();
                }

                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", person.Name.ToString(NameOrderOption.FirstNameLastName));
            }

            return stringBuilder.ToString(0, stringBuilder.Length - 1);
        }

        public string ModifiedByNameCommaSeparated()
        {
            if (_modifiedBy == null || _modifiedBy.Count == 0)
                return string.Empty;

            var stringBuilder = new StringBuilder();
            var sortedList = _modifiedBy.OrderBy(n => n.Name.FirstName).ToList();

            foreach (var person in sortedList)
            {
                if((stringBuilder.Length + person.Name.ToString(NameOrderOption.FirstNameLastName).Length) > 650)
                {
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                    return stringBuilder.Append("...").ToString();
                }

                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0},", person.Name.ToString(NameOrderOption.FirstNameLastName));
            }

            return stringBuilder.ToString(0, stringBuilder.Length - 1);
        }
    }
}
