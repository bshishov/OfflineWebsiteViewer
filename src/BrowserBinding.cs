using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OfflineWebsiteViewer
{
    class BrowserBinding
    {
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void Execute(string cmd, object arg=null)
        {
            if (_commands.ContainsKey(cmd))
            {
                Debug.WriteLine("CMD {0} with arg {1}", cmd);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _commands[cmd].Execute(arg);
                });
            }
        }
      
        internal void Bind(string name, ICommand command)
        {
            if (!string.IsNullOrEmpty(name) && command != null)
                _commands.Add(name, command);
        }
    }
}
