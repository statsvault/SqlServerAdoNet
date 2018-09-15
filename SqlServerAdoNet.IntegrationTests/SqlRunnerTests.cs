using System;
using System.Data;
using System.IO;
using NUnit.Framework;
using FluentAssertions;
using System.Data.SqlClient;
using System.Collections.Generic;
using KellermanSoftware.CompareNetObjects;
using System.Linq;

namespace StatKings.SqlServerAdoNet.IntegrationTests
{
    /// <summary>
    /// Integration tests for StatKings.SqlServerAdoNet.SqlRunner.
    /// </summary>
    [TestFixture]
    public class SqlRunnerTests
    {
        #region Setup and TearDown

        /// <summary>
        /// Create the test database.  This will be run once.
        /// </summary>
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Helper.CreateDatabase();
        }

        /// <summary>
        /// Drop the test database.  This will be run once.
        /// </summary>
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Helper.DropDatabase();
        }

        /// <summary>
        /// Refresh the data.  This will be run before each test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            var scriptPath = Path.Combine(Helper.GetDatabaseDirectory(), "SqlServerSetup.sql");
            Helper.RunDatabaseScript(scriptPath);
        }

        #endregion

        #region ExecuteReader

        /// <summary>
        /// Test that the expected albums are returned for an artist that has albums.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumsGetByArtist")]
        [TestCase(CommandType.Text, "select * from Album where ArtistId = @ArtistId")]
        public void ExecuteReader_GetAlbumsByArtist_ExpectedAlbumsReturned(CommandType commandType, string commandText)
        {
            var artistId = 1;
            var expectedAlbums = MakeAlbums(artistId);
            IEnumerable<Album> actualAlbums = null;

            var parameters = new List<IDbDataParameter>
            {
                new SqlParameter { ParameterName = "@ArtistId", Value = artistId }
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.ExecuteReader<Album>(cmdSettings);
            }
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that the expected albums are returned for an artist that has albums when using a non-Album
        /// model that is annotated to mimic an Album.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByArtistSimilarModel_ExpectedAlbumsReturned()
        {
            var artistId = 1;
            var expectedAlbums = MakeAlbums(artistId);
            IEnumerable<Album> actualAlbums = null;

            var parameters = new List<IDbDataParameter>
            {
                new SqlParameter { ParameterName = "@ArtistId", Value = artistId }
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGetByArtist", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = ConvertToAlbums(runner.ExecuteReader<SomeModel>(cmdSettings));
            }
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that no albums are returned for an invalid artist id.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByInvalidArtistId_NoAlbumsReturned()
        {
            IEnumerable<Album> actualAlbums = null;

            var parameters = new List<IDbDataParameter>
            {
                new SqlParameter { ParameterName = "@ArtistId", Value = -1 }
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGetByArtist", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.ExecuteReader<Album>(cmdSettings);
            }
            actualAlbums.Should().BeNullOrEmpty();
        }

        /// <summary>
        /// Test that the expected albums are returned when using a table-value parameter that accepts a list of album ids.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByIdsTvp_ExpectedAlbumsReturned()
        {
            var albumIds = new List<int> { 1, 2 };
            var expectedAlbums = MakeAlbums(albumIds);
            IEnumerable<Album> actualAlbums = null;

            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@AlbumIds", albumIds)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGet", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.ExecuteReader<Album>(cmdSettings);
            }
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that the expected albums are returned when executing a query with a IN clause of album ids.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByIdsInClause_ExpectedAlbumsReturned()
        {
            var albumIds = new List<int> { 1, 2 };
            var expectedAlbums = MakeAlbums(albumIds);
            IEnumerable<Album> actualAlbums = null;

            var inClauseProps = QueryHelper.ParameterizeInClauseQuery("select * from Album where AlbumId in ({0})", albumIds);
            var cmdSettings = new CommandSettings(CommandType.Text, inClauseProps.Query, inClauseProps.Parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.ExecuteReader<Album>(cmdSettings);
            }
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that the expected albums are returned when using a complex table-value parameter.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByAlbumsTvp_ExpectedAlbumsReturned()
        {
            var expectedAlbums = MakeAlbums(new List<int> { 1, 2 });
            IEnumerable<Album> actualAlbums = null;

            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@Albums", expectedAlbums)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGetByAlbums", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.ExecuteReader<Album>(cmdSettings);
            }
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that a DataException is thrown when using a table-value parameter that doesn't match the user-defined type.
        /// </summary>
        [Test]
        public void ExecuteReader_GetAlbumsByIdsWrongTvp_DataExceptionThrown()
        {
            var albumIds = new List<string> { "blah1", "blah2" };
            
            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@AlbumIds", albumIds)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGet", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.ExecuteReader<Album>(cmdSettings);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that an error condition throws a DataException.
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        [TestCase(CommandType.StoredProcedure, "blah")]
        [TestCase(CommandType.Text, "select * from blah")]
        [TestCase(CommandType.StoredProcedure, "spRaiseErrorTest")]
        public void ExecuteReader_ErrorCondition_DataExceptionThrown(CommandType commandType, string commandText)
        {
            var cmdSettings = new CommandSettings(commandType, commandText);
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.ExecuteReader<Album>(cmdSettings);
                act.Should().Throw<DataException>();
            }
        }

        #endregion

        #region ExecuteReaderFirst

        /// <summary>
        /// Test that the expected album is returned for an album id that exists.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumGet")]
        [TestCase(CommandType.Text, "select* from Album where AlbumId = @AlbumId")]
        public void ExecuteReaderFirst_GetAlbumById_ExpectedAlbumReturned(CommandType commandType, string commandText)
        {
            var albumId = 1;
            var expectedAlbum = MakeAlbum(albumId);
            var actualAlbum = GetAlbum(commandType, commandText, albumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that no album is returned for an invalid album id.
        /// </summary>
        [Test]
        public void ExecuteReaderFirst_GetAlbumByInvalidId_NullAlbumReturned()
        {
            var actualAlbum = GetAlbum(-1);
            actualAlbum.Should().BeNull();
        }

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// Test that an error condition throws a DataException.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        [TestCase(CommandType.StoredProcedure, "blah")]
        [TestCase(CommandType.Text, "select * from blah")]
        [TestCase(CommandType.StoredProcedure, "spRaiseErrorTest")]
        [TestCase(CommandType.Text, "insert Album (GenreId, ArtistId, Title) output inserted.AlbumId values (1, 1, null)")]
        [TestCase(CommandType.Text, "insert Artist (Id, Name) output inserted.ArtistId values (1, 'blah')")]
        public void ExecuteScalar_ErrorCondition_DataExceptionThrown(CommandType commandType, string commandText)
        {
            var cmdSettings = new CommandSettings(commandType, commandText);
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.ExecuteScalar(cmdSettings);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that the expected album title is returned for an album that exists.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumTitleGet")]
        [TestCase(CommandType.Text, "select Title from Album where AlbumId = @AlbumId")]
        public void ExecuteScalar_GetAlbumTitle_ExpectedTitleReturned(CommandType commandType, string commandText)
        {
            var albumId = 1;
            var expectedAlbum = MakeAlbum(albumId);
            var title = "";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                title = runner.ExecuteScalar(cmdSettings)?.ToString();
            }

            title.Should().Be(expectedAlbum.Title);
        }

        /// <summary>
        /// Test that a null value was returned for the title of a non-existent album.
        /// </summary>
        [Test]
        public void ExecuteScalar_GetAlbumTitleInvalidId_NullTitleReturned()
        {
            var title = "";

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", -1)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumTitleGet", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                title = runner.ExecuteScalar(cmdSettings)?.ToString();
            }

            title.Should().BeNull();
        }

        /// <summary>
        /// Test that an album to be inserted, with and without a transaction, was actually inserted.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        /// <param name="withTransaction">Flag indicating if a transaction should be started.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumInsert_Id", true)]
        [TestCase(CommandType.Text, "insert Album (GenreId, ArtistId, Title) output inserted.AlbumId values (@GenreId, @ArtistId, @Title)", true)]
        [TestCase(CommandType.StoredProcedure, "spAlbumInsert_Id", false)]
        [TestCase(CommandType.Text, "insert Album (GenreId, ArtistId, Title) output inserted.AlbumId values (@GenreId, @ArtistId, @Title)", false)]
        public void ExecuteScalar_InsertAlbumCommit_AlbumWasInserted(CommandType commandType, string commandText, bool withTransaction)
        {
            var expectedAlbum = new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "Diary of a Madman"
            };
            var actualAlbumId = 0;
            
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                actualAlbumId = Convert.ToInt32(runner.ExecuteScalar(cmdSettings));

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(actualAlbumId);
            expectedAlbum.AlbumId = actualAlbumId;
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that an album to be inserted whose transaction was rolled back was not actually inserted.
        /// </summary>
        /// <param name="explicitRollback">Flag indicating whether or not the transaction is explicilty rolled back.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteScalar_InsertAlbumRollback_AlbumNotInserted(bool explicitRollback)
        {
            var actualAlbumId = 0;

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@GenreId", 1),
                new SqlParameter("@ArtistId", 1),
                new SqlParameter("@Title", "blah")
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumInsert_Id", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                actualAlbumId = Convert.ToInt32(runner.ExecuteScalar(cmdSettings));

                if (explicitRollback)
                {
                    uow.Rollback();
                }
            }

            var actualAlbum = GetAlbum(actualAlbumId);
            actualAlbum.Should().BeNull();
        }

        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// Test that an error condition throws a DataException.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or query text.</param>
        [TestCase(CommandType.StoredProcedure, "blah")]
        [TestCase(CommandType.Text, "select * from blah")]
        [TestCase(CommandType.StoredProcedure, "spRaiseErrorTest")]
        [TestCase(CommandType.Text, "insert Album (GenreId, ArtistId, Title) values (1, 1, null);")]
        [TestCase(CommandType.Text, "insert Artist (Id, Name) values (1, 'blah');")]
        [TestCase(CommandType.Text, "update Album set Title = null where AlbumId = 1;")]
        [TestCase(CommandType.Text, "delete Artist where ArtistId = 1;")]
        public void ExecuteNonQuery_ErrorCondition_DataExceptionThrown(CommandType commandType, string commandText)
        {
            var cmdSettings = new CommandSettings(commandType, commandText);
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.ExecuteNonQuery(cmdSettings);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that an album selected using output parameters returns the expected ablum fields.
        /// </summary>
        [Test]
        public void ExecuteNonQuery_AlbumGetOutputParams_ExpectedAlbumReturned()
        {
            var albumId = 1;

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId)
            };
            var genreIdParam = new SqlParameter("@GenreId", SqlDbType.Int);
            genreIdParam.Direction = ParameterDirection.Output;
            parameters.Add(genreIdParam);
            var artistIdParam = new SqlParameter("@ArtistId", SqlDbType.Int);
            artistIdParam.Direction = ParameterDirection.Output;
            parameters.Add(artistIdParam);
            var titleParam = new SqlParameter("@Title", SqlDbType.VarChar, 100);
            titleParam.Direction = ParameterDirection.Output;
            parameters.Add(titleParam);
            var priceParam = new SqlParameter("@Price", SqlDbType.Decimal);
            priceParam.Precision = 5;
            priceParam.Scale = 2;
            priceParam.Direction = ParameterDirection.Output;
            parameters.Add(priceParam);

            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumGetOutParams", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);
            }

            var expectedAlbum = MakeAlbum(albumId);
            var actualAlbum = new Album
            {
                AlbumId = albumId,
                GenreId = Convert.ToInt32(genreIdParam.Value),
                ArtistId = Convert.ToInt32(artistIdParam.Value),
                Title = titleParam.Value.ToString(),
                Price = priceParam.Value.ToDecimal()
            };
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();            
        }

        /// <summary>
        /// Test that an album being inserted using a stored procedure is inserted when no transaction was
        /// started, or when a transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumSprocCommit_AlbumWasInserted(bool withTransaction)
        {
            var expectedAlbum = new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "Diary of a Madman",
                Price = 15.99M
            };

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title),
                new SqlParameter("@Price", expectedAlbum.Price)
            };
            var idParam = new SqlParameter("@AlbumId", SqlDbType.Int);
            idParam.Direction = ParameterDirection.Output;
            parameters.Add(idParam);
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumInsert", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbumId = Convert.ToInt32(idParam.Value);
            expectedAlbum.AlbumId = actualAlbumId;
            var actualAlbum = GetAlbum(actualAlbumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that an album being inserted with inline sql is inserted when no transaction was
        /// started, or when a transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumTextCommit_AlbumWasInserted(bool withTransaction)
        {
            var expectedAlbum = new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "Diary of a Madman",
                Price = 15.99M
            };

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title),
                new SqlParameter("@Price", expectedAlbum.Price)
            };
            var commandText = "insert Album (GenreId, ArtistId, Title, Price) values (@GenreId, @ArtistId, @Title, @Price)";
            var cmdSettings = new CommandSettings(CommandType.Text, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(expectedAlbum.GenreId, expectedAlbum.ArtistId, expectedAlbum.Title, expectedAlbum.Price);
            expectedAlbum.AlbumId = actualAlbum.AlbumId;
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that an album being inserted with a stored procedure is not inserted when the transaction is rolled back.
        /// </summary>
        /// <param name="explicitTransaction">Explicitly or implicitly roll back the transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumSprocRollback_AlbumNotInserted(bool explicitTransaction)
        {
            var expectedAlbum = new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "Diary of a Madman",
                Price = 15.99M
            };

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title),
                new SqlParameter("@Price", expectedAlbum.Price)
            };
            var idParam = new SqlParameter("@AlbumId", SqlDbType.Int);
            idParam.Direction = ParameterDirection.Output;
            parameters.Add(idParam);
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumInsert", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (explicitTransaction)
                {
                    uow.Rollback();
                }
            }

            var actualAlbumId = Convert.ToInt32(idParam.Value);
            var actualAlbum = GetAlbum(actualAlbumId);
            actualAlbum.Should().BeNull();
        }

        /// <summary>
        /// Test that an album being insert with inline sql is not inserted when the transaction is rolled back.
        /// </summary>
        /// <param name="explicitTransaction">Explicitly or implicitly roll back the transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumTextRollback_AlbumNotInserted(bool explicitTransaction)
        {
            var expectedAlbum = new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "Diary of a Madman",
                Price = 15.99M
            };

            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title),
                new SqlParameter("@Price", expectedAlbum.Price)
            };
            var commandText = "insert Album (GenreId, ArtistId, Title, Price) values (@GenreId, @ArtistId, @Title, @Price)";
            var cmdSettings = new CommandSettings(CommandType.Text, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (explicitTransaction)
                {
                    uow.Rollback();
                }
            }

            var actualAlbum = GetAlbum(expectedAlbum.GenreId, expectedAlbum.ArtistId, expectedAlbum.Title, expectedAlbum.Price);
            actualAlbum.Should().BeNull();
        }

        /// <summary>
        /// Test that an album being deleted is deleted when a transaction is not started, or when a transaction
        /// is started and committed.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or sql.</param>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumDelete", true)]
        [TestCase(CommandType.StoredProcedure, "spAlbumDelete", false)]
        [TestCase(CommandType.Text, "delete Album where AlbumId = @AlbumId", true)]
        [TestCase(CommandType.Text, "delete Album where AlbumId = @AlbumId", false)]
        public void ExecuteNonQuery_DeleteAlbumCommit_AlbumWasDeleted(CommandType commandType, string commandText, bool withTransaction)
        {
            var albumId = 4;

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(albumId);
            actualAlbum.Should().BeNull();
        }

        /// <summary>
        /// Test that an album being deleted in a transaction that is rolled back is not actually deleted.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or sql.</param>
        /// <param name="explicitTransaction">Explicit or implicity rollback.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumDelete", true)]
        [TestCase(CommandType.StoredProcedure, "spAlbumDelete", false)]
        [TestCase(CommandType.Text, "delete Album where AlbumId = @AlbumId", true)]
        [TestCase(CommandType.Text, "delete Album where AlbumId = @AlbumId", false)]
        public void ExecuteNonQuery_DeleteAlbumRollback_AlbumNotDeleted(CommandType commandType, string commandText, bool explicitTransaction)
        {
            var expectedAlbum = new Album
            {
                AlbumId = 4,
                GenreId = 1,
                ArtistId = 2,
                Title = "Vs."
            };

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", expectedAlbum.AlbumId)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (explicitTransaction)
                {
                    uow.Rollback();
                }
            }

            var actualAlbum = GetAlbum(expectedAlbum.AlbumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that albums being inserted using a table-valued parameter are not inserted when no transaction was
        /// started, or when a transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumsTvpCommit_AlbumsWereInserted(bool withTransaction)
        {
            var expectedAlbums = new List<Album>
            {
                new Album
                {
                    GenreId = 1,
                    ArtistId = 1,
                    Title = "Diary of a Madman",
                    Price = 15.99M
                },
                new Album
                {
                    GenreId = 1,
                    ArtistId = 2,
                    Title = "No More Tears"
                }
            };
            var parameters = new List<IDbDataParameter> { QueryHelper.MakeTableValuedParameter("@Albums", expectedAlbums) };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsInsert", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbums = GetAlbums(expectedAlbums);
            AreEqual(actualAlbums, expectedAlbums, new List<string> { "AlbumId " });
        }

        /// <summary>
        /// Test that albums being inserted with a table-valued parameter are not inserted when the transaction is rolled back.
        /// </summary>
        /// <param name="explicitTransaction">Explicitly or implicitly roll back the transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_InsertAlbumsTvpRollback_AlbumsNotInserted(bool explicitRollback)
        {
            var expectedAlbums = new List<Album>
            {
                new Album
                {
                    GenreId = 1,
                    ArtistId = 1,
                    Title = "Diary of a Madman",
                    Price = 15.99M
                },
                new Album
                {
                    GenreId = 1,
                    ArtistId = 2,
                    Title = "No More Tears"
                }
            };
            var parameters = new List<IDbDataParameter> { QueryHelper.MakeTableValuedParameter("@Albums", expectedAlbums) };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsInsert", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (explicitRollback)
                {
                    uow.Rollback();
                }
            }

            var actualAlbums = GetAlbums(expectedAlbums);
            actualAlbums.Should().BeEmpty();
        }

        /// <summary>
        /// Test that an album being updated is updated when no transaction was started, or when a transaction
        /// was started and committed.
        /// </summary>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumUpdate", false)]
        [TestCase(CommandType.StoredProcedure, "spAlbumUpdate", true)]
        [TestCase(CommandType.Text, "update Album set GenreId = @GenreId, ArtistId = @ArtistId, Title = @Title, Price = @Price where AlbumId = @AlbumId", false)]
        [TestCase(CommandType.Text, "update Album set GenreId = @GenreId, ArtistId = @ArtistId, Title = @Title, Price = @Price where AlbumId = @AlbumId", true)]
        public void ExecuteNonQuery_UpdateAlbumCommit_AlbumWasUpdated(CommandType commandType, string commandText, bool withTransaction)
        {
            var albumId = 1;
            var expectedAlbum = MakeAlbum(albumId);
            expectedAlbum.Title = "Bark at the Moon (Remastered)";
            expectedAlbum.Price = 14.99M;

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId),
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", expectedAlbum.Title),
                new SqlParameter("@Price", expectedAlbum.Price)
                {
                    Precision = 5,
                    Scale = 2
                }
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);
            }

            var actualAlbum = GetAlbum(albumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that an album being updated is not updated when the transaction is rolled back.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or sql.</param>
        /// <param name="explicitTransaction">Explicitly or implicitly roll back the transaction.</param>
        [TestCase(CommandType.StoredProcedure, "spAlbumUpdate", false)]
        [TestCase(CommandType.StoredProcedure, "spAlbumUpdate", true)]
        [TestCase(CommandType.Text, "update Album set GenreId = @GenreId, ArtistId = @ArtistId, Title = @Title, Price = @Price where AlbumId = @AlbumId", false)]
        [TestCase(CommandType.Text, "update Album set GenreId = @GenreId, ArtistId = @ArtistId, Title = @Title, Price = @Price where AlbumId = @AlbumId", true)]
        public void ExecuteNonQuery_UpdateAlbumRollback_AlbumNotUpdated(CommandType commandType, string commandText, bool explicitTransaction)
        {
            var albumId = 1;
            var expectedAlbum = MakeAlbum(albumId);

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId),
                new SqlParameter("@GenreId", expectedAlbum.GenreId),
                new SqlParameter("@ArtistId", expectedAlbum.ArtistId),
                new SqlParameter("@Title", "Bark at the Moon (Remastered)"),
                new SqlParameter("@Price", 14.99M)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);
            }

            var actualAlbum = GetAlbum(albumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeFalse();
        }

        /// <summary>
        /// Test that an album being updated with a table-value parameter is updated when no transaction was 
        /// started, or when a transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">With or without a transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_UpdateAlbumsTvpCommit_AlbumsWereUpdated(bool withTransaction)
        {
            var album1 = MakeAlbum(1);
            album1.Title = "Bark at the Moon (Remastered)";
            album1.Price = 14.99M;

            var album2 = MakeAlbum(2);
            album2.Title = "Blizzard of Ozz (Live in LA)";
            album2.Price = null;

            var expectedAlbums = new List<Album> { album1, album2 };

            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@Albums", expectedAlbums)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsUpdate", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbums = GetAlbums(new List<int> { 1, 2 });
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        /// <summary>
        /// Test that an album being updated with a table-value parameter is not updated when the transaction is rolled back.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Stored procedure name or sql.</param>
        /// <param name="explicitTransaction">Explicitly or implicitly roll back the transaction.</param>
        [TestCase(true)]
        [TestCase(false)]
        public void ExecuteNonQuery_UpdateAlbumsTvpRollback_AlbumsNotUpdated(bool explicitTransaction)
        {
            var album1 = MakeAlbum(1);
            album1.Title = "Bark at the Moon (Remastered)";
            album1.Price = 14.99M;

            var album2 = MakeAlbum(2);
            album2.Title = "Blizzard of Ozz (Live in LA)";
            album2.Price = null;

            var expectedAlbums = new List<Album> { album1, album2 };

            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@Albums", expectedAlbums)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsUpdate", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                runner.ExecuteNonQuery(cmdSettings);

                if (explicitTransaction)
                {
                    uow.Rollback();
                }
            }

            var actualAlbums = GetAlbums(new List<int> { 1, 2 });
            AreEqual(actualAlbums, expectedAlbums).Should().BeFalse();
        }

        #endregion

        #region Get

        /// <summary>
        /// Test that a DataException is thrown when trying to get a record from a non-existent table.
        /// </summary>
        [Test]
        public void Get_TableDoesNotExist_DataExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Get<MyModel>(1);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that the expected album is returned for the specified ablum id which is the primary key value.
        /// </summary>
        [Test]
        public void Get_AlbumByValidId_ExpectedAlbumReturned()
        {
            var albumId = 1;
            Album actualAlbum;

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbum = runner.Get<Album>(albumId);
            }

            var expectedAlbum = MakeAlbum(albumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that a null album is returned for an invalid album id.
        /// </summary>
        [Test]
        public void Get_AlbumByInvalidId_NullAlbumReturned()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                runner.Get<Album>(-1).Should().BeNull();
            }
        }

        /// <summary>
        /// Test that the expected rating is returned using its two primary keys, ablum id and rating date.
        /// </summary>
        [Test]
        public void Get_RatingByAlbumIdAndRatingDate_ExpectedRatingReturned()
        {
            var albumId = 1;
            var ratingDate = new DateTime(2018, 8, 3);
            Rating actualRating;

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualRating = runner.Get<Rating>(albumId, ratingDate);
            }

            var expectedRating = MakeRating(albumId, ratingDate);
            AreEqual(actualRating, expectedRating).Should().BeTrue();
        }

        #endregion

        #region GetAll

        /// <summary>
        /// Test that a DataException is thrown when trying to get all records from a non-existent table.
        /// </summary>
        [Test]
        public void GetAll_TableDoesNotExist_DataExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.GetAll<MyModel>();
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that all the records from the Album table are returned.
        /// </summary>
        [Test]
        public void GetAll_Albums_ExpectedAlbumsReturned()
        {
            IEnumerable<Album> actualAlbums;

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                actualAlbums = runner.GetAll<Album>();
            }

            var expectedAlbums = MakeAlbums();
            AreEqual(actualAlbums, expectedAlbums).Should().BeTrue();
        }

        #endregion

        #region Delete

        /// <summary>
        /// Test that a DataException is thrown when attempting to delete from a table that does not exist.
        /// </summary>
        [Test]
        public void Delete_TableDoesNotExist_DataExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Delete<MyModel>(1);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that a DataException is thrown when attempting to delete a genre that is tied to an album.
        /// </summary>
        [Test]
        public void Delete_GenreForeignKeyContraint_DataExceptionThrown()
        {
            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Delete<Genre>(1);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that an album is deleted from the database when no transaction was started, or when a
        /// transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">Flag for whether or not a transaction will be started.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Delete_AlbumCommit_AlbumWasDeleted(bool withTransaction)
        {
            var albumId = 4;

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.Delete<Album>(albumId);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(albumId);
            actualAlbum.Should().BeNull();
        }

        /// <summary>
        /// Test an album is not deleted from the database when the transaction is explicitly or implicitly rolled back.
        /// </summary>
        /// <param name="explicitRollback">Explicitly rollback or implicitly rollback the transaction.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Delete_AlbumRollback_AlbumNotDeleted(bool explicitRollback)
        {
            var albumId = 4;
            
            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                runner.Delete<Album>(albumId);

                if (explicitRollback)
                {
                    uow.Rollback();
                }
            }

            var expectedAlbum = MakeAlbum(albumId);
            var actualAlbum = GetAlbum(albumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        #endregion

        #region Insert

        /// <summary>
        /// Test that a DataException is thrown when attempting to insert into a table that does not exist.
        /// </summary>
        [Test]
        public void Insert_TableDoesNotExist_DataExceptionThrown()
        {
            var myModel = new MyModel { Id = 1, Name = "blah" };

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Insert(myModel);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that a DataException is thrown when attempting to insert a genre that already exists.
        /// </summary>
        [Test]
        public void Insert_GenreAlreadyExists_DataExceptionThrown()
        {
            var genre = MakeGenre(1);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Insert(genre);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that a genre is inserted into the database when no transaction was started, or when a
        /// transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">Flag for whether or not a transaction will be started.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Insert_GenreCommit_GenreWasInserted(bool withTransaction)
        {
            var expectedGenre = MakeGenreForInsert();

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.Insert(expectedGenre);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualGenre = GetGenre(expectedGenre.GenreId);
            AreEqual(actualGenre, expectedGenre).Should().BeTrue();
        }

        /// <summary>
        /// Test that a newly inserted genre does not exist in the database when the transaction is explicitly or implicitly rolled back.
        /// </summary>
        /// <param name="explicitRollback">Explicitly rollback or implicitly rollback the transaction.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Insert_GenreRollback_GenreNotInserted(bool explicitRollback)
        {
            var expectedGenre = MakeGenreForInsert();

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                runner.Insert(expectedGenre);

                if (explicitRollback)
                {
                    uow.Rollback();
                }
            }

            var actualGenre = GetGenre(expectedGenre.GenreId);
            actualGenre.Should().BeNull();
        }

        #endregion

        #region InsertForId

        /// <summary>
        /// Test that a DataException is thrown when attempting to insert into a table that does not exist.
        /// </summary>
        [Test]
        public void InsertForId_TableDoesNotExist_DataExceptionThrown()
        {
            var myModel = new MyModel { Name = "blah" };

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.InsertForId(myModel);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that a DataException is thrown when attempting to insert an album that already exists.
        /// </summary>
        [Test]
        public void InsertForId_AlbumAlreadyExists_DataExceptionThrown()
        {
            var album = MakeAlbum(1);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.InsertForId(album);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that an album is inserted into the database when no transaction was started, or when a
        /// transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">Flag for whether or not a transaction will be started.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void InsertForId_AlbumCommit_AlbumWasInserted(bool withTransaction)
        {
            var albumId = 0;
            var expectedAlbum = MakeAlbumForInsert();

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                albumId = runner.InsertForId(expectedAlbum);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(albumId);
            expectedAlbum.AlbumId = albumId;
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that a newly inserted album does not exist in the database when the transaction is explicitly or implicitly rolled back.
        /// </summary>
        /// <param name="explicitRollback">Explicitly rollback or implicitly rollback the transaction.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void InsertForId_AlbumRollback_AlbumNotInserted(bool explicitRollback)
        {
            var albumId = 0;
            var expectedAlbum = MakeAlbumForInsert();

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();

                var runner = new SqlRunner(uow);
                albumId = runner.InsertForId(expectedAlbum);

                if (explicitRollback)
                {
                    uow.Rollback();
                }
            }

            var actualAlbum = GetAlbum(albumId);
            actualAlbum.Should().BeNull();
        }

        #endregion

        #region Update

        /// <summary>
        /// Test that a DataException is thrown when attempting to insert into a table that does not exist.
        /// </summary>
        [Test]
        public void Update_TableDoesNotExist_DataExceptionThrown()
        {
            var myModel = new MyModel { Id = 1, Name = "blah" };

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Update(myModel);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that a DataException is thrown when attempting to update an ablum title to null.
        /// </summary>
        [Test]
        public void Update_AlbumTitleToNull_DataExceptionThrown()
        {
            var album = MakeAlbum(1);
            album.Title = null;

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                Action act = () => runner.Update(album);
                act.Should().Throw<DataException>();
            }
        }

        /// <summary>
        /// Test that an album was updated in the database when no transaction was started, or when a
        /// transaction was started and committed.
        /// </summary>
        /// <param name="withTransaction">Flag for whether or not a transaction will be started.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Update_AlbumCommit_AlbumWasUpdated(bool withTransaction)
        {
            var expectedAlbum = MakeAlbumForUpdate();

            using (var uow = Helper.CreateUnitOfWork())
            {
                if (withTransaction)
                {
                    uow.BeginTransaction();
                }

                var runner = new SqlRunner(uow);
                runner.Update(expectedAlbum);

                if (withTransaction)
                {
                    uow.Commit();
                }
            }

            var actualAlbum = GetAlbum(expectedAlbum.AlbumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeTrue();
        }

        /// <summary>
        /// Test that an updated album was not updated in the database when the transaction is explicitly or implicitly rolled back.
        /// </summary>
        /// <param name="explicitRollback">Explicitly rollback or implicitly rollback the transaction.</param>
        [TestCase(false)]
        [TestCase(true)]
        public void Update_AlbumRollback_AlbumNotUpdated(bool explicitTransaction)
        {
            var expectedAlbum = MakeAlbumForUpdate();

            using (var uow = Helper.CreateUnitOfWork())
            {
                uow.BeginTransaction();
                
                var runner = new SqlRunner(uow);
                runner.Update(expectedAlbum);

                if (explicitTransaction)
                {
                    uow.Rollback();
                }
            }

            var actualAlbum = GetAlbum(expectedAlbum.AlbumId);
            AreEqual(actualAlbum, expectedAlbum).Should().BeFalse();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Compare two objects for property value equality.
        /// </summary>
        /// <typeparam name="T1">Type of object 1.</typeparam>
        /// <typeparam name="T2">Type of object 2.</typeparam>
        /// <param name="object1">Object 1 to compare.</param>
        /// <param name="object2">Object 2 to compare.</param>
        /// <returns><c>true</c> if objects are equal, otherwise <c>false</c>.</returns>
        private bool AreEqual<T1, T2>(T1 object1, T2 object2)
        {
            return AreEqual(object1, object2, new List<string>());
        }

        /// <summary>
        /// Compare two objects for property value equality.
        /// </summary>
        /// <typeparam name="T1">Type of object 1.</typeparam>
        /// <typeparam name="T2">Type of object 2.</typeparam>
        /// <param name="object1">Object 1 to compare.</param>
        /// <param name="object2">Object 2 to compare.</param>
        /// <param name="membersToIgnore">Members to ignore when comparing.</param>
        /// <returns><c>true</c> if objects are equal, otherwise <c>false</c>.</returns>
        private bool AreEqual<T1, T2>(T1 object1, T2 object2, List<string> membersToIgnore)
        {
            var compareLogic = new CompareLogic();
            compareLogic.Config.MembersToIgnore = membersToIgnore;
            var result = compareLogic.Compare(object1, object2);
            return result.AreEqual;
        }

        /// <summary>
        /// Make expected genres.
        /// </summary>
        /// <returns>List of genres.</returns>
        private IEnumerable<Genre> MakeGenres()
        {
            return new List<Genre>
            {
                new Genre { GenreId = 1, Name = "Rock" },
                new Genre { GenreId = 2, Name = "Jazz" }
            };
        }

        /// <summary>
        /// Make expected genre.
        /// </summary>
        /// <param name="genreId">Id of genre.</param>
        /// <returns>Genre</returns>
        private Genre MakeGenre(int genreId)
        {
            return MakeGenres().FirstOrDefault(x => x.GenreId == genreId);
        }

        /// <summary>
        /// Make expected artists.
        /// </summary>
        /// <returns>List of artists.</returns>
        private IEnumerable<Artist> MakeArtists()
        {
            return new List<Artist>
            {
                new Artist { ArtistId = 1, Name = "Ozzy Osbourne" },
                new Artist { ArtistId = 2, Name = "Pearl Jam" }
            };
        }

        /// <summary>
        /// Make expected artist.
        /// </summary>
        /// <param name="artistId">Id of artist.</param>
        /// <returns>Artist</returns>
        private Artist MakeArtist(int artistId)
        {
            return MakeArtists().FirstOrDefault(x => x.ArtistId == artistId);
        }

        /// <summary>
        /// Make list of expected albums.
        /// </summary>
        /// <returns>List of albums.</returns>
        private IEnumerable<Album> MakeAlbums()
        {
            return new List<Album>
            {
                new Album { AlbumId = 1, GenreId = 1, ArtistId = 1, Title = "Bark at the Moon", Price = 12.99M },
                new Album { AlbumId = 2, GenreId = 1, ArtistId = 1, Title = "Blizzard of Ozz", Price = 11.99M },
                new Album { AlbumId = 3, GenreId = 1, ArtistId = 2, Title = "Ten" },
                new Album { AlbumId = 4, GenreId = 1, ArtistId = 2, Title = "Vs." }
            };
        }

        /// <summary>
        /// Make list of expected albums.
        /// </summary>
        /// <param name="artistId">Id of artist.</param>
        /// <returns>List of albums.</returns>
        private IEnumerable<Album> MakeAlbums(int artistId)
        {
            return MakeAlbums().Where(x => x.ArtistId == artistId).ToList();
        }

        /// <summary>
        /// Make list of expected albums.
        /// </summary>
        /// <param name="albumIds">List of albums ids.</param>
        /// <returns>List of albums.</returns>
        private IEnumerable<Album> MakeAlbums(IEnumerable<int> albumIds)
        {
            return MakeAlbums().Where(x => albumIds.Contains(x.AlbumId)).ToList();
        }

        /// <summary>
        /// Convert list of SomeModel objects to list of Album objects.
        /// </summary>
        /// <param name="someModels">List of SomeModel objects.</param>
        /// <returns>List of Album objects.</returns>
        private IEnumerable<Album> ConvertToAlbums(IEnumerable<SomeModel> someModels)
        {
            var albums = new List<Album>();
            foreach (var someModel in someModels)
            {
                albums.Add(new Album
                {
                    AlbumId = someModel.MemberA,
                    GenreId = someModel.MemberB,
                    ArtistId = someModel.MemberC,
                    Title = someModel.MemberD,
                    Price = someModel.MemberE
                });
            }
            return albums;
        }

        /// <summary>
        /// Make expected album.
        /// </summary>
        /// <param name="albumId">Id of album.</param>
        /// <returns>Album</returns>
        private Album MakeAlbum(int albumId)
        {
            return MakeAlbums().FirstOrDefault(x => x.AlbumId == albumId);
        }

        /// <summary>
        /// Make expected ratings.
        /// </summary>
        /// <returns>List of ratings.</returns>
        private IEnumerable<Rating> MakeRatings()
        {
            return new List<Rating>
            {
                new Rating { AlbumId = 1, RatingDate = new DateTime(2018, 8, 1), Stars = 4M },
                new Rating { AlbumId = 2, RatingDate = new DateTime(2018, 8, 4), Stars = 4.5M },
                new Rating { AlbumId = 3, RatingDate = new DateTime(2018, 8, 3), Stars = 5M },
                new Rating { AlbumId = 3, RatingDate = new DateTime(2018, 8, 6), Stars = 4.5M }
            };
        }

        /// <summary>
        /// Make expected ratings.
        /// </summary>
        /// <param name="albumId">Id of album.</param>
        /// <returns>List of ratings.</returns>
        private IEnumerable<Rating> MakeRatings(int albumId)
        {
            return MakeRatings().Where(x => x.AlbumId == albumId).ToList();
        }

        /// <summary>
        /// Make expected rating.
        /// </summary>
        /// <param name="albumId">Id of album.</param>
        /// <param name="ratingDate">Date of rating.</param>
        /// <returns>Rating</returns>
        private Rating MakeRating(int albumId, DateTime ratingDate)
        {
            return MakeRatings().FirstOrDefault(x => x.AlbumId == albumId && x.RatingDate == ratingDate);
        }

        /// <summary>
        /// Make genre for insert.
        /// </summary>
        /// <returns>Genre</returns>
        private Genre MakeGenreForInsert()
        {
            return new Genre
            {
                GenreId = 3,
                Name = "Pop"
            };
        }

        /// <summary>
        /// Make album for insert.
        /// </summary>
        /// <returns>Album</returns>
        private Album MakeAlbumForInsert()
        {
            return new Album
            {
                GenreId = 1,
                ArtistId = 1,
                Title = "No More Tears",
                Price = 14.99M
            };
        }

        /// <summary>
        /// Make album for update.
        /// </summary>
        /// <returns>Album</returns>
        private Album MakeAlbumForUpdate()
        {
            var album = MakeAlbum(1);
            album.Title = album.Title + " (Remastered)";
            album.Price = 18.99M;
            return album;
        }

        /// <summary>
        /// Get album from the database.
        /// </summary>
        /// <param name="albumId">Id of album to retrieve.</param>
        /// <returns></returns>
        private Album GetAlbum(int albumId)
        {
            return GetAlbum(CommandType.StoredProcedure, "spAlbumGet", albumId);
        }

        /// <summary>
        /// Get album from the database.
        /// </summary>
        /// <param name="commandType">Stored procedure or text.</param>
        /// <param name="commandText">Text to execute.</param>
        /// <param name="albumId">Id of album to retrieve.</param>
        /// <returns></returns>
        private Album GetAlbum(CommandType commandType, string commandText, int albumId)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@AlbumId", albumId)
            };
            var cmdSettings = new CommandSettings(commandType, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                return runner.ExecuteReaderFirst<Album>(cmdSettings);
            }
        }

        /// <summary>
        /// Get album from the database.
        /// </summary>
        /// <param name="genreId">Id of genre.</param>
        /// <param name="artistId">Id of artist.</param>
        /// <param name="title">Title of album.</param>
        /// <param name="price">Price of album</param>
        /// <returns></returns>
        private Album GetAlbum(int genreId, int artistId, string title, decimal? price)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@GenreId", genreId),
                new SqlParameter("@ArtistId", artistId),
                new SqlParameter("@Title", title),
                new SqlParameter("@Price", price)
            };
            var commandText = "select * from Album where GenreId = @GenreId and ArtistId = @ArtistId and Title = @Title and Price = @Price;";
            var cmdSettings = new CommandSettings(CommandType.Text, commandText, parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                return runner.ExecuteReaderFirst<Album>(cmdSettings);
            }
        }

        /// <summary>
        /// Get albums from the database.
        /// </summary>
        /// <param name="albums">List of albums to retrieve.</param>
        /// <returns></returns>
        private IEnumerable<Album> GetAlbums(IEnumerable<Album> albums)
        {
            var parameters = new List<IDbDataParameter> { QueryHelper.MakeTableValuedParameter("@Albums", albums) };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGetByAlbums", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                return runner.ExecuteReader<Album>(cmdSettings);
            }
        }

        /// <summary>
        /// Get albums from the database.
        /// </summary>
        /// <param name="albumIds">Ids of albums to retrieve.</param>
        /// <returns></returns>
        private IEnumerable<Album> GetAlbums(IEnumerable<int> albumIds)
        {
            var parameters = new List<IDbDataParameter>
            {
                QueryHelper.MakeTableValuedParameter("@AlbumIds", albumIds)
            };
            var cmdSettings = new CommandSettings(CommandType.StoredProcedure, "spAlbumsGet", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                return runner.ExecuteReader<Album>(cmdSettings);
            }
        }

        /// <summary>
        /// Get genre from the database.
        /// </summary>
        /// <param name="genreId">Id of genre to retrieve.</param>
        /// <returns></returns>
        private Genre GetGenre(int genreId)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@GenreId", genreId)
            };
            var cmdSettings = new CommandSettings(CommandType.Text, "select * from Genre where GenreId = @GenreId", parameters);

            using (var uow = Helper.CreateUnitOfWork())
            {
                var runner = new SqlRunner(uow);
                return runner.ExecuteReaderFirst<Genre>(cmdSettings);
            }
        }

        #endregion
    }
}
