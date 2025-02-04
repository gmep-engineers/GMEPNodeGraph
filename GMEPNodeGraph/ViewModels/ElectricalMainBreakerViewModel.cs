using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalMainBreakerViewModel : DefaultNodeViewModel
  {
    public int NumPoles
    {
      get => _NumPoles;
      set => RaisePropertyChangedIfSet(ref _NumPoles, value);
    }
    int _NumPoles = 3;

    public bool HasGroundFaultProtection
    {
      get => _HasGroundFaultProtection;
      set => RaisePropertyChangedIfSet(ref _HasGroundFaultProtection, value);
    }
    bool _HasGroundFaultProtection = false;
    public bool HasSurgeProtection
    {
      get => _HasSurgeProtection;
      set => RaisePropertyChangedIfSet(ref _HasSurgeProtection, value);
    }
    bool _HasSurgeProtection = false;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalMainBreakerViewModel(
      string Id,
      string NodeId,
      string NodeParentId,
      int Amp,
      int NumPoles,
      bool HasGroundFaultProtection,
      bool HasSurgeProtection,
      string Status,
      Point Position
    )
    {
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      this.Amp = Amp;
      this.NumPoles = NumPoles;
      this.HasGroundFaultProtection = HasGroundFaultProtection;
      this.HasSurgeProtection = HasSurgeProtection;
      this.Status = ViewModels.Status.New;
      if (Status == "EXISTING")
      {
        this.Status = ViewModels.Status.Existing;
      }
      if (Status == "RELOCATED")
      {
        this.Status = ViewModels.Status.Relocated;
      }
      this.Position = Position;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Outputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }
}
