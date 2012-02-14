IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[sp_deleteAllData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[sp_deleteAllData]
