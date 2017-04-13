using System.IO;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting.Import;
using Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Models;

namespace Teleopti.Ccc.WinCode.Forecasting.ImportForecast.Presenters
{
    public class ValidateImportForecastFileCommand : IValidateImportForecastFileCommand
    {
        private readonly IForecastsRowExtractor _rowExtractor;
        private readonly ImportForecastModel _model;

        public ValidateImportForecastFileCommand(IForecastsRowExtractor rowExtractor, ImportForecastModel model)
        {
            _rowExtractor = rowExtractor;
            _model = model;
        }

        public void Execute(string fileName)
        {
            validateFile(fileName);
        }

        private void validateFile(string uploadFileName)
        {
            resetValidationErrors();

            using (var fs = new FileStream(uploadFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var streamReader = new StreamReader(fs);
                var rowNumber = 1;
                try
                {
                    if (streamReader.Peek() == -1) throw new ValidationException("File is empty.");
                    if (streamReader.BaseStream.Length > 1024*1024*100) throw new ValidationException("File should be less than 100MB");
                    while (!streamReader.EndOfStream)
                    {
                        if (rowNumber > 100) break;

                        var line = streamReader.ReadLine();
                        _rowExtractor.Extract(line, _model.SelectedSkill.TimeZone);
                        rowNumber++;
                    }
                }
                catch (ValidationException exception)
                {
                    _model.HasValidationError = true;
                    _model.ValidationMessage = string.Format("Line {0}, Error:{1}", rowNumber, exception.Message);
                    return;
                }
                var fileContent = new byte[streamReader.BaseStream.Length];
                streamReader.BaseStream.Seek(0, SeekOrigin.Begin);
                streamReader.BaseStream.Read(fileContent, 0, (int)streamReader.BaseStream.Length);
                _model.FileContent = fileContent;
            }
        }

        private void resetValidationErrors()
        {
            _model.FileContent = null;
            _model.ValidationMessage = string.Empty;
            _model.HasValidationError = false;
        }
    }
}