using System;
using System.Text.Json;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FluentAssertions;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Converters
{
    public class UnixEpochDateTimeConverter
    {
        // 2021-05-13T14:30:00Z
        private const long SampleUnixMillis = 1620916200000;

        [Fact]
        public void Read_ParsesUnixEpoch_AsUtcInstant()
        {
            var json = $"{{\"ts\":\"/Date({SampleUnixMillis})/\"}}";

            var dto = JsonSerializer.Deserialize<TetraControlDto>(json)!;

            // Value must represent the exact UTC instant...
            dto.TimestampUTC.Should().Be(new DateTime(2021, 5, 13, 14, 30, 0, DateTimeKind.Utc));
            // ...and be tagged as UTC so it serializes with a 'Z' designator. Otherwise the
            // browser would parse the timestamp as local time (timezone-offset bug in the live view).
            dto.TimestampUTC.Kind.Should().Be(DateTimeKind.Utc);
        }

        [Fact]
        public void Read_ProducesTimestamp_ThatSerializesAsUtc()
        {
            var json = $"{{\"ts\":\"/Date({SampleUnixMillis})/\"}}";
            var dto = JsonSerializer.Deserialize<TetraControlDto>(json)!;

            // Mirrors how SignalR/minimal APIs serialize the timestamp for the frontend.
            var serialized = JsonSerializer.Serialize(dto.TimestampUTC);

            serialized.Should().Be("\"2021-05-13T14:30:00Z\"");
        }

        [Fact]
        public void Write_RoundTripsUnixEpoch()
        {
            var json = $"{{\"ts\":\"/Date({SampleUnixMillis})/\"}}";
            var dto = JsonSerializer.Deserialize<TetraControlDto>(json)!;

            var roundTripped = JsonSerializer.Serialize(dto);

            roundTripped.Should().Contain($"/Date({SampleUnixMillis})/");
        }
    }
}
