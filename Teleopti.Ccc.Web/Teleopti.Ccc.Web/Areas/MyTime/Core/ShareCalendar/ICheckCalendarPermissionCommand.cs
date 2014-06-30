using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface ICheckCalendarPermissionCommand
    {
        void Execute(IDataSource dataSource, IPerson person, IPersonRepository personRepository);
    }
}