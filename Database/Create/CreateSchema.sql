/********************************************************************************
$Revision: 
$Archive: 
$Author: 
$Date: 
$Modtime: 
$Workfile: 
*********************************************************************************
Description:	Add a database schema according to input parameter from DBManager or SQLCMD

Created:	2009-02-09	David Jonsson
Changed:   <yyyy-mm-dd> <name>			<changes done>

********************************************************************************/
--Get local variables, uncomment this part if you like to run this script from Management Studio
--Remember to comment back again! Else SET commends in DOS will not apply!

/*
:SETVAR SCHEMANAME RTA
*/
CREATE SCHEMA $(SCHEMANAME) AUTHORIZATION [dbo]
