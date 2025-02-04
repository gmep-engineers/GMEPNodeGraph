using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalServiceViewModel : DefaultNodeViewModel
  {
    public string Body
    {
      get => _Body;
      set => RaisePropertyChangedIfSet(ref _Body, value);
    }
    string _Body = string.Empty;

    public string VoltageType
    {
      get => _VoltageType;
      set => RaisePropertyChangedIfSet(ref _VoltageType, value);
    }
    string _VoltageType = "120/208 3Φ";

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalServiceViewModel(
      string Id,
      string NodeId,
      string Name,
      string VoltageType,
      int Amp,
      string ColorCode,
      string Status,
      Point Position
    )
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      ServiceAmpVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      this.Position = Position;
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      this.Name = Name;
      this.VoltageType = VoltageType;
      this.Amp = Amp;
      this.ColorCode = ColorCode;
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
