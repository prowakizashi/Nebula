using NebulaLauncher.Managers;
using NebulaLauncher.ViewModels.Widgets;
using System;
using System.Collections.ObjectModel;

namespace NebulaLauncher.ViewModels.Windows
{
    public class NebulaWindowViewModel : WindowViewModelBase
    {
        public ObservableCollection<GameAppViewModel> Games { get; set; }

        public NebulaWindowViewModel() : base()
        {
            AppManager.Instance.RetrieveGamesFromServer();
            AppManager.Instance.NeedsUpdate = Update;

            var tmpGames = new ObservableCollection<GameAppViewModel>();
            foreach (var game in AppManager.Instance.Games)
                tmpGames.Add(new GameAppViewModel(game.Value));
            Games = tmpGames;
        }

        private void Update()
        {
            Games = new ObservableCollection<GameAppViewModel>(Games);
            OnPropertyChanged("Games");
        }
    }
}
