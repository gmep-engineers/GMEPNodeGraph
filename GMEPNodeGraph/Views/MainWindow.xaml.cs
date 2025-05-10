using System;
using System.Diagnostics;
using System.Windows;
using GMEPNodeGraph.ViewModels;

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
      string projectNo = string.Empty;
      string projectVersion = string.Empty;
      InitializeComponent();

      if (args.Length > 1)
      {
        // Get project ID from args
        projectNo = args[1];
        if (args.Length > 2)
        {
          projectVersion = args[2];
        }
        else
        {
          projectVersion = "latest";
        }

        var vm = (MainWindowViewModel)this.DataContext;
        vm.ProjectNo = projectNo;
        vm.LoadProjectCommand.Execute();
        vm.ProjectVersion = projectVersion;
        vm.LoadProjectNodesCommand.Execute();
      }
    }
  }
}
