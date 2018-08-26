USE [AdoNetTest];
GO

DELETE FROM Rating;
DELETE FROM Album;
DELETE FROM Artist;
DELETE FROM Genre;

INSERT INTO Genre (GenreId, Name) VALUES (1, 'Rock');
INSERT INTO Genre (GenreId, Name) VALUES (2, 'Jazz');

INSERT INTO Artist (ArtistId, Name) VALUES (1, 'Ozzy Osbourne');
INSERT INTO Artist (ArtistId, Name) VALUES (2, 'Pearl Jam');

SET IDENTITY_INSERT Album ON;
INSERT INTO Album (AlbumId, GenreId, ArtistId, Title, Price) VALUES (1, 1, 1, 'Bark at the Moon', 12.99);
INSERT INTO Album (AlbumId, GenreId, ArtistId, Title, Price) VALUES (2, 1, 1, 'Blizzard of Ozz', 11.99);
INSERT INTO Album (AlbumId, GenreId, ArtistId, Title) VALUES (3, 1, 2, 'Ten');
INSERT INTO Album (AlbumId, GenreId, ArtistId, Title) VALUES (4, 1, 2, 'Vs.');
SET IDENTITY_INSERT Album OFF;

INSERT INTO Rating (AlbumId, RatingDate, Stars) VALUES (1, '8/1/2018', 4.0)
INSERT INTO Rating (AlbumId, RatingDate, Stars) VALUES (2, '8/4/2018', 4.5)
INSERT INTO Rating (AlbumId, RatingDate, Stars) VALUES (3, '8/3/2018', 5.0)
INSERT INTO Rating (AlbumId, RatingDate, Stars) VALUES (3, '8/6/2018', 4.5)
GO