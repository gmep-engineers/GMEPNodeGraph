using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public string GetProjectId(string projectNo, string projectVersion)
    {
      string query =
        "SELECT id FROM projects WHERE gmep_project_no = @projectNo AND version = @projectVersion";
      if (projectVersion == "latest")
      {
        query = "SELECT id FROM projects WHERE gmep_project_no = @projectNo ORDER BY version DESC";
      }
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      MySqlDataReader reader = command.ExecuteReader();

      string id = string.Empty;
      if (reader.Read())
      {
        id = reader.GetString("id");
      }
      reader.Close();

      CloseConnection();
      return id;
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
        projectVersions.Add(reader.GetString("version"));
      }
      reader.Close();

      CloseConnection();
      return projectVersions;
    }

    public List<GroupNodeViewModel> GetGroupNodes(string projectId)
    {
      List<GroupNodeViewModel> groups = new List<GroupNodeViewModel>();
      string query = "SELECT * FROM electrical_single_line_grousp WHERE project_id = @projectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        groups.Add(
          new GroupNodeViewModel(
            reader.GetString("id"),
            reader.GetString("name"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y")),
            reader.GetInt32("width"),
            reader.GetInt32("height")
          )
        );
      }
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
        electrical_services.amp_rating_id,    
        electrical_services.color_code,
        electrical_services.status_id,
        electrical_services.electrical_service_voltage_id,
        electrical_services.electrical_service_amp_rating_id,
        electrical_single_line_nodes.id as node_id,   
        electrical_single_line_nodes.groud_id,   
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        from electrical_services
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_services.node_id
        WHERE project_id = @projectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        services.Add(
          new ElectricalServiceViewModel(
            reader.GetString("service_id"),
            reader.GetString("node_id"),
            reader.GetString("name"),
            reader.GetInt32("voltage_id"),
            reader.GetInt32("amp_rating_id"),
            reader.GetString("color_code"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
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
        electrical_single_line_nodes.parent_id as node_parent_id,
        from electrical_meters
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_meters.node_id
        WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        meters.Add(
          new ElectricalMeterViewModel(
            reader.GetString("meter_id"),
            reader.GetString("node_id"),
            reader.GetBoolean("has_cts"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
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
        electrical_single_line_nodes.id as node_id
        electrical_single_line_nodes.parent_id as node_parent_id
        FROM electrical_main_breakers
        LEFT JOIN statuses ON statuses.id = electrical_main_breakers.status_id
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_main_breakers.node_id
        WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        mainBreakers.Add(
          new ElectricalMainBreakerViewModel(
            reader.GetString("meter_id"),
            reader.GetString("node_id"),
            reader.GetInt32("amp_rating_id"),
            reader.GetInt32("num_poles"),
            reader.GetBoolean("has_ground_fault_protection"),
            reader.GetBoolean("has_ground_surge_protection"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
      return mainBreakers;
    }

    public List<ElectricalDistributionBusViewModel> GetElectricalDistributionBuses(string projectId)
    {
      List<ElectricalDistributionBusViewModel> distributionBuses =
        new List<ElectricalDistributionBusViewModel>();
      string query =
        @"
        SELECT
        electrical_main_breakers.id as dist_bus_id,
        electrical_main_breakers.status_id,
        electrical_main_breakers.amp_rating_id,
        electrical_single_line_nodes.id as node_id
        electrical_single_line_nodes.parent_id as node_parent_id
        FROM electrical_main_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_main_breakers.node_id
        WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBuses.Add(
          new ElectricalDistributionBusViewModel(
            reader.GetString("dist_bus_id"),
            reader.GetString("node_id"),
            reader.GetInt32("amp_rating_id"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
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
        electrical_single_line_nodes.id as node_id
        electrical_single_line_nodes.parent_id as node_parent_id
        FROM electrical_distribution_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_breakers.node_id
        WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBreakers.Add(
          new ElectricalDistributionBreakerViewModel(
            reader.GetString("dist_bkr_id"),
            reader.GetString("node_id"),
            reader.GetInt32("amp_rating_id"),
            reader.GetInt32("num_poles"),
            reader.GetBoolean("is_fuse_only"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
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
        electrical_panels.loc_x,
        electrical_panels.loc_y,
        electrical_panels.status_id, 
        electrical_panels.bus_amp_rating_id, 
        electrical_panels.main_amp_rating_id, 
        electrical_panels.voltage_id, 
        electrical_panels.status_id, 
        FROM electrical_panels
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_breakers.node_id
        WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        panels.Add(
          new ElectricalPanelViewModel(
            reader.GetString("panel_id"),
            reader.GetString("node_id"),
            reader.GetString("name"),
            reader.GetInt32("voltage_id"),
            reader.GetInt32("bus_amp_rating"),
            reader.GetInt32("maint_amp_rating"),
            reader.GetString("color_code"),
            reader.GetBoolean("is_mlo"),
            reader.GetInt32("status_id"),
            new Point(reader.GetInt32("loc_x"), reader.GetInt32("loc_y"))
          )
        );
      }
      return panels;
    }

    public List<NodeLinkViewModel> GetNodeConnectors(string projectId)
    {
      List<NodeLinkViewModel> nodeConnectors = new List<NodeLinkViewModel>();
      string query =
        @"
        SELECT * from electrical_single_line_node_connectors WHERE project_id = @projectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectId", projectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        nodeConnectors.Add(
          new NodeLinkViewModel(
            reader.GetString("id"),
            reader.GetString("input_connector_id"),
            reader.GetString("output_connector_id"),
            reader.GetString("input_connector_node_id"),
            reader.GetString("output_connector_node_id")
          )
        );
      }
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
