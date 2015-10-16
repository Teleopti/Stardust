using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SystemCheck
{

    public class SystemCheckerValidator
    {
        private readonly ICollection<string> _result;
        private ICollection<ISystemCheck> _systemCheckCollection;
        
        public SystemCheckerValidator(IEnumerable<ISystemCheck> systemChecks)
        {
            _result = new List<string>();
            _systemCheckCollection = new HashSet<ISystemCheck>(systemChecks);
        }

        public IEnumerable<string> Result
        {
            get { return _result; }
        }

        
        public bool IsOk()
        {
            var ok = true;
            _result.Clear();
            foreach (var systemCheck in _systemCheckCollection)
            {
                if (!systemCheck.IsRunningOk())
                {
                    ok = false;
                    _result.Add(systemCheck.WarningText);
                }
            }
            return ok;
        }
    }
}
