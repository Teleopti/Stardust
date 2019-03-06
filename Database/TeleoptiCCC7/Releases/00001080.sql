-- Index to speed up reading of PersonAccounts
CREATE NONCLUSTERED INDEX [Account_Parent]
ON [dbo].[Account] ([Parent])
INCLUDE ([Id],[AccountType],[Extra],[Accrued],[BalanceIn],[LatestCalculatedBalance],[StartDate],[BalanceOut])

