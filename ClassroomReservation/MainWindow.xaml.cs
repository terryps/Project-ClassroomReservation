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

namespace ClassroomReservation
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isUserMode = true;

		DispatcherTimer animationTimer = new DispatcherTimer();
		private double reservationStatusPerDayWidth;
		double delta = 0;
		int deltaDirection = 1;
		double startPos;

		public MainWindow()
		{
			InitializeComponent();

			DateTime today = DateTime.Now;

			for (int i = 0; i < 7; i++)
			{
				if (today.AddDays(i).DayOfWeek != 0)
				{
					ReservationStatusPerDay fileInputBox1 = new ReservationStatusPerDay(today.AddDays(i));
					scrollViewContentPanel.Children.Add(fileInputBox1);
				}
			}
			
			MainWindow_DatePicker.SelectedDate = today;

			ChangeModeButton.Click += new RoutedEventHandler(changeMode);
			readExcelFileButton.Click += new RoutedEventHandler(readExcelFile);

			AdminButtonPanel.Visibility = System.Windows.Visibility.Hidden;

			animationTimer.Interval = new TimeSpan(120);
			animationTimer.Tick += new EventHandler(MyTimer_Tick);

			button4.Click += new RoutedEventHandler(Button_Click);
		}

        public void changeMode(object sender, RoutedEventArgs e)
        {
            isUserMode = !isUserMode;

            if(isUserMode)
            {
                AdminButtonPanel.Visibility = System.Windows.Visibility.Hidden;
            } else
            {
                AdminButtonPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }

        public static DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.RegisterAttached("HorizontalOffset",
                                                typeof(double),
                                                typeof(MainWindow),
                                                new UIPropertyMetadata(0.0, OnHorizontalOffsetChanged));

        private static void OnHorizontalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ScrollViewer scrollViewer = target as ScrollViewer;

            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
            }
        }

		void MyTimer_Tick(object sender, EventArgs e)
		{
			if(Math.Abs(delta) > reservationStatusPerDayWidth)
			{
				animationTimer.Stop();
			}
			else
			{
				delta += deltaDirection;
				ScrollViewer.ScrollToHorizontalOffset(startPos + delta);
			}
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
			ReservationStatusPerDay child = scrollViewContentPanel.Children.OfType<ReservationStatusPerDay>().FirstOrDefault();
			reservationStatusPerDayWidth = child.ActualWidth;

			animationTimer.Start();
			deltaDirection = (e.Delta < 0) ? 2 : -2;
			delta = 0;
			startPos = ScrollViewer.HorizontalOffset;

			e.Handled = true;
        }

        public void readExcelFile(object sender, RoutedEventArgs e)
        {
            //var fileName = string.Format("{0}\\fileNameHere", Directory.GetCurrentDirectory());
            //var connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", fileName);

            //var adapter = new OleDbDataAdapter("SELECT * FROM [workSheetNameHere$]", connectionString);
            //var ds = new DataSet();

            //adapter.Fill(ds, "anyNameHere");

            //DataTable data = ds.Tables["anyNameHere"];

            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".txt";
            dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                //fileOpenTextBox.Text = dlg.FileName;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ReservationWindow win2 = new ReservationWindow();
            win2.ShowDialog();
        }
    }
}
