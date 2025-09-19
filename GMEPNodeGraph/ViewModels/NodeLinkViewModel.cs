using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GMEPNodeGraph.Utilities;
using Livet;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace GMEPNodeGraph.ViewModels
{
  public class NodeLinkViewModel : ViewModel
  {
    public Guid Guid
    {
      get => _Guid;
      set => RaisePropertyChangedIfSet(ref _Guid, value);
    }
    Guid _Guid = Guid.NewGuid();

    public Guid InputConnectorGuid
    {
      get => _InputConnectorGuid;
      set => RaisePropertyChangedIfSet(ref _InputConnectorGuid, value);
    }
    Guid _InputConnectorGuid = Guid.NewGuid();

    public Guid OutputConnectorGuid
    {
      get => _OutputConnectorGuid;
      set => RaisePropertyChangedIfSet(ref _OutputConnectorGuid, value);
    }
    Guid _OutputConnectorGuid = Guid.NewGuid();

    public Guid InputConnectorNodeGuid
    {
      get => _InputNodeGuid;
      set => RaisePropertyChangedIfSet(ref _InputNodeGuid, value);
    }
    Guid _InputNodeGuid = Guid.NewGuid();

    public Guid OutputConnectorNodeGuid
    {
      get => _OutputNodeGuid;
      set => RaisePropertyChangedIfSet(ref _OutputNodeGuid, value);
    }
    Guid _OutputNodeGuid = Guid.NewGuid();

    public bool IsLocked
    {
      get => _IsLocked;
      set => RaisePropertyChangedIfSet(ref _IsLocked, value);
    }
    bool _IsLocked = false;

    public bool IsSelected
    {
      get => _IsSelected;
      set => RaisePropertyChangedIfSet(ref _IsSelected, value);
    }
    bool _IsSelected = false;

    public NodeLinkViewModel(
      string Id,
      string InputConnectorId,
      string OuputConnectorId,
      string InputConnectorNodeId,
      string OutputConnectorNodeId
    )
    {
      Guid = Guid.Parse(Id);
      InputConnectorGuid = Guid.Parse(InputConnectorId);
      OutputConnectorGuid = Guid.Parse(OuputConnectorId);
      InputConnectorNodeGuid = Guid.Parse(InputConnectorNodeId);
      OutputConnectorNodeGuid = Guid.Parse(OutputConnectorNodeId);
    }

    public MySqlCommand Create(string projectId, string electricalProjectId, GmepDatabase db)
    {
      string query =
        @"
        INSERT INTO electrical_single_line_node_links
        ( id,  project_id,  electrical_project_id,  input_connector_id,  output_connector_id,  input_connector_node_id,  output_connector_node_id) VALUES
        (@id, @project_id, @electrical_project_id, @input_connector_id, @output_connector_id, @input_connector_node_id, @output_connector_node_id)
        ";
      MySqlCommand createNodeLinkCommand = new MySqlCommand(query, db.Connection);
      createNodeLinkCommand.Parameters.AddWithValue("@id", Guid.ToString());
      createNodeLinkCommand.Parameters.AddWithValue("@project_id", projectId);
      createNodeLinkCommand.Parameters.AddWithValue("@electrical_project_id", electricalProjectId);
      createNodeLinkCommand.Parameters.AddWithValue(
        "@input_connector_id",
        InputConnectorGuid.ToString()
      );
      createNodeLinkCommand.Parameters.AddWithValue(
        "@output_connector_id",
        OutputConnectorGuid.ToString()
      );
      createNodeLinkCommand.Parameters.AddWithValue(
        "@input_connector_node_id",
        InputConnectorNodeGuid.ToString()
      );
      createNodeLinkCommand.Parameters.AddWithValue(
        "@output_connector_node_id",
        OutputConnectorNodeGuid.ToString()
      );
      return createNodeLinkCommand;
    }

    public MySqlCommand Update(GmepDatabase db)
    {
      string query =
        @"
        UPDATE electrical_single_line_node_links
        SET input_connector_id = @inputConnectorId, output_connector_id = @outputConnectorId, input_connector_node_id = @inputConnectorNodeId, output_connector_node_id = @outputConnectorNodeId
        WHERE id = @id
        ";
      MySqlCommand updateNodeLinkCommand = new MySqlCommand(query, db.Connection);
      updateNodeLinkCommand.Parameters.AddWithValue("@id", Guid.ToString());
      updateNodeLinkCommand.Parameters.AddWithValue(
        "@inputConnectorId",
        InputConnectorGuid.ToString()
      );
      updateNodeLinkCommand.Parameters.AddWithValue(
        "@outputConnectorId",
        OutputConnectorGuid.ToString()
      );
      updateNodeLinkCommand.Parameters.AddWithValue(
        "@inputConnectorNodeId",
        InputConnectorNodeGuid.ToString()
      );
      updateNodeLinkCommand.Parameters.AddWithValue(
        "@outputConnectorNodeId",
        OutputConnectorNodeGuid.ToString()
      );
      return updateNodeLinkCommand;
    }

    public MySqlCommand Delete(GmepDatabase db)
    {
      string query =
        @"
        DELETE FROM electrical_single_line_node_links
        WHERE id = @id
        ";
      MySqlCommand deleteNodeLinkCommand = new MySqlCommand(query, db.Connection);
      deleteNodeLinkCommand.Parameters.AddWithValue("@id", Guid.ToString());

      return deleteNodeLinkCommand;
    }
  }
}
