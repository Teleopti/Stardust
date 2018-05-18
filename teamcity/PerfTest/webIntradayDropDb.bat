@ECHO off

set appDb=Telia_perfTest_TeleoptiCCC7
set analDb=Telia_perfTest_TeleoptiAnalytics
set aggDb=Telia_perfTest_TeleoptiCCCAgg
set dbServer=%1
IF [%1]==[] goto wrongInput

SQLCMD -S%dbServer% -E -Q "alter database [%analDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%analDb%"") drop database [%analDb%]"
SQLCMD -S%dbServer% -E -Q "alter database [%aggDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%aggDb%"") drop database [%aggDb%]"
SQLCMD -S%dbServer% -E -Q "alter database [%appDb%] set single_user with rollback immediate"
SQLCMD -S%dbServer% -E -Q "if exists(select 1 from sys.databases where name=""%appDb%"") drop database [%appDb%]"

exit

:wrongInput
echo Missing server name!
