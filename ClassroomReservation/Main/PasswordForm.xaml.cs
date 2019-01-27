using System;
using System.Collections.Generic;
using System.IO;
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
using System.Security.Cryptography;
using System.Reflection;
using ClassroomReservation.Other;

namespace ClassroomReservation.Main
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>

    public delegate void OnPasswordInput(PasswordForm form, string password);

    public partial class PasswordForm : Window
    {
        private OnPasswordInput callback;

        public PasswordForm(OnPasswordInput func)
        {
            InitializeComponent();
            this.ShowInTaskbar = false;

            this.callback = func;

            passwordBox.KeyDown += OnKeyDownHandler;
            passwordBox.Focus();

            LoginButton.Click += new RoutedEventHandler((sender, e) => {
                if (Essential.hasKorean(passwordBox.Password))
                    MessageBox.Show("비밀번호에 한글을 넣을 수 없습니다", "", MessageBoxButton.OK, MessageBoxImage.Warning);
                else {
                    callback?.Invoke(this, passwordBox.Password);
                }
            });
        }

        private void OnKeyDownHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Return) {
                callback?.Invoke(this, passwordBox.Password);
            }
        }
    }
}
