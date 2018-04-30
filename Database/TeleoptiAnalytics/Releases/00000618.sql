/*RTA is purged via Hangfire jobs these days and this was not removed when that happened*/
delete from mart.etl_maintenance_configuration
where configuration_name = N'daysToKeepRTAEvents'
