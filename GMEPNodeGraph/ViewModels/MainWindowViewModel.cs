using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
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
    bool _ProjectLoaded = false;
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

    public ViewModelCommand AddGroupNodeCommand => _AddGroupNodeCommand.Get(AddGroupNode);
    ViewModelCommandHandler _AddGroupNodeCommand = new ViewModelCommandHandler();

    public ViewModelCommand RemoveNodesCommand => _RemoveNodesCommand.Get(RemoveNodes);
    ViewModelCommandHandler _RemoveNodesCommand = new ViewModelCommandHandler();

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
      _NodeViewModels.Add(new ServiceFeeder(p));
    }

    public void LoadService(ElectricalServiceViewModel electricalServiceViewModel)
    {
      _NodeViewModels.Add(electricalServiceViewModel);
    }

    void AddMainBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(new MainBreaker(p));
    }

    void AddMeter()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(new Meter(p));
    }

    void AddBus()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(new Bus(p));
    }

    void AddPanel()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(
        new ElectricalPanelViewModel(
          new Guid().ToString(),
          new Guid().ToString(),
          new Guid().ToString(),
          "New Panel",
          "120/208 3Φ",
          100,
          0,
          "#FFFFFFFF",
          true,
          "NEW",
          p
        )
      );
    }

    void AddDistributionBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(new DistributionBreaker(p));
    }

    void AddPanelBreaker()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _NodeViewModels.Add(new DistributionBreaker(p) { Name = "Panel Breaker" });
    }

    void AddGroupNode()
    {
      Point p = new Point((RightClickPoint.X - (Offset.X)), (RightClickPoint.Y - (Offset.Y)));
      _GroupNodeViewModels.Add(new GroupNodeViewModel(new Guid().ToString(), "Section", p, 50, 90));
    }

    void RemoveNodes()
    {
      var removeNodes = _NodeViewModels.Where(arg => arg.IsSelected).ToArray();
      foreach (var removeNode in removeNodes)
      {
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
      var nodeLink = new NodeLinkViewModel()
      {
        OutputConnectorGuid = param.OutputConnectorGuid,
        OutputConnectorNodeGuid = param.OutputConnectorNodeGuid,
        InputConnectorGuid = param.InputConnectorGuid,
        InputConnectorNodeGuid = param.InputConnectorNodeGuid,
        IsLocked = IsLockedAllNodeLinks,
      };
      _NodeLinkViewModels.Add(nodeLink);
    }

    void Disconnected(DisconnectedLinkOperationEventArgs param)
    {
      var nodeLink = _NodeLinkViewModels.First(arg => arg.Guid == param.NodeLinkGuid);
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
  }
}
