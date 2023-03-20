
using MovManagerr.Core.Data.Helpers;
using MovManagerr.Core.Helpers.Extractors.Series;
using MovManagerr.Core.Helpers.Parsers;

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
        [InlineData("Game.of.Thrones.S05E02.720p.HDTV.x264-IMMERSE.mkv", 5, 2)]
        [InlineData("Chernobyl.S01.MULTi.720p.WEB.H264-FRATERNiTY/Chernobyl.S01E01.MULTi.720p.WEB.H264-FRATERNiTY/Chernobyl.S01E01.MULTi.720p.WEB.H264-FRATERNiTY.mkv", 1, 1)]
        public void ExtractSeasonAndEpisodeNumbers_ReturnsExpectedValues(string tvgName, int expectedSeasonNumber, int expectedEpisodeNumber)
        {
            // Act
            (string show, int seasonNumber, int episodeNumber) = SerieHelper.ExtractSeasonAndEpisodeNumbers(tvgName);

            // Assert
            Assert.Equal(expectedSeasonNumber, seasonNumber);
            Assert.Equal(expectedEpisodeNumber, episodeNumber);
        }


        [Theory]
        [InlineData("D:\\MovieTask\\Downloads\\Done\\series\\Prison Break The Complete Series MULTi [1080p] BluRay x264-PopHD", 9, 4)]
        public void ExtractFullSeasonData(string path, int expectedEpisode, int expectedSeason)
        {
            SerieExtractor extractor = new SerieExtractor();

            var result = extractor.ExtractSerie(path);


            Assert.True(result != null);
        }

        [Theory]
        [InlineData("\\\\192.168.0.11\\movie\\Action\\2 Fast 2 Furious (2003)\\2 Fast 2 Furious (2003) MULTi Bluray-1080p.mkv")]
        [InlineData("[ Torrent911.cc ] Abuela.2021.MULTi.1080p.BluRay.DTS.x264-UTT The Grandmother (2022) Bluray-1080p.mkv")]
        public void ExtractFullSeasonData2(string path)
        {
            var result = Parser.ParseMoviePath(path);

            Assert.True(result != null);
        }
    }
}