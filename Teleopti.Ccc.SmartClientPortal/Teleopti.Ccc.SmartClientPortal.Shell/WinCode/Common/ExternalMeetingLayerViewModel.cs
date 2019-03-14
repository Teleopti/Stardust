using Microsoft.Practices.Composite.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class ExternalMeetingLayerViewModel : LayerViewModel
	{
		public ExternalMeetingLayerViewModel(ILayerViewModelObserver observer, ILayer<IPayload> layer, IEventAggregator eventAggregator, IPerson person) : base(observer, layer, eventAggregator, false, person)
		{
		}

		public override string LayerDescription { get; } = UserTexts.Resources.Meeting;
		public override bool IsMovePermitted()
		{
			return false;
		}

		public override bool CanMoveUp { get; } = false;
		public override bool CanMoveDown { get; } = false;
		public override int VisualOrderIndex { get; } = 302;
		
		public override bool ShouldBeIncludedInGroupMove(ILayerViewModel sender)
		{
			return false;
		}
	}
}