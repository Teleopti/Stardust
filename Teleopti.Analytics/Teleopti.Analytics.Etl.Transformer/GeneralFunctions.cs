using System.Collections.Generic;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.TransformerInfrastructure;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using DataSourceEtl=Teleopti.Ccc.Domain.Analytics.DataSourceEtl;

namespace Teleopti.Analytics.Etl.Transformer
{
    public class GeneralFunctions : IGeneralFunctions
    {
        private readonly string _dataMartConnectionString;
        private readonly IGeneralInfrastructure _generalInfrastructure;

        private GeneralFunctions()
        {
        }

        public GeneralFunctions(string dataMartConnectionString)
            : this()
        {
            _dataMartConnectionString = dataMartConnectionString;
            _generalInfrastructure = new GeneralInfrastructure(_dataMartConnectionString);
        }

        // Used for test only
		public GeneralFunctions(IGeneralInfrastructure generalInfrastructure, string dataMartConnectionString)
            : this()
		{
			_generalInfrastructure = generalInfrastructure;
			_dataMartConnectionString = dataMartConnectionString;
		}

    	public IList<IDataSourceEtl> DataSourceValidList
        {
            get
            {
                return getDatasourceList(true, false);
            }
        }

        public IList<IDataSourceEtl> DataSourceInvalidList
        {
            get
            {
                return getDatasourceList(false, false);
            }
        }

        public IList<IDataSourceEtl> DataSourceValidListIncludedOptionAll
        {
            get
            {
                return getDatasourceList(true, true);
            }
        }

        private IList<IDataSourceEtl> getDatasourceList(bool getValidDatasources, bool includeOptionAll)
        {
            IList<IDataSourceEtl> origList = _generalInfrastructure.GetDataSourceList(getValidDatasources, includeOptionAll);
            var retList = new List<IDataSourceEtl>();
            foreach (IDataSourceEtl source in origList)
            {
                string name = source.DataSourceName;
                if (includeOptionAll && name.Equals("ResourceKeyAliasForAll"))
                    name = Resources.AllSelection;

                IDataSourceEtl newSource = new DataSourceEtl(source.DataSourceId, name, source.TimeZoneId,
                                                       source.TimeZoneCode, source.IntervalLength, source.Inactive);
                retList.Add(newSource);
            }
            return retList;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public EtlToolStateType GetInitialLoadState()
        {
            int state = _generalInfrastructure.GetInitialLoadState();
            return (EtlToolStateType) state;
        }

        public void LoadNewDataSources()
        {
            _generalInfrastructure.LoadNewDataSourcesFromAggregationDatabase();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IList<ITimeZoneDim> GetTimeZoneList()
        {
            return _generalInfrastructure.GetTimeZonesFromMart();
        }

        public void SaveDataSource(int dataSourceId, int timeZoneId)
        {
            _generalInfrastructure.SaveDataSource(dataSourceId, timeZoneId);
        }
        public void SetUtcTimeZoneOnRaptorDataSource()
        {
            _generalInfrastructure.SetUtcTimeZoneOnRaptorDataSource();
        }

    	public IBaseConfiguration LoadBaseConfiguration()
    	{
    		return _generalInfrastructure.LoadBaseConfiguration();
    	}

    	public void SaveBaseConfiguration(IBaseConfiguration configuration)
    	{
    		_generalInfrastructure.SaveBaseConfiguration(configuration);
    	}

    	public int? LoadIntervalLengthInUse()
    	{
    		int dimIntervalRowCount = _generalInfrastructure.LoadRowsInDimIntervalTable();

    		if (dimIntervalRowCount == 0)
    			return null;

    		return 1440/dimIntervalRowCount;
    	}
    }
}