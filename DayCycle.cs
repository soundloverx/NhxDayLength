namespace NhxDayLength
{
    // The day/night cycle with the configured day and night lengths.
    //
    // The game's cycle is EnvMan.m_dayLengthSec long (1800 s, set in the game's scene), counted from midnight. The part
    // on each side of midnight is night, 0.15 of the cycle before and after it, and the rest is daylight: 9 and 21
    // minutes. Here the cycle is the day plus the night, and half the night is on each side of midnight.
    internal static class DayCycle
    {
        // Half the night as a fraction of the cycle, i.e. where morning starts: the game's 0.15
        internal static double HalfNight { get; private set; } = 0.15;

        // How long sleeping takes, in real seconds: the game's
        internal const double SkipRealSeconds = 12.0;

        internal static void Apply()
        {
            int dayMinutes = Plugin.DayMinutes;
            int nightMinutes = Plugin.NightMinutes;
            long cycleSec = (dayMinutes + nightMinutes) * 60L;

            HalfNight = nightMinutes / (2.0 * (dayMinutes + nightMinutes));

            if (EnvMan.instance != null)
            {
                EnvMan.instance.m_dayLengthSec = cycleSec;
            }

            Plugin.Log.LogInfo($"Day/night cycle: {dayMinutes} min of day and {nightMinutes} min of night ({cycleSec} s).");
        }

        // The game's RescaleDayFraction with the configured night. It turns the position in the cycle (0 to 1 from
        // midnight) into the sun's position, which the game's code works with: 0.25 is sunrise and 0.75 is sunset, so
        // night and day each get their share of the cycle.
        internal static float SunFraction(float cycleFraction)
        {
            float halfNight = (float)HalfNight;

            // After midnight
            if (cycleFraction < halfNight)
            {
                return cycleFraction / halfNight * 0.25f;
            }

            // Before midnight
            if (cycleFraction > 1f - halfNight)
            {
                return 0.75f + (cycleFraction - (1f - halfNight)) / halfNight * 0.25f;
            }

            return 0.25f + (cycleFraction - halfNight) / (1f - 2f * halfNight) * 0.5f;
        }

        // Sleeping: the speeds of the skip from timeSeconds to morningSec, in world seconds per real second. The first
        // until sunset, if it starts before, and the second for the night.
        //
        // The game's skip goes at one speed, but players' games only get the time from the server every 2 seconds. With
        // a long day and a short night, from the afternoon, one of those updates can jump from before sunset to after
        // sunrise, and their sun then takes the short way there: backward, with no "Day N" message. So the skip moves
        // the sun at a steady speed instead, which gives the night at least 8 of the 12 seconds.
        internal static void SkipSpeeds(double timeSeconds, double morningSec, long cycleSec, out double sunsetSec,
            out double afternoonSpeed, out double nightSpeed)
        {
            double nightSec = 2.0 * HalfNight * cycleSec;
            sunsetSec = morningSec - nightSec;

            // Already night: what's left of it, at one speed like the game
            if (timeSeconds >= sunsetSec)
            {
                afternoonSpeed = nightSpeed = (morningSec - timeSeconds) / SkipRealSeconds;
                return;
            }

            // The sun's way to go, in half turns: what's left of the daylight, then the night
            double daylightSec = cycleSec - nightSec;
            double halfTurns = (sunsetSec - timeSeconds) / daylightSec + 1.0;

            afternoonSpeed = daylightSec * halfTurns / SkipRealSeconds;
            nightSpeed = nightSec * halfTurns / SkipRealSeconds;
        }
    }
}
