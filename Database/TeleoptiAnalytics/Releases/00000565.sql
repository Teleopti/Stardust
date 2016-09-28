UPDATE mart.dim_acd_login
SET log_object_name= sys.log_object_name
FROM mart.dim_acd_login d INNER JOIN mart.sys_datasource sys on sys.datasource_id=d.datasource_id
WHERE d.log_object_name is null