using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalMeterViewModel : DefaultNodeViewModel
  {
    public bool HasCts
    {
      get => _HasCts;
      set => RaisePropertyChangedIfSet(ref _HasCts, value);
    }
    bool _HasCts = false;
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalMeterViewModel(
      string Id,
      string NodeId,
      string NodeParentId,
      bool HasCts,
      string Status,
      Point Position
    )
    {
      Guid = Guid.Parse(NodeId);
      ParentGuid = Guid.Parse(NodeParentId);
      this.Id = Id;
      this.Position = Position;
      this.HasCts = HasCts;
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
      return Outputs.FirstOrDefault(arg => arg.Guid == guid);
    }
  }
}
