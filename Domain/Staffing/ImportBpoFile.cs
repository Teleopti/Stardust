using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class SkillCombinationResourceBpo
	{
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
		public double Resources { get; set; }
		public Guid SkillCombinationId { get; set; }
		public string Source { get; set; }
	}

	public class LineWithNumber
	{
		public string LineContent;
		public int LineNumber;
	}

	public class ImportBpoFileResult
	{
		public bool Success = true;
		public HashSet<string> ErrorInformation = new HashSet<string>();
	}

	public class ImportBpoFile
	{
		private readonly ISkillRepository _skillRepository;
		private const string lineSeparator = "\r\n";
	
		// valid field names
		private const string source = "source";
		private const string skillgroup = "skillgroup";
		private const string startdatetime = "startdatetime";
		private const string enddatetime = "enddatetime";
		private const string resources = "resources";

		private readonly IReadOnlyCollection<string> validFieldNames =
			new List<string> { source, skillgroup, startdatetime, enddatetime, resources };

		public ImportBpoFile(ISkillRepository skillRepository)
		{
			_skillRepository = skillRepository;
		}

		public ImportBpoFileResult ImportFile(string fileContents, IFormatProvider importFormatProvider,char tokenSeparator = ',', char skillSeparator = '|')
		{
			var result = new ImportBpoFileResult();
			
			var lines = fileContents.Split(new[] { lineSeparator }, StringSplitOptions.None);
			var lineNumber = 1;
			var linesWithNumbers = lines.Select(line => new LineWithNumber{LineContent = line, LineNumber = lineNumber++}).ToList();
			var fieldNames = linesWithNumbers[0].LineContent.Split(tokenSeparator);
			assertFieldNames(fieldNames, result);

			if (!result.Success) return result;

			var allSkills = _skillRepository.LoadAllSkills().ToList();

			var bpoList = new List<SkillCombinationResourceBpo>();
			foreach (var line in linesWithNumbers.Skip(1))
			{
				var bpoStrings = tokenizeSkillCombinationResourceBpo(line, fieldNames, tokenSeparator);
				var resourceBpo = createSkillCombinationResourceBpo(bpoStrings, importFormatProvider, skillSeparator, allSkills, result);
				bpoList.Add(resourceBpo);
			}
			
			return result;
		}

		private void assertFieldNames(string[] fieldNames, ImportBpoFileResult result)
		{
			foreach (var fieldName in fieldNames)
			{
				if (!validFieldNames.Contains(fieldName))
				{
					result.Success = false;
					result.ErrorInformation.Add($"Invalid field name in header: '{fieldName}'. Valid field names are: ({string.Join(",", validFieldNames)})");
				}
			}
		}

		private SkillCombinationResourceBpo createSkillCombinationResourceBpo(Dictionary<string, string> bpoStrings, IFormatProvider importFormatProvider, char skillSeparator, IEnumerable<ISkill> allSkills, ImportBpoFileResult result)
		{
			var resourceBpo = new SkillCombinationResourceBpo
			{
				Source = bpoStrings[source],
				StartDateTime = DateTime.Parse(bpoStrings[startdatetime], importFormatProvider),
				EndDateTime = DateTime.Parse(bpoStrings[enddatetime], importFormatProvider),
				Resources = double.Parse(bpoStrings[resources], importFormatProvider),
				SkillCombinationId = lookupSkillCombinationId(bpoStrings[skillgroup], skillSeparator, allSkills, result)
			};
			return resourceBpo;
		}

		private Guid lookupSkillCombinationId(string skillGroupString, char skillSeparator, IEnumerable<ISkill> allSkills, ImportBpoFileResult result)
		{
			var skillStringList = skillGroupString.Replace(" ", "").Split(skillSeparator);
			// TODO: lookup Ids for single Skills
			foreach (var skillString in skillStringList)
			{
				var skillCount = allSkills.Count(s => s.Name == skillString);
				if (skillCount == 0)
				{
					result.Success = false;
					result.ErrorInformation.Add($"The skill with name {skillString} is not defined in the system.");
				}
				else if (skillCount > 1)
				{
					result.Success = false;
					result.ErrorInformation.Add($"The skill with name {skillString} is defined {skillCount} times in the system. Only once is allowed when using this function.");
				}
			}
			// TODO: lookup SkillCombinationId for this combination
			return new Guid(); // replace with real guid
		}

		private Dictionary<string, string> tokenizeSkillCombinationResourceBpo(LineWithNumber lineWithNumber, string[] fieldNames, char tokenSeparator)
		{
			var parameters = lineWithNumber.LineContent.Split(tokenSeparator);

			if (parameters.Length != fieldNames.Length)
			{
				throw new Exception(FormatGeneralLineErrorMessage(lineWithNumber, $"Line has {parameters.Length} parameters but expected {fieldNames.Length} parameters."));
			}

			var result = new Dictionary<string, string>();
			for (int i = 0; i < parameters.Length; i++)
			{
				result.Add(fieldNames[i], parameters[i]);
			}

			return result;
		}

		private string FormatGeneralLineErrorMessage(LineWithNumber lineWithNumber, string specificMessage)
		{
			var lineMessage = $"File import error. Error found on row number {lineWithNumber.LineNumber} with contents:\r\n{lineWithNumber.LineContent}\r\n{specificMessage}";
			return lineMessage;
		}
	}
}
