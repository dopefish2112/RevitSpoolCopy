using System.Collections.Generic;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    public class SpoolExportLogicTests
    {
        // ---- NormalizeSpool ----

        [Theory]
        [InlineData(null, "(empty)")]
        [InlineData("", "(empty)")]
        [InlineData("   ", "(empty)")]
        [InlineData("SP-01", "SP-01")]
        public void NormalizeSpool_BlankBecomesEmptyLabel(string input, string expected)
        {
            Assert.Equal(expected, SpoolExportLogic.NormalizeSpool(input));
        }

        // ---- CollectIdsForSpools ----

        [Fact]
        public void CollectIdsForSpools_ReturnsOnlyMatching_PreservesOrder()
        {
            var parts = new List<KeyValuePair<string, int>>
            {
                new("SP-01", 1),
                new("SP-02", 2),
                new("SP-01", 3),
                new("SP-03", 4),
            };
            var selected = new HashSet<string> { "SP-01", "SP-03" };

            var ids = SpoolExportLogic.CollectIdsForSpools(parts, selected);

            Assert.Equal(new[] { 1, 3, 4 }, ids);
        }

        [Fact]
        public void CollectIdsForSpools_MatchesEmptyLabel_ForBlankSpools()
        {
            var parts = new List<KeyValuePair<string, int>>
            {
                new("", 1),
                new(null, 2),
                new("SP-01", 3),
            };
            var selected = new HashSet<string> { "(empty)" };

            var ids = SpoolExportLogic.CollectIdsForSpools(parts, selected);

            Assert.Equal(new[] { 1, 2 }, ids);
        }

        [Fact]
        public void CollectIdsForSpools_NoSelection_ReturnsEmpty()
        {
            var parts = new List<KeyValuePair<string, int>> { new("SP-01", 1) };

            Assert.Empty(SpoolExportLogic.CollectIdsForSpools(parts, new HashSet<string>()));
            Assert.Empty(SpoolExportLogic.CollectIdsForSpools(parts, null));
            Assert.Empty(SpoolExportLogic.CollectIdsForSpools<int>(null, new HashSet<string> { "SP-01" }));
        }

        // ---- SanitizeFileStem ----

        [Theory]
        [InlineData("SP-01", "SP-01")]
        [InlineData("Level 1/Riser", "Level 1_Riser")]
        [InlineData("a:b*c?d", "a_b_c_d")]
        [InlineData("name.", "name")]
        [InlineData("  trim  ", "trim")]
        [InlineData("", "spool")]
        [InlineData("   ", "spool")]
        public void SanitizeFileStem_ReplacesInvalidAndTrims(string input, string expected)
        {
            Assert.Equal(expected, SpoolExportLogic.SanitizeFileStem(input));
        }

        // ---- MajFileName ----

        [Fact]
        public void MajFileName_AddsExtension_AndSanitizes()
        {
            Assert.Equal("SP-01.maj", SpoolExportLogic.MajFileName("SP-01"));
            Assert.Equal("A_B.maj", SpoolExportLogic.MajFileName("A/B"));
            Assert.Equal("(empty).maj", SpoolExportLogic.MajFileName(null));
        }

        // ---- ViewName ----

        [Fact]
        public void ViewName_PrefixesSpool()
        {
            Assert.Equal("Spool - SP-01", SpoolExportLogic.ViewName("SP-01"));
            Assert.Equal("Spool - (empty)", SpoolExportLogic.ViewName(""));
        }
    }
}
