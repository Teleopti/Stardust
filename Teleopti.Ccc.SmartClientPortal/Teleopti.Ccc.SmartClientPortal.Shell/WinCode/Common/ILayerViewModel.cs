using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface ILayerViewModel : INotifyPropertyChanged
    {
	    void UpdateModel();
        bool IsSelected { get; set; }
        bool IsChanged { get; set; }
        IScheduleDay SchedulePart { get; set; }
        TimeSpan Interval { get; set; }
        Domain.InterfaceLegacy.Domain.DateTimePeriod Period { get; set; }
        Color DisplayColor { get; }
        string Description { get; }
        string LayerDescription { get; }
        void UpdatePeriod();
        bool IsMovePermitted();
        bool IsPayloadChangePermitted { get; }
        void StartTimeChanged(FrameworkElement parent, double change);
        void TimeChanged(FrameworkElement panel, double change);
        void EndTimeChanged(FrameworkElement parent, double change);
        bool CanMoveAll{get; set;}
        bool IsProjectionLayer { get; }
        IPayload Payload { get; set; }
        bool CanMoveUp { get; }
        bool CanMoveDown { get; }
        bool CanDelete();
        /// <summary>
        /// Gets a value indicating whether this <see cref="ILayerViewModel"/> is opaque.
        /// This is used for showing different layers with different opacity
        /// </summary>
        /// <value><c>true</c> if opaque; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2009-01-28
        /// </remarks>
        bool Opaque { get; }
        void MoveLayer(TimeSpan span);
    
        CommandModel MoveUpCommand { get; }
        
        CommandModel MoveDownCommand { get; }
        
        CommandModel DeleteCommand { get; }

        /// <summary>
        /// Gets the index of the visual order.
        /// </summary>
        /// <remarks>
        /// This is based on the OrderIndex of the layer and the type of the layer:
        /// -MainShift
        /// -Overtime
        /// -Personal
        /// -Metting
        /// -Absence
        /// </remarks>
        int VisualOrderIndex { get; }

        void Delete();
        void MoveDown();
        void MoveUp();
        bool ShouldBeIncludedInGroupMove(ILayerViewModel sender);
    }
}
