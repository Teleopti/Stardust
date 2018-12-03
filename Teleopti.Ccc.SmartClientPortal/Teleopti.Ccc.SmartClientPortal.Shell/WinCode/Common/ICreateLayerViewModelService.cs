using System;
using System.Collections.Generic;
using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    /// <summary>
    /// Creates LayerViewModels from domain objects
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2010-06-14
    /// </remarks>
    public interface ICreateLayerViewModelService
    {
        /// <summary>
        /// >Creates LayerViewModels from projection.
        /// </summary>
        /// <param name="scheduleRange">The schedule range.</param>
        /// <param name="period">The period.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="interval">The interval.</param>
        /// <returns>A list of created layerviwemodels</returns>
        IList<ILayerViewModel> CreateProjectionViewModelsFromSchedule(IScheduleRange scheduleRange, DateTimePeriod period, IEventAggregator eventAggregator, TimeSpan interval, IAuthorization authorization);

        /// <summary>
        /// Creates the viewmodels from the projection.
        /// </summary>
        /// <param name="projectionSource">The projection source.</param>
        /// <returns>
        /// Only the projected layers
        /// </returns>
        IList<ILayerViewModel> CreateProjectionViewModelsFromProjectionSource(IScheduleDay projectionSource, TimeSpan interval);

        /// <summary>
        /// Creates  all the viewmodels from schedule.
        /// </summary>
        /// <param name="scheduleDay">The schedule day.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="observer">The observer.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-14
        /// </remarks>
        IList<ILayerViewModel> CreateViewModelsFromSchedule(IScheduleDay scheduleDay, IEventAggregator eventAggregator, TimeSpan interval, ILayerViewModelObserver observer, IAuthorization authorization);

        /// <summary>
        /// Creates a new service that tries to keep the models selected
        /// </summary>
        /// <param name="selector">The selector.</param>
        /// <param name="scheduleDay">The schedule day.</param>
        /// <param name="eventAggregator">The event aggregator.</param>
        /// <param name="interval">The interval.</param>
        /// <param name="observer">The observer.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2010-06-14
        /// </remarks>
        //ICreateLayerViewModelService TryToSelect(ISpecification<ILayerViewModel> specification);
        IList<ILayerViewModel> CreateViewModelsFromSchedule(ILayerViewModelSelector selector, IScheduleDay scheduleDay, IEventAggregator eventAggregator, TimeSpan interval, ILayerViewModelObserver observer, IAuthorization authorization);
    }
}