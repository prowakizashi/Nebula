using NebulaLauncher.Managers;
using System;
using System.Windows.Input;

namespace NebulaLauncher.ViewModels
{
    public abstract class WindowViewModelBase : ViewModelBase
    {
        public ICommand MinimizeCommand { get; set; }
        public ICommand MaximizeCommand { get; set; }
        public ICommand CloseCommand { get; set; }

        public Action? CloseAction;
        public Action? MinimizeAction;
        public Action? MaximizeAction;

        public WindowViewModelBase()
        {
            MinimizeCommand = new RelayCommand(() => {
                MinimizeAction?.Invoke();
                });
            MaximizeCommand = new RelayCommand(() => MaximizeAction?.Invoke());
            CloseCommand = new RelayCommand(() => {
                CloseAction?.Invoke();
                });
        }
    }
}
