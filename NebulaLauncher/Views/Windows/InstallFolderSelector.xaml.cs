using NebulaLauncher.ViewModels.Widgets;
using System.Windows;

namespace NebulaLauncher.Views.Windows
{
    public partial class InstallFolderSelector : Window
    {
        public InstallFolderSelector()
        {
            var vm = new InstallFolderSelectorViewModel()
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
    }
}
