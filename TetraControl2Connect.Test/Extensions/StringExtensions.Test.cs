using FeuerSoftware.TetraControl2Connect.Extensions;
using FluentAssertions;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Extensions
{
    public class StringExtensions
    {
        [Fact]
        public void RemoveSubnetAddresses_Positive_Single()
        {
            var value = "&39Test das ist eine Alarmierung";

            var result = value.RemoveSubnetAddresses();

            result.Should().Be("Test das ist eine Alarmierung");
        }

        [Fact]
        public void RemoveSubnetAddresses_Negative()
        {
            var value = "Test das ist eine Alarmierung für eine GSSI";

            var result = value.RemoveSubnetAddresses();

            result.Should().Be("Test das ist eine Alarmierung für eine GSSI");
        }

        [Fact]
        public void RemoveSubnetAddresses_Positive_Multiple()
        {
            var value = "&01&23&56Test das ist eine Alarmierung für mehrere SNA";

            var result = value.RemoveSubnetAddresses();

            result.Should().Be("Test das ist eine Alarmierung für mehrere SNA");
        }
    }
}
