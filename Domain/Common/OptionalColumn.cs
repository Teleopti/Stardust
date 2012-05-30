using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;


namespace Teleopti.Ccc.Domain.Common
{
    public class OptionalColumn : AggregateRootWithBusinessUnit, IOptionalColumn, IDeleteTag
    {
        protected OptionalColumn()
        {}

        public OptionalColumn(string name)
            : this()
        {
            _name = name;
        }

        private string _name;
        private string _tableName;
        //private readonly IList<IOptionalColumnValue> _valueCollection = new List<IOptionalColumnValue>();
        private bool _isDeleted;

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual string TableName
        {
            get { return _tableName; }
            set { _tableName = value; }
        }


		//public virtual ReadOnlyCollection<IOptionalColumnValue> ValueCollection
		//{
		//    get
		//    {
		//        return new ReadOnlyCollection<IOptionalColumnValue>(_valueCollection);
		//    }
		//}

        public virtual bool IsDeleted
        {
            get { return _isDeleted; }
        }

		///// <summary>
		///// Adds the value.
		///// </summary>
		///// <param name="value">The value.</param>
		///// <remarks>
		///// Created by: Viraj Siriwardana
		///// Created date: 2008-07-24
		///// </remarks>
		//public virtual void AddOptionalColumnValue(IOptionalColumnValue value)
		//{
		//    InParameter.NotNull("value", value);

		//    if (!_valueCollection.Contains(value))
		//    {
		//        value.SetParent(this);
		//        _valueCollection.Add(value);
		//    }
		//}

		///// <summary>
		///// Removes the value.
		///// </summary>
		///// <param name="value">The value.</param>
		///// <remarks>
		///// Created by: Viraj Siriwardana
		///// Created date: 2008-07-24
		///// </remarks>
		//public virtual void RemoveOptionalColumnValue(IOptionalColumnValue value)
		//{
		//    InParameter.NotNull("value", value);

		//    _valueCollection.Remove(value);
		//}

		///// <summary>
		///// Get Optional Column Value by Id
		///// </summary>
		///// <param name="id"></param>
		///// <returns></returns>
		//public virtual IOptionalColumnValue GetColumnValueById(Guid? id)
		//{
		//    IOptionalColumnValue result = _valueCollection.FirstOrDefault(v => v.ReferenceId.Equals(id));
		//    return result;
		//}

        public virtual void SetDeleted()
        {
            _isDeleted = true;
        }
    }
}