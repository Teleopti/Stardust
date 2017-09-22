using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.ShareCalendar
{
    public interface ICheckCalendarPermissionCommand
    {
        void Execute(IDataSource dataSource, IPerson person, IPersonRepository personRepository);
    }
}