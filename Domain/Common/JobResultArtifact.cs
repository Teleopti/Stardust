using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

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
	}
}
