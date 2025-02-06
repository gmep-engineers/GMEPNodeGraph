using System;
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
  public class ElectricalDistributionBusViewModel : DefaultNodeViewModel
  {
    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalDistributionBusViewModel(
      string Id,
      string NodeId,
      int Amp,
      int StatusId,
      Point Position
    )
    {
      this.Id = Id;
      Guid = Guid.Parse(NodeId);
      this.Amp = Amp;
      this.StatusId = StatusId;
      this.Position = Position;
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      ServiceAmpVisible = Visibility.Visible;
      Name = "Distribution Bus";
      NodeType = NodeType.DistributionBus;
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
        INSERT INTO electrical_distribution_buses
        (id, parent_id, project_id, node_id, amp_rating_id, status_id)
        VALUES (@id, @projectId, @ampRatingId, @statusId)
        ";
      MySqlCommand createBreakerCommand = new MySqlCommand(query, db.Connection);
      createBreakerCommand.Parameters.AddWithValue("@id", Id);
      createBreakerCommand.Parameters.AddWithValue("@parentId", ParentId);
      createBreakerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createBreakerCommand.Parameters.AddWithValue("@projectId", projectId);
      createBreakerCommand.Parameters.AddWithValue("@ampRatingId", PanelAmpRatingId);
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
        UPDATE electrical_distribution_buses
        SET parent_id = @parentId, amp_rating_id = @ampRatingId
        WHERE id = @id
        ";
      MySqlCommand updateBreakerCommand = new MySqlCommand(query, db.Connection);
      updateBreakerCommand.Parameters.AddWithValue("@id", Id);
      updateBreakerCommand.Parameters.AddWithValue("@parentId", ParentId);
      updateBreakerCommand.Parameters.AddWithValue("@ampRatingId", PanelAmpRatingId);
      commands.Add(updateBreakerCommand);
      commands.Add(GetUpdateNodeCommand(db));
      return commands;
    }

    public override List<MySqlCommand> Delete(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        DELETE FROM electrical_distribution_buses
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
