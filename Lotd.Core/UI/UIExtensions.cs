using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Lotd.UI
{
    static class UIExtensions
    {
        public static DataGridViewRow[] GetSelectedRows(this DataGridView dataGridView)
        {
            List<DataGridViewRow> rows = new List<DataGridViewRow>();
            if (dataGridView.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dataGridView.SelectedRows)
                {
                    rows.Add(row);
                }
            }
            if (dataGridView.SelectedCells.Count > 0)
            {
                foreach (DataGridViewCell cell in dataGridView.SelectedCells)
                {
                    DataGridViewRow row = cell.OwningRow;
                    if (!rows.Contains(row))
                    {
                        rows.Add(row);
                    }
                }
            }
            return rows.ToArray();
        }
    }
}
