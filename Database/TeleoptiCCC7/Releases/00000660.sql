create schema [auth] authorization [dbo]
go


CREATE TABLE [auth].[CryptoKeyStore](
	[Id] [uniqueidentifier] NOT NULL,
	[Bucket] [nvarchar](256) NOT NULL,
	[Handle] [nvarchar](16) NOT NULL,
	[CryptoKey] [varbinary](50) NOT NULL,
	[CryptoKeyExpiration] [datetime] NOT NULL,
 CONSTRAINT [PK_CryptoKeyStore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_BucketHandle_CryptoKeyStore] ON [auth].[CryptoKeyStore]
(
	[Bucket] ASC,
	[Handle] ASC
) ON [PRIMARY]
GO


CREATE TABLE [auth].[NonceStore](
	[Id] [uniqueidentifier] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Nonce] [nvarchar](16) NOT NULL,
	[Context] [nvarchar](256) Not NULL,
 CONSTRAINT [PK_NonceStore] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)
) ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IX_Timestamp_NonceStore] ON [auth].[NonceStore]
(
	[Timestamp] ASC
) ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_TimestampNonceContext_NonceStore] ON [auth].[NonceStore]
(
	[Timestamp] ASC,
	[Nonce] ASC,
	[Context] ASC
) ON [PRIMARY]
GO
