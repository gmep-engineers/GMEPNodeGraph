using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalDistributionBreakerViewModel : DefaultNodeViewModel
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
    public int PanelAmpRatingId
    {
      get => _PanelAmpRatingId;
      set => RaisePropertyChangedIfSet(ref _PanelAmpRatingId, value);
    }
    int _PanelAmpRatingId = 1;
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalDistributionBreakerViewModel(
      string Id,
      string ProjectId,
      string NodeId,
      int AmpRatingId,
      int NumPoles,
      bool IsFuseOnly,
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
      this.PanelAmpRatingId = AmpRatingId;
      this.NumPoles = NumPoles;
      this.IsFuseOnly = IsFuseOnly;
      this.Position = Position;
      this.StatusId = StatusId;

      PanelBusAmpVisible = Visibility.Visible;
      PoleVisible = Visibility.Visible;
      NodeType = NodeType.DistributionBreaker;
      Name = "Distrib. Bkr.";
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
        INSERT INTO electrical_distribution_breakers
        (id, project_id, node_id, amp_rating_id, num_poles, is_fuse_only, status_id)
        VALUES (@id, @projectId, @nodeId, @ampRatingId, @numPoles, @isFuseOnly, @statusId)
        ";
      MySqlCommand createBreakerCommand = new MySqlCommand(query, db.Connection);
      createBreakerCommand.Parameters.AddWithValue("@id", Id);
      createBreakerCommand.Parameters.AddWithValue("@projectId", projectId);
      createBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createBreakerCommand.Parameters.AddWithValue("@ampRatingId", PanelAmpRatingId);
      createBreakerCommand.Parameters.AddWithValue("@numPoles", NumPoles);
      createBreakerCommand.Parameters.AddWithValue("@isFuseOnly", IsFuseOnly);
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
        UPDATE electrical_distribution_breakers
        SET node_id = @nodeId, amp_rating_id = @ampRatingId, num_poles = @numPoles, is_fuse_only = @isFuseOnly, status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updateBreakerCommand = new MySqlCommand(query, db.Connection);
      updateBreakerCommand.Parameters.AddWithValue("@id", Id);
      updateBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updateBreakerCommand.Parameters.AddWithValue("@ampRatingId", PanelAmpRatingId);
      updateBreakerCommand.Parameters.AddWithValue("@numPoles", NumPoles);
      updateBreakerCommand.Parameters.AddWithValue("@isFuseOnly", IsFuseOnly);
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
        DELETE FROM electrical_distribution_breakers
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
