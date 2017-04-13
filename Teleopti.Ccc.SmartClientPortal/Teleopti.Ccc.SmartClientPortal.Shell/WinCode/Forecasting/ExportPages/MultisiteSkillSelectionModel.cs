using System.Collections.Generic;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class MultisiteSkillSelectionModel
    {
        public MultisiteSkillSelectionModel()
        {
            ChildSkillMappingModels = new List<ChildSkillMappingModel>();
        }
        public MultisiteSkillModel MultisiteSkillModel { get; set; }
        public ICollection<ChildSkillMappingModel> ChildSkillMappingModels { get; private set; }
    }
}