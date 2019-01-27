using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Reflection;

namespace ClassroomReservation.Server {
    class ServerClient {
        private static ServerClient instance;
        public static ServerClient getInstance() {
            if (instance == null)
                instance = new ServerClient();
            return instance;
        }

        private const string serverDomain = "http://cs.korea.ac.kr/reservation/";

        private const string reservationListWeekUrl = "reservation_list_week.php";
        private const string reservationAddUrl = "reservation_add.php";
        private const string reservationDeleteOneUrl = "reservation_delete_one.php";
        private const string reservationDeletePeriodUrl = "reservation_delete_period.php";
        private const string reservationModifyUrl = "reservation_modify.php";
        
        private const string lectureAddUrl = "lecture_add.php";
        private const string lectureDeleteUrl = "lecture_delete.php";

        private const string classroomListUrl = "classroom_list.php";
        private const string classroomAddUrl = "classroom_add.php";
        private const string classroomDeleteUrl = "classroom_delete.php";

        private const string classtimeListUrl = "classtime_list.php";
        private const string classtimeAddUrl = "classtime_add.php";
        private const string classtimeModifyUrl = "classtime_modify.php";
        private const string classtimeDeleteUrl = "classtime_delete.php";

        private string RESERVATION_FILE_PATH = "reservation.csv";
        private string STATUS_FILE_PATH = "status.csv";
        private string LECTURE_FILE_PATH = "lecture.csv";
        private string CLASSTIME_FILE_PATH = "classtime.txt";
        private string CLASSROOM_FILE_PATH = "classroom.txt";

        private const int RESERVATION_ID = 0;
        private const int RESERVATION_START_DATE = 1;
        private const int RESERVATION_END_DATE = 2;
        private const int RESERVATION_START_CLASS = 3;
        private const int RESERVATION_END_CLASS = 4;
        private const int RESERVATION_CLASSROOM = 5;
        private const int RESERVATION_USER_NAME = 6;
        private const int RESERVATION_CONTACT = 7;
        private const int RESERVATION_CONTENT = 8;
        private const int RESERVATION_PASSWORD = 9;

        private const int STATUS_ID = 0;
        private const int STATUS_RESERV_ID = 1;
        private const int STATUS_TYPE = 2;
        private const int STATUS_DATE= 3;
        private const int STATUS_CLASSTIEM = 4;
        private const int STATUS_CLASSROOM = 5;
        private const int STATUS_USER_NAME = 6;
        private const int STATUS_CONTACT = 7;
        private const int STATUS_CONTENT = 8;

        private const int LECTURE_ID = 0;
        private const int LECTURE_YEAR = 1;
        private const int LECTURE_SEMESTER = 2;
        private const int LECTURE_DAYOFWEEK = 3;
        private const int LECTURE_CLASSTIME = 4;
        private const int LECTURE_CLASSROOM = 5;
        private const int LECTURE_PROFESSOR = 6;
        private const int LECTURE_CONTACT = 7;
        private const int LECTURE_CODE = 8;
        private const int LECTURE_NAME = 9;

        public Hashtable classTimeTable { get; private set; }
        public List<string> classroomList { get; private set; }
        private List<StatusItem> status;

        private ServerClient() {
            reloadClassroomList();
            reloadClasstimeList();

            RESERVATION_FILE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), RESERVATION_FILE_PATH);
            STATUS_FILE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), STATUS_FILE_PATH);
            LECTURE_FILE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), LECTURE_FILE_PATH);
            CLASSROOM_FILE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CLASSROOM_FILE_PATH);
            CLASSTIME_FILE_PATH = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), CLASSTIME_FILE_PATH);

            createFileIfNotExist(RESERVATION_FILE_PATH);
            createFileIfNotExist(STATUS_FILE_PATH);
            createFileIfNotExist(LECTURE_FILE_PATH);
            createFileIfNotExist(CLASSROOM_FILE_PATH);
            createFileIfNotExist(CLASSTIME_FILE_PATH);
        }

        public List<StatusItem> reservationListWeek(DateTime datePara) {
            List<StatusItem> items = new List<StatusItem>();

            var statusReader = new StreamReader(STATUS_FILE_PATH);
            var statusIdToInsert = -1;
            while (!statusReader.EndOfStream) {
                var line = statusReader.ReadLine();
                var values = line.Split(',');

                int reservID = Int32.Parse(values[1]);
                int type = Int32.Parse(values[2]);
                DateTime date = Convert.ToDateTime(values[3]);
                int classtime = Int32.Parse(values[4]);
                string classroom = values[5];
                string userName = values[6];
                string contact = values[7];
                string content = values[8];

                StatusItem item = new StatusItem(reservID, type, date, classtime, classroom, userName, contact, content);

                items.Add(item);
            }
            statusIdToInsert += 1;
            statusReader.Close();

            status = items;
            return items;
        }

        public void reservationAdd(ReservationItem reservation) {
            int reservLastId = -1;
            foreach (string row in File.ReadAllLines(RESERVATION_FILE_PATH)) {
                string[] values = row.Split(',');
                reservLastId = Int32.Parse(values[RESERVATION_ID]);
            }
            
            var str = (++reservLastId).ToString();
            str += "," + reservation.startDate.ToString("yyyy-MM-dd");
            str += "," + reservation.endDate.ToString("yyyy-MM-dd");
            str += "," + reservation.startClass;
            str += "," + reservation.endClass;
            str += "," + reservation.classroom;
            str += "," + reservation.userName;
            str += "," + reservation.contact;
            str += "," + reservation.content;
            str += "," + reservation.password;
            str += Environment.NewLine;
            File.AppendAllText(RESERVATION_FILE_PATH, str);

            int statusLastId = -1;
            foreach (string row in File.ReadAllLines(STATUS_FILE_PATH)) {
                string[] values = row.Split(',');
                statusLastId = Int32.Parse(values[STATUS_ID]);
            }

            DateTime date = reservation.startDate;
            for (; date.Date <= reservation.endDate.Date; date = date.AddDays(1)) {
                for (int time = reservation.startClass; time <= reservation.endClass; time++) {
                    str = (++statusLastId).ToString();
                    str += "," + reservLastId;
                    str += "," + 1;
                    str += "," + date.ToString("yyyy-MM-dd");
                    str += "," + time.ToString();
                    str += "," + reservation.classroom;
                    str += "," + reservation.userName;
                    str += "," + reservation.contact;
                    str += "," + reservation.content;
                    str += Environment.NewLine;
                    File.AppendAllText(STATUS_FILE_PATH, str);
                }
            }
        }
        
        public bool reservationDeleteOne(int reservID, string password, bool isUserMode) {
            string[] rows = File.ReadAllLines(RESERVATION_FILE_PATH);

            foreach (string row in rows) {
                string[] values = row.Split(',');

                if (reservID == Int32.Parse(values[RESERVATION_ID])) {
                    if (isUserMode && !password.Equals(values[RESERVATION_PASSWORD])) {
                        return false;
                    }

                    File.WriteAllText(RESERVATION_FILE_PATH, File.ReadAllText(RESERVATION_FILE_PATH).Replace(row + Environment.NewLine, ""));
                    break;
                }
            }
            
            rows = File.ReadAllLines(STATUS_FILE_PATH);

            foreach (string row in rows) {
                string[] values = row.Split(',');

                if (reservID == Int32.Parse(values[STATUS_RESERV_ID])) {
                    File.WriteAllText(STATUS_FILE_PATH, File.ReadAllText(STATUS_FILE_PATH).Replace(row + Environment.NewLine, ""));
                }
            }

            return true;
        }

        public void reservationDeletePeriod(DateTime startDate, DateTime endDate, bool deleteLecture) {

        }

        public bool reservationModify(int reservID, string password, string userName, string contact, string content, bool isUserMode) {
            return true;
        }

        public bool[] checkClassroomStatusByClasstime(DateTime startDate, DateTime endDate, int startTime, int endTime) {
            reservationListWeek(startDate);
            bool[] answer = Enumerable.Repeat(true, classroomList.Count).ToArray();

            foreach (StatusItem item in status) {
                if (startDate.Date <= item.date.Date && item.date.Date <= endDate.Date) {
                    if (startTime <= item.classtime && item.classtime <= endTime) {
                        answer[GetRowByClassroom(item.classroom)] = false;
                    }
                }
            }

            return answer;
        }

        public bool[] checkClasstimeStatusByClassrom(DateTime startDate, DateTime endDate, string classroom) {
            reservationListWeek(startDate);
            bool[] answer = Enumerable.Repeat(true, classTimeTable.Count).ToArray();

            foreach (StatusItem item in status) {
                if (startDate.Date <= item.date.Date && item.date.Date <= endDate.Date) {
                    if (item.classroom.Equals(classroom)) {
                        answer[item.classtime - 1] = false;
                    }
                }
            }

            return answer;
        }


        public void lectureAdd(LectureItem lecture, DateTime semesterStartDate) {
            DateTime semesterEndDate = semesterStartDate.AddDays((7 * 16) - 1);

            int lectureLastId = -1;
            foreach (string row in File.ReadAllLines(RESERVATION_FILE_PATH)) {
                Console.WriteLine(row);
                string[] values = row.Split(',');
                Console.WriteLine(values[RESERVATION_ID]);
                try {
                    lectureLastId = Int32.Parse(values[RESERVATION_ID]);
                } catch (Exception ex) {
                    Console.WriteLine("1234");
                }
            }

            var str = (++lectureLastId).ToString();
            str += "," + lecture.year;
            str += "," + lecture.semester;
            str += "," + lecture.dayOfWeek;
            str += "," + lecture.classtime;
            str += "," + lecture.classroom;
            str += "," + lecture.professor;
            str += "," + lecture.contact;
            str += "," + lecture.code;
            str += "," + lecture.name;
            str += Environment.NewLine;
            File.AppendAllText(RESERVATION_FILE_PATH, str);

            int statusLastId = -1;
            foreach (string row in File.ReadAllLines(STATUS_FILE_PATH)) {
                string[] values = row.Split(',');
                statusLastId = Int32.Parse(values[STATUS_ID]);
            }

            string[] inputDays = lecture.dayOfWeek.Split(';');
            string[] inputClasstimes = lecture.classtime.Split(';');
            string[] inputClassrooms = lecture.classroom.Split(';');

            /*
            if(!(count($input_days) == count($input_classtimes) &&
            count($input_days) == count($input_classrooms))) {
              echo("{\"res\":\"0\", \"msg\":\"input error\", \"query\":\"$sql\"}");
              die();
            }
            */

            int dayNum = inputDays.Length;
            var days = new Dictionary<DayOfWeek, string> {
                {DayOfWeek.Sunday, "일"},
                {DayOfWeek.Monday, "월"},
                {DayOfWeek.Tuesday, "화"},
                {DayOfWeek.Wednesday, "수"},
                {DayOfWeek.Thursday, "목"},
                {DayOfWeek.Friday, "금"},
                {DayOfWeek.Saturday, "토"},
            };
            int reservId = lectureLastId;
            int type = 0;
            string content = lecture.code + " - " + lecture.name;

            for (int i = 0; i < dayNum; i++) {
                string nowDay = inputDays[i];
                int startClass = Int32.Parse(inputClasstimes[i].Split('-')[0]);
                int endClass = Int32.Parse(inputClasstimes[i].Split('-')[1]);
                string nowClassroom = inputClassrooms[i];

                DateTime now = semesterStartDate;
                DateTime end = semesterEndDate;

                for (; now.Date <= end.Date; now = now.AddDays(1)) {
                    DayOfWeek dw = now.DayOfWeek;

                    if (nowDay.Equals(days[dw])) {
                        for (int time = startClass; time <= endClass; time++) {
                            string nowStr = now.ToString("yyyy-MM-dd");

                            str = (++statusLastId).ToString();
                            str += "," + lectureLastId;
                            str += "," + type;
                            str += "," + nowStr;
                            str += "," + time;
                            str += "," + nowClassroom;
                            str += "," + lecture.professor;
                            str += "," + lecture.contact;
                            str += "," + content;
                            str += Environment.NewLine;
                            File.AppendAllText(STATUS_FILE_PATH, str);
                        }
                    }
                }
            }
        }

        public void lectureDelete(int lectureID) {

        }


        public void reloadClassroomList() {
            classroomList = new List<string>();

            foreach (string row in File.ReadAllLines(CLASSROOM_FILE_PATH, Encoding.GetEncoding("euc-kr"))) {
                if (!row.Trim().Equals("")) {
                    classroomList.Add(row.Trim());
                }
            }

            classroomList.Sort();
        }

        public void classroomAdd(string classroom) {

        }

        public void classroomDelete(string classroom) {

        }

        public int GetRowByClassroom(string classroom) {
            if (classroomList == null)
                reloadClassroomList();

            for (int i = 0; i < classroomList.Count; i++) {
                if ((classroomList[i] as string).Equals(classroom))
                    return i;
            }
            return -1;
        }


        public void reloadClasstimeList() {
            classTimeTable = new Hashtable();
            int time = 1;

            foreach (string row in File.ReadAllLines(CLASSTIME_FILE_PATH, Encoding.GetEncoding("euc-kr"))) {
                if (!row.Trim().Equals("")) {
                    classTimeTable.Add(time++, row.Trim());
                }
            }
        }

        public void classtimeAdd(string classtime) {

        }

        public void classtimeModify(int time, string detail) {

        }

        public void classtimeDelete() {

        }

        private void createFileIfNotExist(string filePath) {
            if (!File.Exists(filePath)) {
                File.Create(filePath);
            }
        }
    }
}
