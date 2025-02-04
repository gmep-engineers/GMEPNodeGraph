using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GMEPNodeGraph.ViewModels;
using GMEPNodeGraph.Views;

namespace GMEPNodeGraph.Views
{
  /// <summary>
  /// MainWindow.xaml の相互作用ロジック
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      string[] args = Environment.GetCommandLineArgs();
      string projectId = string.Empty;
      if (args.Length > 1)
      {
        // Get project ID from args
        projectId = args[1];

        // Load project nodes from DB

        // If nodes, load nodes
        var vm = (MainWindowViewModel)this.DataContext;
        vm.ProjectId = projectId;
      }
      InitializeComponent();
    }
  }
}
