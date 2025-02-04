using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalDistributionBusesViewModel : DefaultNodeViewModel
  {
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalDistributionBusesViewModel(
      string Id,
      string NodeId,
      string NodeParentId,
      int Amp,
      string Status,
      Point Position
    )
    {
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      ParentGuid = Guid.Parse(NodeParentId);
      this.Amp = Amp;
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
