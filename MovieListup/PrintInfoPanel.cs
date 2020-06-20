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

using BRY;

using Codeplex.Data;

namespace MovieListup
{
	public class PrintInfoPanel : GroupBox
	{
		private readonly int m_RowHeight = 32;
		private readonly int m_LabelWidth = 60;
		private readonly int m_LabelHeight = 26;
		private readonly int m_CombWidth = 200;
		private readonly int m_SideWidth = 5;
		private readonly int m_TopHeight = 20;
		private readonly int m_BottomHeight = 5;
		private readonly int m_ButtonWidth = 100;


		private readonly string[]  m_Captions = new string[]{"タイトル","メモ","","日付","会社"};

		private readonly int m_Count = 5;

		private Label[] m_Labels = new Label[5];
		private ComboBox [] m_Combs = new ComboBox[5];


		private Button m_BtnDate = new Button();

		private string GetCombText(int idx)
		{
			string ret = "";
			if ((idx < 0) || (idx > 5)) return ret;
			if (m_Combs[idx] != null)
			{
				ret =  m_Combs[idx].Text;
			}
			return ret;
		}
		private void SetCombText(int idx,string s)
		{
			if ((idx < 0) || (idx > 5)) return;
			if (m_Combs[idx] != null)
			{
				if (m_Combs[idx].Text != s)
				{
					m_Combs[idx].Text = s;
					HistryPushComb(m_Combs[idx]);
				}
			}
		}
		public string PTitle
		{
			get{ return GetCombText(0); }
			set{ SetCombText(0, value); }
		}

		public string PMemo1
		{
			get{ return GetCombText(1); }
			set{ SetCombText(1, value); }
		}
		public string PMemo2
		{
			get{ return GetCombText(2); }
			set{ SetCombText(2, value); }
		}
		public string PDate
		{
			get{ return GetCombText(3); }
			set{ SetCombText(3, value); }
		}
		public string PCampany
		{
			get{ return GetCombText(4); }
			set{ SetCombText(4, value); }
		}

		public PrintInfoPanel()
		{
			Font f =new Font(this.Font.FontFamily,12);
			this.Font = f;
			int h = m_TopHeight + m_BottomHeight + m_RowHeight * 5;

			this.Size = new Size(m_SideWidth*2 + m_LabelWidth + m_CombWidth, h);

			for (int i=0;i<m_Count;i++)
			{
				NewLabel(i, m_Captions[i]);
				NewComb(i);

				this.Controls.Add(m_Labels[i]);
				this.Controls.Add(m_Combs[i]);
			}
			this.Text = "印刷項目";
			m_BtnDate = new Button();
			m_BtnDate.Name = "Date";
			m_BtnDate.Text = "日付";
			m_BtnDate.Size = new Size(m_ButtonWidth, m_LabelHeight);
			m_BtnDate.Location = new Point(m_Combs[3].Left + m_Combs[3].Width + 5, m_Combs[3].Top);
			m_BtnDate.Click += M_BtnDate_Click;
			this.Controls.Add(m_BtnDate);
			this.SetStyle(ControlStyles.DoubleBuffer, true);
			this.SetStyle(ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);


		}

		private void M_BtnDate_Click(object sender, EventArgs e)
		{
			DateTime dt = DateTime.Now;
			m_Combs[3].Text = String.Format("{0}年{1}月{2}日({3})", dt.Year, dt.Month, dt.Day, dt.DayOfWeek);
		}

		protected override void InitLayout()
		{
			base.InitLayout();
			this.Text = "印刷項目";
		}
		private void NewLabel(int idx,string s = "")
		{
			m_Labels[idx] = new Label();
			m_Labels[idx].Text = s;
			m_Labels[idx].TextAlign = ContentAlignment.MiddleRight;
			m_Labels[idx].AutoSize = false;
			m_Labels[idx].Size = new Size(m_LabelWidth, m_LabelHeight);
			m_Labels[idx].Location = new Point(m_SideWidth, m_TopHeight + m_RowHeight * idx);
			m_Labels[idx].Margin = new Padding(0);
			m_Labels[idx].Padding = new Padding(0);

		}
		private void NewComb(int idx)
		{
			m_Combs[idx] = new ComboBox();
			m_Combs[idx].AutoSize = false;
			if (idx == 3)
			{
				m_Combs[idx].Size = new Size(m_CombWidth-m_ButtonWidth-10, m_LabelHeight);
			}
			else
			{
				m_Combs[idx].Size = new Size(m_CombWidth, m_LabelHeight);
			}
			int l = m_SideWidth + m_LabelWidth;
			m_Combs[idx].Location = new Point(l, m_TopHeight + m_RowHeight * idx);
			m_Combs[idx].Margin = new Padding(0);
			m_Combs[idx].Padding = new Padding(0);
			m_Combs[idx].KeyDown += PrintInfoPanel_KeyDown;

		}

		private void PrintInfoPanel_KeyDown(object sender, KeyEventArgs e)
		{
			if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
			{
				ComboBox cmb = (ComboBox)sender;
				HistryPushComb(cmb);
			}
		}

		protected override void OnResize(EventArgs e)
		{
			if (m_Labels[0] != null)
			{
				int w = this.ClientSize.Width - (m_Labels[0].Left + m_Labels[0].Width + m_SideWidth * 2);
				for (int i = 0; i < m_Count; i++)
				{
					if (m_Combs[i] != null)
					{
						if (i == 3)
						{
							int w2  = w - (m_ButtonWidth+10);
							if (m_Combs[i].Width != w2)
							{
								m_Combs[i].Width = w2;
							}
						}
						else
						{
							if (m_Combs[i].Width != w)
							{
								m_Combs[i].Width = w;
							}
						}
					}
				}
			}
			if (m_Combs[3] != null)
			{
				m_BtnDate.Location = new Point(m_Combs[3].Left + m_Combs[3].Width + 5, m_Combs[3].Top);
			}

			base.OnResize(e);
		}
		public void HistryPush()
		{
			for (int i = 0; i < m_Count; i++) HistryPushComb(m_Combs[i]);
		}
		private void HistryPushComb(ComboBox cmb)
		{
			if (cmb == null) return;
			string s = cmb.Text.Trim();
			if (s == "") return;

			int idx = -1;
			if(cmb.Items.Count>0)
			{
				for (int i = 0; i < cmb.Items.Count; i++) {
					if (cmb.Items[i].ToString() == s)
					{
						idx = i;
						break;
					}
				}
			}
			if (idx >= 0)
			{
				cmb.Items.RemoveAt(idx);
			}
			cmb.Items.Insert(0, s);
		}
		private string[] CombToArray(ComboBox cmb)
		{
			if (cmb == null) return new string[0];
			int cnt = cmb.Items.Count;
			string[] ret = new string[cnt];
			if(cnt>0)
			{
				for (int i=0;i<cnt; i++)
				{
					ret[i] = cmb.Items[i].ToString().Trim();
				}
			}
			return ret;
		}
		public string ToJson()
		{
			HistryPush();
			dynamic obj = new DynamicJson();

			obj["Title"] = m_Combs[0].Text;
			obj["TitleHis"] = CombToArray(m_Combs[0]);

			obj["Momo1"] = m_Combs[1].Text;
			obj["Momo1His"] = CombToArray(m_Combs[1]);

			obj["Momo2"] = m_Combs[2].Text;
			obj["Momo2His"] = CombToArray(m_Combs[2]);

			obj["Date"] = m_Combs[3].Text;
			obj["DateHis"] = CombToArray(m_Combs[3]);

			obj["Campany"] = m_Combs[4].Text;
			obj["CampanyHis"] = CombToArray(m_Combs[4]);

			string ret = obj.ToString();
			return ret;

		}
		public string HisFilePath()
		{
			return Path.Combine(Application.UserAppDataPath, Path.GetFileNameWithoutExtension(Application.ExecutablePath) + "_his.json");

		}
		public bool SaveHis()
		{
			bool ret = false;
			try
			{
				string js = ToJson();
				File.WriteAllText(HisFilePath(), js, Encoding.GetEncoding("utf-8"));
				ret = true;
			}
			catch
			{
				ret = false;
			}
			return ret;
		}
		private void CombFromObj(ComboBox cmb,dynamic obj,string key1,string key2)
		{
			if (cmb == null) return;
			if (obj.IsDefined(key1) == true)
			{
				cmb.Text = obj[key1];

			}
			if (obj.IsDefined(key2) == true)
			{
				if (obj[key2].IsArray)
				{
					string [] ary = obj[key2];
					cmb.Items.AddRange(ary);
				}
			}
		}
		public bool LoadHis()
		{
			bool ret = false;
			string p = HisFilePath();
			dynamic json = new DynamicJson();
			try
			{
				if (File.Exists(p) == true)
				{
					string str = File.ReadAllText(p, Encoding.GetEncoding("utf-8"));
					if (str != "")
					{
						json = DynamicJson.Parse(str);
						ret = true;
					}
				}
			}
			catch
			{
				ret = false;
				return ret;
			}
			CombFromObj(m_Combs[0], json, "Title", "TitleHis");
			CombFromObj(m_Combs[1], json, "Momo1", "Momo1His");
			CombFromObj(m_Combs[2], json, "Momo2", "Momo2His");
			CombFromObj(m_Combs[3], json, "Date", "DateHis");
			CombFromObj(m_Combs[4], json, "Campany", "CampanyHis");
			return ret;
		}
	}
}
