using System;
using System.Windows.Input;

namespace OfflineWebsiteViewer.Utility
{
    class GenericCommand<T> : ICommand
    {
        private readonly Action<T> _action;
        private readonly Func<T, bool> _canExecute;

        public GenericCommand(Action<T> action, Func<T, bool> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_action == null)
                return false;

            if (_canExecute != null && parameter is T)
                return _canExecute((T) parameter);
            
            return true;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }

    class GenericCommand : ICommand
    {
        private readonly Action _action;
        private readonly Func<bool> _canExecute;

        public GenericCommand(Action action, Func<bool> canExecute = null)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            if (_action != null)
            {
                if (_canExecute != null)
                    return _canExecute();
                return true;
            }
            return false;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }

        public event EventHandler CanExecuteChanged;


        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}