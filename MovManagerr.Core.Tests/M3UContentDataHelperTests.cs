
using MovManagerr.Core.Data.Helpers;

namespace M3USync.Tests
{
    public class M3UContentDataHelperTests
    {
        [Theory]
        [InlineData("|FR| En Famille S10 En.famille.S10E26", 10, 26)]
        [InlineData("|FR| En Famille S22 En.famille.S22E26", 22, 26)]
        [InlineData("|FR| Fugueuse S01 Épisode 1", 1, 1)]
        [InlineData("|FR| Clickbait S01 Épisode 3", 1, 3)]
        [InlineData("|FR| Some Show S20E50", 20, 50)]
        [InlineData("Grizzy et les Lemmings S01 E04", 1, 4)]
        [InlineData("Chouerreb S00 E01", 0, 1)]
        public void ExtractSeasonAndEpisodeNumbers_ReturnsExpectedValues(string tvgName, int expectedSeasonNumber, int expectedEpisodeNumber)
        {
            // Act
            (string show, int seasonNumber, int episodeNumber) = SerieHelper.ExtractSeasonAndEpisodeNumbers(tvgName);

            // Assert
            Assert.Equal(expectedSeasonNumber, seasonNumber);
            Assert.Equal(expectedEpisodeNumber, episodeNumber);
        }
    }
}