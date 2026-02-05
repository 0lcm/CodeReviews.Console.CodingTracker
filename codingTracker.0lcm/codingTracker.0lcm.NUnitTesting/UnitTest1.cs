using codingTracker._0lcm.Services;

namespace codingTracker._0lcm.NUnitTesting
{
    public class TimeValidationTests
    {
        [TestCase("2026-1-1", true)]
        [TestCase("2026-01-01", true)]
        [TestCase("2026-01-1", true)]
        [TestCase("2026-1-01", true)]
        [TestCase("2026-31-1", false)]
        [TestCase("26-1-1", false)]
        [TestCase("2026/1/1", false)]
        public void TryValidateDateTime_GivenValidInput_ReturnCorrectBool(string input, bool expectedResult)
        {
            bool? result;

            try
            {
                result = TimeValidationService.TryValidateDateTime(input, out DateOnly date, out string? errorMessage);
            }
            catch (Exception)
            {
                result = null;
            }

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [TestCase("10:10", "12:20", true)]
        [TestCase("10:10", "08:20", true)]
        [TestCase("10:10", "12:20", true)]
        [TestCase("1:10", "02:20", true)]
        [TestCase("01:10", "2:20", true)]
        [TestCase("10:01", "12:00", true)]
        [TestCase("10:1", "12:20", false)]
        public void TryValidateStartAndEndTimes_GivenValidInputs_ReturnCorrectBool(string startTimeInput, string endTimeInput, bool expectedResult)
        {
            bool? result;

            try
            {
                result = TimeValidationService.TryValidateStartAndEndTimes(startTimeInput, endTimeInput,
                    out DateTime startTime, out DateTime endTime, out string? errorMessage);
            }
            catch (Exception)
            {
                result = null;
            }

            Assert.That(result, Is.EqualTo(expectedResult));
        }
    }
}
