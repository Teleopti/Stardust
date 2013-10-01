using System;
using System.Collections.Generic;
using Teleopti.Ccc.WinCode.PeopleAdmin.Models;

namespace Teleopti.Ccc.WinCode.PeopleAdmin.Comparers
{
    public class WorkflowControlSetComparer : IComparer<PersonGeneralModel>
    {
        public int Compare(PersonGeneralModel x, PersonGeneralModel y)
        {
            string firstString = x.WorkflowControlSet == null ? string.Empty : x.WorkflowControlSet.Name;
            string secondString = y.WorkflowControlSet == null ? string.Empty : y.WorkflowControlSet.Name;
            return string.Compare(firstString, secondString, StringComparison.CurrentCulture);
        }
    }
}