using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using NLog;

namespace OfflineWebsiteViewer.Browser
{
    class BrowserBinding
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();
        private readonly Dictionary<string, Func<object,object>> _actions = new Dictionary<string, Func<object, object>>();

        public object Execute(string cmd, object arg=null)
        {
            if (_commands.ContainsKey(cmd))
            {
                Logger.Trace($"Executing binding command '{cmd}' with arg '{arg}'");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _commands[cmd].Execute(arg);
                });
                return null;
            }

            if(_actions.ContainsKey(cmd))
            {
                Logger.Trace($"Executing binding delegate '{cmd}' with arg '{arg}'");
                return _actions[cmd].Invoke(arg);
            }

            return null;
        }
      
        internal void Bind(string name, ICommand command)
        {
            if (!string.IsNullOrEmpty(name) && command != null)
            {
                Logger.Trace($"Binding command '{command}' with name '{name}'");
                _commands.Add(name, command);
            }
        }

        internal void Bind(string name, Func<object, object> action)
        {
            if (!string.IsNullOrEmpty(name) && action != null)
            {
                Logger.Trace($"Binding delegate '{action}' with name '{name}'");
                _actions.Add(name, action);
            }
        }
    }
}
