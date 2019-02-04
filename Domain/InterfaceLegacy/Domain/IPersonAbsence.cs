using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Class describing an PersonAbsence
	/// </summary>
	public interface IPersonAbsence : IPersistableScheduleData,
										IExportToAnotherScenario,
										IAggregateRoot_Events,
										ICloneableEntity<IPersonAbsence>,
										IVersioned
	{
		/// <summary>
		/// Gets the layer.
		/// </summary>
		/// <value>The layer.</value>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-15
		/// </remarks>
		IAbsenceLayer Layer { get; }

		/// <summary>
		/// Returns a list with the absence splitted on specified period
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		IList<IPersonAbsence> Split(DateTimePeriod period);

		/// <summary>
		/// Returns a new PersonAbsence, this absence merged with inparameter, null if no merge
		/// </summary>
		/// <param name="personAbsence"></param>
		/// <returns></returns>
		IPersonAbsence Merge(IPersonAbsence personAbsence);

		/// <summary>personAbsence
		/// Get/set last change
		/// </summary>
		DateTime? LastChange { get; set; }

		void ReplaceLayer(IAbsence newAbsence, DateTimePeriod newPeriod);
		void RemovePersonAbsence(TrackedCommandInfo trackedCommandInfo, DateTimePeriod? eventPeriod = null);

		void FullDayAbsence(IPerson person, TrackedCommandInfo trackedCommandInfo);

		void IntradayAbsence(IPerson person, TrackedCommandInfo trackedCommandInfo, bool muteEvent = false);
	}
}
