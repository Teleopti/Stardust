using System.Reflection;
using Teleopti.Ccc.DatabaseConverter;
using Teleopti.Ccc.DatabaseConverter.CollectionConverter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverterTest.Helpers
{
    internal static class ConverterEventHelper
    {
        internal static void ExecuteOnPersisted<TNew, TOld>(CccConverter<TNew, TOld> converter, ObjectPairCollection<TOld, TNew> pairCollection) where TNew : IEntity
        {
            converter.GetType().GetMethod("OnPersisted", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(converter, new object[] {pairCollection});
        }
    }
}
