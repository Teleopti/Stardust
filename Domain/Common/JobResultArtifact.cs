using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Common
{

	public enum JobResultArtifactCategory
	{
		Input,
		OutputWarning,
		OutputError
	}

	public class JobResultArtifact : AggregateEntity
	{
		private readonly JobResultArtifactCategory _category;
		private readonly string _name;
		private readonly byte[] _content;
		private readonly DateTime _createTime;


		public JobResultArtifact(JobResultArtifactCategory category, string name, byte[] content)
		{
			_category = category;
			_name = name;
			_content = content;
			_createTime = DateTime.UtcNow;
		}

		public JobResultArtifact()
		{

		}


		public virtual string Name
		{
			get { return _name; }
		}

		public virtual byte[] Content
		{
			get { return _content; }
		}

		public virtual JobResultArtifactCategory Category
		{
			get { return _category; }
		}

		public virtual DateTime CreateTime
		{
			get { return _createTime; }
		}

		public virtual string FileName => isFileNameValid ? Name.Substring(0, Name.LastIndexOf(".")) : string.Empty;

		public virtual string FileType => isFileNameValid ? Name.Substring(Name.LastIndexOf(".") + 1, Name.Length - (Name.LastIndexOf(".") + 1)) : string.Empty;

		private bool isFileNameValid => !Name.IsNullOrEmpty() && Name.LastIndexOf(".") != -1;


	}
}
