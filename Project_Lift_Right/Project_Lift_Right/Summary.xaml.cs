using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Project_Lift_Right
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Summary : Page
    {


        public Summary()
        {
            this.InitializeComponent();
        }

        private void menu_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), null);
        }

        private void restart_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(WorkoutSetup), null);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PassedData data = e.Parameter as PassedData;
            int success_ = data.left_count + data.right_count;
            int failed_ = data.left_failed_count + data.right_failed_count;

            float result_;
            if ((success_ + failed_) == 0)
                result_ = 0;
            else
                result_ = (success_ *100) / (success_ + failed_);
            
            workout.Text = data.Name;
            success.Text = success_.ToString();
            failed.Text = failed_.ToString();
            result.Text = result_.ToString() + "%";

            if (result_ < 49)
                grade.Text = "F";
            else if (result_ < 52)
                grade.Text = "D-";
            else if (result_ < 56)
                grade.Text = "D";
            else if (result_ < 59)
                grade.Text = "D+";
            else if (result_ < 62)
                grade.Text = "C-";
            else if (result_ < 66)
                grade.Text = "C";
            else if (result_ < 69)
                grade.Text = "C+";
            else if (result_ < 72)
                grade.Text = "B-";
            else if (result_ < 76)
                grade.Text = "B";
            else if (result_ < 79)
                grade.Text = "B+";
            else if (result_ < 84)
                grade.Text = "A-";
            else if (result_ < 89)
                grade.Text = "A";
            else
                grade.Text = "A+";
        }
    }
}
