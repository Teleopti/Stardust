using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Responsible for merging (or extending) number
	/// of layers in a IVisualLayerCollection
	/// </summary>
	public interface IProjectionMerger : ICloneable
	{
		/// <summary>
		/// Transforms an unmergedCollection to a merged collection
		/// </summary>
		/// <param name="unmergedCollection"></param>
		/// <param name="person"></param>
		/// <returns></returns>
		IVisualLayer[] MergedCollection(IVisualLayer[] unmergedCollection, IPerson person);
	}
}