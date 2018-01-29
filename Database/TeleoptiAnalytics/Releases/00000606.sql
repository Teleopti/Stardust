-- Fix different windows logon in [mart].[dim_person]
-- Refer to bug #47602: ETL Nightly fails on dim_person_windows_login

UPDATE mart.dim_person
   SET windows_domain = 'Not Defined'
 WHERE windows_domain IS NULL
    OR windows_domain = ''

UPDATE mart.dim_person
   SET windows_username = 'Not Defined'
 WHERE windows_username IS NULL
    OR windows_username = ''
