IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[RTA].[rta_load_datasources]') AND type in (N'P', N'PC'))
DROP PROCEDURE [RTA].[rta_load_datasources]
GO

