using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalServiceViewModel : DefaultNodeViewModel
  {
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
      string ProjectId,
      string ElectricalProjectId,
      string NodeId,
      string Name,
      int VoltageId,
      int AmpRatingId,
      string ColorCode,
      int StatusId,
      Point Position,
      string OutputConnectorId
    )
    {
      if (Guid.TryParse(OutputConnectorId, out Guid outputId))
      {
        NodeOutputViewModel output = new NodeOutputViewModel($"Output");
        output.Guid = outputId;
        _Outputs.Add(output);
      }
      else
      {
        _Outputs.Add(new NodeOutputViewModel($"Output"));
      }
      ServiceAmpVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      this.Position = Position;
      this.Id = Id;
      if (Guid.TryParse(NodeId, out Guid id))
      {
        Guid = id;
      }
      else
      {
        Guid = Guid.NewGuid();
        GmepDatabase db = new GmepDatabase();
        db.OpenConnection();
        MySqlCommand createNodeCommand = GetCreateSerivceNodeCommand(
          ProjectId,
          ElectricalProjectId,
          db
        );
        createNodeCommand.ExecuteNonQuery();
        List<MySqlCommand> updateNodeCommand = Update(db);
        updateNodeCommand[0].ExecuteNonQuery();
        updateNodeCommand[1].ExecuteNonQuery();
        db.CloseConnection();
      }
      this.Name = Name;
      this.VoltageId = VoltageId;
      this.AmpRatingId = AmpRatingId;
      this.ColorCode = ColorCode;
      this.StatusId = StatusId;
      NodeType = NodeType.Service;
      Inheritable = true;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Outputs.FirstOrDefault(arg => arg.Guid == guid);
    }

    public MySqlCommand GetCreateSerivceNodeCommand(
      string projectId,
      string electricalProjectId,
      GmepDatabase db
    )
    {
      string query =
        @"
        INSERT INTO electrical_single_line_nodes
        ( id,  project_id,  electrical_project_id,  loc_x,  loc_y,  input_connector_id,  output_connector_id) VALUES
        (@id, @project_id, @electrical_project_id, @loc_x, @loc_y, @input_connector_id, @output_connector_id)
        ";
      MySqlCommand createNodeCommand = new MySqlCommand(query, db.Connection);
      createNodeCommand.Parameters.AddWithValue("@id", Guid.ToString());
      createNodeCommand.Parameters.AddWithValue("@project_id", projectId);
      createNodeCommand.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      createNodeCommand.Parameters.AddWithValue("@loc_x", Position.X);
      createNodeCommand.Parameters.AddWithValue("@loc_y", Position.Y);
      createNodeCommand.Parameters.AddWithValue("@input_connector_id", "0");
      createNodeCommand.Parameters.AddWithValue(
        "@output_connector_id",
        Outputs.First().Guid.ToString()
      );
      return createNodeCommand;
    }

    public override List<MySqlCommand> Create(
      string projectId,
      string electricalProjectId,
      GmepDatabase db
    )
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        INSERT INTO electrical_services
        ( id,  project_id,  electrical_project_id,  node_id,  name,  electrical_service_amp_rating_id,  electrical_service_voltage_id,  color_code,  status_id) VALUES
        (@id, @project_id, @electrical_project_id, @node_id, @name, @electrical_service_amp_rating_id, @electrical_service_voltage_id, @color_code, @status_id) 
        ";
      MySqlCommand createServiceCommand = new MySqlCommand(query, db.Connection);
      createServiceCommand.Parameters.AddWithValue("@id", Id);
      createServiceCommand.Parameters.AddWithValue("@project_id", projectId);
      createServiceCommand.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      createServiceCommand.Parameters.AddWithValue("@node_id", Guid.ToString());
      createServiceCommand.Parameters.AddWithValue("@name", Name);
      createServiceCommand.Parameters.AddWithValue(
        "@electrical_service_amp_rating_id",
        AmpRatingId
      );
      createServiceCommand.Parameters.AddWithValue("@electrical_service_voltage_id", VoltageId);
      createServiceCommand.Parameters.AddWithValue("@color_code", ColorCode);
      createServiceCommand.Parameters.AddWithValue("@status_id", StatusId);
      commands.Add(createServiceCommand);
      commands.Add(GetCreateSerivceNodeCommand(projectId, electricalProjectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_services
        SET
        node_id = @nodeId,
        electrical_service_amp_rating_id = @ampRatingId,
        electrical_service_voltage_id = @voltageId,
        color_code = @colorCode,
        name = @name,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updatePanelCommand = new MySqlCommand(query, db.Connection);
      updatePanelCommand.Parameters.AddWithValue("@id", Id);
      updatePanelCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
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
