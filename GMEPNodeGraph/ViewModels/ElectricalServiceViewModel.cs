using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

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

    public int VoltageId
    {
      get => _VoltageId;
      set => RaisePropertyChangedIfSet(ref _VoltageId, value);
    }
    int _VoltageId = 1;

    public int AmpRatingId
    {
      get => _AmpRatingId;
      set => RaisePropertyChangedIfSet(ref _AmpRatingId, value);
    }
    int _AmpRatingId = 1;

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
      int VoltageId,
      int AmpRatingId,
      string ColorCode,
      int StatusId,
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
      this.VoltageId = VoltageId;
      this.AmpRatingId = AmpRatingId;
      this.ColorCode = ColorCode;
      this.StatusId = StatusId;
      NodeType = NodeType.Service;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Outputs.FirstOrDefault(arg => arg.Guid == guid);
    }

    public override List<MySqlCommand> Create(string projectId, GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        INSERT INTO electrical_services
        (id, project_id, node_id, name, electrical_service_amp_rating_id, electrical_service_voltage_id, color_code, status_id)
        VALUES (@id, @projectId, @nodeId, @name, @ampRatingId, @voltageId, @colorCode, @statusId)
        ";
      MySqlCommand createServiceCommand = new MySqlCommand(query, db.Connection);
      createServiceCommand.Parameters.AddWithValue("@id", Id);
      createServiceCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createServiceCommand.Parameters.AddWithValue("@projectId", projectId);
      createServiceCommand.Parameters.AddWithValue("@name", Name);
      createServiceCommand.Parameters.AddWithValue("@ampRatingId", AmpRatingId);
      createServiceCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      createServiceCommand.Parameters.AddWithValue("@colorCode", ColorCode);
      createServiceCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(createServiceCommand);
      query =
        @"
        INSERT INTO electrical_single_line_nodes
        (id, project_id, loc_x, loc_y, output_connector_id)
        VALUES (@id, @projectId, @locX, @locY, @outputConnectorId)
        ";
      MySqlCommand createNodeCommand = new MySqlCommand(query, db.Connection);
      createNodeCommand.Parameters.AddWithValue("@id", Guid.ToString());
      createNodeCommand.Parameters.AddWithValue("@projectId", projectId);
      createNodeCommand.Parameters.AddWithValue("@locX", Position.X);
      createNodeCommand.Parameters.AddWithValue("@locY", Position.Y);
      createNodeCommand.Parameters.AddWithValue("@outputConnectorId", Outputs.First());
      commands.Add(createNodeCommand);
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_services
        SET
        electrical_service_amp_rating_id = @ampRatingId,
        electrical_service_voltage_id = @voltageId,
        color_code = @colorCode,
        name = @name,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updatePanelCommand = new MySqlCommand(query, db.Connection);
      updatePanelCommand.Parameters.AddWithValue("@id", Id);
      updatePanelCommand.Parameters.AddWithValue("@ampRatingId", AmpRatingId);
      updatePanelCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      updatePanelCommand.Parameters.AddWithValue("@colorCode", ColorCode);
      updatePanelCommand.Parameters.AddWithValue("@name", Name);
      updatePanelCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(updatePanelCommand);
      commands.Add(GetUpdateNodeCommand(db));
      return commands;
    }

    public override List<MySqlCommand> Delete(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        DELETE FROM electrical_services
        WHERE id = @id
        ";
      MySqlCommand deletePanelCommand = new MySqlCommand(query, db.Connection);
      deletePanelCommand.Parameters.AddWithValue("@id", Id);
      commands.Add(deletePanelCommand);
      commands.Add(GetDeleteNodeCommand(db));
      return commands;
    }
  }
}
