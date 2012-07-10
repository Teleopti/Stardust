using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Forecasting.ExportPages
{
    public class ExportSkillModel : IDisposable
    {
        public ExportSkillModel()
        {
            ExportMultisiteSkillToSkillCommandModel = new ExportMultisiteSkillToSkillCommandModel();
            ExportSkillToFileCommandModel = new ExportSkillToFileCommandModel();
            ExportToFile = true;
        }

        public ExportMultisiteSkillToSkillCommandModel ExportMultisiteSkillToSkillCommandModel { get; set; }
        public ExportSkillToFileCommandModel ExportSkillToFileCommandModel { get; set; }

        public void ChangeExportType(bool exportToFile)
        {
            if (exportToFile)
            {
                ExportMultisiteSkillToSkillCommandModel.ToExportSkillToFileModel(ExportSkillToFileCommandModel);
            }
            else
            {
                ExportSkillToFileCommandModel.ToExportMultisiteModel(ExportMultisiteSkillToSkillCommandModel);
            }
            ExportToFile = exportToFile;
        }

        public bool ExportToFile { get; private set; }

        public void Dispose()
        {
            ExportMultisiteSkillToSkillCommandModel = null;
            ExportSkillToFileCommandModel = null;
        }
    }

    public class ExportSkillToFileCommandModel
    {
        public enum TypeOfExport
        {
            Agents,
            Calls,
            AgentsAndCalls
        }

        public TypeOfExport ExportType { get; set; } 
        public string FileName { get; set; }
        public ISkill Skill { get; set; }
        public DateOnlyPeriod Period { get; set; }
        public IScenario Scenario { get; set; }

        public void ToExportMultisiteModel(ExportMultisiteSkillToSkillCommandModel model)
        {
            model.Period = new DateOnlyPeriodDto { StartDate = new DateOnlyDto(Period.StartDate), EndDate = new DateOnlyDto(Period.EndDate) };
        }
    }

    public class ExportMultisiteSkillToSkillCommandModel
    {
        public ICollection<MultisiteSkillSelectionModel> MultisiteSkillSelectionModels { get; private set; }
        public DateOnlyPeriodDto Period { get; set; }

        public ExportMultisiteSkillToSkillCommandModel()
        {
            MultisiteSkillSelectionModels = new List<MultisiteSkillSelectionModel>();
            Period = new DateOnlyPeriodDto(new DateOnlyPeriod(DateOnly.Today,DateOnly.Today.AddDays(30)));
        }

        public bool HasChildSkillMappings
        {
            get
            {
                return MultisiteSkillSelectionModels.Select(r => r.ChildSkillMappingModels.Count > 0).FirstOrDefault();
            }
        }

        public ExportMultisiteSkillToSkillCommandDto TransformToDto()
        {
            var exportMultisiteSkillToSkillCommandDto = new ExportMultisiteSkillToSkillCommandDto();
            exportMultisiteSkillToSkillCommandDto.Period = Period;

            foreach (var multisiteSkillSelectionModel in MultisiteSkillSelectionModels)
            {
                var multisiteSkillSelectionDto = new MultisiteSkillSelectionDto();
                multisiteSkillSelectionDto.MultisiteSkill = multisiteSkillSelectionModel.MultisiteSkillModel.SkillDto;

                foreach (var childSkillMappingModel in multisiteSkillSelectionModel.ChildSkillMappingModels)
                {
                    var childSkillMappingDto = new ChildSkillMappingDto();
                    childSkillMappingDto.SourceSkill = childSkillMappingModel.SourceSkill;
                    childSkillMappingDto.TargetSkill = childSkillMappingModel.TargetSkill;
                    multisiteSkillSelectionDto.ChildSkillMapping.Add(childSkillMappingDto);
                }
                exportMultisiteSkillToSkillCommandDto.MultisiteSkillSelection.Add(multisiteSkillSelectionDto);
            }
            
            return exportMultisiteSkillToSkillCommandDto;
        }

        public void ToExportSkillToFileModel(ExportSkillToFileCommandModel model)
        {
            model.Period = new DateOnlyPeriod(new DateOnly(Period.StartDate.DateTime),
                                                  new DateOnly(Period.EndDate.DateTime));
        }
    }
}