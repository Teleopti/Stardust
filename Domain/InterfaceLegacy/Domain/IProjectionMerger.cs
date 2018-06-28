using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Responsible for merging (or extending) number
	/// of layers in a IVisualLayerCollection
	/// </summary>
	public interface IProjectionMerger : ICloneable
	{
		IVisualLayer[] MergedCollection(IVisualLayer[] unmergedCollection);
	}
}