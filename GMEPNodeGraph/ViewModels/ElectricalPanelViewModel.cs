using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalPanelViewModel : DefaultNodeViewModel
  {
    public int VoltageId
    {
      get => _VoltageId;
      set => RaisePropertyChangedIfSet(ref _VoltageId, value);
    }
    int _VoltageId = 1;

    public bool IsMlo
    {
      get => _IsMlo;
      set => RaisePropertyChangedIfSet(ref _IsMlo, value);
    }
    bool _IsMlo = true;

    public bool IsRecessed
    {
      get => _IsRecessed;
      set => RaisePropertyChangedIfSet(ref _IsRecessed, value);
    }
    bool _IsRecessed = true;

    public int PanelAmpRatingId
    {
      get => _BusAmp;
      set => RaisePropertyChangedIfSet(ref _BusAmp, value);
    }
    int _BusAmp = 1;

    public int MainAmpRatingId
    {
      get => _MainAmp;
      set => RaisePropertyChangedIfSet(ref _MainAmp, value);
    }
    int _MainAmp = 1;

    public int NumBreakers
    {
      get => _NumBreakers;
      set => RaisePropertyChangedIfSet(ref _NumBreakers, value);
    }
    int _NumBreakers = 42;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalPanelViewModel(
      string Id,
      string ProjectId,
      string NodeId,
      string Name,
      int VoltageId,
      int PanelAmpRatingId,
      int MainAmpRatingId,
      string ColorCode,
      bool IsMlo,
      bool IsRecessed,
      int NumBreakers,
      int StatusId,
      Point Position,
      string InputConnectorId,
      string OutputConnectorId
    )
    {
      PanelBusAmpVisible = Visibility.Visible;
      PanelMainAmpVisible = Visibility.Visible;
      PanelAmpLabelsVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      MloVisible = Visibility.Visible;
      RecessedVisible = Visibility.Visible;
      NumBreakersVisible = Visibility.Visible;
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
      Inheritable = true;
      this.Name = Name;
      this.Position = Position;
      this.IsMlo = IsMlo;
      this.IsRecessed = IsRecessed;
      this.PanelAmpRatingId = PanelAmpRatingId;
      this.ColorCode = ColorCode;
      this.VoltageId = VoltageId;
      this.NumBreakers = NumBreakers;
      this.Id = Id;
      this.StatusId = StatusId;
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
      this.MainAmpRatingId = MainAmpRatingId;
      NodeType = NodeType.Panel;
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
        INSERT INTO electrical_panels
        (id, parent_id, project_id, node_id, bus_amp_rating_id, main_amp_rating_id, is_mlo, is_recessed, voltage_id, color_code, @num_breakers, name, status_id)
        VALUES (@id, @parentId, @projectId, @nodeId, @busAmpRatingId, @mainAmpRatingId, @isMlo, @isRecessed, @voltageId, @colorCode, @numBreakers, @name, @statusId)
        ";
      MySqlCommand createPanelCommand = new MySqlCommand(query, db.Connection);
      createPanelCommand.Parameters.AddWithValue("@id", Id);
      createPanelCommand.Parameters.AddWithValue("@parentId", ParentId);
      createPanelCommand.Parameters.AddWithValue("@projectId", projectId);
      createPanelCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createPanelCommand.Parameters.AddWithValue("@busAmpRatingId", PanelAmpRatingId);
      createPanelCommand.Parameters.AddWithValue("@mainAmpRatingId", MainAmpRatingId);
      createPanelCommand.Parameters.AddWithValue("@isMlo", IsMlo);
      createPanelCommand.Parameters.AddWithValue("@isRecessed", IsRecessed);
      createPanelCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      createPanelCommand.Parameters.AddWithValue("@colorCode", ColorCode);
      createPanelCommand.Parameters.AddWithValue("@numBreakers", NumBreakers);
      createPanelCommand.Parameters.AddWithValue("@name", Name);
      createPanelCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(createPanelCommand);
      commands.Add(GetCreateNodeCommand(projectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_panels
        SET
        node_id = @nodeId,
        parent_id = @parentId,
        bus_amp_rating_id = @busAmpRatingId,
        main_amp_rating_id = @mainAmpRatingId,
        is_mlo = @isMlo,
        is_recessed = @isRecessed,
        voltage_id = @voltageId,
        color_code = @colorCode,
        num_breakers = @numBreakers,
        name = @name,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updatePanelCommand = new MySqlCommand(query, db.Connection);
      updatePanelCommand.Parameters.AddWithValue("@id", Id);
      updatePanelCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updatePanelCommand.Parameters.AddWithValue("@parentId", ParentId);
      updatePanelCommand.Parameters.AddWithValue("@busAmpRatingId", PanelAmpRatingId);
      updatePanelCommand.Parameters.AddWithValue("@mainAmpRatingId", MainAmpRatingId);
      updatePanelCommand.Parameters.AddWithValue("@isMlo", IsMlo);
      updatePanelCommand.Parameters.AddWithValue("@isRecessed", IsRecessed);
      updatePanelCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      updatePanelCommand.Parameters.AddWithValue("@colorCode", ColorCode);
      updatePanelCommand.Parameters.AddWithValue("@numBreakers", NumBreakers);
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
        DELETE FROM electrical_panels
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
