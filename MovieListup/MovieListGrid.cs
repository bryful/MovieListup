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

		private List<MovieInfo> m_MovieList = new List<MovieInfo>();

		private int m_BoldCount = 8;

		public int BoldCount
		{
			get { return m_BoldCount; }
			set
			{
				int b = value;
				if (m_BoldCount != b) m_BoldCount = b;
				if(m_MovieList.Count > 0)
				{
					for (int i = 0; i < m_MovieList.Count; i++)
					{
						if ( m_MovieList[i].BoldCount != b)
						{
							m_MovieList[i].BoldCount = b;
							MovieInfoToGrid(i);
						}
					}
				}
			}
		}

		public MovieListGrid()
		{
			this.AllowDrop = true;

			int w = 0;
			for (int i = 0; i < WidthDef.Length; i++) w += WidthDef[i];
			this.Size = new Size(w, 100);

			this.AllowUserToDeleteRows = false;
			this.AllowUserToAddRows = false;
			this.AllowUserToOrderColumns = true;

			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);



			this.CellValueChanged += MovieListGrid_CellValueChanged;
		}

		private void MovieListGrid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			int col = e.ColumnIndex;
			int idx = e.RowIndex;
			if((col==5)&&(idx>=0))
			{
				string s =this[col, idx].Value.ToString();
				if(m_MovieList[idx].Comment !=s)
				{
					m_MovieList[idx].Comment = s;
				}
			}
		}

		protected override void InitLayout()
		{
			this.ColumnCount = 6;
			for ( int i = 0; i<6; i++)
			{
				this.Columns[i].Width = WidthDef[i];
				this.Columns[i].HeaderText = HeaderDef[i];
				this.Columns[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
			}
		}
		protected override void OnDragEnter(DragEventArgs drgevent)
		{
			//base.OnDragEnter(drgevent);

			if (drgevent.Data.GetDataPresent(DataFormats.FileDrop))
			{
				drgevent.Effect = DragDropEffects.All;
			}
			else
			{
				drgevent.Effect = DragDropEffects.None;
			}
		}
		protected override void OnDragDrop(DragEventArgs drgevent)
		{
			//base.OnDragDrop(drgevent);
			string[] files = (string[])drgevent.Data.GetData(DataFormats.FileDrop, false);
			foreach(string f in files)
			{
				AddMovie(f);
			}
		}
		protected override void OnResize(EventArgs e)
		{
			if (this.ColumnCount < 6) return;
			int addW = 0;
			for ( int i = 0; i<6; i++)
			{
				WidthDef[i] = this.Columns[i].Width;
				addW += this.Columns[i].Width;
			}
			int addW2 = addW - this.Columns[5].Width;

			if ( this.ClientSize.Width > addW2)
			{
				this.Columns[5].Width = this.ClientSize.Width - addW2;
			}
			base.OnResize(e);
		}
		/// <summary>
		/// 登録されたムービーから検索
		/// </summary>
		/// <param name="p"></param>
		/// <returns></returns>
		private int FindMovie(MovieInfo mi)
		{
			int ret = -1;

			if((m_MovieList.Count>0)&&(mi.FilePath!=""))
			{
				for ( int i= 0; i<m_MovieList.Count; i++)
				{
					if(m_MovieList[i].FilePath == mi.FilePath)
					{
						ret = i;
						break;
					}
				}
			}

			return ret;
		}
		/// <summary>
		/// ムービーを追加
		/// </summary>
		/// <param name="p">ムービーのパス</param>
		public void AddMovie(string p)
		{
			MovieInfo mi = new MovieInfo(p);
			if(mi.FileName!="")
			{
				if (FindMovie(mi) < 0)
				{
					mi.BoldCount = m_BoldCount;
					int idx = this.Rows.Add();
					this[0, idx].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
					this[1, idx].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
					this[2, idx].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
					this[3, idx].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
					this[4, idx].Style.Alignment = DataGridViewContentAlignment.MiddleRight;
					this[5, idx].Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
					m_MovieList.Add(mi);
					MovieInfoToGrid(idx);
				}
			}
		}
		public void AddMovies(string [] files)
		{
			if (files.Length > 0)
			{
				foreach (string s in files)
				{
					AddMovie(s);
				}
			}
		}
		/// <summary>
		/// MovieリストをGridへ表示
		/// </summary>
		/// <param name="idx"></param>
		public void MovieInfoToGrid(int idx)
		{
			this[0, idx].Value = m_MovieList[idx].FileName;
			this[1, idx].Value = m_MovieList[idx].SizeInfoStr;
			this[2, idx].Value = m_MovieList[idx].FrameRateInfoStr;
			this[3, idx].Value = m_MovieList[idx].DurationInfoStr;
			this[4, idx].Value = m_MovieList[idx].FileSizeInfoStr;
			this[5, idx].Value = m_MovieList[idx].Comment;

		}

	}
}
