using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Teleopti.Interfaces.MessageBroker.Core;

namespace Teleopti.Core
{
    public abstract class ObjectReader<T> : IObjectReader<T>
    {
        private readonly string _connectionString;

        protected ObjectReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        #pragma warning disable 1692

        protected CommandType CommandType
        {
            get { return CommandType.StoredProcedure; }
        }

        #pragma warning restore 1692

        protected abstract string GetAllItems { get; }
        protected abstract string GetItemsById { get; }

        protected string ConnectionString
        {
            get { return _connectionString; }
        }

        protected abstract ICollection<IDataParameter> GetParameters(IDbCommand command, int id);
        protected abstract ICollection<IDataParameter> GetParameters(IDbCommand command, Guid id);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected abstract IMapperBase<T> GetMapper();

        public IList<T> Execute()
        {
            using (IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection())
            {
                IDbCommand command = connection.CreateCommand();
                command.Connection = connection;
                command.CommandType = CommandType;
                command.CommandText = GetAllItems;
                using (IDataReader reader = command.ExecuteReader())
                {
                    try
                    {
                        IMapperBase<T> mapper = GetMapper();
                        IList<T> list = mapper.MapAll(reader);
                        return list;
                    }
                    finally
                    {
                        //reader.Close();
                        command.Dispose();
                        //connection.Close();
                    }
                }
            }
        }

        public T Execute(int id)
        {
            IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection();
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = GetItemsById;
            foreach (IDataParameter param in GetParameters(command, id))
                command.Parameters.Add(param);
            using (IDataReader reader = command.ExecuteReader())
            {
                try
                {
                    IMapperBase<T> mapper = GetMapper();
                    IList<T> collection = mapper.MapAll(reader);
                    if (collection != null && collection.Count > 0)
                        return collection[0];
                    return default(T);
                }
                finally
                {
                    //reader.Close();
                    connection.Close();
                }
            }
        }


        public T Execute(Guid id)
        {
            IDbConnection connection = ConnectionFactory.GetInstance(ConnectionString).GetOpenConnection();
            IDbCommand command = connection.CreateCommand();
            command.Connection = connection;
            command.CommandType = CommandType;
            command.CommandText = GetItemsById;
            foreach (IDataParameter param in GetParameters(command, id))
                command.Parameters.Add(param);
            using (IDataReader reader = command.ExecuteReader())
            {
                try
                {
                    IMapperBase<T> mapper = GetMapper();
                    IList<T> collection = mapper.MapAll(reader);
                    if (collection != null && collection.Count > 0)
                        return collection[0];
                    return default(T);
                }
                finally
                {
                    //reader.Close();
                    //connection.Close();
                }
            }
        }

    }
}