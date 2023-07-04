-- DB Script 0.2.0
-- Author: Pedro Pereira

-- Schemas
GO
CREATE SCHEMA portal
GO
GO
CREATE SCHEMA nps
GO

-- Tables
CREATE TABLE [portal].[Client] (
                                   [id] [int] IDENTITY(1000,1) NOT NULL,
                                   [cnpj] [char](14) NOT NULL,
                                   [name] [varchar](100) NULL,
                                   [segment] [char](50) NULL,
                                   [createdAt] [Datetime2] NOT NULL,

                                   CONSTRAINT [PK_Client] PRIMARY KEY CLUSTERED([id] ASC),
                                   CONSTRAINT [UN_Cnpj] UNIQUE (cnpj)
);

CREATE TABLE [portal].[Insurer] (
                                    [id] [int] IDENTITY(1000,1) NOT NULL,
                                    [name] [varchar](100) NOT NULL,
                                    [hasIntegration] [bit] NOT NULL,
                                    [active] [bit] NOT NULL,
                                    [picture] [varchar](100) NULL,
                                    [realName] [varchar](100) NOT NULL,
                                    [cnpj] [char](14) NOT NULL,

                                    CONSTRAINT [PK_Insurer] PRIMARY KEY CLUSTERED([id] ASC),
);

CREATE TABLE [portal].[User] (
                                 [id] [int] IDENTITY(1000,1) NOT NULL,
                                 [name] [varchar](100) NULL,
                                 [username] [varchar](100) NOT NULL,
                                 [passwordHash] [varchar](100) NULL,
                                 [authorities] [varchar](100) NULL,
                                 [department] [varchar](100) NULL,
                                 [lastLogin] [Datetime2] NULL,

                                 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED([id] ASC),
                                 CONSTRAINT [UN_Username] UNIQUE (username)
);

CREATE TABLE [portal].[Enrollment] (
                                       [clientPK] [int] NOT NULL,
                                       [insurerPK] [int] NOT NULL,
                                       [expireAt] [Datetime2] NULL,
                                       [createdAt] [Datetime2] NOT NULL,
                                       [rating] [varchar](50) NULL,
                                       [status] [char](50) NOT NULL,
                                       [warn] [varchar](max) NULL,
                                       [isActive] [bit] NOT NULL,
                                       
                                       CONSTRAINT [FK_Client] FOREIGN KEY (clientPK) REFERENCES [portal].[Client](id) ON DELETE CASCADE,
                                       CONSTRAINT [FK_Insurer] FOREIGN KEY (insurerPK) REFERENCES [portal].[Insurer](id),
                                       CONSTRAINT [PK_ClientInsurer] PRIMARY KEY ([clientPK], [insurerPK]),
);

CREATE TABLE [portal].[Taker] (
                                  [id] [int] IDENTITY(1,1) NOT NULL,
                                  [category] [varchar](50) NOT NULL,
                                  [balance] [float] NULL,
                                  [limit] [float] NULL,
                                  [rate] [float](24) NULL,
                                  [clientFK] [int] NOT NULL,
                                  [insurerFK] [int] NOT NULL,

                                  CONSTRAINT [FK_ClientInsurer] FOREIGN KEY (clientFK, insurerFK) REFERENCES portal.Enrollment(clientPK, insurerPK) ON DELETE CASCADE,
                                  CONSTRAINT [PK_Taker] PRIMARY KEY ([id])
);

CREATE TABLE [portal].[Issue] (
                                  [id] [int] IDENTITY(1000,1) NOT NULL,
                                  [bounty] [float] NOT NULL,
                                  [commission] [float] NOT NULL,
                                  [amountInsured] [float] NULL,
                                  [validity] [int] NULL,
                                  [validUntil] [Datetime2] NULL,
                                  [dealId] [varchar](50) NULL,
                                  [policyId] [varchar](100) NOT NULL,
                                  [insured] [char](14) NULL,
                                  [issuedAt] [Datetime2] NOT NULL,
                                  [product] [char](50) NULL,
                                  [isPaid] [bit] NOT NULL,
                                  [reason] [char](50) NULL,
                                  [clientFK] [int] NULL,
                                  [insurerFK] [int] NULL,
                                  [lastRate] [float](24) NULL,

                                  CONSTRAINT [FK_ClientIssue] FOREIGN KEY (clientFK) REFERENCES portal.Client(id) ON DELETE SET NULL,
                                  CONSTRAINT [FK_InsurerIssue] FOREIGN KEY (insurerFK) REFERENCES portal.Insurer(id) ON DELETE SET NULL,

                                  CONSTRAINT [PK_Issue] PRIMARY KEY CLUSTERED([id] ASC),
                                  CONSTRAINT [UN_Deal] UNIQUE (dealId)
);

CREATE TABLE [portal].[Issue_User] (
                                       [issuesId] [int] NOT NULL,
                                       [usersId] [int] NOT NULL,

                                       CONSTRAINT [PK_IssueUser] PRIMARY KEY ([issuesId], [usersId]),
                                       CONSTRAINT [FK_Issue] FOREIGN KEY (issuesId) REFERENCES [portal].[Issue](id) ON DELETE CASCADE,
                                       CONSTRAINT [FK_User] FOREIGN KEY (usersId) REFERENCES [portal].[User](id) ON DELETE CASCADE
);

CREATE TABLE [portal].[Proposal_Parameters](
                    [proposalType] [char](50) NOT NULL,
                    [insurerFK] [int] NOT NULL,
                    [ccg] [float],
                    [minimumBrokerage] [int],
                    [internalRetroactivity] [int],
                    [externalRetroactivity] [int],
                    [exclusive] [bit] NOT NULL,
                    [pstp] [bit] NULL,
                    [baseCommission] [float](24) NOT NULL,
                    [maximumCommission] [float](24) NOT NULL,
                    [minimumBounty] [float](24),
                    [grievanceRule] [varchar](50),

                    CONSTRAINT [FK_InsurerParams] FOREIGN KEY (insurerFK) REFERENCES portal.Insurer(id) ON DELETE CASCADE,
                    CONSTRAINT [PK_ProposalParameters] PRIMARY KEY ([insurerFK],[proposalType])
);

-- NPS

CREATE TABLE [nps].[Score](
                              [id] [int] IDENTITY(1,1),
                              [value] [int],
                              [comment] [varchar](max) NULL,

                              CONSTRAINT [PK_Score] PRIMARY KEY([id]),
);


CREATE TABLE [nps].[ScoreRequest](
                                     [id] [int] IDENTITY(1,1),
                                     [key] [varchar](50),
                                     [email] [varchar](100),
                                     [reference] [varchar](50),
                                     [name] [varchar](100),
                                     [createdAt] [Datetime2],
                                     [expired] [bit],
                                     [scoreFK] [int] NULL,

                                     CONSTRAINT [PK_ScoreRequest] PRIMARY KEY CLUSTERED([id] ASC),
                                     CONSTRAINT [FK_Score] FOREIGN KEY (scoreFK) REFERENCES [nps].[Score](id) ON DELETE SET NULL
);

-- AAA

INSERT INTO portal.Proposal_Parameters(proposalType, insurerFK, ccg, minimumBrokerage, internalRetroactivity, externalRetroactivity, exclusive, pstp, baseCommission, maximumCommission, minimumBounty) VALUES
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1004, 500000.0, 24,0,180,1,1,0.27,0.5,150.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1000, 0.0, 48, 1, 60, 1, 1, 0.25, 0.30,200.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1001, 500000.0, 24,0, 90, 0, 1,0.2,0.4, 170.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1009, 300000.0, 24,0, 60, 0, 0,0.2,0.25, 170.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1006, 500000.0, 48,1, 90, 0, 1,0.28,0.5, 190.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1010, 400000.0, 48,0, 60, 0, 1,0.25,0.4, 150.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1008, 300000.0, 24,0, 90, 1, 1,0.20,0.3, 190.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1011, 750000.0, 24,1, 90, 0, 1,0.25,0.35, 150.0),
                                                                                                                                                                                                            ('PUBLIC_CONTRACT', 1005, 300000.0, 24,1,180,1,1,0.25,0.4,190.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1005, 300000.0, 24, 1, 180, 1, 1, 0.25, 0.4, 190.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1004, 500000.0, 24, 0, 60, 1, 1, 0.27, 0.5, 150.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1000, 0.0, 48, 1, 60, 1, 1, 0.25, 0.30, 200.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1001, 500000.0, 24,0, 60, 0, 1,0.2,0.4, 170.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1009, 300000.0, 24,0, 60, 0, 0,0.2,0.3, 170.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1006, 500000.0, 48,1, 90, 0, 1,0.28,0.5, 190.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1010, 400000.0, 48,0, 60, 0, 1,0.25,0.4, 150.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1008, 300000.0, 24,0, 90, 1, 1,0.20,0.3, 190.0),
                                                                                                                                                                                                            ('PRIVATE_CONTRACT', 1011, 750000.0, 24,1, 90, 0, 1,0.25,0.35, 150.0);