using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Collections
{
    /// <summary>
    /// Collection that loads its enteties when created
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// TODO: Add messagebroker funtionality, so when something is added, its added to the collection (must be on guithread, use dispatcher)
    /// Created by: henrika
    /// Created date: 2009-10-01
    /// </remarks>
    public class CommonRootCollection<T>:ObservableCollection<T> where T : IAggregateRoot
    {
        public CommonRootCollection(IRepository<T> repository)
        {
            repository.LoadAll().ForEach(Add); 
        }
    }
}
