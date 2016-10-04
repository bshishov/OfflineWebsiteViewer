using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NLog;
using NLog.Targets;
using OfflineWebsiteViewer.Logging.ViewModels;
using OfflineWebsiteViewer.Logging.Views;

namespace OfflineWebsiteViewer.Logging
{
    [Target("ShoutingTarget")]
    public class ShoutingTarget : TargetWithLayout
    {
        private readonly Action<string> _addmethod;
        public ObservableCollection<string> Messages { get; set; }


        public ShoutingTarget()
        {
            Messages = new ObservableCollection<string>();
            _addmethod = Messages.Add;
        }

        protected override void Write(LogEventInfo logEvent)
        {
            Application.Current.Dispatcher.BeginInvoke(_addmethod, this.Layout.Render(logEvent));
        }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            (LoggingView.Instance?.DataContext as LoggingViewModel)?.Register(this);
        }
    }
}
