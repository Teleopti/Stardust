using Microsoft.Practices.Composite.Presentation.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Budgeting.Events
{
    public class BudgetGroupNeedsRefresh : CompositePresentationEvent<IBudgetGroup>
    {
    }
}