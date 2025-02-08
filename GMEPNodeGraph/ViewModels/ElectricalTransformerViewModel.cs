using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GMEPNodeGraph;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Pqc.Crypto.Lms;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalTransformerViewModel : DefaultNodeViewModel
  {
    public int VoltageId
    {
      get => _VoltageId;
      set => RaisePropertyChangedIfSet(ref _VoltageId, value);
    }
    int _VoltageId = 1;

    public int KvaId
    {
      get => _KvaId;
      set => RaisePropertyChangedIfSet(ref _KvaId, value);
    }
    int _KvaId = 1;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalTransformerViewModel(
      string Id,
      string NodeId,
      string Name,
      int VoltageId,
      int KvaId,
      string ColorCode,
      int StatusId,
      Point position,
      string InputConnectorId,
      string OutputConnectorId
    )
    {
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
        MySqlCommand createNodeCommand = GetCreateNodeCommand(ProjectId, db);
        createNodeCommand.ExecuteNonQuery();
        List<MySqlCommand> updateNodeCommand = Update(db);
        updateNodeCommand[0].ExecuteNonQuery();
        updateNodeCommand[1].ExecuteNonQuery();
        db.CloseConnection();
      }
      this.Name = Name;
      this.VoltageId = VoltageId;
      this.KvaId = KvaId;
      this.ColorCode = ColorCode;
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
      KvaVisible = Visibility.Visible;
      TransformerVoltageVisible = Visibility.Visible;
      Position = position;
      this.StatusId = StatusId;
      NodeType = NodeType.Transformer;
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
        INSERT INTO electrical_transformers
        (id, parent_id, project_id, node_id, kva_id, voltage_id, name, color_code, status_id)
        VALUES (@id, @projectId, @nodeId, @kvaId, @voltageId, @name, @colorCode, @statusId)
        ";
      MySqlCommand createTransformerCommand = new MySqlCommand(query, db.Connection);
      createTransformerCommand.Parameters.AddWithValue("@id", Id);
      createTransformerCommand.Parameters.AddWithValue("@parentId", ParentId);
      createTransformerCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createTransformerCommand.Parameters.AddWithValue("@projectId", projectId);
      createTransformerCommand.Parameters.AddWithValue("@name", Name);
      createTransformerCommand.Parameters.AddWithValue("@kvaId", KvaId);
      createTransformerCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      createTransformerCommand.Parameters.AddWithValue("@colorCode", ColorCode);
      createTransformerCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(createTransformerCommand);
      commands.Add(GetCreateNodeCommand(projectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_transformers
        SET
        parent_id = @parentId,
        kva_id = @kvaId,
        voltage_id = @voltageId,
        color_code = @colorCode,
        name = @name,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updatePanelCommand = new MySqlCommand(query, db.Connection);
      updatePanelCommand.Parameters.AddWithValue("@id", Id);
      updatePanelCommand.Parameters.AddWithValue("@parentId", ParentId);
      updatePanelCommand.Parameters.AddWithValue("@kvaId", KvaId);
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
        DELETE FROM electrical_transformers
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
