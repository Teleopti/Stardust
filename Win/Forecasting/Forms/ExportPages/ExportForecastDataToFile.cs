using System.IO;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Forecasting.Export;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Forecasting.Forms.ExportPages
{
    public class ExportForecastDataToFile
    {
        private readonly ExportSkillToFileCommandModel _model;
        private readonly IEnumerable<ISkillDay> _skillDays;
        private readonly ISkill _skill;
       
        public ExportForecastDataToFile(ISkill skill, ExportSkillToFileCommandModel model, IEnumerable<ISkillDay> skillDays)
        {
            _skill = skill;
            _model = model;
            _skillDays = skillDays;
        }

        public void ExportForecastData()
        {
            IExportForecastDataToFileSerializer temp = new ExportForecastDataToFileSerializer();
            var fileData = temp.SerializeForecastData(_skill,_model,_skillDays);
            WriteToFile(_model.FileName, fileData);
        }

        private static void WriteToFile(string fileName, IEnumerable<string> fileData)
        {
            using (var writter = new StreamWriter(fileName))
            {
                foreach (var row in fileData)
                {
                    writter.WriteLine(row);
                }
            }
        }
    }
}
