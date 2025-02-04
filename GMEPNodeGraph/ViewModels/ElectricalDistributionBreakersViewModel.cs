using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalDistributionBreakersViewModel : DefaultNodeViewModel
  {
    public int NumPoles
    {
      get => _NumPoles;
      set => RaisePropertyChangedIfSet(ref _NumPoles, value);
    }
    int _NumPoles = 3;
    public bool IsFuseOnly
    {
      get => _IsFuseOnly;
      set => RaisePropertyChangedIfSet(ref _IsFuseOnly, value);
    }
    bool _IsFuseOnly = false;
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalDistributionBreakersViewModel(
      string Id,
      string NodeId,
      string NodeParentId,
      int Amp,
      int NumPoles,
      bool IsFuseOnly,
      string Status,
      Point Position
    )
    {
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      ParentGuid = Guid.Parse(NodeParentId);
      this.Amp = Amp;
      this.NumPoles = NumPoles;
      this.IsFuseOnly = IsFuseOnly;
      this.Position = Position;
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
