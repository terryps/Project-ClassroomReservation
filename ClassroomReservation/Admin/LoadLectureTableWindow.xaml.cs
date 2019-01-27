using ClassroomReservation.Resource;
using ClassroomReservation.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClassroomReservation.Main {

    public delegate void OnLectureAdd(DateTime semesterStartDate, List<LectureItem> items);

    /// <summary>
    /// LoadLectureTableWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class LoadLectureTableWindow : Window {
        private OnLectureAdd onLectureAddSuccess;
        private List<LectureItem> items;
        private int semester;

        public LoadLectureTableWindow(OnLectureAdd onLectureAddSuccess) {
            InitializeComponent();

            this.onLectureAddSuccess = onLectureAddSuccess;

            datePicker.SelectedDate = DateTime.Today;

            excelSearchButton.Click += new RoutedEventHandler(processExcel);
            processButton.Click += new RoutedEventHandler(MakeLecture);
        }

        private void processExcel(object sender, RoutedEventArgs e) {
            ExcelReadClient.onFileSelected = (fileName) => excelFileNameText.Text = fileName;
            items = ExcelReadClient.readExcel();

            if (items == null) {
                Close();
            }
        }

        private void MakeLecture(object sender, RoutedEventArgs e) {
            if (datePicker.SelectedDate.HasValue) {
                DateTime start = datePicker.SelectedDate.Value;

                if (start.Month != 3 && start.Month != 9) {
                    MessageBoxResult r = MessageBox.Show("개강 날짜가 3월 또는 9월이 아닙니다. 진행하시겠습니까?", "개강 날짜 주의", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                    if (r != MessageBoxResult.OK) {
                        return;
                    }
                }
                
                Close();
                onLectureAddSuccess?.Invoke(start, items);
            }
        }
    }
}

                