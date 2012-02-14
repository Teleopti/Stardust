using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Teleopti.Ccc.WpfControls.Demo.Models;

namespace Teleopti.Ccc.WpfControls.Demo
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Fetchable")]
    public interface IFetchableModel<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Sets the State to Fetching and starts to collect the "expensive" data
        /// </summary>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-08-26
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        bool FetchData(out T data);

        /// <summary>
        /// Gets the state of the model.
        /// </summary>
        /// <value>The state of the model.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-08-26
        /// </remarks>
        ModelState ModelState { get; }
    }
}
