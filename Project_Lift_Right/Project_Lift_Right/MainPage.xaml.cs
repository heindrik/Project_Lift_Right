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
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            
        }
        
        private void profile_select_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        

        }

        private void new_user_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(NewUser),null);
        }

        private void sel_workout_btn_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SelectWorkout), null);
        }

        private void straight_to_workout_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(Workout), null);
        }
        private void straight_to_barbell_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(BarbellFrontRaise), null);
        }
    }
    //Hello
}
