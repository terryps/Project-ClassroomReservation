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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClassroomReservation.Reservation {
    public delegate void OnClassroomSelectChanged(string nowSelectedClassroom, bool isDataChanged);

    /// <summary>
    /// TimeSelectControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClassroomSelectControl : UserControl {
        public OnClassroomSelectChanged onClassroomSelectChanged { set; private get; }

        private int TOTAL_NUM;

        private ClassroomLabel[] buttons;
        private Rectangle[] overlaps;

        private SolidColorBrush selectedColor = (SolidColorBrush)Application.Current.FindResource("SelectedColorInReservationWindow");
        private SolidColorBrush hoverColor = (SolidColorBrush)Application.Current.FindResource("HoverColor");
        private SolidColorBrush backgroundEven = (SolidColorBrush)Application.Current.FindResource("BackgroundOfEvenRow");
        private SolidColorBrush backgroundOdd = (SolidColorBrush)Application.Current.FindResource("BackgroundOfOddRow");
        private SolidColorBrush disableOverlap = (SolidColorBrush)Application.Current.FindResource("DisableOverlap");

        private Label beforeSelected;
        private ClassroomLabel nowSelected;
        private bool mouseLeftButtonDown = false;
        private int previousColor = -1;

        public ClassroomSelectControl() {
            InitializeComponent();

            List<string> classroomList = ServerClient.getInstance().classroomList;
            TOTAL_NUM = classroomList.Count;
            buttons = new ClassroomLabel[TOTAL_NUM];
            overlaps = new Rectangle[TOTAL_NUM];
            Border nowBuildingLabelBorder = null;
            int buildingLabelOrder = 0;
            for (int row = 0; row < TOTAL_NUM; row++) {

                //Add RowDefinition
                RowDefinition rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                mainGrid.RowDefinitions.Add(rowDef);

                //Get building name and classroom name
                string buildingName = (classroomList[row] as string).Split(':')[0];
                string classroomName = (classroomList[row] as string).Split(':')[1];

                //Add label to Grid
                ClassroomLabel classroomLabel = new ClassroomLabel(buildingName, classroomName);
                classroomLabel.Content = classroomName;
                classroomLabel.MouseLeftButtonDown += new MouseButtonEventHandler(OnMouseLeftButtonDown);
                classroomLabel.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
                classroomLabel.MouseEnter += new MouseEventHandler(OnMouseEnter);
                classroomLabel.MouseLeave += new MouseEventHandler(OnMouseLeave);

                Grid.SetRow(classroomLabel, row);
                Grid.SetColumn(classroomLabel, 1);

                buttons[row] = classroomLabel;
                mainGrid.Children.Add(classroomLabel);

                //Adjust building label
                if (nowBuildingLabelBorder != null && (nowBuildingLabelBorder.Child as Label).Content.Equals(buildingName)) {
                    Grid.SetRowSpan(nowBuildingLabelBorder, Grid.GetRowSpan(nowBuildingLabelBorder) + 1);
                } else {
                    Label buildingLabel = new Label();
                    buildingLabel.Content = buildingName;
                    buildingLabel.Style = Resources["buildingLableStyle"] as Style;
                    buildingLabel.Background = (buildingLabelOrder++ % 2 == 0) ? backgroundEven : backgroundOdd;
                    
                    Border border = new Border();
                    border.Style = Resources["buildingBorderStyle"] as Style;
                    border.Child = buildingLabel;

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, 0);
                    Grid.SetRowSpan(border, 1);

                    nowBuildingLabelBorder = border;
                    mainGrid.Children.Add(border);
                }

                Rectangle overlap = new Rectangle();

                Grid.SetRow(overlap, row);
                Grid.SetColumn(overlap, 1);

                overlaps[row] = overlap;
                mainGrid.Children.Add(overlap);
            }

            ResetBackground();
        }


        public void enable(bool enable) {
            foreach (Rectangle overlap in overlaps)
                overlap.Visibility = (enable) ? Visibility.Hidden : Visibility.Visible;
        }

        public void ResetSelection() {
            beforeSelected = null;
            nowSelected = null;
            ResetBackground();
        }

        public bool HasSelectedClassroom() {
            return nowSelected != null;
        }

        public string GetSelectedClassroom() {
            return nowSelected.GetFullName();
        }

        public void SetSelectedClassroom(int classroomRow) {
            enable(true);
            beforeSelected = new ClassroomLabel("","");
            nowSelected = buttons[classroomRow];
            nowSelected.Background = selectedColor;
        }

        public void selectiveEnable(bool[] list) {
            for (int i = 0; i < list.Length; i++) {
                if (list[i])
                    overlaps[i].Visibility = Visibility.Hidden;
                else
                    overlaps[i].Visibility = Visibility.Visible;

            }
        }


        private void OnMouseLeftButtonDown(object sender, RoutedEventArgs e) {
            nowSelected = sender as ClassroomLabel;

            ResetBackground();

            nowSelected.Background = selectedColor;
            previousColor = -1;
            mouseLeftButtonDown = true;
        }

        private void OnMouseLeftButtonUp(object sender, RoutedEventArgs e) {
            mouseLeftButtonDown = false;

            if(beforeSelected != nowSelected)
                onClassroomSelectChanged?.Invoke(nowSelected.GetFullName(), beforeSelected != null);

            beforeSelected = nowSelected;
        }

        private void OnMouseEnter(object sender, RoutedEventArgs e) {
            if (!mouseLeftButtonDown) {
                if (previousColor >= 0 && previousColor % 2 == 0 && buttons[previousColor].Background != selectedColor) {
                    buttons[previousColor].Background = backgroundOdd;
                } else if (previousColor >= 0 && previousColor % 2 == 1 && buttons[previousColor].Background != selectedColor) {
                    buttons[previousColor].Background = backgroundEven;
                }

                previousColor = Grid.GetRow(sender as Label);
                if (buttons[previousColor].Background != selectedColor)
                    buttons[previousColor].Background = hoverColor;
            }
        }

        private void OnMouseLeave(object sender, RoutedEventArgs e) {
            for (int i = 0; i < TOTAL_NUM; i++) {
                if (buttons[i].Background == selectedColor)
                    buttons[i].Background = selectedColor;
                else if (i % 2 == 0)
                    buttons[i].Background = backgroundOdd;
                else if (i % 2 == 1)
                    buttons[i].Background = backgroundEven;
            }
        }

        private void ResetBackground() {
            for (int i = 0; i < TOTAL_NUM; i++) {
                if (i % 2 == 0) {
                    buttons[i].Background = backgroundOdd;
                } else {
                    buttons[i].Background = backgroundEven;
                }
            }
        }
    }

    class ClassroomLabel : Label {
        public string buildingName { get; private set; }
        public string classroomName { get; private set; }

        public ClassroomLabel(string buildingName, string classroomName) {
            this.buildingName = buildingName;
            this.classroomName = classroomName;
        }

        public string GetFullName() {
            return buildingName + ":" + classroomName;
        }
    }
}
