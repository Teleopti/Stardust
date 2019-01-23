using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Responsible for merging (or extending) number
	/// of layers in a IVisualLayerCollection
	/// </summary>
	public interface IProjectionMerger : ICloneable
	{
		IEnumerable<IVisualLayer> MergedCollection(IEnumerable<IVisualLayer>  unmergedCollection);
	}
}