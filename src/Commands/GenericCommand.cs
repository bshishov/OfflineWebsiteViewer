using System;
using System.Windows.Input;

namespace OfflineWebsiteViewer.Commands
{
    class GenericCommand<T> : ICommand
    {
        private readonly Action<T> _action;

        public GenericCommand(Action<T> action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return _action != null && parameter is T;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }

        public event EventHandler CanExecuteChanged;
    }

    class GenericCommand : ICommand
    {
        private readonly Action _action;

        public GenericCommand(Action action)
        {
            _action = action;
        }

        public bool CanExecute(object parameter)
        {
            return _action != null;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        public event EventHandler CanExecuteChanged;
    }
}