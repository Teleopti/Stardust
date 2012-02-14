IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[ReadModel].[CollationCulture]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [ReadModel].[CollationCulture]
GO


CREATE FUNCTION [ReadModel].[CollationCulture]
()
RETURNS TABLE
AS
RETURN
SELECT 1053 as [Culture],'Finnish_Swedish_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Albanian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Arabic_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Azeri_Cyrillic_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Azeri_Latin_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Chinese_Hong_Kong_Stroke_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Chinese_PRC_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Chinese_PRC_Stroke_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Chinese_Taiwan_Bopomofo_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Chinese_Taiwan_Stroke_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Croatian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Cyrillic_General_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Czech_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Danish_Norwegian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Divehi_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Estonian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'French_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Georgian_Modern_Sort_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'German_PhoneBook_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Greek_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Hebrew_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Hungarian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Hungarian_Technical_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Icelandic_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Indic_General_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Japanese_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Japanese_Unicode_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Kazakh_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Korean_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Latin1_General_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Latvian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Lithuanian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Macedonian_FYROM_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Modern_Spanish_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Polish_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Romanian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Slovak_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Slovenian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Syriac_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Tatar_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Thai_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Traditional_Spanish_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Turkish_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Ukrainian_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Uzbek_Latin_90_CI_AS'  as [Collation]  UNION ALL
SELECT NULL as [Culture],'Vietnamese_CI_AS'  as [Collation] 

GO