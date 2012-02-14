using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Persisters {
    public interface IPersonAbsenceAccountValidator
    {
        void Validate(IPersonAbsenceAccount personAbsenceAccount);
    }
}