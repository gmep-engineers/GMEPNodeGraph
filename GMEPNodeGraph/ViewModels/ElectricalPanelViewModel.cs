using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using GMEPNodeGraph;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalPanelViewModel : DefaultNodeViewModel
  {
    public string VoltageType
    {
      get => _VoltageType;
      set => RaisePropertyChangedIfSet(ref _VoltageType, value);
    }
    string _VoltageType = "120/208 3Φ";

    public bool IsMlo
    {
      get => _IsMlo;
      set => RaisePropertyChangedIfSet(ref _IsMlo, value);
    }
    bool _IsMlo = true;

    public int BusAmp
    {
      get => _BusAmp;
      set => RaisePropertyChangedIfSet(ref _BusAmp, value);
    }
    int _BusAmp = 0;

    public int MainAmp
    {
      get => _MainAmp;
      set => RaisePropertyChangedIfSet(ref _MainAmp, value);
    }
    int _MainAmp = 0;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalPanelViewModel(
      string Id,
      string NodeId,
      string NodeParentId,
      string Name,
      string VoltageType,
      int BusAmp,
      int MainAmp,
      string ColorCode,
      bool IsMlo,
      string Status,
      Point Position
    )
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      PanelBusAmpVisible = Visibility.Visible;
      PanelMainAmpVisible = Visibility.Visible;
      PanelAmpLabelsVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      MloVisible = Visibility.Visible;

      this.Name = Name;
      this.Position = Position;
      this.IsMlo = IsMlo;
      this.BusAmp = BusAmp;
      this.ColorCode = ColorCode;
      this.VoltageType = VoltageType;
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      ParentGuid = Guid.Parse(NodeParentId);
      this.MainAmp = MainAmp;
      this.Status = ViewModels.Status.New;
      if (Status == "EXISTING")
      {
        this.Status = ViewModels.Status.Existing;
      }
      if (Status == "RELOCATED")
      {
        this.Status = ViewModels.Status.Relocated;
      }
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Inputs.FirstOrDefault(arg => arg.Guid == guid);
    }

    private int _mainSize;
    private bool _isMlo;
    private int _distanceFromParent;
    private int _aicRating;

    private int _type;
    private bool _powered;
    private bool _isHiddenOnPlan;

    public ObservableCollection<DefaultNodeViewModel> leftComponents { get; set; } =
      new ObservableCollection<DefaultNodeViewModel>();
    public ObservableCollection<DefaultNodeViewModel> rightComponents { get; set; } =
      new ObservableCollection<DefaultNodeViewModel>();
  }
}
