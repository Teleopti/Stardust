using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models
{
    public interface IImportForecastModel
    {
        string GetSelectedSkillName();
        Guid SaveValidatedForecastFileInDb(string fileName);
        void ValidateFile(StreamReader streamReader);
        IWorkload LoadWorkload();
        byte[] FileContent { get; set; }
    }

    public class ImportForecastModel : IImportForecastModel
    {
        private readonly ISkill _skill;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IImportForecastsRepository _importForecastsRepository;

        public ImportForecastModel(ISkill preselectedSkill, IUnitOfWorkFactory unitOfWorkFactory, IImportForecastsRepository importForecastsRepository)
        {
            _skill = preselectedSkill;
            _unitOfWorkFactory = unitOfWorkFactory;
            _importForecastsRepository = importForecastsRepository;
        }

        public string GetSelectedSkillName()
        {
            return _skill.Name;
        }

        public byte[] FileContent { get; set; }

        public Guid SaveValidatedForecastFileInDb(string fileName)
        {
            if(string.IsNullOrEmpty(fileName))
                throw new InvalidOperationException("Incorrect file name.");
            var result = Guid.Empty;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var forecastFile = new ForecastFile(fileName, FileContent);
               _importForecastsRepository.Add(forecastFile);
               var savedItem = uow.PersistAll();
               var item = savedItem.FirstOrDefault();
               if (item != null) result = extractId(item.Root);
            }
            return result;
        }

        private static Guid extractId(object root)
        {
            var entity = root as IEntity;
            if (entity != null) return entity.Id.GetValueOrDefault();

            var custom = root as ICustomChangedEntity;
            if (custom != null) return custom.Id.GetValueOrDefault();

            return Guid.Empty;
        }

        public void ValidateFile(StreamReader streamReader)
        {
            var fileContent = new StringBuilder();
            var rowNumber = 1;
            try
            {
                if (streamReader.Peek() == -1) throw new ValidationException("File is empty.");
                var reader = new CsvFileReader(streamReader);
                var row = new CsvFileRow();
                var validators = setUpForecastsFileValidators();

                using (PerformanceOutput.ForOperation("Validate forecasts import file."))
                {
                    while (reader.ReadNextRow(row))
                    {
                        validateRowByRow(validators, row);
                        fileContent.Append(row + Environment.NewLine);
                        row.Clear();
                        rowNumber++;
                    }
                }
            }
            catch (ValidationException exception)
            {
                throw new ValidationException(
                    string.Format("LineNumber{0}, Error:{1}", rowNumber, exception.Message), exception);
            }
            FileContent = Encoding.UTF8.GetBytes(fileContent.ToString().TrimEnd(Environment.NewLine.ToCharArray()));
        }

        public IWorkload LoadWorkload()
        {
            return _skill.WorkloadCollection.FirstOrDefault();
        }

        private static void validateRowByRow(IList<IForecastsFileValidator> validators, IFileRow row)
        {
            if (!ForecastsFileRowCreator.IsFileColumnValid(row))
            {
                throw new ValidationException("There are more or less columns than expected.");
            }
            for (var i = 0; i < row.Count; i++)
            {
                if (!validators[i].Validate(row.Content[i]))
                    throw new ValidationException(validators[i].ErrorMessage);
            }
        }

        private static List<IForecastsFileValidator> setUpForecastsFileValidators()
        {
            var validators = new List<IForecastsFileValidator>
                                 {
                                     new ForecastsFileSkillNameValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileDateTimeValidator(),
                                     new ForecastsFileIntegerValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator(),
                                     new ForecastsFileDoubleValueValidator()
                                 };
            return validators;
        }
    }
}
