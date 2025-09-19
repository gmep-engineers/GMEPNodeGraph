using System;
using System.Collections.Generic;
using System.Windows;
using GMEPNodeGraph.ViewModels;
using MySql.Data.MySqlClient;

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

    public (string, string, string, string) GetProjectNameIdVersion(
      string projectNo,
      string projectVersion
    )
    {
      string query = // HERE check
        @"
        SELECT
        projects.gmep_project_name,
        projects.id as project_id,
        electrical_projects.id as electrical_project_id,
        electrical_projects.version
        FROM projects
        LEFT JOIN electrical_projects ON electrical_projects.project_id = projects.id
        WHERE gmep_project_no = @projectNo
        AND electrical_projects.version = @projectVersion
        ";

      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      int projectVersionNum = 1;
      if (projectVersion != "latest")
      {
        if (Int32.TryParse(projectVersion, out int v))
        {
          projectVersionNum = v;
        }
      }

      command.Parameters.AddWithValue("@projectNo", projectNo);
      command.Parameters.AddWithValue("@projectVersion", projectVersionNum);
      MySqlDataReader reader = command.ExecuteReader();

      string projectId = string.Empty;
      string electricalProjectId = string.Empty;
      string name = string.Empty;
      string version = string.Empty;
      if (reader.Read())
      {
        projectId = GetSafeString(reader, "project_id");
        electricalProjectId = GetSafeString(reader, "electrical_project_id");
        name = GetSafeString(reader, "gmep_project_name");
        version = GetSafeInt(reader, "version").ToString();
      }
      reader.Close();

      CloseConnection();
      return (name, projectId, electricalProjectId, version);
    }

    public List<string> GetElectricalProjectVersions(string projectNo)
    {
      List<string> projectVersions = new List<string>();
      string query =
        @"
        SELECT electrical_projects.version
        FROM electrical_projects
        LEFT JOIN
        projects on projects.id = electrical_projects.project_id
        WHERE
        projects.gmep_project_no = @projectNo
";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@projectNo", projectNo);
      MySqlDataReader reader = command.ExecuteReader();

      while (reader.Read())
      {
        projectVersions.Add(GetSafeInt(reader, "version").ToString());
      }
      reader.Close();

      CloseConnection();
      return projectVersions;
    }

    public List<GroupNodeViewModel> GetGroupNodes(string electricalProjectId)
    {
      List<GroupNodeViewModel> groups = new List<GroupNodeViewModel>();
      string query =
        "SELECT * FROM electrical_single_line_groups WHERE electrical_project_id = @electricalProjectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
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

    public List<ElectricalServiceViewModel> GetElectricalServices(string electricalProjectId)
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
        electrical_services.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.output_connector_id
        from electrical_services
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_services.node_id
        WHERE electrical_services.electrical_project_id = @electricalProjectId";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        services.Add(
          new ElectricalServiceViewModel(
            GetSafeString(reader, "service_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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

    public List<ElectricalMeterViewModel> GetElectricalMeters(string electricalProjectId)
    {
      List<ElectricalMeterViewModel> meters = new List<ElectricalMeterViewModel>();
      string query =
        @"
        SELECT
        electrical_meters.id as meter_id,
        electrical_meters.has_cts,
        electrical_meters.is_space,
        electrical_meters.status_id,
        electrical_meters.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        from electrical_meters
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_meters.node_id
        WHERE electrical_meters.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        meters.Add(
          new ElectricalMeterViewModel(
            GetSafeString(reader, "meter_id"), // HERE
            GetSafeString(reader, "project_id"),
            electricalProjectId,
            GetSafeString(reader, "node_id"),
            GetSafeBoolean(reader, "has_cts"),
            GetSafeBoolean(reader, "is_space"),
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

    public List<ElectricalMainBreakerViewModel> GetElectricalMainBreakers(
      string electricalProjectId
    )
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
        electrical_main_breakers.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_main_breakers
        LEFT JOIN statuses ON statuses.id = electrical_main_breakers.status_id
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_main_breakers.node_id
        WHERE electrical_main_breakers.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        mainBreakers.Add(
          new ElectricalMainBreakerViewModel(
            GetSafeString(reader, "breaker_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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

    public List<ElectricalDistributionBusViewModel> GetElectricalDistributionBuses(
      string electricalProjectId
    )
    {
      List<ElectricalDistributionBusViewModel> distributionBuses =
        new List<ElectricalDistributionBusViewModel>();
      string query =
        @"
        SELECT
        electrical_distribution_buses.id as dist_bus_id,
        electrical_distribution_buses.status_id,
        electrical_distribution_buses.amp_rating_id,
        electrical_distribution_buses.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_distribution_buses
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_buses.node_id
        WHERE electrical_distribution_buses.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBuses.Add(
          new ElectricalDistributionBusViewModel(
            GetSafeString(reader, "dist_bus_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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
      string electricalProjectId
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
        electrical_distribution_breakers.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_distribution_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_distribution_breakers.node_id
        WHERE electrical_distribution_breakers.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBreakers.Add(
          new ElectricalDistributionBreakerViewModel(
            GetSafeString(reader, "dist_bkr_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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

    public List<ElectricalPanelViewModel> GetElectricalPanels(string electricalProjectId)
    {
      List<ElectricalPanelViewModel> panels = new List<ElectricalPanelViewModel>();
      string query =
        @"
        SELECT
        electrical_panels.id as panel_id,
        electrical_panels.is_mlo,
        electrical_panels.is_recessed,
        electrical_panels.color_code,
        electrical_panels.name,
        electrical_panels.project_id,
        electrical_single_line_nodes.loc_x,
        electrical_single_line_nodes.loc_y,
        electrical_panels.status_id, 
        electrical_panels.bus_amp_rating_id, 
        electrical_panels.main_amp_rating_id, 
        electrical_panels.voltage_id, 
        electrical_panels.status_id,
        electrical_panels.num_breakers,
        electrical_single_line_nodes.id as node_id,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_panels
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_panels.node_id
        WHERE electrical_panels.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        panels.Add(
          new ElectricalPanelViewModel(
            GetSafeString(reader, "panel_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
            GetSafeString(reader, "node_id"),
            GetSafeString(reader, "name"),
            GetSafeInt(reader, "voltage_id"),
            GetSafeInt(reader, "bus_amp_rating_id"),
            GetSafeInt(reader, "main_amp_rating_id"),
            GetSafeString(reader, "color_code"),
            GetSafeBoolean(reader, "is_mlo"),
            GetSafeBoolean(reader, "is_recessed"),
            GetSafeInt(reader, "num_breakers"),
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

    public List<ElectricalPanelBreakerViewModel> GetElectricalPanelBreakers(
      string electricalProjectId
    )
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
        electrical_panel_breakers.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_panel_breakers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_panel_breakers.node_id
        WHERE electrical_panel_breakers.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        distributionBreakers.Add(
          new ElectricalPanelBreakerViewModel(
            GetSafeString(reader, "panel_bkr_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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

    public List<ElectricalDisconnectViewModel> GetElectricalDisconnects(string electricalProjectId)
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
        electrical_disconnects.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_disconnects
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_disconnects.node_id
        WHERE electrical_disconnects.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        disconnects.Add(
          new ElectricalDisconnectViewModel(
            GetSafeString(reader, "disconnect_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
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

    public List<ElectricalTransformerViewModel> GetElectricalTransformers(
      string electricalProjectId
    )
    {
      List<ElectricalTransformerViewModel> transformers =
        new List<ElectricalTransformerViewModel>();
      string query =
        @"
        SELECT
        electrical_transformers.id as transformer_id,
        electrical_transformers.status_id,
        electrical_transformers.kva_id,
        electrical_transformers.voltage_id,
        electrical_transformers.name,
        electrical_transformers.color_code,
        electrical_transformers.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_transformers
        LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_transformers.node_id
        WHERE electrical_transformers.electrical_project_id = @electricalProjectId
        ";
      OpenConnection();

      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        transformers.Add(
          new ElectricalTransformerViewModel(
            GetSafeString(reader, "transformer_id"),
            GetSafeString(reader, "project_id"),
            electricalProjectId,
            GetSafeString(reader, "node_id"),
            GetSafeString(reader, "name"),
            GetSafeInt(reader, "voltage_id"),
            GetSafeInt(reader, "kva_id"),
            GetSafeString(reader, "color_code"),
            GetSafeInt(reader, "status_id"),
            new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
            GetSafeString(reader, "input_connector_id"),
            GetSafeString(reader, "output_connector_id")
          )
        );
      }
      CloseConnection();
      return transformers;
    }

    public List<ElectricalEquipmentViewModel> GetElectricalEquipment(string electricalProjectId)
    {
      List<ElectricalEquipmentViewModel> equipment = new List<ElectricalEquipmentViewModel>();
      string query =
        @"
        SELECT
        electrical_equipment.id as equipment_id,
        electrical_equipment.node_id,
        electrical_equipment.voltage_id,
        electrical_equipment.mca,
        electrical_equipment.fla,
        electrical_equipment.aic_rating,
        electrical_equipment.is_three_phase,
        electrical_equipment.equip_no,
        electrical_equipment.category_id,
        electrical_equipment.hp,
        electrical_equipment.status_id,
        electrical_equipment.project_id,
        electrical_single_line_nodes.id as node_id,  
        electrical_single_line_nodes.loc_x,   
        electrical_single_line_nodes.loc_y,
        electrical_single_line_nodes.input_connector_id,
        electrical_single_line_nodes.output_connector_id
        FROM electrical_equipment
		    LEFT JOIN electrical_single_line_nodes ON electrical_single_line_nodes.id = electrical_equipment.node_id
        WHERE electrical_equipment.electrical_project_id = @electricalProjectId
        AND node_id IS NOT NULL
        ";
      OpenConnection();

      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
      MySqlDataReader reader = command.ExecuteReader();
      while (reader.Read())
      {
        if (!String.IsNullOrEmpty(GetSafeString(reader, "node_id")))
        {
          equipment.Add(
            new ElectricalEquipmentViewModel(
              GetSafeString(reader, "equipment_id"),
              GetSafeString(reader, "project_id"),
              electricalProjectId,
              GetSafeString(reader, "node_id"),
              GetSafeString(reader, "equip_no"),
              GetSafeInt(reader, "voltage_id"),
              GetSafeFloat(reader, "mca"),
              GetSafeFloat(reader, "fla"),
              GetSafeFloat(reader, "aic_rating"),
              GetSafeBoolean(reader, "is_three_phase"),
              GetSafeString(reader, "hp"),
              GetSafeInt(reader, "category_id"),
              GetSafeInt(reader, "status_id"),
              new Point(GetSafeInt(reader, "loc_x"), GetSafeInt(reader, "loc_y")),
              GetSafeString(reader, "input_connector_id")
            )
          );
        }
      }
      CloseConnection();
      return equipment;
    }

    public List<NodeLinkViewModel> GetNodeLinks(string electricalProjectId)
    {
      List<NodeLinkViewModel> nodeConnectors = new List<NodeLinkViewModel>();
      string query =
        @"
        SELECT * from electrical_single_line_node_links WHERE electrical_project_id = @electricalProjectId
        ";
      OpenConnection();
      MySqlCommand command = new MySqlCommand(query, Connection);
      command.Parameters.AddWithValue("@electricalProjectId", electricalProjectId);
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
