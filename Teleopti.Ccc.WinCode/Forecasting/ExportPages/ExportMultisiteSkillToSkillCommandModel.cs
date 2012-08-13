using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Export;
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
            ExportToFile = exportToFile;
        }

        public bool ExportToFile { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ExportMultisiteSkillToSkillCommandModel = null;
                ExportSkillToFileCommandModel = null;
            }
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
    }
}