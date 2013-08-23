using Microsoft.Practices.CompositeUI;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Common.Services
{
    public delegate void ActionDelegate(object caller, object target);

    public interface IActionCatalogService
    {
        bool CanExecute(string action, WorkItem context, object caller, object target);
        bool CanExecute(string action);
        void Execute(string action, WorkItem context, object caller, object target);

        void RegisterSpecificCondition(string action, IActionCondition actionCondition);
        void RegisterGeneralCondition(IActionCondition actionCondition);
        void RemoveSpecificCondition(string action, IActionCondition actionCondition);
        void RemoveGeneralCondition(IActionCondition actionCondition);

        void RemoveActionImplementation(string action);
        void RegisterActionImplementation(string action, ActionDelegate actionDelegate);
    }
}