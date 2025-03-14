using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;

namespace GMEPNodeGraph.ViewModels
{
  public class ElectricalEquipmentViewModel : DefaultNodeViewModel
  {
    public int VoltageId
    {
      get => _VoltageId;
      set => RaisePropertyChangedIfSet(ref _VoltageId, value);
    }
    int _VoltageId = 0;

    public double Fla
    {
      get => _Fla;
      set => RaisePropertyChangedIfSet(ref _Fla, value);
    }
    double _Fla = 0;

    public double Mca
    {
      get => _Mca;
      set => RaisePropertyChangedIfSet(ref _Mca, value);
    }
    double _Mca = 0;

    public double AicRating
    {
      get => _AicRating;
      set => RaisePropertyChangedIfSet(ref _AicRating, value);
    }
    double _AicRating = 0;

    public bool IsThreePhase
    {
      get => _IsThreePhase;
      set => RaisePropertyChangedIfSet(ref _IsThreePhase, value);
    }
    bool _IsThreePhase = true;

    public string Hp
    {
      get => _Hp;
      set => RaisePropertyChangedIfSet(ref _Hp, value);
    }
    string _Hp = string.Empty;

    public int CategoryId
    {
      get => _CategoryId;
      set => RaisePropertyChangedIfSet(ref _CategoryId, value);
    }
    int _CategoryId = 0;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalEquipmentViewModel(
      string Id,
      string ProjectId,
      string NodeId,
      string Name,
      int VoltageId,
      double Mca,
      double Fla,
      double AicRating,
      bool IsThreePhase,
      string Hp,
      int CategoryId,
      int StatusId,
      Point Position,
      string InputConnectorId
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
      if (Guid.TryParse(NodeId, out Guid id))
      {
        Guid = id;
      }
      else
      {
        Guid = Guid.NewGuid();
        GmepDatabase db = new GmepDatabase();
        db.OpenConnection();
        MySqlCommand createNodeCommand = GetCreateEquipmentNodeCommand(ProjectId, db);
        createNodeCommand.ExecuteNonQuery();
        List<MySqlCommand> updateNodeCommand = Update(db);
        updateNodeCommand[0].ExecuteNonQuery();
        updateNodeCommand[1].ExecuteNonQuery();
        db.CloseConnection();
      }
      this.Name = Name;
      this.StatusId = StatusId;
      this.Position = Position;
      this.VoltageId = VoltageId;
      this.Mca = Mca;
      this.Fla = Fla;
      this.IsThreePhase = IsThreePhase;
      this.Hp = Hp;
      this.CategoryId = CategoryId;
      this.AicRating = AicRating;
      NodeType = NodeType.Equipment;
      EquipmentVoltageVisible = Visibility.Visible;
      LoadAmperageVisible = Visibility.Visible;
      IsThreePhaseVisible = Visibility.Visible;
      HpVisible = Visibility.Visible;
      Inheritable = true;
    }

    public override NodeConnectorViewModel FindConnector(Guid guid)
    {
      return Inputs.FirstOrDefault(arg => arg.Guid == guid);
    }

    public MySqlCommand GetCreateEquipmentNodeCommand(string projectId, GmepDatabase db)
    {
      string query =
        @"
        INSERT INTO electrical_single_line_nodes
        (id, project_id, loc_x, loc_y, input_connector_id, output_connector_id )
        VALUES (@id, @projectId, @locX, @locY, @inputConnectorId, @outputConnectorId)
        ";
      MySqlCommand createNodeCommand = new MySqlCommand(query, db.Connection);
      createNodeCommand.Parameters.AddWithValue("@id", Guid.ToString());
      createNodeCommand.Parameters.AddWithValue("@projectId", projectId);
      createNodeCommand.Parameters.AddWithValue("@locX", Position.X);
      createNodeCommand.Parameters.AddWithValue("@locY", Position.Y);
      createNodeCommand.Parameters.AddWithValue(
        "@inputConnectorId",
        Inputs.First().Guid.ToString()
      );
      createNodeCommand.Parameters.AddWithValue("@outputConnectorId", "0");
      return createNodeCommand;
    }

    public override List<MySqlCommand> Create(string projectId, GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        INSERT INTO electrical_equipment
        (id, project_id, equip_no, node_id, status_id, voltage_id, mca, fla, is_three_phase, hp, category_id, connection_type_id)
        VALUES (@id, @projectId, @name, @nodeId, @statusId, @voltageId, @mca, @fla, @isThreePhase, @hp, @categoryId,  @connectionTypeId)
        ";
      MySqlCommand createEquipmentCommand = new MySqlCommand(query, db.Connection);
      createEquipmentCommand.Parameters.AddWithValue("@id", Id);
      createEquipmentCommand.Parameters.AddWithValue("@name", Name);
      createEquipmentCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createEquipmentCommand.Parameters.AddWithValue("@projectId", projectId);
      createEquipmentCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      createEquipmentCommand.Parameters.AddWithValue("@mca", Mca);
      createEquipmentCommand.Parameters.AddWithValue("@fla", Fla);
      createEquipmentCommand.Parameters.AddWithValue("@isThreePhase", IsThreePhase);
      createEquipmentCommand.Parameters.AddWithValue("@hp", Hp);
      createEquipmentCommand.Parameters.AddWithValue("@categoryId", CategoryId);
      createEquipmentCommand.Parameters.AddWithValue("@connectionTypeId", 3);
      createEquipmentCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(createEquipmentCommand);
      commands.Add(GetCreateEquipmentNodeCommand(projectId, db));
      return commands;
    }

    public override List<MySqlCommand> Update(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        UPDATE electrical_equipment
        SET 
        equip_no = @name,
        node_id = @nodeId, 
        parent_id = @parentId, 
        status_id = @statusId,
        voltage_id = @voltageId,
        mca = @mca,
        fla = @fla,
        is_three_phase = @isThreePhase,
        hp = @hp,
        category_id = @categoryId,
        connection_type_id = @connectionTypeId
        WHERE id = @id
        ";
      MySqlCommand updateEquipmentCommand = new MySqlCommand(query, db.Connection);
      updateEquipmentCommand.Parameters.AddWithValue("@id", Id);
      updateEquipmentCommand.Parameters.AddWithValue("@name", Name);
      updateEquipmentCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      updateEquipmentCommand.Parameters.AddWithValue("@parentId", ParentId);
      updateEquipmentCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      updateEquipmentCommand.Parameters.AddWithValue("@mca", Mca);
      updateEquipmentCommand.Parameters.AddWithValue("@fla", Fla);
      updateEquipmentCommand.Parameters.AddWithValue("@isThreePhase", IsThreePhase);
      updateEquipmentCommand.Parameters.AddWithValue("@hp", Hp);
      updateEquipmentCommand.Parameters.AddWithValue("@categoryId", CategoryId);
      updateEquipmentCommand.Parameters.AddWithValue("@connectionTypeId", 3);
      updateEquipmentCommand.Parameters.AddWithValue("@statusId", StatusId);
      commands.Add(updateEquipmentCommand);
      commands.Add(GetUpdateNodeCommand(db));
      return commands;
    }

    public override List<MySqlCommand> Delete(GmepDatabase db)
    {
      List<MySqlCommand> commands = new List<MySqlCommand>();
      string query =
        @"
        DELETE FROM electrical_equipment
        WHERE id = @id
        ";
      MySqlCommand deleteEquipmentCommand = new MySqlCommand(query, db.Connection);
      deleteEquipmentCommand.Parameters.AddWithValue("@id", Id);
      commands.Add(deleteEquipmentCommand);
      commands.Add(GetDeleteNodeCommand(db));
      return commands;
    }
  }
}
