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
	public enum MOVIE_TYPE
	{
		QT = 0,
		MP4,
		AVI,
		OTHER
	}


	public class MovieInfo
	{
		private MOVIE_TYPE m_MOVIE_TYPE = MOVIE_TYPE.OTHER;
		public MOVIE_TYPE MOVIE_TYPE {  get { return m_MOVIE_TYPE; } }
		private string m_FilePath = "";
		public string FilePath {  get { return m_FilePath; } }
		private string m_FileName = "";
		public string FileName
		{
			get { return m_FileName; }
		}
		private int m_Width = 0;
		public int Width { get { return m_Width; } }
		private int m_Height = 0;
		public int Height { get { return m_Height; } }
		private double m_FrameRate = 0;
		public double FrameRate { get { return m_FrameRate; } }
		private double m_Duration = 0;
		public double Duration { get { return m_Duration; } }
		private int m_FrameCount = 0;
		public int FrameCount { get { return m_FrameCount; } }
		private long m_FileSize = 0;
		public long FileSize { get { return m_FileSize; } }
		public string Comment = "";
		// ********************************************
		public MovieInfo(string p = "")
		{
			Init();
			if (p!="") SetFilePath(p);
		}
		// ********************************************
		public void Init()
		{
			m_MOVIE_TYPE = MOVIE_TYPE.OTHER;
			m_FilePath = "";
			m_FileName = "";
			m_Width = 0;
			m_Height = 0;
			m_FrameRate = 0;
			m_Duration = 0;
			m_FileSize = 0;
			Comment = "";
		}
		// ********************************************
		private byte [] GetByteData(string p ,long sz,long start = 0)
		{
			byte[] ret = new byte[0];
			if (File.Exists(p) == false) return ret;
			FileStream fs = new System.IO.FileStream(p,FileMode.Open,FileAccess.Read);
			ret = new byte[sz];
			try
			{
				int rs = fs.Read(ret, (int)start, (int)sz);
				if (rs != sz)
				{
					ret = new byte[0];
				}
			}
			catch
			{
				ret = new byte[0];
			}
			finally
			{
				fs.Close();
			}
			return ret;
		}
		// ********************************************
		private long FindByteData(byte [] h,string tag)
		{
			long ret = -1;
			if (h.Length <= 0) return ret;
			if (tag.Length <= 0) return ret;
			byte[] tagB = System.Text.Encoding.ASCII.GetBytes(tag);
			int v = tagB.Length;
			if (v > 4) v = 4;
			int cnt = h.Length - v;
			for ( int i = 0; i<cnt; i++)
			{
				bool b = true;
				for ( int j=0; j < v;j++)
				{
					if( h[i+j] != tagB[j])
					{
						b = false;
						break;
					}
				}
				if(b==true)
				{
					ret = i;
					break;
				}
			}

			return ret;
		}
		// ********************************************
		private MOVIE_TYPE GetHeader(string p)
		{
			 MOVIE_TYPE ret = MOVIE_TYPE.OTHER;

			byte[] bs = GetByteData(p, 16, 0);
			if (bs.Length <= 0) return ret;

			FileStream fs = new System.IO.FileStream(p,FileMode.Open,FileAccess.Read);
			if ((bs[0]==0x52)&&(bs[1]==0x49)&&(bs[2]==0x46)&&(bs[3]==0x46) // 52 49 46 46
				&&(bs[8]==0x41)&&(bs[9]==0x56)&&(bs[10]==0x49)&&(bs[11]==0x20) ) //41 56 49 20
			{
				ret = MOVIE_TYPE.AVI;
			}
			else if ( (bs[4]==0x66)&&(bs[5]==0x74)&&(bs[6]==0x79)&&(bs[7]==0x70))//66 74 79 70
			{
				if( (bs[8]==0x71)&&(bs[9]==0x74)) //71 74 20 20
				{
					ret = MOVIE_TYPE.QT;
				}else if( (bs[8]==0x6D)&&(bs[9]==0x70)&&(bs[10]==0x34)) //6D 70 34
				{
					ret = MOVIE_TYPE.MP4;
				}
			}
			return ret;
		}
		// ********************************************
		public bool SetFilePath(string p)
		{
			bool ret = false;
			Init();
			if (File.Exists(p) == false) return ret;
			m_MOVIE_TYPE = GetHeader(p);
			if (m_MOVIE_TYPE == MOVIE_TYPE.OTHER) return ret;
			m_FilePath = p;
			m_FileName = Path.GetFileName(p);
			FileInfo finfo = new FileInfo(p);
			m_FileSize = finfo.Length;

			switch (m_MOVIE_TYPE)
			{
				case MOVIE_TYPE.AVI:
					GetStatusAVI();
					ret = true;
					break;
				case MOVIE_TYPE.QT:
				case MOVIE_TYPE.MP4:
					GetStatusQT();
					ret = true;
					break;
			}
			return ret;
		}
		// ********************************************
		private void GetStatusAVI()
		{
			byte[] h = GetByteData(m_FilePath, 0x50, 0);
			if (h.Length <= 0) return;
			m_Width         = ((int)h[0x40]) + ((int)h[0x41] << 8) + ((int)h[0x42] << 16) + (int)(h[0x43] << 24);
			m_Height        = ((int)h[0x44]) + ((int)h[0x45] << 8) + ((int)h[0x46] << 16) + ((int)h[0x47] << 24);
			m_FrameCount    = (h[0x30]) + (h[0x31] << 8) + (h[0x32] << 16) + (h[0x33] << 24);
			int fri         = (h[0x20]) + (h[0x21] << 8) + (h[0x22] << 16) + (h[0x23] << 24);
			if (fri>0) {
				double fr = 1000000.0 / (double)fri;

				int tmp = (int)(fr * 1000 + 0.5);
				fr  = (double)tmp / 1000;
				m_FrameRate     = fr;
			}
			int frv = (int)(m_FrameRate + 0.5);
			m_Duration = (double)m_FrameCount /(double)frv;
		}
		// ********************************************
		private void GetStatusQT()
		{
			int sz = 1024 * 30;
			//まず頭
			byte[] h = GetByteData(m_FilePath, sz);
			long idx = -1;
			idx = FindByteData(h, "moov");
			if (idx < 0)
			{
				long v = m_FileSize - sz;
				if (v < 0) v = 0;
				h = GetByteData(m_FilePath, sz, v);
				idx = FindByteData(h, "moov");
				if (idx < 0) return;
			}
			idx = FindByteData(h, "tkhd");

			if (idx >= 0)
			{
				m_Width = ((int)h[idx + 78] << 24) + ((int)h[idx + 79] << 16) + ((int)h[idx + 80] << 8) + (int)h[idx + 81];
				m_Height = ((int)h[idx + 82] << 24) + ((int)h[idx + 83] << 16) + ((int)h[idx + 84] << 8) + (int)h[idx + 85];
			}
			idx = FindByteData(h, "stts");
			if (idx >= 0)
			{
				m_FrameCount = ((int)h[idx + 12] << 24) + ((int)h[idx + 13] << 16) + ((int)h[idx + 14] << 8) + (int)h[idx + 15];
			}
			if (m_FrameCount > 0)
			{
				idx = FindByteData(h, "mvhd");
				if (idx >= 0)
				{
					int ts = ((int)h[idx + 16] << 24) + ((int)h[idx + 17] << 16) + ((int)h[idx + 18] << 8) + (int)h[idx + 19];
					int du = ((int)h[idx + 20] << 24) + ((int)h[idx + 21] << 16) + ((int)h[idx + 22] << 8) + (int)h[idx + 23];
					m_Duration = ((double)du / (double)ts);
					m_FrameRate = (double)m_FrameCount / (double)m_Duration;
					m_FrameRate = (double)((int)(m_FrameRate * 1000 + 0.5)) / 1000;


					if ((m_FrameRate == 24.0) || (m_FrameRate == 30.0) || (m_FrameRate == 29.97) || (m_FrameRate == 23.976))
					{

					}
					else if ((m_FrameRate > 23.96) && (m_FrameRate < 23.98))
					{
						m_FrameRate = 23.976;
					}


					//フレームれーとを四捨五入してからdurationを求める
					int frv = (int)(m_FrameRate + 0.5);
					m_Duration = (double)m_FrameCount / (double)frv;
				}
			}
		}
		public string InfoStr()
		{
			return String.Format("Name:{0},Image Size:{1}x{2},FrameRate:{3},Duration:{4},FrameCount:{5},File Size:{6}",
				m_FileName,
				m_Width, m_Height,
				m_FrameRate,
				m_Duration,
				m_FrameCount,
				m_FileSize
				);
		}
	}
}
