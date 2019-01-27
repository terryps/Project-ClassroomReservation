using ClassroomReservation.Resource;
using ClassroomReservation.Server;
using System;
using System.Collections;
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

    public delegate void OnClassroomAdd(string classroom);
    public delegate void OnClassroomDelete(string classroom);

    /// <summary>
    /// ClassroomAddWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClassroomAddWindow : Window {
        public OnClassroomAdd onClassroomAdd { private get; set; }
        public OnClassroomDelete onClassroomDelete { private get; set; }

        public ClassroomAddWindow() {
            InitializeComponent();

            listBox.SelectionChanged += OnItemSelected;
            addButton.Click += OnAddButtonClicked;
            deleteButton.Click += OnDeleteButtonClicked;

            refresh();
        }

        private void refresh() {
            try {
                ServerClient.getInstance().reloadClassroomList();
                List<string> classroomList = ServerClient.getInstance().classroomList;

                listBox.Items.Clear();

                for(int row = 0; row < classroomList.Count; row++)
                    listBox.Items.Add(classroomList[row]);
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의실 불러오기에 실패했습니다.", "강의실 불러오기", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnItemSelected(object sender, RoutedEventArgs e) {
            //ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
            //string content = item.Content.ToString();
            //inputBox.Text = content;
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e) {
            try {
                string classroom = inputBox.Text;
                if (classroom.Equals(""))
                    MessageBox.Show("강의실 이름을 입력해 주세요", "강의실 추가", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (!classroom.Contains(":"))
                    MessageBox.Show("건물과 강의실을 구분해주는 ':'을 써주세요", "강의실 추가", MessageBoxButton.OK, MessageBoxImage.Warning);
                else if (classroom.Split(':').Length - 1 > 1)
                    MessageBox.Show("건물과 강의실을 구분해주는 ':'은 한 개만 써주세요", "강의실 추가", MessageBoxButton.OK, MessageBoxImage.Warning);
                else {
                    ServerClient.getInstance().classroomAdd(classroom);
                    MessageBox.Show("강의실 추가에 성공했습니다.", "강의실 추가", MessageBoxButton.OK, MessageBoxImage.Information);
                    refresh();
                    onClassroomAdd?.Invoke(classroom);
                }
                inputBox.Text = "";
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의실 추가에 실패했습니다.", "강의실 추가", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteButtonClicked(object sender, RoutedEventArgs e) {
            try {
                ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;

                if (item == null) {
                    MessageBox.Show("강의실을 선택해 주세요", "강의실 삭제", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string classroom = item.Content.ToString();
                ServerClient.getInstance().classroomDelete(classroom);
                MessageBox.Show("강의실 삭제에 성공했습니다.", "강의실 삭제", MessageBoxButton.OK, MessageBoxImage.Information);
                refresh();
                onClassroomDelete?.Invoke(classroom);
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의실 삭제에 실패했습니다.", "강의실 삭제", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
