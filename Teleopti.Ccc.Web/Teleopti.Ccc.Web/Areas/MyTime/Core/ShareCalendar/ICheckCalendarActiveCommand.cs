using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface ICheckCalendarActiveCommand
    {
        void Execute(IUnitOfWork uow, IPerson person);
    }
}