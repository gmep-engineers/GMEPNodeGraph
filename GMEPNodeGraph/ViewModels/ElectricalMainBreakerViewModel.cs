using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

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

    public ElectricalMainBreakerViewModel(
      string Id,
      string ProjectId,
      string NodeId,
      int AmpRatingId,
      int NumPoles,
      bool HasGroundFaultProtection,
      bool HasSurgeProtection,
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
        MySqlCommand createNodeCommand = GetCreateNodeCommand(ProjectId, db);
        createNodeCommand.ExecuteNonQuery();
        List<MySqlCommand> updateNodeCommand = Update(db);
        updateNodeCommand[0].ExecuteNonQuery();
        updateNodeCommand[1].ExecuteNonQuery();
        db.CloseConnection();
      }
      this.AmpRatingId = AmpRatingId;
      this.NumPoles = NumPoles;
      this.HasGroundFaultProtection = HasGroundFaultProtection;
      this.HasSurgeProtection = HasSurgeProtection;
      this.StatusId = StatusId;
      this.Position = Position;

      ServiceAmpVisible = Visibility.Visible;
      PoleVisible = Visibility.Visible;
      Name = "Main Breaker";
      NodeType = NodeType.MainBreaker;
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

    public override List<MySqlCommand> Create(string projectId, GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        INSERT INTO electrical_main_breakers
        (id, project_id, node_id, amp_rating_id, has_ground_fault_protection, has_surge_protection, num_poles, status_id)
        VALUES (@id, @projectId, @nodeId, @ampRatingId, @hasGroundFaultProtection, @hasSurgeProtection, @numPoles, @statusId)
        ";
      MySqlCommand createBreakerCommand = new MySqlCommand(query, db.Connection);
      createBreakerCommand.Parameters.AddWithValue("@id", Id);
      createBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createBreakerCommand.Parameters.AddWithValue("@projectId", projectId);
      createBreakerCommand.Parameters.AddWithValue("@ampRatingId", AmpRatingId);
      createBreakerCommand.Parameters.AddWithValue(
        "@hasGroundFaultProtection",
        HasGroundFaultProtection
      );
      createBreakerCommand.Parameters.AddWithValue("@hasSurgeProtection", HasSurgeProtection);
      createBreakerCommand.Parameters.AddWithValue("@numPoles", NumPoles);
      createBreakerCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(createBreakerCommand);
      commands.Add(GetCreateNodeCommand(projectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_main_breakers
        SET node_id = @nodeId, amp_rating_id = @ampRatingId, has_ground_fault_protection = @hasGroundFaultProtection, has_surge_protection = @hasSurgeProtection, num_poles = @numPoles, status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updateBreakerCommand = new MySqlCommand(query, db.Connection);
      updateBreakerCommand.Parameters.AddWithValue("@id", Id);
      updateBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updateBreakerCommand.Parameters.AddWithValue("@ampRatingId", AmpRatingId);
      updateBreakerCommand.Parameters.AddWithValue(
        "@hasGroundFaultProtection",
        HasGroundFaultProtection
      );
      updateBreakerCommand.Parameters.AddWithValue("@hasSurgeProtection", HasSurgeProtection);
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
        DELETE FROM electrical_main_breakers
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
