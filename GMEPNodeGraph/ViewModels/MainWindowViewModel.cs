using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GMEPNodeGraph.Utilities;
using GMEPNodeGraph.ViewModels;
using GMEPNodeGraph.Views;
using Livet;
using Livet.Commands;
using MySql.Data.MySqlClient;
using NodeGraph.NET6.Controls;
using NodeGraph.NET6.Operation;
using NodeGraph.Utilities;

namespace GMEPNodeGraph.ViewModels
{
  public enum GroupIntersectType
  {
    CursorPointVMDefine,
    BoundingBoxVMDefine,
  }

  public enum RangeSelectionMode
  {
    ContainVMDefine,
    IntersectVMDefine,
  }

  public class MainWindowViewModel : ViewModel
  {
    Stack<Action> UndoActions = new Stack<Action>();
    Stack<Action> RedoActions = new Stack<Action>();
    Stack<Action> ExecutableRedoActions = new Stack<Action>();
    public string ProjectNo
    {
      get => _ProjectNo;
      set => RaisePropertyChangedIfSet(ref _ProjectNo, value);
    }
    string _ProjectNo = string.Empty;

    public string ProjectVersion
    {
      get => _ProjectVersion;
      set => RaisePropertyChangedIfSet(ref _ProjectVersion, value);
    }
    string _ProjectVersion = string.Empty;

    public List<string> ProjectVersions
    {
      get => _ProjectVersions;
      set => RaisePropertyChangedIfSet(ref _ProjectVersions, value);
    }
    List<string> _ProjectVersions;

    public string ProjectId
    {
      get => _ProjectId;
      set => RaisePropertyChangedIfSet(ref _ProjectId, value);
    }
    string _ProjectId = string.Empty;

    public bool ProjectLoaded
    {
      get => _ProjectLoaded;
      set => RaisePropertyChangedIfSet(ref _ProjectLoaded, value);
    }
    bool _ProjectLoaded = true;
    public double Scale
    {
      get => _Scale;
      set => RaisePropertyChangedIfSet(ref _Scale, value);
    }
    double _Scale = 1.0f;

    public Point Offset
    {
      get => _Offset;
      set => RaisePropertyChangedIfSet(ref _Offset, value);
    }
    Point _Offset = new Point();

    public Point RightClickPoint
    {
      get => _RightClickPoint;
      set => RaisePropertyChangedIfSet(ref _RightClickPoint, value);
    }
    Point _RightClickPoint = new Point();

    Queue<MySqlCommand> CommandQueue = new Queue<MySqlCommand>();
    GmepDatabase db = new GmepDatabase();

    public ViewModelCommand LoadProjectNodesCommand =>
      _LoadProjectNodesCommand.Get(LoadProjectNodes);
    ViewModelCommandHandler _LoadProjectNodesCommand = new ViewModelCommandHandler();

    public ViewModelCommand AddServiceCommand => _AddServiceCommand.Get(AddService);
    ViewModelCommandHandler _AddServiceCommand = new ViewModelCommandHandler();

    public ViewModelCommand AddMainBreakerCommand => _AddMainBreakerCommand.Get(AddMainBreaker);
    ViewModelCommandHandler _AddMainBreakerCommand = new ViewModelCommandHandler();
    public ViewModelCommand AddMeterCommand => _AddMeterCommand.Get(AddMeter);
    ViewModelCommandHandler _AddMeterCommand = new ViewModelCommandHandler();
    public ViewModelCommand AddBusCommand => _AddBusCommand.Get(AddBus);
    ViewModelCommandHandler _AddBusCommand = new ViewModelCommandHandler();
    public ViewModelCommand AddDistributionBreakerCommand =>
      _AddDistributionBreakerCommand.Get(AddDistributionBreaker);
    ViewModelCommandHandler _AddDistributionBreakerCommand = new ViewModelCommandHandler();

    public ViewModelCommand AddPanelBreakerCommand => _AddPanelBreakerCommand.Get(AddPanelBreaker);
    ViewModelCommandHandler _AddPanelBreakerCommand = new ViewModelCommandHandler();
    public ViewModelCommand AddPanelCommand => _AddPanelCommand.Get(AddPanel);
    ViewModelCommandHandler _AddPanelCommand = new ViewModelCommandHandler();

    public ViewModelCommand AddTransformerCommand => _AddTransformerCommand.Get(AddTransformer);
    ViewModelCommandHandler _AddTransformerCommand = new ViewModelCommandHandler();

    public ViewModelCommand AddGroupNodeCommand => _AddGroupNodeCommand.Get(AddGroupNode);
    ViewModelCommandHandler _AddGroupNodeCommand = new ViewModelCommandHandler();

    public ViewModelCommand RemoveNodesCommand => _RemoveNodesCommand.Get(RemoveNodes);
    ViewModelCommandHandler _RemoveNodesCommand = new ViewModelCommandHandler();

    public ViewModelCommand SaveCommand => _SaveCommand.Get(Save);
    ViewModelCommandHandler _SaveCommand = new ViewModelCommandHandler();

    public ViewModelCommand UndoCommand => _UndoCommand.Get(Undo);
    ViewModelCommandHandler _UndoCommand = new ViewModelCommandHandler();

    public ViewModelCommand RedoCommand => _RedoCommand.Get(Redo);
    ViewModelCommandHandler _RedoCommand = new ViewModelCommandHandler();

    public ListenerCommand<PreviewConnectLinkOperationEventArgs> PreviewConnectLinkCommand =>
      _PreviewConnectLinkCommand.Get(PreviewConnect);
    ViewModelCommandHandler<PreviewConnectLinkOperationEventArgs> _PreviewConnectLinkCommand =
      new ViewModelCommandHandler<PreviewConnectLinkOperationEventArgs>();

    public ListenerCommand<ConnectedLinkOperationEventArgs> ConnectedLinkCommand =>
      _ConnectedLinkCommand.Get(Connected);
    ViewModelCommandHandler<ConnectedLinkOperationEventArgs> _ConnectedLinkCommand =
      new ViewModelCommandHandler<ConnectedLinkOperationEventArgs>();

    public ListenerCommand<DisconnectedLinkOperationEventArgs> DisconnectedLinkCommand =>
      _DisconnectedLinkCommand.Get(Disconnected);
    ViewModelCommandHandler<DisconnectedLinkOperationEventArgs> _DisconnectedLinkCommand =
      new ViewModelCommandHandler<DisconnectedLinkOperationEventArgs>();

    public ListenerCommand<EndMoveNodesOperationEventArgs> EndMoveNodesCommand =>
      _EndMoveNodesCommand.Get(NodesMoved);
    ViewModelCommandHandler<EndMoveNodesOperationEventArgs> _EndMoveNodesCommand =
      new ViewModelCommandHandler<EndMoveNodesOperationEventArgs>();

    public ListenerCommand<IList> SelectionChangedCommand =>
      _SelectionChangedCommand.Get(SelectionChanged);
    ViewModelCommandHandler<IList> _SelectionChangedCommand = new ViewModelCommandHandler<IList>();

    public ViewModelCommand ClearNodesCommand => _ClearNodesCommand.Get(ClearNodes);
    ViewModelCommandHandler _ClearNodesCommand = new ViewModelCommandHandler();

    public ViewModelCommand ClearNodeLinksCommand => _ClearNodeLinksCommand.Get(ClearNodeLinks);
    ViewModelCommandHandler _ClearNodeLinksCommand = new ViewModelCommandHandler();

    public ViewModelCommand MoveGroupNodeCommand => _MoveGroupNodeCommand.Get(MoveGroupNode);
    ViewModelCommandHandler _MoveGroupNodeCommand = new ViewModelCommandHandler();

    public ViewModelCommand ChangeGroupInnerSizeCommand =>
      _ChangeGroupInnerSizeCommand.Get(ChangeGroupInnerSize);
    ViewModelCommandHandler _ChangeGroupInnerSizeCommand = new ViewModelCommandHandler();

    public ViewModelCommand ChangeGroupInnerPositionCommand =>
      _ChangeGroupInnerPositionCommand.Get(ChangeGroupInnerPosition);
    ViewModelCommandHandler _ChangeGroupInnerPositionCommand = new ViewModelCommandHandler();

    public ViewModelCommand ResetScaleCommand => _ResetScaleCommand.Get(ResetScale);
    ViewModelCommandHandler _ResetScaleCommand = new ViewModelCommandHandler();

    public IEnumerable<DefaultNodeViewModel> NodeViewModels => _NodeViewModels;
    ObservableCollection<DefaultNodeViewModel> _NodeViewModels =
      new ObservableCollection<DefaultNodeViewModel>();

    public IEnumerable<NodeLinkViewModel> NodeLinkViewModels => _NodeLinkViewModels;
    ObservableCollection<NodeLinkViewModel> _NodeLinkViewModels =
      new ObservableCollection<NodeLinkViewModel>();

    public IEnumerable<GroupNodeViewModel> GroupNodeViewModels => _GroupNodeViewModels;
    ObservableCollection<GroupNodeViewModel> _GroupNodeViewModels =
      new ObservableCollection<GroupNodeViewModel>();

    public GroupIntersectType[] GroupIntersectTypes { get; } =
      Enum.GetValues(typeof(GroupIntersectType)).OfType<GroupIntersectType>().ToArray();
    public RangeSelectionMode[] RangeSelectionModes { get; } =
      Enum.GetValues(typeof(RangeSelectionMode)).OfType<RangeSelectionMode>().ToArray();

    public GroupIntersectType SelectedGroupIntersectType
    {
      get => _SelectedGroupIntersectType;
      set => RaisePropertyChangedIfSet(ref _SelectedGroupIntersectType, value);
    }
    GroupIntersectType _SelectedGroupIntersectType;

    public RangeSelectionMode SelectedRangeSelectionMode
    {
      get => _SelectedRangeSelectionMode;
      set => RaisePropertyChangedIfSet(ref _SelectedRangeSelectionMode, value);
    }
    RangeSelectionMode _SelectedRangeSelectionMode = RangeSelectionMode.ContainVMDefine;

    public bool IsLockedAllNodeLinks
    {
      get => _IsLockedAllNodeLinks;
      set => UpdateIsLockedAllNodeLinksProperty(value);
    }
    bool _IsLockedAllNodeLinks = false;

    public bool IsEnableAllNodeConnectors
    {
      get => _IsEnableAllNodeConnectors;
      set => UpdateIsEnableAllNodeConnectorsProperty(value);
    }
    bool _IsEnableAllNodeConnectors = true;

    public bool AllowToOverrideConnection
    {
      get => _AllowToOverrideConnection;
      set => RaisePropertyChangedIfSet(ref _AllowToOverrideConnection, value);
    }
    bool _AllowToOverrideConnection = true;

    public bool ClipToBounds
    {
      get => _ClipToBounds;
      set => RaisePropertyChangedIfSet(ref _ClipToBounds, value);
    }
    bool _ClipToBounds = true;

    public MainWindowViewModel() { }

    void AddService()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalServiceViewModel service = new ElectricalServiceViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "Service Feeder",
        1,
        100,
        "#FFFFFFFF",
        1,
        p
      );
      _NodeViewModels.Add(service);
      service.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(service);
        service.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddService);
    }

    public void LoadService(ElectricalServiceViewModel electricalServiceViewModel)
    {
      _NodeViewModels.Add(electricalServiceViewModel);
    }

    void AddMainBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalMainBreakerViewModel mainBreaker = new ElectricalMainBreakerViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        100,
        3,
        false,
        false,
        1,
        p
      );
      _NodeViewModels.Add(mainBreaker);
      mainBreaker.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(mainBreaker);
        mainBreaker.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddMainBreaker);
    }

    void AddMeter()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalMeterViewModel meter = new ElectricalMeterViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        false,
        1,
        p
      );
      _NodeViewModels.Add(meter);
      meter.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(meter);
        meter.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddMeter);
    }

    void AddBus()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalDistributionBusViewModel distributionBus = new ElectricalDistributionBusViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        100,
        1,
        p
      );
      _NodeViewModels.Add(distributionBus);
      distributionBus.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(distributionBus);
        distributionBus.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddBus);
    }

    void AddPanel()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalPanelViewModel panel = new ElectricalPanelViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "New Panel",
        1,
        100,
        0,
        "#FFFFFFFF",
        true,
        1,
        p
      );
      _NodeViewModels.Add(panel);
      panel.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(panel);
        panel.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddPanel);
    }

    void AddDistributionBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalDistributionBreakerViewModel distributionBreaker =
        new ElectricalDistributionBreakerViewModel(
          Guid.NewGuid().ToString(),
          Guid.NewGuid().ToString(),
          100,
          3,
          false,
          1,
          p
        );
      _NodeViewModels.Add(distributionBreaker);
      distributionBreaker.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(distributionBreaker);
        distributionBreaker.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddDistributionBreaker);
    }

    void AddTransformer()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalTransformerViewModel transformer = new ElectricalTransformerViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        "Transformer",
        1,
        1,
        "#FFFFFFFF",
        1,
        p
      );
      _NodeViewModels.Add(transformer);
      transformer.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(transformer);
        transformer.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddTransformer);
    }

    void AddPanelBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      ElectricalPanelBreakerViewModel panelBreaker = new ElectricalPanelBreakerViewModel(
        Guid.NewGuid().ToString(),
        Guid.NewGuid().ToString(),
        1,
        3,
        1,
        p
      );
      _NodeViewModels.Add(panelBreaker);
      panelBreaker.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
      UndoActions.Push(() =>
      {
        _NodeViewModels.Remove(panelBreaker);
        panelBreaker.Delete(db).ForEach(CommandQueue.Enqueue);
      });
      RedoActions.Push(AddPanelBreaker);
    }

    void AddGroupNode()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _GroupNodeViewModels.Add(
        new GroupNodeViewModel(Guid.NewGuid().ToString(), "Section", p, 500, 900)
      );
    }

    void RemoveNodes()
    {
      var removeNodes = _NodeViewModels.Where(arg => arg.IsSelected).ToArray();
      foreach (var removeNode in removeNodes)
      {
        removeNode.Delete(db).ForEach(CommandQueue.Enqueue);
        _NodeViewModels.Remove(removeNode);

        var removeNodeLink = NodeLinkViewModels.FirstOrDefault(arg =>
          arg.InputConnectorNodeGuid == removeNode.Guid
          || arg.OutputConnectorNodeGuid == removeNode.Guid
        );
        _NodeLinkViewModels.Remove(removeNodeLink);
      }
      var removeGroups = _GroupNodeViewModels.Where(arg => arg.IsSelected).ToArray();
      foreach (var removeGroup in removeGroups)
      {
        _GroupNodeViewModels.Remove(removeGroup);
      }

      UndoActions.Push(() =>
      {
        foreach (var removeNode in removeNodes)
        {
          removeNode.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);
          _NodeViewModels.Add(removeNode);

          var removeNodeLink = NodeLinkViewModels.FirstOrDefault(arg =>
            arg.InputConnectorNodeGuid == removeNode.Guid
            || arg.OutputConnectorNodeGuid == removeNode.Guid
          );
          _NodeLinkViewModels.Add(removeNodeLink);
        }
        var removeGroups = _GroupNodeViewModels.Where(arg => arg.IsSelected).ToArray();
        foreach (var removeGroup in removeGroups)
        {
          _GroupNodeViewModels.Add(removeGroup);
        }
      });
      RedoActions.Push(RemoveNodes);
    }

    void ClearNodes()
    {
      _NodeLinkViewModels.Clear();
      _NodeViewModels.Clear();
    }

    void ClearNodeLinks()
    {
      _NodeLinkViewModels.Clear();
    }

    void MoveGroupNode()
    {
      _GroupNodeViewModels[0].InterlockPosition = new Point(0, 0);
    }

    void ChangeGroupInnerSize()
    {
      _GroupNodeViewModels[0].InnerWidth = 300;
      _GroupNodeViewModels[0].InnerHeight = 300;
    }

    void ChangeGroupInnerPosition()
    {
      _GroupNodeViewModels[0].InnerPosition = new Point(0, 0);
    }

    void ResetScale()
    {
      Scale = 1.0f;
    }

    void UpdateIsLockedAllNodeLinksProperty(bool value)
    {
      _IsLockedAllNodeLinks = !_IsLockedAllNodeLinks;

      foreach (var nodeLink in _NodeLinkViewModels)
      {
        nodeLink.IsLocked = _IsLockedAllNodeLinks;
      }

      RaisePropertyChanged(nameof(IsLockedAllNodeLinks));
    }

    void UpdateIsEnableAllNodeConnectorsProperty(bool value)
    {
      _IsEnableAllNodeConnectors = !_IsEnableAllNodeConnectors;

      foreach (var node in _NodeViewModels)
      {
        foreach (var input in node.Inputs)
        {
          input.IsEnable = _IsEnableAllNodeConnectors;
        }
        foreach (var output in node.Outputs)
        {
          output.IsEnable = _IsEnableAllNodeConnectors;
        }
      }

      RaisePropertyChanged(nameof(IsEnableAllNodeConnectors));
    }

    void PreviewConnect(PreviewConnectLinkOperationEventArgs args)
    {
      var inputNode = NodeViewModels.First(arg => arg.Guid == args.ConnectToEndNodeGuid);
      var inputConnector = inputNode.FindConnector(args.ConnectToEndConnectorGuid);
      try
      {
        args.CanConnect = inputConnector.Label == "Limited Input" == false;
      }
      catch (Exception ex)
      {
        return;
      }
    }

    void Connected(ConnectedLinkOperationEventArgs param)
    {
      var nodeLink = new NodeLinkViewModel(
        Guid.NewGuid().ToString(),
        param.InputConnectorGuid.ToString(),
        param.OutputConnectorGuid.ToString(),
        param.InputConnectorNodeGuid.ToString(),
        param.OutputConnectorNodeGuid.ToString()
      );
      _NodeLinkViewModels.Add(nodeLink);

      nodeLink.Create(ProjectId, db).ForEach(CommandQueue.Enqueue);

      DefaultNodeViewModel childNode = _NodeViewModels.FirstOrDefault(arg =>
        arg.Guid == param.InputConnectorNodeGuid
      );
      DefaultNodeViewModel parentNode = _NodeViewModels.FirstOrDefault(arg =>
        arg.Guid == param.OutputConnectorNodeGuid
      );

      if (parentNode != null && childNode != null)
      {
        childNode.ParentId = parentNode.Id;
      }
    }

    void Disconnected(DisconnectedLinkOperationEventArgs param)
    {
      var nodeLink = _NodeLinkViewModels.First(arg => arg.Guid == param.NodeLinkGuid);
      nodeLink.Delete(db).ForEach(CommandQueue.Enqueue);
      DefaultNodeViewModel childNode = _NodeViewModels.FirstOrDefault(arg =>
        arg.Guid == param.InputConnectorNodeGuid
      );
      DefaultNodeViewModel parentNode = _NodeViewModels.FirstOrDefault(arg =>
        arg.Guid == param.OutputConnectorNodeGuid
      );
      if (childNode != null)
      {
        childNode.ParentId = string.Empty;
      }
      _NodeLinkViewModels.Remove(nodeLink);
    }

    void NodesMoved(EndMoveNodesOperationEventArgs param) { }

    void SelectionChanged(IList list) { }

    void LoadProjectNodes()
    {
      if (string.IsNullOrEmpty(_ProjectNo))
      {
        return;
      }
      if (string.IsNullOrEmpty(_ProjectVersion))
      {
        _ProjectVersion = "latest";
      }
      GmepDatabase db = new GmepDatabase();
      string projectId = db.GetProjectId(ProjectNo, ProjectVersion);
      ProjectVersions = db.GetProjectVersions(ProjectNo);
      ProjectVersions[0] += " (latest)";
      List<GroupNodeViewModel> groupNodes = db.GetGroupNodes(projectId);
      List<ElectricalServiceViewModel> services = db.GetElectricalServices(projectId);
      List<ElectricalMeterViewModel> meters = db.GetElectricalMeters(projectId);
      // get everything else
      // load main window with everything
      foreach (ElectricalServiceViewModel service in services)
      {
        LoadService(service);
      }
    }

    void Save()
    {
      {
        MySqlCommand command = CommandQueue.Dequeue();
        while (command != null)
        {
          command.ExecuteNonQuery();
          command = CommandQueue.Dequeue();
        }
      }
      foreach (DefaultNodeViewModel node in _NodeViewModels)
      {
        List<MySqlCommand> commands = node.Update(db);
        foreach (MySqlCommand command in commands)
        {
          command.ExecuteNonQuery();
        }
      }
    }

    void Undo()
    {
      Action undoAction = UndoActions.Pop();
      if (undoAction != null)
      {
        undoAction();
        ExecutableRedoActions.Push(RedoActions.Pop());
      }
    }

    void Redo()
    {
      Action redoAction = ExecutableRedoActions.Pop();
      if (redoAction != null)
      {
        redoAction();
      }
    }
  }
}
