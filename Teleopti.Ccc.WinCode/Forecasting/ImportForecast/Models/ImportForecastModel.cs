using System;
using System.IO;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models
{
    public interface IImportForecastModel
    {
        string SelectedSkillName { get; }
        Guid SaveValidatedForecastFile(IForecastFile forecastFile);
        void ValidateFile(StreamReader streamReader);
        IWorkload LoadWorkload();
        byte[] FileContent { get; set; }
    }

    public class ImportForecastModel : IImportForecastModel
    {
        private readonly ISkill _skill;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IImportForecastsRepository _importForecastsRepository;
        private readonly IForecastsRowExtractor _rowExtractor;

        public ImportForecastModel(ISkill preselectedSkill, IUnitOfWorkFactory unitOfWorkFactory, IImportForecastsRepository importForecastsRepository, IForecastsRowExtractor rowExtractor)
        {
            _skill = preselectedSkill;
            _unitOfWorkFactory = unitOfWorkFactory;
            _importForecastsRepository = importForecastsRepository;
            _rowExtractor = rowExtractor;
        }

        public string SelectedSkillName
        {
            get { return _skill.Name; }
        }

        public byte[] FileContent { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public Guid SaveValidatedForecastFile(IForecastFile forecastFile)
        {
            Guid result;
            using (var uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                _importForecastsRepository.Add(forecastFile);
                uow.PersistAll();
                result = forecastFile.Id.GetValueOrDefault();
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public void ValidateFile(StreamReader streamReader)
        {
            var rowNumber = 1;
            try
            {
                if (streamReader.Peek() == -1) throw new ValidationException("File is empty.");

                while (!streamReader.EndOfStream)
                {
                    if (rowNumber > 100) break;

                    var line = streamReader.ReadLine();
                    _rowExtractor.Extract(line, _skill.TimeZone);
                    rowNumber++;
                }
            }
            catch (ValidationException exception)
            {
                throw new ValidationException(string.Format("Line {0}, Error:{1}", rowNumber, exception.Message), exception);
            }
            var fileContent = new byte[streamReader.BaseStream.Length];
            streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
            streamReader.BaseStream.Read(fileContent, 0, (int) streamReader.BaseStream.Length);
            FileContent = fileContent;
        }

        public IWorkload LoadWorkload()
        {
            return _skill.WorkloadCollection.FirstOrDefault();
        }
    }
}
