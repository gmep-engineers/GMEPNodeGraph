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
using GMEPNodeGraph.Utilities;
using Livet;
using MySql.Data.MySqlClient;
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

    public int StatusId
    {
      get => _StatusId;
      set => RaisePropertyChangedIfSet(ref _StatusId, value);
    }
    int _StatusId = 1;

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
      this.InnerWidth = Width;
      this.InnerHeight = Height;
    }

    public MySqlCommand Create(string projectId, GmepDatabase db)
    {
      string query =
        @"
        INSERT INTO electrical_single_line_groups
        (id,project_id, name, loc_x, loc_y, width, height, status_id)
        VALUES (@id, @projectId, @name, @locX, @locY, @width, @height, @statusId)
        ";
      MySqlCommand createGroupCommand = new MySqlCommand(query, db.Connection);
      createGroupCommand.Parameters.AddWithValue("@id", Guid.ToString());
      createGroupCommand.Parameters.AddWithValue("@projectId", projectId);
      createGroupCommand.Parameters.AddWithValue("@name", Name);
      createGroupCommand.Parameters.AddWithValue("@locX", Position.X);
      createGroupCommand.Parameters.AddWithValue("@locY", Position.Y);
      createGroupCommand.Parameters.AddWithValue("@width", InnerWidth);
      createGroupCommand.Parameters.AddWithValue("@height", InnerHeight);
      createGroupCommand.Parameters.AddWithValue("@statusId", StatusId);
      return createGroupCommand;
    }

    public MySqlCommand Update(GmepDatabase db)
    {
      string query =
        @"
        UPDATE electrical_single_line_groups
        SET
        name = @name,
        loc_x = @locX,
        loc_y = @locY,
        width = @width,
        height = @height,
        status_id = @statusId
        WHERE id = @id
        ";
      MySqlCommand updateGroupCommand = new MySqlCommand(query, db.Connection);
      updateGroupCommand.Parameters.AddWithValue("@id", Guid.ToString());
      updateGroupCommand.Parameters.AddWithValue("@name", Name);
      updateGroupCommand.Parameters.AddWithValue("@locX", Position.X);
      updateGroupCommand.Parameters.AddWithValue("@locY", Position.Y);
      updateGroupCommand.Parameters.AddWithValue("@width", InnerWidth);
      updateGroupCommand.Parameters.AddWithValue("@height", InnerHeight);
      updateGroupCommand.Parameters.AddWithValue("@statusId", StatusId);
      return updateGroupCommand;
    }

    public MySqlCommand Delete(GmepDatabase db)
    {
      string query =
        @"
        DELETE FROM electrical_single_line_groups
        WHERE id = @id
        ";
      MySqlCommand deleteGroupCommand = new MySqlCommand(query, db.Connection);
      deleteGroupCommand.Parameters.AddWithValue("@id", Guid.ToString());
      return deleteGroupCommand;
    }
  }
}
