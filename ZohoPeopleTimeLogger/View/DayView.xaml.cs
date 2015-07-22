using System.Windows;
using System.Windows.Input;

namespace ZohoPeopleTimeLogger.View
{
    /// <summary>
    /// Interaction logic for DayView.xaml
    /// </summary>
    public partial class DayView
    {
        public DayView()
        {
            InitializeComponent();
        }

        private void DayViewOnMouseEnter(object sender, MouseEventArgs e)
        {
            DeleteButton.Visibility = Visibility.Visible;
        }

        private void DayViewOnMouseLeave(object sender, MouseEventArgs e)
        {
            DeleteButton.Visibility = Visibility.Hidden;
        }
    }
}
