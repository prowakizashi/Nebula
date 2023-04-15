using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaLauncher.Models
{
    public class GameData
    {
        public string GameID { get; set; }
        public string? GameName { get; set; }
        public string? ExecFile { get; set; }
        public bool Installed { get; set; }
        public string? InstallFolder { get; set; }
        public bool Favorite { get; set; }
        public bool NoMoreOnServer { get; set; }

        public GameData() { GameID = "InvalidID"; }

        public GameData MergeWithOnline(GameData OnlineData)
        {
            GameName = OnlineData.GameName;
            NoMoreOnServer = false;
            return this;
        }
    }
}
