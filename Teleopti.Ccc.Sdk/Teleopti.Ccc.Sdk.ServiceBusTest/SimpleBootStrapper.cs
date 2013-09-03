using Rhino.ServiceBus.Autofac;

namespace Teleopti.Ccc.Sdk.ServiceBusTest
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Strapper", Justification = "As the base class is named as it is, this will remain like this."),
     System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "BootStrapper", Justification = "As the base class is named as it is, this will remain like this.")]
    public class SimpleBootStrapper : AutofacBootStrapper
    {
    }
}