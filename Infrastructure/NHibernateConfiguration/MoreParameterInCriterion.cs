using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Engine;
using NHibernate.SqlCommand;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class MoreParameterInCriterion : AbstractCriterion
	{
		private readonly IProjection _projection;
		private readonly Guid[] _values;

		public MoreParameterInCriterion(IProjection projection, Guid[] values)
		{
			_projection = projection;
			_values = values;
		}

		public override SqlString ToSqlString(ICriteria criteria, ICriteriaQuery criteriaQuery, IDictionary<string, IFilter> enabledFilters)
		{
			if (_values.Length == 0)
				return new SqlString("1=0");

			var result = new SqlStringBuilder();
			var columnNames = CriterionUtil.GetColumnNames(null, _projection, criteriaQuery, criteria, enabledFilters);

			for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
			{
				var columnName = columnNames[columnIndex];

				if (columnIndex > 0)
					result.Add(" and ");

				var inValues = getInValues();

				result.Add(columnName).Add(" in (").Add(inValues).Add(")");
			}

			return result.ToSqlString();
		}

		public override string ToString()
		{
			return $"{_projection} in ({getInValues()})";
		}

		public override TypedValue[] GetTypedValues(ICriteria criteria, ICriteriaQuery criteriaQuery)
		{
			throw new NotImplementedException();
		}

		public override IProjection[] GetProjections()
		{
			return new[] { _projection };
		}

		private string getInValues()
		{
			return "'" + string.Join("','", _values) + "'";
		}
	}
}
