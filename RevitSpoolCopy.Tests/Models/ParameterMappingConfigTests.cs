using System.IO;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    public class ParameterMappingConfigTests
    {
        private static string NewTempPath() =>
            Path.Combine(Path.GetTempPath(), "rsc_mappings_" + Guid.NewGuid() + ".json");

        [Fact]
        public void SaveAndLoad_PreservesMappings()
        {
            // Arrange
            var testPath = NewTempPath();
            var config = new ParameterMappingCollection
            {
                Mappings =
                {
                    new ParameterMapping("Assembly Name", "Spool") { IsEnabled = true },
                    new ParameterMapping("Mark", "Comments") { IsEnabled = false }
                }
            };

            try
            {
                // Act: Save
                Assert.True(ParameterMappingConfig.Save(config, testPath), "Save should report success");
                Assert.True(File.Exists(testPath), "Config file should be created");

                // Act: Load
                var loaded = ParameterMappingConfig.Load(testPath);

                // Assert
                Assert.NotNull(loaded);
                Assert.Equal(2, loaded.Mappings.Count);
                Assert.Equal("Assembly Name", loaded.Mappings[0].SourceParameter);
                Assert.Equal("Spool", loaded.Mappings[0].TargetParameter);
                Assert.True(loaded.Mappings[0].IsEnabled);
                Assert.Equal("Mark", loaded.Mappings[1].SourceParameter);
                Assert.False(loaded.Mappings[1].IsEnabled);
            }
            finally
            {
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }

        [Fact]
        public void Load_NonexistentFile_ReturnsEmptyCollection()
        {
            // Arrange
            var testPath = NewTempPath();

            // Act
            var loaded = ParameterMappingConfig.Load(testPath);

            // Assert
            Assert.NotNull(loaded);
            Assert.Empty(loaded.Mappings);
        }

        [Fact]
        public void Save_EmptyCollection_CreatesValidFile()
        {
            // Arrange
            var testPath = NewTempPath();
            var config = new ParameterMappingCollection();

            try
            {
                // Act
                Assert.True(ParameterMappingConfig.Save(config, testPath));

                // Assert
                Assert.True(File.Exists(testPath));
                var loaded = ParameterMappingConfig.Load(testPath);
                Assert.Empty(loaded.Mappings);
            }
            finally
            {
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }

        [Fact]
        public void Save_SetsLastModified()
        {
            // Arrange
            var testPath = NewTempPath();
            var config = new ParameterMappingCollection
            {
                Mappings = { new ParameterMapping("A", "B") }
            };

            try
            {
                // Act
                ParameterMappingConfig.Save(config, testPath);
                var loaded = ParameterMappingConfig.Load(testPath);

                // Assert: round-trips and LastModified persisted
                Assert.Single(loaded.Mappings);
                Assert.Equal(config.LastModified, loaded.LastModified);
            }
            finally
            {
                if (File.Exists(testPath))
                    File.Delete(testPath);
            }
        }
    }
}
