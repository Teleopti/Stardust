﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands
{
    /// <summary>
    /// This command adds a new main shift to a schedule. The mainshift is created according to a specified shift category of <see cref="ShiftCategoryId"/> and a collection of activity layers <see cref="LayerCollection"/>. You can specify the schedule by a person's <see cref="PersonId"/> and the <see cref="Date"/> of schedule.
    /// </summary>
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2011/10/")]
    public class NewMainShiftCommandDto : CommandDto
    {
        private ICollection<ActivityLayerDto> _layerCollection = new List<ActivityLayerDto>();

        /// <summary>
        /// Gets or sets the person Id.
        /// </summary>
        /// <value>The person Id.</value>
        [DataMember]
        public Guid PersonId { get; set; }

        /// <summary>
        /// Gets or sets the target date.
        /// </summary>
        /// <value>The target date.</value>
        [DataMember]
        public DateOnlyDto Date { get; set; }

        /// <summary>
        /// Gets or sets the id of the shift category.
        /// </summary>
        [DataMember]
        public Guid ShiftCategoryId { get; set; }

        /// <summary>
        /// Gets the collection of layers with details about this shift.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode"), DataMember]
        public ICollection<ActivityLayerDto> LayerCollection
        {
            get { return _layerCollection; }
            private set
            {
                if (value != null)
                {
                    _layerCollection = new List<ActivityLayerDto>(value);
                }
            }
        }

		/// <summary>
		/// Gets or sets the scenario id. If omitted default scenario will be used.
		/// </summary>
		[DataMember(Order = 1, IsRequired = false)]
		public Guid? ScenarioId { get; set; }
    }
}
