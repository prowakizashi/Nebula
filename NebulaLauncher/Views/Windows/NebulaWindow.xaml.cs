using NebulaLauncher.ViewModels.Windows;
using System.Windows;

namespace NebulaLauncher.Views.Windows
{
    public partial class NebulaWindow : Window
    {
        public NebulaWindow()
        {
            var vm = new NebulaWindowViewModel()
            {
                CloseAction = CloseWindow,
                MinimizeAction = MinimizeWindow,
                MaximizeAction = MaximizeWindow
            };
            DataContext = vm;

            InitializeComponent();
        }

        private void CloseWindow()
        {
            Close();
        }

        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow()
        {
            WindowState ^= WindowState.Maximized;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            //var data = Model.Apps[0].GameData;
            //data.GameName = "BBB";
            //Model.Apps[0].GameData.GameName = "BBB";
        }
    }
}
