using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    public class ParameterMappingTests
    {
        [Fact]
        public void Ctor_SetsSourceTargetAndEnabledByDefault()
        {
            var m = new ParameterMapping("Assembly Name", "Spool");

            Assert.Equal("Assembly Name", m.SourceParameter);
            Assert.Equal("Spool", m.TargetParameter);
            Assert.True(m.IsEnabled);
        }

        [Fact]
        public void ToString_ShowsSourceArrowTarget()
        {
            var m = new ParameterMapping("Mark", "Comments");

            Assert.Equal("Mark → Comments", m.ToString());
        }

        [Fact]
        public void Collection_DefaultsToEmptyMappings()
        {
            var c = new ParameterMappingCollection();

            Assert.NotNull(c.Mappings);
            Assert.Empty(c.Mappings);
        }
    }
}
