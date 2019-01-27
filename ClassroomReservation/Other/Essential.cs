using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClassroomReservation.Other {
    class Essential {
        public static bool hasSpecialChar(string str) {
            string regex = "[\"\';:\\\\/+=*#|]|--|where|select|from|union|where|substr|concat|ascii|insert|update|delete|0x";
            Regex rex = new Regex(regex);
            return rex.IsMatch(str.ToLower());
        }

        public static bool hasKorean(string str) {
            string regex = "[ㄱ-힗]";
            Regex rex = new Regex(regex);
            return rex.IsMatch(str.ToLower());
        }

        public static string[] dayOfWeekToString = { "일", "월", "화", "수", "목", "금", "토" };

        public static string dateTimeToString(DateTime date) {
            CultureInfo cultures = CultureInfo.CreateSpecificCulture("ko-KR");
            return date.ToString(string.Format("yyyy년 MM월 dd일 ddd요일", cultures));
        }
    }
}
