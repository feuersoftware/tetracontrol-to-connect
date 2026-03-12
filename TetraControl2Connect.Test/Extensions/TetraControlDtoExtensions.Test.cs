using FeuerSoftware.TetraControl2Connect.Extensions;
using FeuerSoftware.TetraControl2Connect.Models.TetraControl;
using FluentAssertions;
using Xunit;

namespace FeuerSoftware.TetraControl2Connect.Test.Extensions
{
    public class TetraControlDtoExtensionsTest
    {
        [Fact]
        public void GetSdsType_Callout()
        {
            var dto = new TetraControlDto()
            {
                Text = "&01Test BMA",
                Remark = "-1;8;24;"
            };

            var result = dto.GetSdsType();

            result.Should().Be(SdsType.Callout);
        }

        [Fact]
        public void GetSdsType_CalloutFeedback()
        {
            var dto = new TetraControlDto()
            {
                Text = "Rückmeldung: komme",
                Remark = "3;-1;76;32768"
            };

            var result = dto.GetSdsType();

            result.Should().Be(SdsType.CalloutFeedback);
        }

        [Fact]
        public void GetSdsType_TacticalAvailability()
        {
            var dto = new TetraControlDto()
            {
                Text = "Verfügbarkeit: verfügbar",
                Remark = "5;-1;0;15"
            };

            var result = dto.GetSdsType();

            result.Should().Be(SdsType.TacticalAvailability);
        }

        [Fact]
        public void GetSdsType_Unknown()
        {
            var dto = new TetraControlDto()
            {
                Text = "Testnacharicht oder ich bin ein Alarm LOL",
                Remark = "0;-1;0;15"
            };

            var result = dto.GetSdsType();

            result.Should().Be(SdsType.Unknown);
        }

        [Fact]
        public void ExtractStatuscode_Coming()
        {
            var dto = new TetraControlDto()
            {
                Remark = "3;-1;146;32768", // KOMME
            };

            var statusCode = dto.ExtractStatuscode();

            statusCode.Should().NotBeNull();
            statusCode.Should().Be(32768);
        }

        [Fact]
        public void ExtractStatuscode_NotComing()
        {
            var dto = new TetraControlDto()
            {
                Remark = "3;-1;146;32769", // ABGELEHNT
            };

            var statusCode = dto.ExtractStatuscode();

            statusCode.Should().NotBeNull();
            statusCode.Should().Be(32769);
        }

        [Fact]
        public void IsSirenAlarmSds_Negative()
        {
            var dto = new TetraControlDto()
            {
                Text = "&21&25&40F BMA - TEST TEST TEST"
            };

            var isSirenAlarm = dto.IsCalloutForSirens();

            isSirenAlarm.Should().BeFalse();
        }

        [Fact]
        public void IsSirenAlarmSds_Positive()
        {
            var dto = new TetraControlDto()
            {
                Text = "&02&06&07$2002" // Steuersignal $2002 -> Feueralarm für Sirenen
            };

            var isSirenAlarm = dto.IsCalloutForSirens();

            isSirenAlarm.Should().BeTrue();
        }

        [Fact]
        public void ExtractSNAs_Single()
        {
            var dto = new TetraControlDto()
            {
                Text = "&21F BMA - TEST TEST TEST"
            };

            var snas = dto.ExtractSNAs();

            snas.Should().HaveCount(1);
            snas.Should().Contain("&21");
        }

        [Fact]
        public void ExtractSNAs_Multiple()
        {
            var dto = new TetraControlDto()
            {
                Text = "&02&06&07F BMA - TEST TEST TEST"
            };

            var snas = dto.ExtractSNAs();

            snas.Should().HaveCount(3);
            snas.Should().Contain("&02");
            snas.Should().Contain("&06");
            snas.Should().Contain("&07");
        }

        [Fact]
        public void ExtractCalloutSeverity()
        {
            var dto = new TetraControlDto()
            {
                Text = "&02&06&07F BMA - TEST TEST TEST",
                Remark = "-1;5;124;&21&25&40"
            };

            var severity = dto.ExtractCalloutSeverity();

            severity.Should().NotBeNull();
            severity.Should().Be("5");
        }

        [Fact]
        public void IsCalloutSelfSent_Positive()
        {
            var dto = new TetraControlDto()
            {
                Text = "Gesendet: &02&06&07F BMA - TEST TEST TEST",
                Remark = "-1;5;124;"
            };

            var selfSent = dto.IsCalloutSelfSent();

            selfSent.Should().BeTrue();
        }

        [Fact]
        public void IsCalloutSelfSent_Negative()
        {
            var dto = new TetraControlDto()
            {
                Text = "&02&06&07F BMA - TEST TEST TEST",
                Remark = "-1;5;124;&02&06&07"
            };

            var selfSent = dto.IsCalloutSelfSent();

            selfSent.Should().BeFalse();
        }

        [Fact]
        public void ExtractSirenCode_Positive()
        {
            var dto = new TetraControlDto()
            {
                Text = "&01$2003*S01*Bundesweiter Warntag 2023",
            };

            var sirenCode = dto.ExtractSirenCode();

            sirenCode.Should().Be("$2003");
        }
    }
}
