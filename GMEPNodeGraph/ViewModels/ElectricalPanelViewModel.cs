﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using GMEPNodeGraph;
using GMEPNodeGraph.Utilities;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Pqc.Crypto.Lms;

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

    public int BusAmpRatingId
    {
      get => _BusAmp;
      set => RaisePropertyChangedIfSet(ref _BusAmp, value);
    }
    int _BusAmp = 0;

    public int MainAmpRatingId
    {
      get => _MainAmp;
      set => RaisePropertyChangedIfSet(ref _MainAmp, value);
    }
    int _MainAmp = 0;

    public override IEnumerable<NodeConnectorViewModel> Inputs => _Inputs;
    readonly ObservableCollection<NodeInputViewModel> _Inputs =
      new ObservableCollection<NodeInputViewModel>();

    public override IEnumerable<NodeConnectorViewModel> Outputs => _Outputs;
    readonly ObservableCollection<NodeOutputViewModel> _Outputs =
      new ObservableCollection<NodeOutputViewModel>();

    public ElectricalPanelViewModel(
      string Id,
      string NodeId,
      string Name,
      int VoltageId,
      int BusAmpRatingId,
      int MainAmpRatingId,
      string ColorCode,
      bool IsMlo,
      int StatusId,
      Point Position
    )
    {
      _Outputs.Add(new NodeOutputViewModel($"Output"));
      _Inputs.Add(new NodeInputViewModel($"Input", true));
      PanelBusAmpVisible = Visibility.Visible;
      PanelMainAmpVisible = Visibility.Visible;
      PanelAmpLabelsVisible = Visibility.Visible;
      VoltagePhaseVisible = Visibility.Visible;
      MloVisible = Visibility.Visible;

      this.Name = Name;
      this.Position = Position;
      this.IsMlo = IsMlo;
      this.BusAmpRatingId = BusAmpRatingId;
      this.ColorCode = ColorCode;
      this.VoltageId = VoltageId;
      this.Id = Id;
      this.StatusId = StatusId;
      Guid = Guid.Parse(NodeId);
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
        (id, parent_id, project_id, node_id, bus_amp_rating_id, main_amp_rating_id, is_mlo, voltage_id, color_code, name, status_id)
        VALUES (@id, @projectId, @nodeId, @busAmpRatingId, @mainAmpRatingId, @isMlo, @voltageId, @colorCode, @name, @statusId)
        ";
      MySqlCommand createPanelCommand = new MySqlCommand(query, db.Connection);
      createPanelCommand.Parameters.AddWithValue("@id", Id);
      createPanelCommand.Parameters.AddWithValue("@parentId", ParentId);
      createPanelCommand.Parameters.AddWithValue("@projectId", projectId);
      createPanelCommand.Parameters.AddWithValue("@nodeId", Guid.ToString());
      createPanelCommand.Parameters.AddWithValue("@busAmpRatingId", BusAmpRatingId);
      createPanelCommand.Parameters.AddWithValue("@mainAmpRatingId", MainAmpRatingId);
      createPanelCommand.Parameters.AddWithValue("@isMlo", IsMlo);
      createPanelCommand.Parameters.AddWithValue("@voltageId", VoltageId);
      createPanelCommand.Parameters.AddWithValue("@colorCode", ColorCode);
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
        parent_id = @parentId,
        bus_amp_rating_id = @busAmpRatingId,
        main_amp_rating_id = @mainAmpRatingId,
        is_mlo = @isMlo,
        voltage_id = @voltageId,
        color_code = @colorCode,
        name = @name,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updatePanelCommand = new MySqlCommand(query, db.Connection);
      updatePanelCommand.Parameters.AddWithValue("@id", Id);
      updatePanelCommand.Parameters.AddWithValue("@parentId", ParentId);
      updatePanelCommand.Parameters.AddWithValue("@busAmpRatingId", BusAmpRatingId);
      updatePanelCommand.Parameters.AddWithValue("@mainAmpRatingId", MainAmpRatingId);
      updatePanelCommand.Parameters.AddWithValue("@isMlo", IsMlo);
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
