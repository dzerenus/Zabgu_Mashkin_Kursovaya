using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace UDPFlood.Flooder.Elements;

public class ThreadsDataGrid : DataGrid
{
    public bool PreventUserClick = false;

    public ThreadsDataGrid()
    {

    }

    protected override void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
    {
        this.UnselectAllCells();
    }
}
