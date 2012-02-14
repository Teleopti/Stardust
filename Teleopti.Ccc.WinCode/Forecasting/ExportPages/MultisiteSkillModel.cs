using System;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class MultisiteSkillModel
    {
        public MultisiteSkillModel(Guid id)
        {
            SkillDto = new SkillDto { Id = id };
            Id = id;
        }

        public SkillDto SkillDto { get; set; }
        public Guid Id { get; set; }
    }
}