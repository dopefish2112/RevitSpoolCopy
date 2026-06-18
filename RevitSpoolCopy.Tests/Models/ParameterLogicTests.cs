using System.Collections.Generic;
using RevitSpoolCopy.Models;

namespace RevitSpoolCopy.Tests.Models
{
    public class ParameterLogicTests
    {
        // ---- CollectParameterNames ----

        [Fact]
        public void CollectParameterNames_DedupesCaseInsensitive_AndSorts()
        {
            var e1 = new FakeElement()
                .Add(new FakeParameter { Name = "Spool" })
                .Add(new FakeParameter { Name = "Mark" });
            var e2 = new FakeElement()
                .Add(new FakeParameter { Name = "spool" })   // dupe of "Spool"
                .Add(new FakeParameter { Name = "Assembly Name" });

            var names = ParameterLogic.CollectParameterNames(new[] { e1, e2 });

            Assert.Equal(new[] { "Assembly Name", "Mark", "Spool" }, names);
        }

        [Fact]
        public void CollectParameterNames_SkipsNullElementsBlankNames()
        {
            var e = new FakeElement()
                .Add(new FakeParameter { Name = "  " })
                .Add(new FakeParameter { Name = null })
                .Add(new FakeParameter { Name = "Real" });

            var names = ParameterLogic.CollectParameterNames(new IElementView[] { null, e });

            Assert.Equal(new[] { "Real" }, names);
        }

        [Fact]
        public void CollectParameterNames_NullInput_ReturnsEmpty()
        {
            Assert.Empty(ParameterLogic.CollectParameterNames(null));
        }

        // ---- ReadValue ----

        [Fact]
        public void ReadValue_StringStorage_ReturnsAsString()
        {
            var e = new FakeElement().Add(new FakeParameter
            { Name = "Spool", IsStringStorage = true, StringValue = "SP-01", DisplayValue = "wrong" });

            Assert.Equal("SP-01", ParameterLogic.ReadValue(e, "Spool"));
        }

        [Fact]
        public void ReadValue_NonStringStorage_ReturnsDisplayValue()
        {
            var e = new FakeElement().Add(new FakeParameter
            { Name = "Length", IsStringStorage = false, StringValue = "raw", DisplayValue = "10 mm" });

            Assert.Equal("10 mm", ParameterLogic.ReadValue(e, "Length"));
        }

        [Fact]
        public void ReadValue_NoValue_ReturnsNull()
        {
            var e = new FakeElement().Add(new FakeParameter { Name = "Spool", HasValue = false });
            Assert.Null(ParameterLogic.ReadValue(e, "Spool"));
        }

        [Fact]
        public void ReadValue_MissingParameter_ReturnsNull()
        {
            var e = new FakeElement();
            Assert.Null(ParameterLogic.ReadValue(e, "Nope"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ReadValue_BlankName_ReturnsNull(string name)
        {
            var e = new FakeElement().Add(new FakeParameter { Name = "Spool", StringValue = "x" });
            Assert.Null(ParameterLogic.ReadValue(e, name));
        }

        [Fact]
        public void ReadValue_AccessorThrows_ReturnsNull()
        {
            var e = new FakeElement().Add(new FakeParameter { Name = "Spool", Throws = true });
            Assert.Null(ParameterLogic.ReadValue(e, "Spool"));
        }

        // ---- WriteValue ----

        [Fact]
        public void WriteValue_StringStorage_SetsStringValue()
        {
            var p = new FakeParameter { Name = "Spool", IsStringStorage = true };
            var e = new FakeElement().Add(p);

            Assert.True(ParameterLogic.WriteValue(e, "Spool", "SP-99"));
            Assert.Equal("SP-99", p.StringValue);
        }

        [Fact]
        public void WriteValue_NonStringStorage_SetsDisplayValue()
        {
            var p = new FakeParameter { Name = "Length", IsStringStorage = false };
            var e = new FakeElement().Add(p);

            Assert.True(ParameterLogic.WriteValue(e, "Length", "5 mm"));
            Assert.Equal("5 mm", p.DisplayValue);
        }

        [Fact]
        public void WriteValue_NullValue_WritesEmptyString()
        {
            var p = new FakeParameter { Name = "Spool", IsStringStorage = true };
            var e = new FakeElement().Add(p);

            Assert.True(ParameterLogic.WriteValue(e, "Spool", null));
            Assert.Equal("", p.StringValue);
        }

        [Fact]
        public void WriteValue_ReadOnly_ReturnsFalse()
        {
            var p = new FakeParameter { Name = "Spool", IsReadOnly = true };
            var e = new FakeElement().Add(p);

            Assert.False(ParameterLogic.WriteValue(e, "Spool", "x"));
        }

        [Fact]
        public void WriteValue_MissingParameter_ReturnsFalse()
        {
            Assert.False(ParameterLogic.WriteValue(new FakeElement(), "Nope", "x"));
        }

        [Fact]
        public void WriteValue_SetterThrows_ReturnsFalse()
        {
            var p = new FakeParameter { Name = "Spool", Throws = true };
            var e = new FakeElement().Add(p);

            Assert.False(ParameterLogic.WriteValue(e, "Spool", "x"));
        }

        // ---- GetMappedSourceValue (Assembly Name special-casing) ----

        [Fact]
        public void GetMappedSourceValue_AssemblyName_PrefersBuiltin()
        {
            var e = new FakeElement
            {
                AssemblyNameParameter = new FakeParameter
                { Name = "Assembly Name", IsStringStorage = true, StringValue = "ASM-1" }
            };
            // also a custom param of the same name that should be ignored
            e.Add(new FakeParameter { Name = "Assembly Name", StringValue = "custom" });

            Assert.Equal("ASM-1", ParameterLogic.GetMappedSourceValue(e, "Assembly Name"));
        }

        [Fact]
        public void GetMappedSourceValue_AssemblyName_FallsBackToCustom_WhenBuiltinEmpty()
        {
            var e = new FakeElement
            {
                AssemblyNameParameter = new FakeParameter { Name = "Assembly Name", HasValue = false }
            };
            e.Add(new FakeParameter { Name = "Assembly Name", StringValue = "custom" });

            Assert.Equal("custom", ParameterLogic.GetMappedSourceValue(e, "Assembly Name"));
        }

        [Fact]
        public void GetMappedSourceValue_OtherName_ReadsCustomParameter()
        {
            var e = new FakeElement().Add(new FakeParameter { Name = "Mark", StringValue = "M-5" });
            Assert.Equal("M-5", ParameterLogic.GetMappedSourceValue(e, "Mark"));
        }

        [Fact]
        public void GetMappedSourceValue_Missing_ReturnsNull()
        {
            Assert.Null(ParameterLogic.GetMappedSourceValue(new FakeElement(), "Mark"));
        }
    }
}
