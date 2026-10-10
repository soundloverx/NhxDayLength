using HarmonyLib;

namespace NhxDayLength.Patches
{
    // EnvMan runs the day/night cycle. Its length is the field m_dayLengthSec, but the game's 0.15 of night on each side
    // of midnight is written into three methods, which are replaced here to use the configured night (see DayCycle).
    // With the game's 21 and 9 minutes they give the game's results. The sleeping's time skip is also replaced, to move
    // the sun at a steady speed.
    internal static class EnvManPatches
    {
        // The skip in progress: its sunset, after which it goes at the night's speed
        private static double _skipSunsetSec;
        private static double _skipNightSpeed;

        // Sets the cycle's length when a world loads
        [HarmonyPatch(typeof(EnvMan), "Awake")]
        private static class AwakePatch
        {
            private static void Postfix()
            {
                DayCycle.Apply();
            }
        }

        // Called every frame to place the sun, and from there everything that depends on the time of day: day or night,
        // afternoon (when you can sleep), cold at night, the "Day N" message at sunrise
        [HarmonyPatch(typeof(EnvMan), "RescaleDayFraction")]
        private static class RescaleDayFractionPatch
        {
            private static bool Prefix(float fraction, ref float __result)
            {
                __result = DayCycle.SunFraction(fraction);
                return false;
            }
        }

        // When a day's morning starts, in world seconds
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.GetMorningStartSec))]
        private static class GetMorningStartSecPatch
        {
            private static bool Prefix(EnvMan __instance, int day, ref double __result)
            {
                __result = (day + DayCycle.HalfNight) * __instance.m_dayLengthSec;
                return false;
            }
        }

        // Sleeping: when everyone is in bed, the server fast-forwards to the next morning in 12 seconds, at a steady speed
        // of the sun (see DayCycle.SkipSpeeds)
        [HarmonyPatch(typeof(EnvMan), nameof(EnvMan.SkipToMorning))]
        private static class SkipToMorningPatch
        {
            private static bool Prefix(EnvMan __instance, ref bool ___m_skipTime, ref double ___m_skipToTime, ref double ___m_timeSkipSpeed)
            {
                double timeSeconds = ZNet.instance.GetTimeSeconds();

                // The day of the last morning: going back half a night counts the hours after midnight as the day before
                int day = __instance.GetDay(timeSeconds - DayCycle.HalfNight * __instance.m_dayLengthSec);
                double morningStartSec = __instance.GetMorningStartSec(day + 1);

                DayCycle.SkipSpeeds(timeSeconds, morningStartSec, __instance.m_dayLengthSec, out _skipSunsetSec,
                    out double afternoonSpeed, out _skipNightSpeed);

                ___m_skipTime = true;
                ___m_skipToTime = morningStartSec;
                ___m_timeSkipSpeed = afternoonSpeed;

                // The game's own log line, plus the night's speed
                ZLog.Log("Time " + timeSeconds + ", day:" + day + "    nextm:" + morningStartSec + "  skipspeed:" + ___m_timeSkipSpeed
                    + "  nightskipspeed:" + _skipNightSpeed);
                return false;
            }
        }

        // The skip's progress, at each update on the server: the game's, with the speed changing to the night's at sunset
        [HarmonyPatch(typeof(EnvMan), "UpdateTimeSkip")]
        private static class UpdateTimeSkipPatch
        {
            private static bool Prefix(float dt, ref bool ___m_skipTime, double ___m_skipToTime, ref double ___m_timeSkipSpeed)
            {
                if (!ZNet.instance.IsServer() || !___m_skipTime)
                {
                    return false;
                }

                double timeSeconds = ZNet.instance.GetTimeSeconds();
                double realSeconds = dt;

                // Reaching sunset in this update: the rest of it at the night's speed
                if (timeSeconds < _skipSunsetSec && timeSeconds + realSeconds * ___m_timeSkipSpeed >= _skipSunsetSec)
                {
                    realSeconds -= (_skipSunsetSec - timeSeconds) / ___m_timeSkipSpeed;
                    timeSeconds = _skipSunsetSec;
                    ___m_timeSkipSpeed = _skipNightSpeed;
                }

                timeSeconds += realSeconds * ___m_timeSkipSpeed;
                if (timeSeconds >= ___m_skipToTime)
                {
                    timeSeconds = ___m_skipToTime;
                    ___m_skipTime = false;
                }

                ZNet.instance.SetNetTime(timeSeconds);
                return false;
            }
        }
    }
}
