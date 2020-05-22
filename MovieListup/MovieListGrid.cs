using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace MovieListup
{
	public class MovieListGrid : DataGridView
	{
		private int[] WidthDef = new int[] { 200, 100, 100, 100, 100, 200 };
		private string [] HeaderDef = new string[] { "Movie Name", "Image Size", "FrameRate", "Duration", "File Size", "Comment" };
		public MovieListGrid()
		{
			int w = 0;
			for (int i = 0; i < WidthDef.Length; i++) w += WidthDef[i];
			this.Size = new Size(w, 100);
			this.AllowUserToDeleteRows = false;
			this.AllowUserToAddRows = false;
		}
		protected override void InitLayout()
		{
			this.ColumnCount = 6;
			for ( int i = 0; i<6; i++)
			{
				this.Columns[i].Width = WidthDef[i];
				this.Columns[i].HeaderText = HeaderDef[i];
			}

		}
	}
}
