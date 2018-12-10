using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Layers to be shown in a projected schedule,
	/// mixing activities and absences
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-02-22
	/// </remarks>
  public interface IVisualLayer : ILayer<IPayload>, ICloneableEntity<ILayer<IPayload>>
	{
		/// <summary>
		/// Gets a value indicating whether this <see cref="IVisualLayer"/> is overtime.
		/// Note: I would really like to remove this prop from here
		/// </summary>
		/// <value><c>true</c> if overtime; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-12
		/// </remarks>
		IMultiplicatorDefinitionSet DefinitionSet { get; }

		/// <summary>
		/// returns the Work time.
		/// </summary>
		/// <returns></returns>
		TimeSpan WorkTime();

		IVisualLayer CloneWithNewPeriod(DateTimePeriod newPeriod);
	}
}
