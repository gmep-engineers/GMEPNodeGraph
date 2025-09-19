using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalDisconnectViewModel : DefaultNodeViewModel
  {
    public int AsSizeId
    {
      get => _AsSizeId;
      set => RaisePropertyChangedIfSet(ref _AsSizeId, value);
    }
    int _AsSizeId = 0;

    public int AfSizeId
    {
      get => _AfSizeId;
      set => RaisePropertyChangedIfSet(ref _AfSizeId, value);
    }
    int _AfSizeId = 0;

    public int NumPoles
    {
      get => _NumPoles;
      set => RaisePropertyChangedIfSet(ref _NumPoles, value);
    }
    int _NumPoles = 3;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

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

    public ElectricalDisconnectViewModel(
      string Id,
      string ProjectId,
      string ElectricalProjectId,
      string NodeId,
      int AsSizeId,
      int AfSizeId,
      int NumPoles,
      int StatusId,
      Point Position,
      string InputConnectorId,
      string OutputConnectorId
    )
    {
      this.Id = Id;
      if (Guid.TryParse(InputConnectorId, out Guid inputId))
      {
        NodeInputViewModel input = new NodeInputViewModel($"Input", true);
        input.Guid = inputId;
        _Inputs.Add(input);
      }
      else
      {
        _Inputs.Add(new NodeInputViewModel($"Input", true));
      }
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
      if (Guid.TryParse(NodeId, out Guid id))
      {
        Guid = id;
      }
      else
      {
        Guid = Guid.NewGuid();
        GmepDatabase db = new GmepDatabase();
        db.OpenConnection();
        MySqlCommand createNodeCommand = GetCreateNodeCommand(ProjectId, ElectricalProjectId, db);
        createNodeCommand.ExecuteNonQuery();
        List<MySqlCommand> updateNodeCommand = Update(db);
        updateNodeCommand[0].ExecuteNonQuery();
        updateNodeCommand[1].ExecuteNonQuery();
        db.CloseConnection();
      }
      this.AsSizeId = AsSizeId;
      this.AfSizeId = AfSizeId;
      Name = "Disconnect";
      this.NumPoles = NumPoles;
      this.StatusId = StatusId;
      this.Position = Position;
      AfVisible = Visibility.Visible;
      AsVisible = Visibility.Visible;
      PoleVisible = Visibility.Visible;
      NodeType = NodeType.Disconnect;
      Inheritable = false;
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
        INSERT INTO electrical_disconnects
        ( id,  project_id,  electrical_project_id,  node_id,  as_size_id,  af_size_id,  num_poles,  status_id) VALUES
        (@id, @project_id, @electrical_project_id, @node_id, @as_size_id, @af_size_id, @num_poles, @status_id)
        ";
      MySqlCommand createDisconnectCommand = new MySqlCommand(query, db.Connection);
      createDisconnectCommand.Parameters.AddWithValue("@id", Id);
      createDisconnectCommand.Parameters.AddWithValue("@project_id", projectId);
      createDisconnectCommand.Parameters.AddWithValue(
        "@electrical_project_id",
        electricalProjectId
      );
      createDisconnectCommand.Parameters.AddWithValue("@node_id", Guid.ToString());
      createDisconnectCommand.Parameters.AddWithValue("@as_size_id", AsSizeId);
      createDisconnectCommand.Parameters.AddWithValue("@af_size_id", AfSizeId);
      createDisconnectCommand.Parameters.AddWithValue("@num_poles", NumPoles);
      createDisconnectCommand.Parameters.AddWithValue("@status_id", StatusId);
      commands.Add(createDisconnectCommand);
      commands.Add(GetCreateNodeCommand(projectId, electricalProjectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_disconnects
        SET node_id = @nodeId, as_size_id = @asSizeId, af_size_id = @afSizeId, num_poles = @numPoles, status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updateBreakerCommand = new MySqlCommand(query, db.Connection);
      updateBreakerCommand.Parameters.AddWithValue("@id", Id);
      updateBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updateBreakerCommand.Parameters.AddWithValue("@asSizeId", AsSizeId);
      updateBreakerCommand.Parameters.AddWithValue("@afSizeId", AfSizeId);
      updateBreakerCommand.Parameters.AddWithValue("@numPoles", NumPoles);
      updateBreakerCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(updateBreakerCommand);
      commands.Add(GetUpdateNodeCommand(db));
      return commands;
    }

    public override List<MySqlCommand> Delete(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        DELETE FROM electrical_disconnects
        WHERE id = @id
        ";
      MySqlCommand deleteBreakerCommand = new MySqlCommand(query, db.Connection);
      deleteBreakerCommand.Parameters.AddWithValue("@id", Id);
      commands.Add(deleteBreakerCommand);
      commands.Add(GetDeleteNodeCommand(db));
      return commands;
    }
  }
}
