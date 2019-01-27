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
    public delegate void OnSelectPeriodDelete(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SelectPeriodSelectWindow : Window {
        private OnSelectPeriodDelete onSelectPeriodDelete;

        public SelectPeriodSelectWindow(OnSelectPeriodDelete onSelectPeriodDelete) {
            InitializeComponent();

            this.onSelectPeriodDelete = onSelectPeriodDelete;

            startDate.SelectedDate = DateTime.Now;
            endDate.SelectedDate = DateTime.Now;

            deleteButton.Click += new RoutedEventHandler((o,s) => {
                if (DateTime.Compare(startDate.SelectedDate.Value, endDate.SelectedDate.Value) <= 0) {
                    Close();
                    onSelectPeriodDelete?.Invoke(startDate.SelectedDate.Value, endDate.SelectedDate.Value);
                } else
                    MessageBox.Show("끝 날짜가 시작 날짜 보다 빠릅니다. 다시 선택해 주세요", "선택 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }
}
