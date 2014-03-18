using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
    public interface IAbsenceAccountProvider
    {
        IAccount GetPersonAccount(IAbsence absence, DateOnly date);
    }
}