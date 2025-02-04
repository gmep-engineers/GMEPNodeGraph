using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Livet;
using NodeGraph.Utilities;

namespace GMEPNodeGraph.ViewModels
{
  public interface INodeViewModel
  {
    Guid Guid { get; set; }
    Point Position { get; set; }
    bool IsSelected { get; set; }
  }

  public enum Status
  {
    New,
    Existing,
    Relocated,
  }

  public abstract class DefaultNodeViewModel : ViewModel, INodeViewModel
  {
    public string Name
    {
      get => _Name;
      set => RaisePropertyChangedIfSet(ref _Name, value);
    }
    string _Name = string.Empty;
    public double Width
    {
      get => _Width;
      set => RaisePropertyChangedIfSet(ref _Width, value);
    }
    double _Width = 0;

    public double Height
    {
      get => _Height;
      set => RaisePropertyChangedIfSet(ref _Height, value);
    }
    double _Height = 0;

    public Guid Guid
    {
      get => _Guid;
      set => RaisePropertyChangedIfSet(ref _Guid, value);
    }
    Guid _Guid = Guid.NewGuid();

    public Point Position
    {
      get => _Position;
      set => RaisePropertyChangedIfSet(ref _Position, value);
    }
    Point _Position = new Point(0, 0);

    public bool IsSelected
    {
      get => _IsSelected;
      set => RaisePropertyChangedIfSet(ref _IsSelected, value);
    }
    bool _IsSelected = false;

    public Visibility VoltagePhaseVisible
    {
      get => _VoltagePhaseVisible;
      set => RaisePropertyChangedIfSet(ref _VoltagePhaseVisible, value);
    }
    Visibility _VoltagePhaseVisible = Visibility.Collapsed;

    public Visibility ServiceAmpVisible
    {
      get => _ServiceAmpVisible;
      set => RaisePropertyChangedIfSet(ref _ServiceAmpVisible, value);
    }
    Visibility _ServiceAmpVisible = Visibility.Collapsed;
    public Visibility PanelBusAmpVisible
    {
      get => _PanelBusAmpVisible;
      set => RaisePropertyChangedIfSet(ref _PanelBusAmpVisible, value);
    }
    Visibility _PanelBusAmpVisible = Visibility.Collapsed;
    public Visibility PanelMainAmpVisible
    {
      get => _PanelMainAmpVisible;
      set => RaisePropertyChangedIfSet(ref _PanelMainAmpVisible, value);
    }
    Visibility _PanelMainAmpVisible = Visibility.Collapsed;
    public Visibility CtsVisible
    {
      get => _CtsVisible;
      set => RaisePropertyChangedIfSet(ref _CtsVisible, value);
    }
    Visibility _CtsVisible = Visibility.Collapsed;

    public Visibility PanelAmpLabelsVisible
    {
      get => _PanelAmpLabelsVisible;
      set => RaisePropertyChangedIfSet(ref _PanelAmpLabelsVisible, value);
    }
    Visibility _PanelAmpLabelsVisible = Visibility.Collapsed;

    public Visibility MloVisible
    {
      get => _MloVisible;
      set => RaisePropertyChangedIfSet(ref _MloVisible, value);
    }
    Visibility _MloVisible = Visibility.Collapsed;

    public Visibility PoleVisible
    {
      get => _PoleVisible;
      set => RaisePropertyChangedIfSet(ref _PoleVisible, value);
    }
    Visibility _PoleVisible = Visibility.Collapsed;

    public string Id
    {
      get => _Id;
      set => RaisePropertyChangedIfSet(ref _Id, value);
    }
    string _Id = string.Empty;

    public string ParentId
    {
      get => _ParentId;
      set => RaisePropertyChangedIfSet(ref _ParentId, value);
    }
    string _ParentId = string.Empty;

    public Guid ParentGuid
    {
      get => _ParentGuid;
      set => RaisePropertyChangedIfSet(ref _ParentGuid, value);
    }
    Guid _ParentGuid = Guid.NewGuid();

    public float PhaseA
    {
      get => _PhaseA;
      set => RaisePropertyChangedIfSet(ref _PhaseA, value);
    }
    float _PhaseA = 0;

    public float PhaseB
    {
      get => _PhaseB;
      set => RaisePropertyChangedIfSet(ref _PhaseB, value);
    }
    float _PhaseB = 0;

    public float PhaseC
    {
      get => _PhaseC;
      set => RaisePropertyChangedIfSet(ref _PhaseC, value);
    }
    float _PhaseC = 0;

    public string ProjectId
    {
      get => _ProjectId;
      set => RaisePropertyChangedIfSet(ref _ProjectId, value);
    }
    string _ProjectId = string.Empty;

    public string ColorCode
    {
      get => _ColorCode;
      set => RaisePropertyChangedIfSet(ref _ColorCode, value);
    }
    string _ColorCode = string.Empty;

    public int CircuitNo
    {
      get => _CircuitNo;
      set => RaisePropertyChangedIfSet(ref _CircuitNo, value);
    }
    int _CircuitNo = 0;

    public virtual int Pole
    {
      get => _Pole;
      set => RaisePropertyChangedIfSet(ref _Pole, value);
    }
    int _Pole = 0;

    public int Amp
    {
      get => _Amp;
      set => RaisePropertyChangedIfSet(ref _Amp, value);
    }
    int _Amp = 0;

    public int ParentDistance
    {
      get => _ParentDistance;
      set => RaisePropertyChangedIfSet(ref _ParentDistance, value);
    }
    int _ParentDistance = 0;

    public Status Status
    {
      get => _Status;
      set => RaisePropertyChangedIfSet(ref _Status, value);
    }
    Status _Status = Status.New;

    public ICommand SizeChangedCommand => _SizeChangedCommand.Get(SizeChanged);
    ViewModelCommandHandler<Size> _SizeChangedCommand = new ViewModelCommandHandler<Size>();

    public ICommand TextBoxDoubleClickCommand => _TextBoxDoubleClickCommand.Get(TextBoxDoubleClick);
    ViewModelCommandHandler<TextBox> _TextBoxDoubleClickCommand =
      new ViewModelCommandHandler<TextBox>();

    private void TextBoxDoubleClick(TextBox textBox)
    {
      textBox.IsEnabled = true;
    }

    public abstract IEnumerable<NodeConnectorViewModel> Inputs { get; }
    public abstract IEnumerable<NodeConnectorViewModel> Outputs { get; }

    public abstract NodeConnectorViewModel FindConnector(Guid guid);

    void SizeChanged(Size newSize)
    {
      Width = newSize.Width;
      Height = newSize.Height;
    }
  }

  public class Meter : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public Meter(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      CtsVisible = Visibility.Visible;
      Name = "Meter";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      var input = Inputs.FirstOrDefault(arg => arg.Guid == guid);
      if (input != null)
      {
        return input;
      }

      var output = Outputs.FirstOrDefault(arg => arg.Guid == guid);
      return output;
    }
  }

  public class MainBreaker : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public MainBreaker(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      ServiceAmpVisible = Visibility.Visible;
      PoleVisible = Visibility.Visible;
      Name = "Main Breaker";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      var input = Inputs.FirstOrDefault(arg => arg.Guid == guid);
      if (input != null)
      {
        return input;
      }

      var output = Outputs.FirstOrDefault(arg => arg.Guid == guid);
      return output;
    }
  }

  public class ServiceFeeder : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ServiceFeeder(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      ServiceAmpVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      Name = "Service Feeder";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Outputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }

  public class DistributionBreaker : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public DistributionBreaker(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      PanelBusAmpVisible = Visibility.Visible;
      PoleVisible = Visibility.Visible;
      Name = "Distribution Breaker";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Inputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }

  public class Bus : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public Bus(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      ServiceAmpVisible = Visibility.Visible;
      Name = "Bus";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Inputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }
}
