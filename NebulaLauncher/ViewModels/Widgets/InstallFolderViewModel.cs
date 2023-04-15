using NebulaLauncher.Commands.GameApp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NebulaLauncher.ViewModels.Widgets
{
    public partial class InstallFolderViewModel : ViewModelBase
    {
        private string path;
        public string Path { get { return path; } set { path = value; } }

        private readonly InstallFolderSelectorViewModel Selector;

        private bool isSelected;
        public bool IsSelected { get { return isSelected; } set { isSelected = value; OnPropertyChanged(); } }

        private Visibility firstSeparatorVisibility;
        public Visibility FirstSeparatorVisibility { get { return firstSeparatorVisibility; } set { firstSeparatorVisibility = value; OnPropertyChanged(); } }

        private bool isNotDefault;
        public bool IsNotDefault { get { return isNotDefault; } set { isNotDefault = value; OnPropertyChanged(); } }

        public ICommand? SelectFolderCommand { get; }
        public ICommand? RemoveFolderCommand { get; }

        public InstallFolderViewModel(InstallFolderSelectorViewModel selector, string path, bool isDefault)
        {
            this.path = path;
            Selector = selector;

            firstSeparatorVisibility = isDefault ? Visibility.Visible : Visibility.Collapsed;
            IsNotDefault = !isDefault;
            isSelected = false;
            SelectFolderCommand = new RelayCommand(SelectFolder);
            RemoveFolderCommand = new RelayCommand(RemoveFolder);
        }

        public void Unselect()
        {
            IsSelected = false;
        }

        private void RemoveFolder()
        {
            if (IsNotDefault)
                Selector.RemoveFolder(this);
        }

        public void SelectFolder()
        {
            if (!IsSelected)
            {
                Selector.SelectFolder(this);
                IsSelected = true;
            }
        }
    }
}
