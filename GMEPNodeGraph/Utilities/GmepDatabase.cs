using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using GMEPNodeGraph.ViewModels;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Utilities.IO;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace GMEPNodeGraph.Utilities
{
  public class GmepDatabase
  {
    public string ConnectionString { get; set; }
    public MySqlConnection Connection { get; set; }

    public GmepDatabase()
    {
      ConnectionString = Properties.Settings.Default.ConnectionString;
      Connection = new MySqlConnection(ConnectionString);
    }

    public void OpenConnection()
    {
      if (Connection.State == System.Data.ConnectionState.Closed)
      {
        Connection.Open();
      }
    }

    public void CloseConnection()
    {
      if (Connection.State == System.Data.ConnectionState.Open)
      {
        Connection.Close();
      }
    }

    string GetSafeString(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetString(index);
      }
      return string.Empty;
    }

    int GetSafeInt(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetInt32(index);
      }
      return 0;
    }

    float GetSafeFloat(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetFloat(index);
      }
      return 0;
    }

    bool GetSafeBoolean(MySqlDataReader reader, string fieldName)
    {
      int index = reader.GetOrdinal(fieldName);
      if (!reader.IsDBNull(index))
      {
        return reader.GetBoolean(index);
      }
      return false;
    }

    public bool LoginUser(string userName, string password)
    {
      string query =
        @"
            SELECT e.passhash
            FROM employees e 
            WHERE e.username = @username";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@username", userName);
      MySqlDataReader reader = command.ExecuteReader();

      string hashedPassword = "";
      bool result = false;
      if (reader.Read())
      {
        hashedPassword = reader.GetString("passhash");
        result = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
      }
      CloseConnection();
      return result;
    }

    public (string, string, string) GetProjectNameIdVersion(string projectNo, string projectVersion)
    {
      string query =
        "SELECT gmep_project_name, id, version FROM projects WHERE gmep_project_no = @projectNo AND version = @projectVersion";
      if (projectVersion == "latest")
      {
        query =
          "SELECT gmep_project_name, id, version FROM projects WHERE gmep_project_no = @projectNo ORDER BY version DESC";
      }
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      MySqlDataReader reader = command.ExecuteReader();

      string id = string.Empty;
      string name = string.Empty;
      string version = string.Empty;
      if (reader.Read())
      {
        id = reader.GetString("id");
        name = reader.GetString("gmep_project_name");
        version = reader.GetInt32("version").ToString();
      }
      reader.Close();

      CloseConnection();
      return (name, id, version);
    }

    public List<string> GetProjectVersions(string projectNo)
    {
      List<string> projectVersions = new List<string>();
      string query =
        "SELECT version FROM projects WHERE gmep_project_no = @projectNo ORDER BY version DESC";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      MySqlDataReader reader = command.ExecuteReader();

      while (reader.Read())
      {
        projectVersions.Add(reader.GetInt32("version").ToString());
      }
      reader.Close();

      CloseConnection();
      return projectVersions;
    }

    public List<GroupNodeViewModel> GetGroupNodes(string projectId)
    {
      List<GroupNodeViewModel> groups = new List<GroupNodeViewModel>();
      string query = "SELECT * FROM electrical_single_line_groups WHERE project_id = @projectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        groups.Add(
          new GroupNodeViewModel(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "name"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeInt(reader, "width"),
            GetSafeInt(reader, "height")
          )
        );
      }
      CloseConnection();
      return groups;
    }

    public List<ElectricalServiceViewModel> GetElectricalServices(string projectId)
    {
      List<ElectricalServiceViewModel> services = new List<ElectricalServiceViewModel>();
      string query =
        @"
        SELECT
        electrical_services.id as service_id,  
        electrical_services.name,         
        electrical_services.color_code,
        electrical_services.status_id,
        electrical_services.electrical_service_voltage_id,
        electrical_services.electrical_service_amp_rating_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.output_connector_id
        from electrical_services
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_services.node_id
        WHERE electrical_services.project_id = @projectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        services.Add(
          new ElectricalServiceViewModel(
            GetSafeString(reader, "service_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeString(reader, "name"),
            GetSafeInt(reader, "electrical_service_voltage_id"),
            GetSafeInt(reader, "electrical_service_amp_rating_id"),
            GetSafeString(reader, "color_code"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return services;
    }

    public List<ElectricalMeterViewModel> GetElectricalMeters(string projectId)
    {
      List<ElectricalMeterViewModel> meters = new List<ElectricalMeterViewModel>();
      string query =
        @"
        SELECT
        electrical_meters.id as meter_id,
        electrical_meters.has_cts,
        electrical_meters.status_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        from electrical_meters
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_meters.node_id
        WHERE electrical_meters.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        meters.Add(
          new ElectricalMeterViewModel(
            GetSafeString(reader, "meter_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeBoolean(reader, "has_cts"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return meters;
    }

    public List<ElectricalMainBreakerViewModel> GetElectricalMainBreakers(string projectId)
    {
      List<ElectricalMainBreakerViewModel> mainBreakers =
        new List<ElectricalMainBreakerViewModel>();
      string query =
        @"
        SELECT
        electrical_main_breakers.id as breaker_id,
        electrical_main_breakers.has_ground_fault_protection,
        electrical_main_breakers.has_surge_protection,
        electrical_main_breakers.num_poles,
        electrical_main_breakers.status_id,
        electrical_main_breakers.amp_rating_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_main_breakers
        LEFT JOIN statuses ON statuses.id = electrical_main_breakers.status_id
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_main_breakers.node_id
        WHERE electrical_main_breakers.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        mainBreakers.Add(
          new ElectricalMainBreakerViewModel(
            GetSafeString(reader, "breaker_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeInt(reader, "amp_rating_id"),
            GetSafeInt(reader, "num_poles"),
            GetSafeBoolean(reader, "has_ground_fault_protection"),
            GetSafeBoolean(reader, "has_surge_protection"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return mainBreakers;
    }

    public List<ElectricalDistributionBusViewModel> GetElectricalDistributionBuses(string projectId)
    {
      List<ElectricalDistributionBusViewModel> distributionBuses =
        new List<ElectricalDistributionBusViewModel>();
      string query =
        @"
        SELECT
        electrical_distribution_buses.id as dist_bus_id,
        electrical_distribution_buses.status_id,
        electrical_distribution_buses.amp_rating_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_distribution_buses
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_buses.node_id
        WHERE electrical_distribution_buses.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBuses.Add(
          new ElectricalDistributionBusViewModel(
            GetSafeString(reader, "dist_bus_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeInt(reader, "amp_rating_id"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return distributionBuses;
    }

    public List<ElectricalDistributionBreakerViewModel> GetElectricalDistributionBreakers(
      string projectId
    )
    {
      List<ElectricalDistributionBreakerViewModel> distributionBreakers =
        new List<ElectricalDistributionBreakerViewModel>();
      string query =
        @"
        SELECT
        electrical_distribution_breakers.id as dist_bkr_id,
        electrical_distribution_breakers.status_id,
        electrical_distribution_breakers.num_poles,
        electrical_distribution_breakers.amp_rating_id,
        electrical_distribution_breakers.is_fuse_only,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_distribution_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_breakers.node_id
        WHERE electrical_distribution_breakers.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBreakers.Add(
          new ElectricalDistributionBreakerViewModel(
            GetSafeString(reader, "dist_bkr_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeInt(reader, "amp_rating_id"),
            GetSafeInt(reader, "num_poles"),
            GetSafeBoolean(reader, "is_fuse_only"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return distributionBreakers;
    }

    public List<ElectricalPanelViewModel> GetElectricalPanels(string projectId)
    {
      List<ElectricalPanelViewModel> panels = new List<ElectricalPanelViewModel>();
      string query =
        @"
        SELECT
        electrical_panels.id as panel_id,
        electrical_panels.is_mlo,
        electrical_panels.color_code,
        electrical_panels.name,
        electrical_single_line_nodes.loc_x,
        electrical_single_line_nodes.loc_y,
        electrical_panels.status_id, 
        electrical_panels.bus_amp_rating_id, 
        electrical_panels.main_amp_rating_id, 
        electrical_panels.voltage_id, 
        electrical_panels.status_id,
        electrical_single_line_nodes.id as node_id,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_panels
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_panels.node_id
        WHERE electrical_panels.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        panels.Add(
          new ElectricalPanelViewModel(
            GetSafeString(reader, "panel_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeString(reader, "name"),
            GetSafeInt(reader, "voltage_id"),
            GetSafeInt(reader, "bus_amp_rating_id"),
            GetSafeInt(reader, "main_amp_rating_id"),
            GetSafeString(reader, "color_code"),
            GetSafeBoolean(reader, "is_mlo"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return panels;
    }

    public List<ElectricalPanelBreakerViewModel> GetElectricalPanelBreakers(string projectId)
    {
      List<ElectricalPanelBreakerViewModel> distributionBreakers =
        new List<ElectricalPanelBreakerViewModel>();
      string query =
        @"
        SELECT
        electrical_panel_breakers.id as panel_bkr_id,
        electrical_panel_breakers.status_id,
        electrical_panel_breakers.num_poles,
        electrical_panel_breakers.amp_rating_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_panel_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_panel_breakers.node_id
        WHERE electrical_panel_breakers.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBreakers.Add(
          new ElectricalPanelBreakerViewModel(
            GetSafeString(reader, "panel_bkr_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeInt(reader, "amp_rating_id"),
            GetSafeInt(reader, "num_poles"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return distributionBreakers;
    }

    public List<ElectricalDisconnectViewModel> GetElectricalDisconnects(string projectId)
    {
      List<ElectricalDisconnectViewModel> disconnects = new List<ElectricalDisconnectViewModel>();
      string query =
        @"
        SELECT
        electrical_disconnects.id as disconnect_id,
        electrical_disconnects.status_id,
        electrical_disconnects.num_poles,
        electrical_disconnects.as_size_id,
        electrical_disconnects.af_size_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_disconnects
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_disconnects.node_id
        WHERE electrical_disconnects.project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        disconnects.Add(
          new ElectricalDisconnectViewModel(
            GetSafeString(reader, "disconnect_id"),
            projectId,
            GetSafeString(reader, "node_id"),
            GetSafeInt(reader, "as_size_id"),
            GetSafeInt(reader, "af_size_id"),
            GetSafeInt(reader, "num_poles"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return disconnects;
    }

    public List<NodeLinkViewModel> GetNodeLinks(string projectId)
    {
      List<NodeLinkViewModel> nodeConnectors = new List<NodeLinkViewModel>();
      string query =
        @"
        SELECT * from electrical_single_line_node_links WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        nodeConnectors.Add(
          new NodeLinkViewModel(
            GetSafeString(reader, "id"),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id"),
            GetSafeString(reader, "input_connector_node_id"),
            GetSafeString(reader, "output_connector_node_id")
          )
        );
      }
      CloseConnection();
      return nodeConnectors;
    }

    public void SaveElectricalDistributionBreaker(
      ElectricalDistributionBreakerViewModel distributionBreaker
    )
    {
      string query = @"";
      OpenConnection();
    }
  }
}
