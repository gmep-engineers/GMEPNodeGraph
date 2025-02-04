using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Livet;
using NodeGraph.Utilities;

namespace GMEPNodeGraph.ViewModels
{
  public class GroupNodeViewModel : ViewModel, INodeViewModel
  {
    public Guid Guid
    {
      get => _Guid;
      set => RaisePropertyChangedIfSet(ref _Guid, value);
    }
    Guid _Guid = Guid.NewGuid();

    public string Name
    {
      get => _Name;
      set => RaisePropertyChangedIfSet(ref _Name, value);
    }
    string _Name = string.Empty;

    public Point Position
    {
      get => _Position;
      set => RaisePropertyChangedIfSet(ref _Position, value, nameof(Comment));
    }
    Point _Position = new Point(0, 0);

    public Point InterlockPosition
    {
      get => _InterlockPosition;
      set => RaisePropertyChangedIfSet(ref _InterlockPosition, value);
    }
    Point _InterlockPosition = new Point(0, 0);

    public Point InnerPosition
    {
      get => _InnerPosition;
      set => RaisePropertyChangedIfSet(ref _InnerPosition, value, nameof(Comment));
    }
    Point _InnerPosition = new Point(0, 0);

    public double InnerWidth
    {
      get => _InnerWidth;
      set => RaisePropertyChangedIfSet(ref _InnerWidth, value, nameof(Comment));
    }
    double _InnerWidth = 100;

    public double InnerHeight
    {
      get => _InnerHeight;
      set => RaisePropertyChangedIfSet(ref _InnerHeight, value, nameof(Comment));
    }
    double _InnerHeight = 100;
    public bool IsEditingName
    {
      get => _IsEditingName;
      set => RaisePropertyChangedIfSet(ref _IsEditingName, value);
    }
    bool _IsEditingName = false;

    public string Comment
    {
      get => $"Width = {(InnerWidth + 4) / 10:F0}, Height = {(InnerHeight + 4) / 10:F0}";
    }

    public bool IsSelected
    {
      get => _IsSelected;
      set => RaisePropertyChangedIfSet(ref _IsSelected, value);
    }
    bool _IsSelected = false;

    public ICommand SizeChangedCommand => _SizeChangedCommand.Get(SizeChanged);
    ViewModelCommandHandler<Size> _SizeChangedCommand = new ViewModelCommandHandler<Size>();

    public ICommand StartEditNameCommand => _StartEditNameCommand.Get(StartEditName);
    ViewModelCommandHandler _StartEditNameCommand = new ViewModelCommandHandler();

    void SizeChanged(Size newSize) { }

    void StartEditName()
    {
      IsEditingName = true;
    }

    public GroupNodeViewModel(string Id, string Name, Point Position, int Width, int Height)
    {
      Guid = Guid.Parse(Id);
      this.Name = Name;
      this.Position = Position;
      this.InnerWidth = Width - 4;
      this.InnerHeight = Height - 4;
    }
  }
}
