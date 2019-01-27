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

namespace ClassroomReservation.Admin {
    public delegate void OnClasstimeModified();
    public delegate void OnClasstimeAdded();
    public delegate void OnClasstimeDeleted();

    /// <summary>
    /// ClasstimeAddWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClasstimeAddWindow : Window {
        public OnClasstimeModified onClasstimeModified { private get; set; }
        public OnClasstimeAdded onClasstimeAdded { private get; set; }
        public OnClasstimeDeleted onClasstimeDeleted { private get; set; }

        public ClasstimeAddWindow() {
            InitializeComponent();

            listBox.SelectionChanged += OnItemSelected;
            modifyButton.Click += OnModifyButtonClicked;
            addButton.Click += OnAddButtonClicked;
            deleteButton.Click += OnDeleteButtonClicked;

            refresh();
        }

        private void refresh() {
            try {
                ServerClient.getInstance().reloadClasstimeList();
                Hashtable classtimeTable = ServerClient.getInstance().classTimeTable;

                listBox.Items.Clear();

                for (int row = 0; row < classtimeTable.Count; row++)
                    listBox.Items.Add((row + 1) + "교시 " + classtimeTable[row + 1]);

                addButton.Content = (classtimeTable.Count + 1) + "교시 추가하기";
                deleteButton.Content = (classtimeTable.Count) + "교시 삭제하기";
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의 시간표 불러오기에 실패했습니다.", "강의 시간표 불러오기", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnItemSelected(object sender, RoutedEventArgs e) {
            ListBoxItem item = listBox.ItemContainerGenerator.ContainerFromItem(listBox.SelectedItem) as ListBoxItem;
            if (item != null) {
                string content = item.Content.ToString();
                inputBox.Text = content.Substring(content.IndexOf(' ') + 1);
            } else {
                inputBox.Text = "";
            }
        }

        private void OnModifyButtonClicked(object sender, RoutedEventArgs e) {
            try {
                int time = listBox.SelectedIndex + 1;
                string detail = inputBox.Text;

                ServerClient.getInstance().classtimeModify(time, detail);
                refresh();
                MessageBox.Show("강의 시간표 수정에 성공했습니다.", "강의 시간표 수정", MessageBoxButton.OK, MessageBoxImage.Information);
                onClasstimeModified?.Invoke();
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의 시간표 수정에 실패했습니다.", "강의 시간표 수정", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnAddButtonClicked(object sender, RoutedEventArgs e) {
            try {
                ServerClient.getInstance().classtimeAdd("00:00AM ~ 99:99PM");
                refresh();
                MessageBox.Show("강의 시간표 추가에 성공했습니다.", "강의 시간표 추가", MessageBoxButton.OK, MessageBoxImage.Information);
                onClasstimeAdded?.Invoke();
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의 시간표 추가에 실패했습니다.", "강의 시간표 추가", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeleteButtonClicked(object sender, RoutedEventArgs e) {
            try {
                ServerClient.getInstance().classtimeDelete();
                refresh();
                MessageBox.Show("강의 시간표 삭제에 성공했습니다.", "강의 시간표 삭제", MessageBoxButton.OK, MessageBoxImage.Information);
                onClasstimeDeleted?.Invoke();
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 강의 시간표 삭제에 실패했습니다.", "강의 시간표 삭제", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
