﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models
{
    public class ImportForecastModel
    {
        public ISkill SelectedSkill { get; set; }

        public byte[] FileContent { get; set; }

        public ImportForecastsMode ImportMode { get; set; }

        public Guid FileId { get; set; }

        public bool HasValidationError { get; set; }

        public string ValidationMessage { get; set; }

        public IWorkload SelectedWorkload()
        {
            return SelectedSkill.WorkloadCollection.FirstOrDefault();
        }
    }
}
