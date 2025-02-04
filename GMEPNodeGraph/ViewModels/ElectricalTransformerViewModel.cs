using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GMEPNodeGraph;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalTransformerViewModel : DefaultNodeViewModel
  {
    private int _distanceFromParent;
    private int _kva;
    private bool _powered;
    public ElectricalPanelViewModel ChildPanel
    {
      get => _ChildPanel;
      set => RaisePropertyChangedIfSet(ref _ChildPanel, value);
    }
    ElectricalPanelViewModel _ChildPanel = null;

    public ElectricalTransformerViewModel(
      string Id,
      string ProjectId,
      string ParentId,
      int distanceFromParent,
      string ColorCode,
      int Voltage,
      string name,
      int kva,
      bool powered,
      int circuitNo,
      bool IsHiddenOnPlan
    )
    {
      this.Id = Id;
      this.ProjectId = ProjectId;
      this.ColorCode = ColorCode;
      this.ParentId = ParentId;
      this.PhaseA = 0;
      this.PhaseB = 0;
      this.PhaseC = 0;
      this.Amp = 0;
      this.Name = name;
      this.CircuitNo = circuitNo;
      _distanceFromParent = distanceFromParent;
      this.Voltage = Voltage;
      _kva = kva;
      _powered = powered;
      _IsHiddenOnPlan = IsHiddenOnPlan;
    }

    public int Voltage
    {
      get => _Voltage;
      set => RaisePropertyChangedIfSet(ref _Voltage, value);
    }
    int _Voltage = 0;

    public int Kva
    {
      get => _Kva;
      set => RaisePropertyChangedIfSet(ref _Kva, value);
    }
    int _Kva = 0;

    public bool Powered
    {
      get => _Powered;
      set => RaisePropertyChangedIfSet(ref _Powered, value);
    }
    bool _Powered = false;

    public bool IsHiddenOnPlan
    {
      get => _IsHiddenOnPlan;
      set => RaisePropertyChangedIfSet(ref _IsHiddenOnPlan, value);
    }
    bool _IsHiddenOnPlan = false;

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

    public ElectricalTransformerViewModel(Point position)
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      PanelBusAmpVisible = Visibility.Visible;
      PanelMainAmpVisible = Visibility.Visible;
      PanelAmpLabelsVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      MloVisible = Visibility.Visible;
      Name = "Transfomer";
      Position = position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Inputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }
}
