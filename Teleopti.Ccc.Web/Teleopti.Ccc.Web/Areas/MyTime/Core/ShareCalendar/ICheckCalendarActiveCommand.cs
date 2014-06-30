using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface ICheckCalendarActiveCommand
    {
        void Execute(IUnitOfWork uow, IPerson person);
    }
}