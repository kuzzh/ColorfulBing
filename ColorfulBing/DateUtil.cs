using System;

namespace ColorfulBing {
    public static class DateUtil {
        public static long GetDateTicks(DateTime dt) {
            return dt.Date.Ticks;
        }

        public static DateTime GetDateTimeFromDateTicks(long dateTicks) {
            return new DateTime(dateTicks);
        }
    }
}