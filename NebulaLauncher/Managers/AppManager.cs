using NebulaLauncher.Models;
using NebulaUtils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using static NebulaUtils.WebRequests;

namespace NebulaLauncher.Managers
{
    internal class AppManager
    {
        private static AppManager instance = new AppManager();
        public static AppManager Instance { get { return instance; } }

        private readonly Dictionary<string, GameData> games;
        public Dictionary<string, GameData> Games { get { return games; } }

        public Action? NeedsUpdate { get; set; }

        public AppManager()
        {
            games = LoadLocalGames();
        }

        public void RetrieveGamesFromServer()
        {
            MergeLocalAndOnlineGames(LoadOnlineGames());
            SaveGamesLocally();
        }

        private void MergeLocalAndOnlineGames(Dictionary<string, GameData> OnlineGames)
        {
            foreach (var oGamePair in OnlineGames)
            {
                GameData? lGame;
                if (Games.TryGetValue(oGamePair.Key, out lGame))
                {
                    lGame.MergeWithOnline(oGamePair.Value);
                }
                else
                {
                    Games.Add(oGamePair.Key, oGamePair.Value);
                }
            }

            List<string> toRemove = new List<string>();
            foreach (var lGamePair in Games)
            {
                if (!OnlineGames.ContainsKey(lGamePair.Key))
                {
                    if (lGamePair.Value.Installed)
                        lGamePair.Value.NoMoreOnServer = true;
                    else
                        toRemove.Add(lGamePair.Key);
                }
            }

            foreach (var gameId in toRemove)
                Games.Remove(gameId);
        }

        private Dictionary<string, GameData> LoadLocalGames()
        {
            if (!File.Exists("games.json"))
                return new Dictionary<string, GameData>();

            var fileContent = File.ReadAllText("games.json");
            var games = JsonConvert.DeserializeObject<List<GameData>>(fileContent);
            if (games == null)
                return new Dictionary<string, GameData>();
            foreach (var game in games)
            {
                if (game.Installed && !File.Exists(Path.Combine(game.InstallFolder!, "version.txt")))
                    game.Installed = false;
            }
            return games.ToDictionary(g => g.GameID!);
        }

        private Dictionary<string, GameData> LoadOnlineGames()
        {
            EDownloadResponse response;
            var fileContent = DownloadStringWithResponseSync(Url.Combine("http://192.168.0.3/", "Games.json"), out response);
            if (response != EDownloadResponse.SUCCESS || string.IsNullOrEmpty(fileContent)) // Todo: fetch some message errors ?
                return new Dictionary<string, GameData>();
            var games = JsonConvert.DeserializeObject<List<GameData>>(fileContent)!.ToArray();
            return games.ToDictionary(g => g.GameID);
        }

        public void SaveGamesLocally()
        {
            foreach (var game in Games.Values)
            {
                if (game.NoMoreOnServer && !game.Installed)
                    Games.Remove(game.GameID);
            }

            using (StreamWriter sw = File.CreateText("games.json"))
            {
                sw.Write(JsonConvert.SerializeObject(Games.Values.ToList()));
            }
            NeedsUpdate?.Invoke();
        }
    }
}
