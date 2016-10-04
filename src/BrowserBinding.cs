using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace OfflineWebsiteViewer
{
    class BrowserBinding
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void Execute(string cmd, object arg=null)
        {
            if (_commands.ContainsKey(cmd))
            {
                Logger.Trace($"Executing binding command '{cmd}' with arg '{arg}'");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _commands[cmd].Execute(arg);
                });
            }
        }
      
        internal void Bind(string name, ICommand command)
        {
            if (!string.IsNullOrEmpty(name) && command != null)
            {
                Logger.Trace($"Binding command '{command}' with name '{name}'");
                _commands.Add(name, command);
            }
        }
    }
}
