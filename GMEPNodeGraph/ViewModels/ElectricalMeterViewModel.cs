using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

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

    public bool IsSpace
    {
      get => _IsSpace;
      set => RaisePropertyChangedIfSet(ref _IsSpace, value);
    }
    bool _IsSpace = false;
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalMeterViewModel(
      string Id,
      string ProjectId,
      string ElectricalProjectId,
      string NodeId,
      bool HasCts,
      bool IsSpace,
      int StatusId,
      Point Position,
      string InputConnectorId,
      string OutputConnectorId
    )
    {
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
      this.Id = Id;
      this.Position = Position;
      this.HasCts = HasCts;
      this.IsSpace = IsSpace;
      this.StatusId = StatusId;

      CtsVisible = Visibility.Visible;
      IsSpaceVisible = Visibility.Visible;
      Name = "Meter";
      NodeType = NodeType.Meter;
    }

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

    public override List<MySqlCommand> Create(
      string projectId,
      string electricalProjectId,
      GmepDatabase db
    )
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        INSERT INTO electrical_meters
        ( id,  project_id,  electrical_project_id,  node_id,  has_cts,  is_space,  status_id) VALUES
        (@id, @project_id, @electrical_project_id, @node_id, @has_cts, @is_space, @status_id)
        ";
      MySqlCommand createBreakerCommand = new MySqlCommand(query, db.Connection);
      createBreakerCommand.Parameters.AddWithValue("@id", Id);
      createBreakerCommand.Parameters.AddWithValue("@node_id", Guid.ToString());
      createBreakerCommand.Parameters.AddWithValue("@project_id", projectId);
      createBreakerCommand.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      createBreakerCommand.Parameters.AddWithValue("@has_cts", HasCts);
      createBreakerCommand.Parameters.AddWithValue("@is_space", IsSpace);
      createBreakerCommand.Parameters.AddWithValue("@status_id", StatusId);
      commands.Add(createBreakerCommand);
      commands.Add(GetCreateNodeCommand(projectId, electricalProjectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_meters
        SET node_id = @nodeId, has_cts = @hasCts, is_space = @isSpace, status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updateBreakerCommand = new MySqlCommand(query, db.Connection);
      updateBreakerCommand.Parameters.AddWithValue("@id", Id);
      updateBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updateBreakerCommand.Parameters.AddWithValue("@hasCts", HasCts);
      updateBreakerCommand.Parameters.AddWithValue("@isSpace", IsSpace);
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
        DELETE FROM electrical_meters
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
