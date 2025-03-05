using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using GMEPNodeGraph.Utilities;
using Livet;
using Microsoft.Win32;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using NodeGraph.Utilities;

namespace GMEPNodeGraph.ViewModels
{
  public interface INodeViewModel
  {
    Guid Guid { get; set; }
    Point Position { get; set; }
    bool IsSelected { get; set; }
  }

  public enum Status
  {
    New,
    Existing,
    Relocated,
  }

  public enum NodeType
  {
    Undefined,
    Service,
    Meter,
    MainBreaker,
    DistributionBus,
    DistributionBreaker,
    Panel,
    PanelBreaker,
    Transformer,
    Equipment,
  }

  public abstract class DefaultNodeViewModel : ViewModel, INodeViewModel
  {
    public string Name
    {
      get => _Name;
      set => RaisePropertyChangedIfSet(ref _Name, value);
    }
    string _Name = string.Empty;

    public NodeType NodeType
    {
      get => _NodeType;
      set => RaisePropertyChangedIfSet(ref _NodeType, value);
    }
    NodeType _NodeType = NodeType.Undefined;

    public double Width
    {
      get => _Width;
      set => RaisePropertyChangedIfSet(ref _Width, value);
    }
    double _Width = 0;

    public double Height
    {
      get => _Height;
      set => RaisePropertyChangedIfSet(ref _Height, value);
    }
    double _Height = 0;

    public Guid Guid
    {
      get => _Guid;
      set => RaisePropertyChangedIfSet(ref _Guid, value);
    }
    Guid _Guid = Guid.NewGuid();

    public Point Position
    {
      get => _Position;
      set => RaisePropertyChangedIfSet(ref _Position, value);
    }
    Point _Position = new Point(0, 0);

    public bool IsSelected
    {
      get => _IsSelected;
      set => RaisePropertyChangedIfSet(ref _IsSelected, value);
    }
    bool _IsSelected = false;

    public Visibility VoltagePhaseVisible
    {
      get => _VoltagePhaseVisible;
      set => RaisePropertyChangedIfSet(ref _VoltagePhaseVisible, value);
    }
    Visibility _VoltagePhaseVisible = Visibility.Collapsed;

    public Visibility ServiceAmpVisible
    {
      get => _ServiceAmpVisible;
      set => RaisePropertyChangedIfSet(ref _ServiceAmpVisible, value);
    }
    Visibility _ServiceAmpVisible = Visibility.Collapsed;
    public Visibility PanelBusAmpVisible
    {
      get => _PanelBusAmpVisible;
      set => RaisePropertyChangedIfSet(ref _PanelBusAmpVisible, value);
    }
    Visibility _PanelBusAmpVisible = Visibility.Collapsed;
    public Visibility PanelMainAmpVisible
    {
      get => _PanelMainAmpVisible;
      set => RaisePropertyChangedIfSet(ref _PanelMainAmpVisible, value);
    }
    Visibility _PanelMainAmpVisible = Visibility.Collapsed;
    public Visibility CtsVisible
    {
      get => _CtsVisible;
      set => RaisePropertyChangedIfSet(ref _CtsVisible, value);
    }
    Visibility _CtsVisible = Visibility.Collapsed;

    public Visibility IsSpaceVisible
    {
      get => _IsSpaceVisible;
      set => RaisePropertyChangedIfSet(ref _IsSpaceVisible, value);
    }
    Visibility _IsSpaceVisible = Visibility.Collapsed;

    public Visibility PanelAmpLabelsVisible
    {
      get => _PanelAmpLabelsVisible;
      set => RaisePropertyChangedIfSet(ref _PanelAmpLabelsVisible, value);
    }
    Visibility _PanelAmpLabelsVisible = Visibility.Collapsed;

    public Visibility MloVisible
    {
      get => _MloVisible;
      set => RaisePropertyChangedIfSet(ref _MloVisible, value);
    }
    Visibility _MloVisible = Visibility.Collapsed;

    public Visibility PoleVisible
    {
      get => _PoleVisible;
      set => RaisePropertyChangedIfSet(ref _PoleVisible, value);
    }
    Visibility _PoleVisible = Visibility.Collapsed;

    public Visibility FuseOnlyVisible
    {
      get => _FuseOnlyVisible;
      set => RaisePropertyChangedIfSet(ref _FuseOnlyVisible, value);
    }
    Visibility _FuseOnlyVisible = Visibility.Collapsed;

    public Visibility TransformerVoltageVisible
    {
      get => _TransformerVoltageVisible;
      set => RaisePropertyChangedIfSet(ref _TransformerVoltageVisible, value);
    }
    Visibility _TransformerVoltageVisible = Visibility.Collapsed;

    public Visibility KvaVisible
    {
      get => _KvaVisible;
      set => RaisePropertyChangedIfSet(ref _KvaVisible, value);
    }
    Visibility _KvaVisible = Visibility.Collapsed;

    public Visibility AfVisible
    {
      get => _AfVisible;
      set => RaisePropertyChangedIfSet(ref _AfVisible, value);
    }
    Visibility _AfVisible = Visibility.Collapsed;

    public Visibility AsVisible
    {
      get => _AsVisible;
      set => RaisePropertyChangedIfSet(ref _AsVisible, value);
    }
    Visibility _AsVisible = Visibility.Collapsed;

    public string Id
    {
      get => _Id;
      set => RaisePropertyChangedIfSet(ref _Id, value);
    }
    string _Id = string.Empty;

    public string ParentId
    {
      get => _ParentId;
      set => RaisePropertyChangedIfSet(ref _ParentId, value);
    }
    string _ParentId = string.Empty;

    public bool Inheritable
    {
      get => _Inheritable;
      set => RaisePropertyChangedIfSet(ref _Inheritable, value);
    }
    bool _Inheritable = false;

    public float PhaseA
    {
      get => _PhaseA;
      set => RaisePropertyChangedIfSet(ref _PhaseA, value);
    }
    float _PhaseA = 0;

    public float PhaseB
    {
      get => _PhaseB;
      set => RaisePropertyChangedIfSet(ref _PhaseB, value);
    }
    float _PhaseB = 0;

    public float PhaseC
    {
      get => _PhaseC;
      set => RaisePropertyChangedIfSet(ref _PhaseC, value);
    }
    float _PhaseC = 0;

    public string ProjectId
    {
      get => _ProjectId;
      set => RaisePropertyChangedIfSet(ref _ProjectId, value);
    }
    string _ProjectId = string.Empty;

    public string ColorCode
    {
      get => _ColorCode;
      set => RaisePropertyChangedIfSet(ref _ColorCode, value);
    }
    string _ColorCode = string.Empty;

    public int CircuitNo
    {
      get => _CircuitNo;
      set => RaisePropertyChangedIfSet(ref _CircuitNo, value);
    }
    int _CircuitNo = 0;

    public virtual int Pole
    {
      get => _Pole;
      set => RaisePropertyChangedIfSet(ref _Pole, value);
    }
    int _Pole = 0;

    public int ParentDistance
    {
      get => _ParentDistance;
      set => RaisePropertyChangedIfSet(ref _ParentDistance, value);
    }
    int _ParentDistance = 0;

    public int StatusId
    {
      get => _StatusId;
      set => RaisePropertyChangedIfSet(ref _StatusId, value);
    }
    int _StatusId = 1;

    public ICommand SizeChangedCommand => _SizeChangedCommand.Get(SizeChanged);
    ViewModelCommandHandler<Size> _SizeChangedCommand = new ViewModelCommandHandler<Size>();

    public ICommand TextBoxDoubleClickCommand => _TextBoxDoubleClickCommand.Get(TextBoxDoubleClick);
    ViewModelCommandHandler<TextBox> _TextBoxDoubleClickCommand =
      new ViewModelCommandHandler<TextBox>();

    private void TextBoxDoubleClick(TextBox textBox)
    {
      textBox.IsEnabled = true;
    }

    public abstract IEnumerable<NodeConnectorViewModel> Inputs { get; }
    public abstract IEnumerable<NodeConnectorViewModel> Outputs { get; }

    public abstract NodeConnectorViewModel FindConnector(Guid guid);

    public abstract List<MySqlCommand> Create(string projectId, GmepDatabase db);

    public abstract List<MySqlCommand> Update(GmepDatabase db);

    public abstract List<MySqlCommand> Delete(GmepDatabase db);

    void SizeChanged(Size newSize)
    {
      Width = newSize.Width;
      Height = newSize.Height;
    }

    public MySqlCommand GetCreateNodeCommand(string projectId, GmepDatabase db)
    {
      string query =
        @"
        INSERT INTO electrical_single_line_nodes
        (id, project_id, loc_x, loc_y, input_connector_id, output_connector_id)
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
      createNodeCommand.Parameters.AddWithValue(
        "@outputConnectorId",
        Outputs.First().Guid.ToString()
      );
      return createNodeCommand;
    }

    public MySqlCommand GetUpdateNodeCommand(GmepDatabase db)
    {
      string query =
        @"
        UPDATE electrical_single_line_nodes
        SET loc_x = @locX, loc_y = @locY
        WHERE id = @id
        ";
      MySqlCommand updateNodeCommand = new MySqlCommand(query, db.Connection);
      updateNodeCommand.Parameters.AddWithValue("@id", Guid.ToString());
      updateNodeCommand.Parameters.AddWithValue("@locX", Position.X);
      updateNodeCommand.Parameters.AddWithValue("@locY", Position.Y);
      return updateNodeCommand;
    }

    public MySqlCommand GetDeleteNodeCommand(GmepDatabase db)
    {
      string query =
        @"
        DELETE FROM electrical_single_line_nodes
        WHERE id = @id
        ";
      MySqlCommand deleteNodeCommand = new MySqlCommand(query, db.Connection);
      deleteNodeCommand.Parameters.AddWithValue("@id", Guid.ToString());
      return deleteNodeCommand;
    }
  }
}
