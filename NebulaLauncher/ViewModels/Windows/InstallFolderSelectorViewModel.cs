using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using WinForms = System.Windows.Forms;

namespace NebulaLauncher.ViewModels.Widgets
{
    public partial class InstallFolderSelectorViewModel : WindowViewModelBase
    {
        public bool Success { get; private set; }
        public InstallFolderViewModel? SelectedFolder { get; private set; }

        private ObservableCollection<InstallFolderViewModel> folders;
        public ObservableCollection<InstallFolderViewModel> Folders { get { return folders; } set { folders = value; OnPropertyChanged(); }  }

        public ICommand AddFolderCommand { get; set; }
        public ICommand ValidateCommand { get; set; }

        public InstallFolderSelectorViewModel() : base()
        {
            folders = new ObservableCollection<InstallFolderViewModel>();

            AddFolderCommand = new RelayCommand(() => AddFolder());
            ValidateCommand = new RelayCommand(() => Validate());

            LoadFolders();
        }

        private void LoadFolders()
        {
            List<string> paths = new List<string>();
            var defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games");
            if (!Directory.Exists(defaultPath))
                Directory.CreateDirectory(defaultPath);

            if (File.Exists("install_folders.json"))
            {
                var fileContent = File.ReadAllText("install_folders.json");
                paths = JsonConvert.DeserializeObject<List<string>>(fileContent) ?? paths;
            }

            // force default to be first in list
            paths.Remove(defaultPath);
            paths.Insert(0, defaultPath);

            bool isDefault = true;
            foreach (var path in paths)
            {
                var folder = new InstallFolderViewModel(this, path, isDefault);
                Folders.Add(folder);
                if (isDefault)
                    folder.SelectFolder();
                isDefault = false;
            }
        }

        private JsonConverter[] JsonSerializerSettings()
        {
            throw new NotImplementedException();
        }

        private void SaveFolders()
        {
            List<string> paths = new List<string>();
            foreach (var folder in Folders)
            {
                paths.Add(folder.Path!);
            }

            using (StreamWriter sw = File.CreateText("install_folders.json"))
            {
                sw.Write(JsonConvert.SerializeObject(paths));
            }
        }

        public void RemoveFolder(InstallFolderViewModel folder)
        {
            if (folder.IsSelected && Folders.Count > 0)
                Folders[0].SelectFolder();
            Folders.Remove(folder);
            SaveFolders();
        }

        public void AddFolder()
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.SelectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Games");
            WinForms.DialogResult result = dialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                Folders.Add(new InstallFolderViewModel(this, dialog.SelectedPath, false));
                SaveFolders();
            }

        }

        public void SelectFolder(InstallFolderViewModel Folder)
        {
            if (Folders.Contains(Folder) && SelectedFolder != Folder)
            {
                if (SelectedFolder != null)
                    SelectedFolder.Unselect();

                SelectedFolder = Folder;
            }
        }

        private void Validate()
        {
            Success = SelectedFolder != null;
            CloseAction?.Invoke();
        }
    }
}
