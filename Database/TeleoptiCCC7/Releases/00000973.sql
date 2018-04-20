-----------------------------------------------------------  
-- Name: Ziggy
-- Date: 2018-04-20
-- Desc: Removing leading and trailing spaces in Source
-----------------------------------------------------------

UPDATE [dbo].[BusinessProcessOutsourcer] set Source = RTRIM(LTRIM(Source))