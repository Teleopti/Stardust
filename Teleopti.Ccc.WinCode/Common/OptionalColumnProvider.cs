﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    public class OptionalColumnProvider : IDisposable
    {
        private IRepositoryFactory _repositoryFactory;
        private IList<IOptionalColumn> _optionalColumnCollection;

        public OptionalColumnProvider(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Gets the optional column name collection.
        /// </summary>
        /// <value>The optional column name collection.</value>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-31
        /// </remarks>
        public IList<IOptionalColumn> OptionalColumnCollection
        {
            get
            {
                return _optionalColumnCollection;
            }
        }

        /// <summary>
        /// Loads all optional columns.
        /// </summary>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-31
        /// </remarks>
        public void LoadAllOptionalColumns(IUnitOfWork uow)
        {
            IOptionalColumnRepository optionalColumnRepository = _repositoryFactory.CreateOptionalColumnRepository(uow);
            _optionalColumnCollection = optionalColumnRepository.GetOptionalColumnValues<Person>();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_optionalColumnCollection != null)
                {
                    _optionalColumnCollection.Clear();
                    _optionalColumnCollection = null;
                }
                _repositoryFactory = null;
            }
        }
    }
}