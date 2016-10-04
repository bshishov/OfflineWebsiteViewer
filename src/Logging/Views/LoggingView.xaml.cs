using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OfflineWebsiteViewer.Logging.ViewModels;

namespace OfflineWebsiteViewer.Logging.Views
{
    /// <summary>
    /// Interaction logic for LoggingView.xaml
    /// </summary>
    public partial class LoggingView : Window
    {
        public static LoggingView Instance = new LoggingView();

        public LoggingView()
        {
            InitializeComponent();
            this.DataContext = new LoggingViewModel();
        }
    }
}
