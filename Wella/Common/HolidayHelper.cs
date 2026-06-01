using System;
using System.Globalization;

namespace Wella.Common
{
    public static class HolidayHelper
    {
        private static readonly KoreanLunisolarCalendar LunarCalendar = new KoreanLunisolarCalendar();

        /// <summary>
        /// 지정한 날짜의 대한민국 공휴일 명칭을 반환합니다. 공휴일이 아니면 null을 반환합니다.
        /// </summary>
        public static string GetHolidayName(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;

            // 1. 양력 공휴일 체크
            if (month == 1 && day == 1) return "신정";
            if (month == 3 && day == 1) return "삼일절";
            if (month == 5 && day == 5) return "어린이날";
            if (month == 6 && day == 6) return "현충일";
            if (month == 8 && day == 15) return "광복절";
            if (month == 10 && day == 3) return "개천절";
            if (month == 10 && day == 9) return "한글날";
            if (month == 12 && day == 25) return "성탄절";

            // 2. 음력 공휴일 체크 (설날 연휴, 부처님오신날, 추석 연휴)
            try
            {
                // 음력 연/월/일 계산
                int lunarYear = LunarCalendar.GetYear(date);
                int lunarMonth = LunarCalendar.GetMonth(date);
                int lunarDay = LunarCalendar.GetDayOfMonth(date);

                // 윤달(Leap Month) 체크
                int leapMonth = LunarCalendar.GetLeapMonth(lunarYear);
                bool isLeap = (leapMonth > 0 && lunarMonth == leapMonth);

                // 실제 음력 월 계산 (윤달이 있을 경우 인덱스 보정)
                int actualLunarMonth = lunarMonth;
                if (leapMonth > 0 && lunarMonth > leapMonth)
                {
                    actualLunarMonth = lunarMonth - 1;
                }

                // 윤달(Leap Month)에는 공휴일이 적용되지 않음 (평달만 적용)
                if (!isLeap)
                {
                    // 부처님 오신 날: 음력 4월 8일
                    if (actualLunarMonth == 4 && lunarDay == 8)
                        return "석탄일"; // 부처님오신날 (석탄일)

                    // 추석 연휴: 음력 8월 14일, 15일, 16일
                    if (actualLunarMonth == 8)
                    {
                        if (lunarDay == 14 || lunarDay == 15 || lunarDay == 16)
                            return "추석";
                    }

                    // 설날 당일 및 다음날: 음력 1월 1일, 1월 2일
                    if (actualLunarMonth == 1)
                    {
                        if (lunarDay == 1 || lunarDay == 2)
                            return "설날";
                    }
                }

                // 설날 전날 체크: 하루 뒤 날짜가 평달 음력 1월 1일인 경우 설날 연휴 첫날(전날)임
                DateTime nextDay = date.AddDays(1);
                int nextLunarYear = LunarCalendar.GetYear(nextDay);
                int nextLunarMonth = LunarCalendar.GetMonth(nextDay);
                int nextLunarDay = LunarCalendar.GetDayOfMonth(nextDay);
                int nextLeapMonth = LunarCalendar.GetLeapMonth(nextLunarYear);
                bool nextIsLeap = (nextLeapMonth > 0 && nextLunarMonth == nextLeapMonth);

                int nextActualLunarMonth = nextLunarMonth;
                if (nextLeapMonth > 0 && nextLunarMonth > nextLeapMonth)
                {
                    nextActualLunarMonth = nextLunarMonth - 1;
                }

                if (!nextIsLeap && nextActualLunarMonth == 1 && nextLunarDay == 1)
                {
                    return "설날";
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // .NET KoreanLunisolarCalendar 범위를 벗어나는 아주 먼 미래/과거 날짜 예외 안전 처리
            }

            return null;
        }

        /// <summary>
        /// 지정한 날짜가 대한민국 공휴일인지 여부를 반환합니다.
        /// </summary>
        public static bool IsHoliday(DateTime date)
        {
            return GetHolidayName(date) != null;
        }
    }
}
