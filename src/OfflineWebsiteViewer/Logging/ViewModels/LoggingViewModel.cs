using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using OfflineWebsiteViewer.Annotations;
using OfflineWebsiteViewer.Logging.Views;

namespace OfflineWebsiteViewer.Logging.ViewModels
{
    class LoggingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static LoggingViewModel Instance;

        public ObservableCollection<ShoutingTarget> Targets { get; set; }


        private ShoutingTarget _active;
        public ShoutingTarget Active
        {
            get { return _active; }
            set
            {
                if (_active?.Messages != null)
                    _active.Messages.CollectionChanged -= OnCollectionChanged;
                _active = value;
                OnPropertyChanged(nameof(Active));
                _active.Messages.CollectionChanged += OnCollectionChanged;
            }
        }

        public LoggingViewModel()
        {
            LoggingViewModel.Instance = this;
            Targets = new ObservableCollection<ShoutingTarget>();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var listbox = LoggingView.Instance.MainListBox;
            if (listbox != null && VisualTreeHelper.GetChildrenCount(listbox) > 0)
            {
                var border = (Border)VisualTreeHelper.GetChild(listbox, 0);
                var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

        public void Register(ShoutingTarget target)
        {
            Targets.Add(target);
            if (Targets.Count == 1)
                Active = Targets.First();
            OnPropertyChanged(nameof(Targets));
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
