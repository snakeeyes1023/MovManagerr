using LiteDB;
using MovManagerr.Core.Infrastructures.Configurations.Dbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovManagerr.Core.Tests.DbHelperTests
{
    public class BaseDbContextTests
    {
        [Fact]
        public void QueryData_ShouldNotThrowError_OnValidConfiguration()
        {

            // Arrange
            var dbContext = new BaseDbContext();
            BsonMapper mapper = new BsonMapper();
            // Act
            var movies = dbContext.Movies
                .UseQuery(query =>
                {
                    query = query.Where(x => x.TmdbId != 0);
                    query = query.Where(x => x.Name != null);
                })
                .ToList();

            // Assert
            Assert.NotNull(movies);
        }

        [Fact]
        public void QueryData_ShouldHaveSomeData_OnValidConfiguration()
        {
            // Arrange
            var dbContext = new BaseDbContext();
            // Act
            var movies = dbContext.Movies
                .UseQuery(query =>
                {
                    query = query.Where(x => x._id != null);
                })
                .ToList();

            // Assert
            Assert.True(movies.Any());
        }
    }
}
