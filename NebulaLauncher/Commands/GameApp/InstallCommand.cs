using NebulaLauncher.ViewModels.Widgets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NebulaLauncher.Commands.GameApp
{
    public class InstallCommand : CommandBase
    {
        private readonly GameAppViewModel gameApp;

        public InstallCommand (GameAppViewModel GameApp)
        {
            gameApp = GameApp;
        }

        public override void Execute(object? parameter)
        {
            //GameApp.
        }

    }
}
