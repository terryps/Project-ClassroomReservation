using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomReservation.Server {
    public class StatusItem {
        public const int LECTURE_TYPE = 0;
        public const int RESERVATION_TYPE = 1;

        public int reservID { get; private set; }
        public int type { get; private set; }
        public DateTime date { get; private set; }
        public int classtime { get; private set; }
        public string classroom { get; private set; }
        public string userName { get; private set; }
        public string contact { get; private set; }
        public string content { get; private set; }

        public StatusItem(int reservID, int type, DateTime date, int classtime, string classroom, string userName, string contact, string content) {
            this.reservID = reservID;
            this.type = type;
            this.date = date;
            this.classtime = classtime;
            this.classroom = classroom;
            this.userName = userName;
            this.contact = contact;
            this.content = content;
        }
    }
}
