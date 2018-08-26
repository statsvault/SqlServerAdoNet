USE [master];
GO

IF EXISTS(select * from sys.databases where name='AdoNetTest') 
BEGIN
	DROP DATABASE [AdoNetTest];
END;
GO

CREATE DATABASE [AdoNetTest]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'AdoNetTest', FILENAME = N'{0}\AdoNetTest.mdf' , SIZE = 4096KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'AdoNetTest_log', FILENAME = N'{0}\AdoNetTest_log.ldf' , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%);
GO

ALTER DATABASE [AdoNetTest] SET COMPATIBILITY_LEVEL = 110;
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
	EXEC [AdoNetTest].[dbo].[sp_fulltext_database] @action = 'enable';
end;

GO
ALTER DATABASE [AdoNetTest] SET ANSI_NULL_DEFAULT OFF; 
GO
ALTER DATABASE [AdoNetTest] SET ANSI_NULLS OFF; 
GO
ALTER DATABASE [AdoNetTest] SET ANSI_PADDING OFF; 
GO
ALTER DATABASE [AdoNetTest] SET ANSI_WARNINGS OFF;
GO
ALTER DATABASE [AdoNetTest] SET ARITHABORT OFF;
GO
ALTER DATABASE [AdoNetTest] SET AUTO_CLOSE OFF;
GO
ALTER DATABASE [AdoNetTest] SET AUTO_CREATE_STATISTICS ON;
GO
ALTER DATABASE [AdoNetTest] SET AUTO_SHRINK OFF;
GO
ALTER DATABASE [AdoNetTest] SET AUTO_UPDATE_STATISTICS ON;
GO
ALTER DATABASE [AdoNetTest] SET CURSOR_CLOSE_ON_COMMIT OFF; 
GO
ALTER DATABASE [AdoNetTest] SET CURSOR_DEFAULT  GLOBAL;
GO
ALTER DATABASE [AdoNetTest] SET CONCAT_NULL_YIELDS_NULL OFF;
GO
ALTER DATABASE [AdoNetTest] SET NUMERIC_ROUNDABORT OFF;
GO
ALTER DATABASE [AdoNetTest] SET QUOTED_IDENTIFIER OFF;
GO
ALTER DATABASE [AdoNetTest] SET RECURSIVE_TRIGGERS OFF; 
GO
ALTER DATABASE [AdoNetTest] SET  DISABLE_BROKER;
GO
ALTER DATABASE [AdoNetTest] SET AUTO_UPDATE_STATISTICS_ASYNC OFF;
GO
ALTER DATABASE [AdoNetTest] SET DATE_CORRELATION_OPTIMIZATION OFF; 
GO
ALTER DATABASE [AdoNetTest] SET TRUSTWORTHY OFF;
GO
ALTER DATABASE [AdoNetTest] SET ALLOW_SNAPSHOT_ISOLATION OFF;
GO
ALTER DATABASE [AdoNetTest] SET PARAMETERIZATION SIMPLE;
GO
ALTER DATABASE [AdoNetTest] SET READ_COMMITTED_SNAPSHOT OFF;
GO
ALTER DATABASE [AdoNetTest] SET HONOR_BROKER_PRIORITY OFF;
GO
ALTER DATABASE [AdoNetTest] SET RECOVERY FULL;
GO
ALTER DATABASE [AdoNetTest] SET  MULTI_USER;
GO
ALTER DATABASE [AdoNetTest] SET PAGE_VERIFY CHECKSUM;
GO
ALTER DATABASE [AdoNetTest] SET DB_CHAINING OFF;
GO
ALTER DATABASE [AdoNetTest] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF );
GO
ALTER DATABASE [AdoNetTest] SET TARGET_RECOVERY_TIME = 0 SECONDS;
GO
ALTER DATABASE [AdoNetTest] SET  READ_WRITE;
GO

USE [AdoNetTest];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Album](
	[AlbumId] [int] IDENTITY(1,1) NOT NULL,
	[GenreId] [int] NOT NULL,
	[ArtistId] [int] NOT NULL,
	[Title] [varchar](100) NOT NULL,
	[Price] [decimal](5,2) NULL
 CONSTRAINT [PK_Album] PRIMARY KEY CLUSTERED 
(
	[AlbumId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Artist](
	[ArtistId] [int] NOT NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Artist] PRIMARY KEY CLUSTERED 
(
	[ArtistId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Genre](
	[GenreId] [int] NOT NULL,
	[Name] [varchar](100) NOT NULL,
 CONSTRAINT [PK_Genre] PRIMARY KEY CLUSTERED 
(
	[GenreId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE [dbo].[Rating](
	[AlbumId] [int] NOT NULL,
	[RatingDate] [date] NOT NULL,
	[Stars] [decimal](3,2) NOT NULL,
 CONSTRAINT [PK_Rating] PRIMARY KEY CLUSTERED 
(
	[AlbumId] ASC,
	[RatingDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY];
GO

ALTER TABLE [dbo].[Album] ADD CONSTRAINT UQ_Album UNIQUE(Title);
GO

ALTER TABLE [dbo].[Album]  WITH NOCHECK ADD  CONSTRAINT [FK_Album_Artist] FOREIGN KEY([ArtistId])
REFERENCES [dbo].[Artist] ([ArtistId]);
GO

ALTER TABLE [dbo].[Album] CHECK CONSTRAINT [FK_Album_Artist];
GO

ALTER TABLE [dbo].[Album]  WITH NOCHECK ADD  CONSTRAINT [FK_Album_Genre] FOREIGN KEY([GenreId])
REFERENCES [dbo].[Genre] ([GenreId]);
GO

ALTER TABLE [dbo].[Album] CHECK CONSTRAINT [FK_Album_Genre];
GO

ALTER TABLE [dbo].[Rating]  WITH NOCHECK ADD  CONSTRAINT [FK_Rating_Album] FOREIGN KEY([AlbumId])
REFERENCES [dbo].[Album] ([AlbumId]);
GO

ALTER TABLE [dbo].[Rating] CHECK CONSTRAINT [FK_Rating_Album];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE TYPE [dbo].[IdType] AS TABLE (Value INT NOT NULL);  
GO

CREATE TYPE [dbo].[AlbumType] AS TABLE (
	AlbumId int NOT NULL,
	GenreId int NOT NULL,
	ArtistId int NOT NULL,
	Title varchar(100) NOT NULL,
	Price decimal(5,2) NULL
);
GO

CREATE PROCEDURE [dbo].[spRaiseErrorTest]
AS
BEGIN
	set nocount on;
	RAISERROR ('Ado.Net Test Error.', 16, 1);
END;
GO

CREATE PROCEDURE [dbo].[spAlbumGet]
	@AlbumId int
AS
BEGIN
	set nocount on;
	select * from Album where AlbumId = @AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumGetOutParams]
	@AlbumId int,
	@GenreId int out,
	@ArtistId int out,
	@Title varchar(100) out,
	@Price decimal(5,2) out
AS
BEGIN
	set nocount on;
	select @GenreId = GenreId,
		@ArtistId = ArtistId,
		@Title = Title,
		@Price = Price
	from Album
	where AlbumId = @AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumsGet]
	@AlbumIds IdType READONLY
AS
BEGIN
	set nocount on;
	select a.*
	from @AlbumIds ids
	join Album a on ids.Value = a.AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumsGetByAlbums]
	@Albums AlbumType READONLY
AS
BEGIN
	set nocount on;
	select a2.*
	from @Albums a1
	join Album a2 
		on a1.GenreId = a2.GenreId
		and a1.ArtistId = a2.ArtistId
		and a1.Title = a2.Title
		and isnull(a1.Price, 0) = isnull(a2.Price, 0);
END;
GO

CREATE PROCEDURE [dbo].[spAlbumsGetByArtist]
	@ArtistId int
AS
BEGIN
	set nocount on;
	select * from Album where ArtistId = @ArtistId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumTitleGet]
	@AlbumId int
AS
BEGIN
	set nocount on;
	select Title from Album where AlbumId = @AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumDelete]
	@AlbumId int
AS
BEGIN
	set nocount on;
	delete from Album where AlbumId = @AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumInsert]
	@GenreId int,
	@ArtistId int,
	@Title varchar(100),
	@Price decimal(5,2),
	@AlbumId int out
AS
BEGIN
	set nocount on;
	insert into Album (GenreId, ArtistId, Title, Price) values (@GenreId, @ArtistId, @Title, @Price);
	set @AlbumId = scope_identity();
END;
GO

CREATE PROCEDURE [dbo].[spAlbumInsert_Id]
	@GenreId int,
	@ArtistId int,
	@Title varchar(100),
	@Price decimal = null
AS
BEGIN
	set nocount on;
	insert into Album (GenreId, ArtistId, Title, Price) values (@GenreId, @ArtistId, @Title, @Price);
	select scope_identity();
END;
GO

CREATE PROCEDURE [dbo].[spAlbumsInsert]
	@Albums AlbumType READONLY
AS
BEGIN
	set nocount on;
	insert Album (GenreId, ArtistId, Title, Price)
	select GenreId, ArtistId, Title, Price from @Albums;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumUpdate]
	@AlbumId int,
	@GenreId int,
	@ArtistId int,
	@Title varchar(100),
	@Price decimal(5,2)
AS
BEGIN
	set nocount on;
	update Album 
	set GenreId = @GenreId,
		ArtistId = @ArtistId,
		Title = @Title,
		Price = @Price
	where AlbumId = @AlbumId;
END;
GO

CREATE PROCEDURE [dbo].[spAlbumsUpdate]
	@Albums AlbumType READONLY
AS
BEGIN
	set nocount on;
	update a2 
	set a2.GenreId = a1.GenreId,
		a2.ArtistId = a1.ArtistId,
		a2.Title = a1.Title,
		a2.Price = a1.Price
	from @Albums a1
	join Album a2 on a1.AlbumId = a2.AlbumId;
END;
GO

