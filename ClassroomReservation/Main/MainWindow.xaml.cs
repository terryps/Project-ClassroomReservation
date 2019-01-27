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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using System.IO;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ClassroomReservation.Reservation;
using ClassroomReservation.Server;
using System.Collections;
using ClassroomReservation.Resource;
using ClassroomReservation.Client;
using ClassroomReservation.Admin;
using ClassroomReservation.Other;

namespace ClassroomReservation.Main
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isUserMode = true;
        private int scrollSpeed = 7;

        private StatusItem nowSelectedItem;

        private SolidColorBrush backgroundEven = (SolidColorBrush)Application.Current.FindResource("BackgroundOfEvenRow");
        private SolidColorBrush backgroundOdd = (SolidColorBrush)Application.Current.FindResource("BackgroundOfOddRow");
        private SolidColorBrush labelBorderBrush = (SolidColorBrush)Application.Current.FindResource("MainColor");

        private DateTime displayDate = DateTime.Now;

        public MainWindow() {
            InitializeComponent();

            labelBorderBrush = new SolidColorBrush(labelBorderBrush.Color);
            labelBorderBrush.Opacity = 0.7;

            try {
                refresh();

                leftTopLogoHover.MouseLeftButtonDown += (o, s) => (new About()).ShowDialog();
                leftTopLogo.MouseEnter += (o, s) => leftTopLogo.Visibility = Visibility.Hidden;
                leftTopLogoHover.MouseLeave += (o, s) => leftTopLogo.Visibility = Visibility.Visible;
                MainWindow_DatePicker.SelectedDate = displayDate;
                MainWindow_DatePicker.SelectedDateChanged += DatePickerSelectedDateChanged;
                changePasswordButton.Click += new RoutedEventHandler(OnPasswordChangeButtonClicked);
                changeModeToUserModeButton.Click += OnChangeModeButtonClicked;
                ChangeModeButton.MouseLeftButtonDown += OnChangeModeButtonClicked;

                readExcelFileButton.Click += new RoutedEventHandler(OnExcelReadButtonClicked);
                halfYearDeleteButton.Content = String.Format("{0}년 {1}({2}월 ~ {3}월) DB 삭제",
                    DateTime.Today.Year,
                    ((DateTime.Today.Month <= 6) ? "상반기" : "하반기"),
                    ((DateTime.Today.Month <= 6) ? 1 : 7),
                    ((DateTime.Today.Month <= 6) ? 6 : 12));
                halfYearDeleteButton.Click += OnHalfYearDeleteButtonClicked;
                selectPeriodDeleteButton.Click += OnSelectPeriodDeleteButtonClicked;
                modifyClassroomButton.Click += OnModifyClassroomButtonClicked;
                modifyClasstimeButton.Click += OnModifyClasstimeButtonClicked;

                modifyReservationUserButton.Click += OnReservationModifyButtonClicked;
                deleteReservationUserButton.Click += OnReservationDeleteButtonClicked;
                
                reservateButton.Click += OnReservateButtonClicked;

                topLeftLogoResize();

                this.SizeChanged += (s, e) => topLeftLogoResize();
            } catch (ServerResult e) {
                MessageBox.Show("서버에 접속 할 수 없습니다.", "서버 접속 불가", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void refresh() {
            try {
                scrollViewContentPanel.Children.Clear();
                for (int i = 0; i < 7; i++) {
                    if (displayDate.AddDays(i).DayOfWeek != 0) {
                        var view = new ReservationStatusPerDay(displayDate.AddDays(i));
                        view.onOneSelected = onOneSelected;
                        if (i == 0) view.mainBorder.BorderThickness = new Thickness(0);
                        scrollViewContentPanel.Children.Add(view);
                    }
                }

                //remake leftLabelGrid
                leftLabelsGrid.Children.Clear();
                leftLabelsGrid.RowDefinitions.Clear();
                List<string> classroomList = ServerClient.getInstance().classroomList;
                Label nowBuildingLabel = null;
                for (int row = 0; row < classroomList.Count; row++) {

                    //Add RowDefinition
                    RowDefinition rowDef = new RowDefinition();
                    rowDef.Height = new GridLength(1, GridUnitType.Star);
                    leftLabelsGrid.RowDefinitions.Add(rowDef);

                    //Get building name and classroom name
                    string buildingName = (classroomList[row] as string).Split(':')[0];
                    string classroomName = (classroomList[row] as string).Split(':')[1];

                    //Add label to Grid
                    Label classroomLabel = new Label();
                    classroomLabel.Content = classroomName;
                    classroomLabel.Background = (row % 2 == 0) ? backgroundEven : backgroundOdd;
                    classroomLabel.HorizontalContentAlignment = HorizontalAlignment.Center;
                    classroomLabel.VerticalContentAlignment = VerticalAlignment.Center;

                    Grid.SetRow(classroomLabel, row);
                    Grid.SetColumn(classroomLabel, 1);

                    leftLabelsGrid.Children.Add(classroomLabel);

                    //Adjust building label
                    if (nowBuildingLabel != null && nowBuildingLabel.Content.Equals(buildingName)) {
                        Grid.SetRowSpan(nowBuildingLabel, Grid.GetRowSpan(nowBuildingLabel) + 1);
                    } else {
                        Label buildingLabel = new Label();
                        buildingLabel.Content = buildingName;
                        buildingLabel.Style = Resources["LabelStyle"] as Style;

                        Grid.SetRow(buildingLabel, row);
                        Grid.SetColumn(buildingLabel, 0);
                        Grid.SetRowSpan(buildingLabel, 1);

                        nowBuildingLabel = buildingLabel;
                        leftLabelsGrid.Children.Add(buildingLabel);
                    }
                }

                //remake reservationStatusControls
                List<StatusItem> items = ServerClient.getInstance().reservationListWeek(displayDate);
                var views = scrollViewContentPanel.Children;
                foreach (ReservationStatusPerDay view in views) {
                    view.clear();
                    view.rearrangeGrid();
                    foreach (StatusItem item in items) {
                        if (view.date.Day == item.date.Day) {
                            int row = Int32.Parse(item.classroom);
                            view.putData(row + 2, item.classtime - 1, item);
                        }
                    }
                }

                endDateLable.Content = " ~ " + Essential.dateTimeToString(displayDate.AddDays(6));
            } catch (Exception ex) {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void topLeftLogoResize() {
            double height1, height2;
            var ele1 = scrollViewContentPanel.Children.OfType<ReservationStatusPerDay>().First().DateTextBlock;
            var ele2 = topDockPanel;

            if (ele1.ActualHeight == 0) {
                ele1.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                height1 = ele1.DesiredSize.Height;
            } else {
                height1 = ele1.ActualHeight;
            }


            if (ele2.ActualHeight == 0) {
                ele2.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                height2 = ele2.DesiredSize.Height;
            } else {
                height2 = ele2.ActualHeight;
            }

            leftTopLayout.Height = height1 * 2 + height2 + 3;
        }


        private void OnPasswordChangeButtonClicked(object sender, RoutedEventArgs e)
        {
            PasswordForm signWin = new PasswordForm((window, password) => {
                LoginClient.getInstance().onChangeSuccess = (() => {
                    MessageBox.Show("성공적으로 변경 했습니다. 다시 로그인 해주세요.", "비밀번호 변경 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    changeMode(true);
                    window.Close();
                });
                LoginClient.getInstance().onChangeFailed = ((msg) => {
                    MessageBox.Show("알 수 없는 오류가 발생해서 변경에 실패 했습니다. - " + msg, "비밀번호 변경 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                });
                LoginClient.getInstance().ChangeAccount(password);
            });
            signWin.LoginButton.Content = "비밀번호 변경";
            signWin.ShowDialog();
        }

        private void OnChangeModeButtonClicked(object sender, RoutedEventArgs e) {
            if (isUserMode) {
                PasswordForm loginWin = new PasswordForm((window, password) => {
                    LoginClient.getInstance().onLoginSuccess = (() => {
                        changeMode(false);
                        window.Close();
                    });
                    LoginClient.getInstance().onPasswordWrong = (() => {
                        MessageBox.Show("비밀번호가 다릅니다.", "로그인 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                        window.passwordBox.Clear();
                    });
                    LoginClient.getInstance().onLoginError = ((msg) => {
                        MessageBox.Show("알 수 없는 오류가 발생 했습니다. 최초 비밀번호로 로그인 해주세요. - " + msg, "로그인 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                    LoginClient.getInstance().Login(password);
                });
                loginWin.LoginButton.Content = "로그인";
                loginWin.ShowDialog();
            } else
                changeMode(true);
        }

        private void changeMode(bool isUserMode)
        {
            this.isUserMode = isUserMode;

            if(isUserMode)
            {
                ChangeModeButton.Visibility = Visibility.Visible;
                changeModeToUserModeButton.Visibility = Visibility.Hidden;
                changePasswordButton.Visibility = Visibility.Hidden;
                AdminButtonPanel.Visibility = Visibility.Hidden;
                leftbottomLogoImage.Visibility = Visibility.Visible;
            } else {
                ChangeModeButton.Visibility = Visibility.Hidden;
                changeModeToUserModeButton.Visibility = Visibility.Visible;
                changePasswordButton.Visibility = Visibility.Visible;
                AdminButtonPanel.Visibility = Visibility.Visible;
                leftbottomLogoImage.Visibility = Visibility.Hidden;
            }
        }

        
        private void onOneSelected(StatusItem item) {
            if (item != null) {
                nowSelectedItem = item;
                
                userNameTextBox.Text = item.userName;
                contactTextBox.Text = item.contact;
                contentTextBox.Text = item.content;

                if (nowSelectedItem.type == StatusItem.RESERVATION_TYPE) {
                    userNameTextBox.IsReadOnly = false;
                    contactTextBox.IsReadOnly = false;
                    contentTextBox.IsReadOnly = false;
                } else {
                    userNameTextBox.IsReadOnly = true;
                    contactTextBox.IsReadOnly = true;
                    contentTextBox.IsReadOnly = true;
                }
            } else {
                userNameTextBox.Text = "";
                contactTextBox.Text = "";
                contentTextBox.Text = "";

                userNameTextBox.IsReadOnly = true;
                contactTextBox.IsReadOnly = true;
                contentTextBox.IsReadOnly = true;
            }
        }

        private void OnReservationModifyButtonClicked(object sender, RoutedEventArgs e) {
            try {
                if (isUserMode) {
                    if (nowSelectedItem.type == StatusItem.RESERVATION_TYPE) {
                        (new PasswordForm((form, password) => {
                            if (ServerClient.getInstance().reservationModify(nowSelectedItem.reservID, LoginClient.EncryptString(password), userNameTextBox.Text, contactTextBox.Text, contentTextBox.Text, isUserMode)) {
                                refresh();
                                form.Close();
                                MessageBox.Show("예약 정보 수정에 성공했습니다", "예약 수정 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                            } else {
                                MessageBox.Show("비밀번호가 틀렸습니다", "예약 수정 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        })).ShowDialog();
                    } else {
                        MessageBox.Show("강의 정보는 관리자만 수정할 수 있습니다", "예약 수정 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                } else {
                    if (nowSelectedItem.type == StatusItem.RESERVATION_TYPE) {
                        ServerClient.getInstance().reservationModify(nowSelectedItem.reservID, "", userNameTextBox.Text, contactTextBox.Text, contentTextBox.Text, isUserMode);
                        refresh();
                        MessageBox.Show("예약 정보 수정에 성공했습니다", "예약 수정 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    } else {
                        MessageBox.Show("강의는 수정 할 수 없습니다.", "강의 수정", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 예약 수정에 실패했습니다.", "예약 수정", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnReservationDeleteButtonClicked(object sender, RoutedEventArgs e) {
            try {
                if (isUserMode) {
                    if (nowSelectedItem.type == StatusItem.RESERVATION_TYPE) {
                        (new PasswordForm((form, password) => {
                            if (ServerClient.getInstance().reservationDeleteOne(nowSelectedItem.reservID, LoginClient.EncryptString(password), isUserMode)) {
                                refresh();
                                form.Close();
                                MessageBox.Show("예약 삭제에 성공했습니다", "예약 삭제 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                            } else {
                                MessageBox.Show("비밀번호가 틀렸습니다", "예약 삭제 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        })).ShowDialog();
                    } else {
                        MessageBox.Show("강의는 관리자만 삭제할 수 있습니다", "예약 삭제 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                } else {
                    if (nowSelectedItem.type == StatusItem.RESERVATION_TYPE) {
                        ServerClient.getInstance().reservationDeleteOne(nowSelectedItem.reservID, "", isUserMode);
                        refresh();
                        MessageBox.Show("예약 삭제에 성공했습니다", "예약 삭제 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    } else {
                        ServerClient.getInstance().lectureDelete(nowSelectedItem.reservID);
                        refresh();
                        MessageBox.Show("강의 삭제에 성공했습니다", "강의 삭제 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            } catch (ServerResult ex) {
                MessageBox.Show("알 수 없는 오류가 발생해서 삭제에 실패했습니다.", "삭제", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void OnExcelReadButtonClicked(object sender, RoutedEventArgs e) {
            LoadLectureTableWindow window = new LoadLectureTableWindow((date, items) => {
                List<LectureItem> fails = new List<LectureItem>();

                foreach (LectureItem item in items) {
                    try {
                        ServerClient.getInstance().lectureAdd(item, date);
                    } catch (ServerResult ex) {
                        fails.Add(item);
                    }
                }

                if (fails.Count == 0)
                    MessageBox.Show("강의 시간표 불러오기에 성공했습니다.", "강의 시간표 불러오기", MessageBoxButton.OK, MessageBoxImage.Information);
                else {
                    string msg = "강의 시간표 불러오기에 실패했습니다.";

                    foreach(LectureItem item in items) 
                        msg += ("\n" + item.code + " - " + item.name);

                    MessageBox.Show(msg, "강의 시간표 불러오기", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                refresh();
            });
            window.ShowInTaskbar = false;
            window.ShowDialog();
        }

        private void OnHalfYearDeleteButtonClicked(object sender, RoutedEventArgs e) {
            MessageBoxResult result = MessageBox.Show("정말로 삭제 하시겠습니까?\n모든 강의와 예약이 삭제 됩니다.", "학기 삭제 확인", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK) {
                try {
                    int year = DateTime.Today.Year;
                    DateTime startDate = (DateTime.Today.Month <= 6) ? new DateTime(year, 1, 1) : new DateTime(year, 7, 1);
                    DateTime endDate = (DateTime.Today.Month <= 6) ? new DateTime(year, 6, 30) : new DateTime(year, 12, 31);
                    ServerClient.getInstance().reservationDeletePeriod(startDate, endDate, true);
                    MessageBox.Show("삭제에 성공 했습니다.", "학기 삭제 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                } catch (Exception re) {
                    MessageBox.Show("알 수 없는 오류로 삭제에 실패 했습니다.", "학기 삭제 실패", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                refresh();
            }
        }

        private void OnSelectPeriodDeleteButtonClicked(object sender, RoutedEventArgs e) {
            SelectPeriodSelectWindow window = new SelectPeriodSelectWindow((start, end) => {
                MessageBoxResult result = MessageBox.Show("선택한 기간에 포함된 강의도 삭제 할까요?\n(강의가 삭제되면 선택되지 않았던 기간에서도 삭제 됩니다)", "기간 삭제 확인", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel) {
                    MessageBox.Show("취소되었습니다", "삭제 취소", MessageBoxButton.OK, MessageBoxImage.Information);
                } else {
                    try {
                        ServerClient.getInstance().reservationDeletePeriod(start, end, result == MessageBoxResult.Yes);
                        MessageBox.Show("삭제에 성공 했습니다.", "", MessageBoxButton.OK, MessageBoxImage.Information);
                        refresh();
                    } catch (ServerResult ex) {
                        MessageBox.Show("알 수 없는 오류로 삭제에 실패 했습니다.", "", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    refresh();
                }
            });
            window.ShowInTaskbar = false;
            window.ShowDialog();
        }

        private void OnModifyClassroomButtonClicked(object sender, RoutedEventArgs e) {
            ClassroomAddWindow window = new ClassroomAddWindow();
            window.onClassroomAdd = (classroom) => refresh();
            window.onClassroomDelete = (classroom) => refresh();
            window.ShowInTaskbar = false;
            window.ShowDialog();
        }

        private void OnModifyClasstimeButtonClicked(object sender, RoutedEventArgs e) {
            ClasstimeAddWindow window = new ClasstimeAddWindow();
            window.onClasstimeModified = () => refresh();
            window.onClasstimeAdded = () => refresh();
            window.onClasstimeDeleted = () => refresh();
            window.ShowInTaskbar = false;
            window.ShowDialog();
        }


        private void OnReservateButtonClicked(object sender, RoutedEventArgs e) {
            DateTime start = new DateTime(1970, 1, 1, 9, 0, 0);
            DateTime end = new DateTime(1970, 1, 1, 17, 30, 0);

#if DEBUG
            var isDebug = true;
#else
            var isDebug = false;
#endif

            if (isDebug || !isUserMode || (start.TimeOfDay <= DateTime.Now.TimeOfDay && DateTime.Now.TimeOfDay <= end.TimeOfDay)) {
                if (ReservationStatusPerDay.IsSelectedAlreadyOccupied()) {
                    MessageBox.Show("이미 예약 되어 있습니다", "예약 불가", MessageBoxButton.OK, MessageBoxImage.Warning);
                } else {
                    ReservationWindow window = new ReservationWindow(isUserMode);
                    window.onReservationSuccess = (item) => {
                        refresh();
                        if (item.classroom.Contains("과도관")) {
                            MessageBox.Show("예약에 성공했습니다\n꼭 예약 종이에 도장을 받아가 주세요", "예약 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                        } else {
                            MessageBox.Show("예약에 성공했습니다", "예약 성공", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    };
                    window.ShowInTaskbar = false;
                    window.ShowDialog();
                }
            } else {
                MessageBox.Show("강의실 예약은 아침 9시 부터 오후 5시 반 까지만 가능합니다.", "예약 불가", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            ScrollViewer scrollviewer = sender as ScrollViewer;
            for (int i = 0; i < scrollSpeed; i++) {
                if (e.Delta > 0)
                    scrollviewer.LineLeft();
                else
                    scrollviewer.LineRight();
            }
        }

        private void DatePickerSelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            var picker = sender as DatePicker;
            
            DateTime? date = picker.SelectedDate;
            if (date.HasValue) {
                displayDate = date.Value;
                refresh();
            }
        }
    }
}
