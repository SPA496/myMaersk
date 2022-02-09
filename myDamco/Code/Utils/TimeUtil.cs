using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myDamco.Code.Utils
{
    public class TimeUtil
    {

        /** Convert from DateTime to unix timestamp. Returns as UTC. */
        public static long DateTimeToUnixTimestamp(DateTime dt)
        {
            return DateTimeToUnixTimestampMiliseconds(dt) / 1000L;
        }

        /** As above, but returns null (or another default value) if the input-parameter "dt" is null */
        public static long? DateTimeToUnixTimestamp(DateTime? dt, long? defaultValue = null)
        {
            return dt == null ? defaultValue : DateTimeToUnixTimestamp(dt.Value);
        }

        /** Convert from DateTime to miliseconds (not seconds) after the unix timestamp epoch. Returns as UTC. */
        public static long DateTimeToUnixTimestampMiliseconds(DateTime dt)
        {
            DateTime unixTimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            double ms = (dt - unixTimeEpoch).TotalMilliseconds;
            return (long)ms;
        }

        /** As above, but returns null (or another default value) if the input-parameter "dt" is null */
        public static long? DateTimeToUnixTimestampMiliseconds(DateTime? dt, long? defaultValue = null)
        {
            return dt == null ? defaultValue : DateTimeToUnixTimestampMiliseconds(dt.Value);
        }
    }
}