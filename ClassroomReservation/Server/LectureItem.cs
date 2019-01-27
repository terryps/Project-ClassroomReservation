using ClassroomReservation.Other;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomReservation.Server {
    public class LectureItem {
        public int year { get; private set; }
        public int semester { get; private set; }
        public string dayOfWeek { get; private set; }
        public string classtime { get; private set; }
        public string classroom { get; private set; }
        public string professor { get; private set; }
        public string contact { get; private set; }
        public string code { get; private set; }
        public string name { get; private set; }

        public LectureItem(int year, int semester, string dayOfWeek, string classtime, string classroom, string professor, string contact, string code, string name) {
            this.year = year;
            this.semester = semester;
            this.dayOfWeek = dayOfWeek;
            this.classtime = classtime;
            this.classroom = classroom;
            this.professor = professor;
            this.contact = contact;
            this.code = code;
            this.name = name;
        }

        public override string ToString() {
            return String.Format("날짜 : {0}년 {1}학기\n교수 : {5}\n강의 : {6}\n",
                year,
                semester,
                professor,
                name
            );
        }
    }
}
