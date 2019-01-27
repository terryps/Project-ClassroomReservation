using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomReservation.Server {
    class Logger {
        private static string add;

        public static void log(string str) {
            try {
                if (add != null && !add.Equals("")) {
                    str += "&plain=" + add;
                    add = null;
                }

                string url = "http://noverish.me/api/cs_reservation/log.php?log=" + str;
                
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url.Replace("&","%26"));
                httpWebRequest.Method = "GET";
                httpWebRequest.Timeout = 5000;

                HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                Stream respPostStream = httpResponse.GetResponseStream();
                StreamReader readerPost = new StreamReader(respPostStream, Encoding.GetEncoding("UTF-8"), true);

                string resultStr = readerPost.ReadToEnd();
            } catch (Exception e) {

            }
        }

        public static void logNext(String str) {
            add = str;
        }
    }
}
